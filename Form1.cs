using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CreateMIDI
{
    public partial class Form1 : Form
    {
        private const int MaxEndpointNameLength = 64;
        private bool _isCreating;

        public Form1()
        {
            InitializeComponent();
            ApplyExecutableIcon();

            cmbEndpointVersion.SelectedIndex = 0;
            UpdatePreviewAndCreateButton();

            if (IsMidiServiceRunning())
            {
                lblMidiStatus.Text = "✓ Windows MIDI Services found";
                lblMidiStatus.ForeColor = Color.Green;
            }
            else
            {
                lblMidiStatus.Text = "✗ Windows MIDI Services not found";
                lblMidiStatus.ForeColor = Color.Red;
            }
        }

        private static bool IsMidiServiceRunning()
        {
            try
            {
                using (ServiceController sc = new ServiceController("midisrv"))
                {
                    return sc.Status == ServiceControllerStatus.Running;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ApplyExecutableIcon()
        {
            try
            {
                Icon exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (exeIcon != null)
                {
                    this.Icon = exeIcon;
                }
            }
            catch
            {
                // Keep default icon if extraction fails.
            }
        }

        private bool IsMidi1Selected()
        {
            return cmbEndpointVersion.SelectedItem != null && cmbEndpointVersion.SelectedItem.ToString() == "MIDI 1.0";
        }

        private void UpdatePreviewAndCreateButton()
        {
            bool isMidi2 = !IsMidi1Selected();
            bool hasName = !string.IsNullOrWhiteSpace(PortName.Text);

            lblPortsWillBeCreated.Text = isMidi2 ? "This bidirectional port will be created" : "These ports will be created:";

            if (hasName)
            {
                string name = PortName.Text.Trim();
                if (!isMidi2)
                {
                    lblToPreview.Text = "WM to " + name;
                    lblFromPreview.Text = "WM from " + name;
                    lblToPreview.ForeColor = SystemColors.ControlText;
                    lblFromPreview.ForeColor = SystemColors.ControlText;
                    lblToSuffix.Text = "(A)";
                    lblToSuffix.Visible = false;
                    lblFromSuffix.Text = "(B)";
                    lblFromSuffix.Visible = false;
                }
                else
                {
                    lblToPreview.Text = name;
                    lblFromPreview.Text = "(A) (B): Labels are shown by some DAW hosts";
                    lblToPreview.ForeColor = SystemColors.ControlText;
                    lblFromPreview.ForeColor = Color.DarkGreen;

                    lblToSuffix.Text = "(A) (B)";
                    int suffixWidth = TextRenderer.MeasureText(lblToSuffix.Text, lblToSuffix.Font).Width;
                    int rightAlignedStart = PortName.Right - suffixWidth;

                    lblToSuffix.Location = new Point(rightAlignedStart, lblToPreview.Top);
                    lblToSuffix.Visible = true;
                    lblFromSuffix.Visible = false;
                }
            }
            else
            {
                lblToPreview.Text = "Waiting for Name";
                lblFromPreview.Text = "Waiting for Name";
                lblToPreview.ForeColor = SystemColors.ControlText;
                lblFromPreview.ForeColor = SystemColors.ControlText;
                lblToSuffix.Text = "(A)";
                lblToSuffix.Visible = false;
                lblFromSuffix.Text = "(B)";
                lblFromSuffix.Visible = false;
            }

            if (_isCreating)
            {
                create.Text = "Creating...";
                create.Enabled = false;
                create.BackColor = SystemColors.Control;
            }
            else
            {
                create.Text = isMidi2 ? "Create Port" : "Create Ports";
                create.Enabled = hasName;
                create.BackColor = hasName ? Color.LimeGreen : SystemColors.Control;
            }
        }

        private void SetCreationInProgress(bool isCreating)
        {
            _isCreating = isCreating;
            UseWaitCursor = isCreating;
            UpdatePreviewAndCreateButton();
        }

        private void PortName_TextChanged(object sender, EventArgs e)
        {
            UpdatePreviewAndCreateButton();
        }

        private void cmbEndpointVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreviewAndCreateButton();
        }

        private static bool ValidateEndpointName(string name, out string validationMessage)
        {
            if (name.Length > MaxEndpointNameLength)
            {
                validationMessage = "Endpoint name is too long. Maximum length is " + MaxEndpointNameLength + " characters.";
                return false;
            }

            if (name.IndexOf('"') >= 0)
            {
                validationMessage = "Endpoint name cannot contain double-quote characters (\").";
                return false;
            }

            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsControl(name[i]))
                {
                    validationMessage = "Endpoint name cannot contain control characters.";
                    return false;
                }
            }

            validationMessage = string.Empty;
            return true;
        }

        private static string ResolveMidiExePath()
        {
            const string exeName = "midi.exe";

            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] pathParts = pathEnv.Split(Path.PathSeparator);
            foreach (string part in pathParts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                try
                {
                    string candidate = Path.Combine(part.Trim(), exeName);
                    if (File.Exists(candidate))
                        return candidate;
                }
                catch
                {
                    // Ignore invalid PATH entries.
                }
            }

            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string[] fallbackCandidates =
            {
                Path.Combine(programFiles, "Windows MIDI Services", "Tools", "Console", exeName),
                Path.Combine(programFiles, "Windows MIDI", "Tools", "Console", exeName)
            };

            foreach (string candidate in fallbackCandidates)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            return fallbackCandidates[0];
        }

        private sealed class CommandRunResult
        {
            public bool Success { get; private set; }
            public string ErrorDetails { get; private set; }

            private CommandRunResult(bool success, string errorDetails)
            {
                Success = success;
                ErrorDetails = errorDetails;
            }

            public static CommandRunResult Ok()
            {
                return new CommandRunResult(true, string.Empty);
            }

            public static CommandRunResult Fail(string errorDetails)
            {
                return new CommandRunResult(false, errorDetails ?? string.Empty);
            }
        }

        private static string BuildCommandErrorDetails(string arguments, int exitCode, string stdOut, string stdErr)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Command failed: " + arguments);
            sb.AppendLine("Exit code: " + exitCode);

            if (!string.IsNullOrWhiteSpace(stdErr))
            {
                sb.AppendLine();
                sb.AppendLine("Error output:");
                sb.AppendLine(stdErr.Trim());
            }

            if (!string.IsNullOrWhiteSpace(stdOut))
            {
                sb.AppendLine();
                sb.AppendLine("Standard output:");
                sb.AppendLine(stdOut.Trim());
            }

            return sb.ToString().Trim();
        }

        private static bool TryRunMidiQuery(string arguments, out string stdOut)
        {
            stdOut = string.Empty;
            string exePath = ResolveMidiExePath();

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        stdOut = output;
                        return true;
                    }
                }
            }
            catch (Win32Exception)
            {
                return false;
            }

            return false;
        }

        private static bool TryGetMidiListOutput(out string output)
        {
            string[] listCommands =
            {
                "endpoint list",
                "loopback list",
                "midi1-loopback list",
                "list"
            };

            for (int i = 0; i < listCommands.Length; i++)
            {
                if (TryRunMidiQuery(listCommands[i], out output) && !string.IsNullOrWhiteSpace(output))
                    return true;
            }

            output = string.Empty;
            return false;
        }

        private static bool EndpointExistsViaMidiList(params string[] names)
        {
            string output;
            if (!TryGetMidiListOutput(out output))
                return false;

            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                for (int j = 0; j < names.Length; j++)
                {
                    string name = names[j];
                    if (!string.IsNullOrWhiteSpace(name) && line.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        private static CommandRunResult RunMidiCommands(string[] args)
        {
            string exePath = ResolveMidiExePath();

            foreach (string arg in args)
            {
                try
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = arg,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        process.Start();

                        string stdOut = process.StandardOutput.ReadToEnd();
                        string stdErr = process.StandardError.ReadToEnd();

                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            return CommandRunResult.Fail(BuildCommandErrorDetails(arg, process.ExitCode, stdOut, stdErr));
                        }
                    }
                }
                catch (Win32Exception ex)
                {
                    return CommandRunResult.Fail("Unable to start midi.exe from: " + exePath + "\r\n" + ex.Message);
                }
            }

            return CommandRunResult.Ok();
        }

        private CommandRunResult CreateMidi1Endpoints(string baseName)
        {
            string[] args =
            {
                $"midi1-loopback create --name \"WM to {baseName}\"",
                $"midi1-loopback create --name \"WM from {baseName}\""
            };

            return RunMidiCommands(args);
        }

        private CommandRunResult CreateMidi2Endpoints(string baseName)
        {
            string[] args =
            {
                $"loopback create --root-name \"{baseName}\""
            };

            return RunMidiCommands(args);
        }

        private CommandRunResult CreateMidi1EndpointWithExactName(string endpointName)
        {
            string escapedName = endpointName.Replace("\"", "\\\"");
            string[] args =
            {
                "midi1-loopback create --name \"" + escapedName + "\""
            };

            return RunMidiCommands(args);
        }

        [DllImport("winmm.dll")]
        private static extern int midiOutGetNumDevs();

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int midiOutGetDevCaps(int uDeviceID, ref MidiOutCaps lpMidiOutCaps, int cbMidiOutCaps);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MidiOutCaps
        {
            public ushort wMid, wPid;
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public ushort wTechnology, wVoices, wNotes, wChannelMask;
            public uint dwSupport;
        }

        private static bool EndpointPairExists(string baseName)
        {
            string toName = "WM to " + baseName;
            string fromName = "WM from " + baseName;

            if (EndpointExistsViaMidiList(toName, fromName))
                return true;

            int count = midiOutGetNumDevs();

            for (int i = 0; i < count; i++)
            {
                MidiOutCaps caps = new MidiOutCaps();
                if (midiOutGetDevCaps(i, ref caps, Marshal.SizeOf(caps)) == 0)
                {
                    if (caps.szPname == toName || caps.szPname == fromName)
                        return true;
                }
            }
            return false;
        }

        private static bool EndpointExists(string endpointName)
        {
            string sideAName = endpointName + " (A)";
            string sideBName = endpointName + " (B)";

            if (EndpointExistsViaMidiList(endpointName, sideAName, sideBName))
                return true;

            int count = midiOutGetNumDevs();

            for (int i = 0; i < count; i++)
            {
                MidiOutCaps caps = new MidiOutCaps();
                if (midiOutGetDevCaps(i, ref caps, Marshal.SizeOf(caps)) == 0)
                {
                    if (caps.szPname == endpointName || caps.szPname == sideAName || caps.szPname == sideBName)
                        return true;
                }
            }

            return false;
        }

        private async void create_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PortName.Text))
            {
                MessageBox.Show("Please enter a name.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string trimmedName = PortName.Text.Trim();
            string validationMessage;
            if (!ValidateEndpointName(trimmedName, out validationMessage))
            {
                MessageBox.Show(validationMessage, "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (IsMidi1Selected())
            {
                if (EndpointPairExists(trimmedName))
                {
                    DialogResult confirm = MessageBox.Show(
                        "A port named 'WM to " + trimmedName + "' or 'WM from " + trimmedName + "' already exists. Create anyway?",
                        "Duplicate Name",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes)
                        return;
                }

                SetCreationInProgress(true);
                try
                {
                    CommandRunResult result = await Task.Run(() => CreateMidi1Endpoints(trimmedName));
                    if (result.Success)
                    {
                        MessageBox.Show(
                            "Created 'WM to " + trimmedName + "' and 'WM from " + trimmedName + "' successfully.",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        PortName.Clear();
                        PortName.Focus();
                    }
                    else
                    {
                        ShowCreationFailedMessage("endpoints", result.ErrorDetails);
                    }
                }
                finally
                {
                    SetCreationInProgress(false);
                }

                return;
            }

            if (EndpointExists(trimmedName))
            {
                DialogResult confirm = MessageBox.Show(
                    "A port named '" + trimmedName + "' already exists. Create anyway?",
                    "Duplicate Name",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;
            }

            SetCreationInProgress(true);
            try
            {
                CommandRunResult result = await Task.Run(() => CreateMidi2Endpoints(trimmedName));
                if (result.Success)
                {
                    MessageBox.Show(
                        "Created '" + trimmedName + "' successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    PortName.Clear();
                    PortName.Focus();
                }
                else
                {
                    ShowCreationFailedMessage("endpoint", result.ErrorDetails);
                }
            }
            finally
            {
                SetCreationInProgress(false);
            }
        }

        private void ShowCreationFailedMessage(string target, string errorDetails)
        {
            string message =
                "The " + target + " could not be created. Ensure you have administrator rights and the MIDI service is running.\r\n\r\n" +
                "If this continues, install the Windows MIDI Services SDK (includes the MIDI Console midi.exe).\r\n" +
                "You can install it with: winget install Microsoft.WindowsMIDIServicesSDK";

            if (!string.IsNullOrWhiteSpace(errorDetails))
            {
                message += "\r\n\r\nDetails:\r\n" + errorDetails;
            }

            MessageBox.Show(message, "Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            string readmePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "readme.txt");

            if (!File.Exists(readmePath))
            {
                MessageBox.Show(
                    "The help file 'readme.txt' was not found next to the application.\r\n\r\n" +
                    "Please ensure readme.txt is included with the distributed files.",
                    "Help File Missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = readmePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to open readme.txt.\r\n\r\n" + ex.Message,
                    "Open Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            const string loopMidiPortsRegistryPath = @"Software\Tobias Erichsen\loopMIDI\Ports";
            int migratedCount = 0;
            List<string> migratedPortNames = new List<string>();

            try
            {
                using (RegistryKey portsKey = Registry.CurrentUser.OpenSubKey(loopMidiPortsRegistryPath))
                {
                    if (portsKey == null)
                    {
                        MessageBox.Show(
                            "loopMIDI is not installed or no loopMIDI ports were found.",
                            "Migration Unavailable",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    string[] valueNames = portsKey.GetValueNames();
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        string portName = valueNames[i];
                        if (string.IsNullOrWhiteSpace(portName))
                            continue;

                        RegistryValueKind valueKind;
                        try
                        {
                            valueKind = portsKey.GetValueKind(portName);
                        }
                        catch
                        {
                            continue;
                        }

                        if (valueKind != RegistryValueKind.DWord)
                            continue;

                        CommandRunResult result = await Task.Run(() => CreateMidi1EndpointWithExactName(portName));
                        if (!result.Success)
                        {
                            ShowCreationFailedMessage("endpoint", result.ErrorDetails);
                            return;
                        }

                        migratedCount++;
                        migratedPortNames.Add(portName);
                    }
                }

                if (migratedCount == 0)
                {
                    MessageBox.Show(
                        "No loopMIDI Ports were found",
                        "Migration Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                StringBuilder successMessage = new StringBuilder();
                successMessage.AppendLine("Successfully migrated ports from loopMIDI:");
                for (int i = 0; i < migratedPortNames.Count; i++)
                {
                    successMessage.AppendLine(migratedPortNames[i]);
                }

                MessageBox.Show(
                    successMessage.ToString().TrimEnd(),
                    "Migration Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(
                    "Access to loopMIDI registry entries was denied.\r\n\r\n" + ex.Message,
                    "Migration Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "loopMIDI is not installed or its registry entries could not be read.\r\n\r\n" + ex.Message,
                    "Migration Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
