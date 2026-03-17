using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CreateMIDI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

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

        private bool IsMidi1Selected()
        {
            return cmbEndpointVersion.SelectedItem != null && cmbEndpointVersion.SelectedItem.ToString() == "MIDI 1.0";
        }

        private void UpdatePreviewAndCreateButton()
        {
            if (!string.IsNullOrWhiteSpace(PortName.Text))
            {
                string name = PortName.Text.Trim();
                if (IsMidi1Selected())
                {
                    lblToPreview.Text = "WM to " + name;
                    lblFromPreview.Text = "WM from " + name;
                }
                else
                {
                    lblToPreview.Text = name + " (A)";
                    lblFromPreview.Text = name + " (B)";
                }

                create.Enabled = true;
                create.BackColor = Color.LimeGreen;
            }
            else
            {
                lblToPreview.Text = "Waiting for Name";
                lblFromPreview.Text = "Waiting for Name";
                create.Enabled = false;
                create.BackColor = SystemColors.Control;
            }
        }

        private void PortName_TextChanged(object sender, EventArgs e)
        {
            UpdatePreviewAndCreateButton();
        }

        private void cmbEndpointVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreviewAndCreateButton();
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

        private static bool RunMidiCommands(string[] args)
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
                            UseShellExecute = false
                        };

                        process.Start();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                            return false;
                    }
                }
                catch (Win32Exception)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CreateMidi1Endpoints(string baseName)
        {
            string[] args =
            {
                $"midi1-loopback create --name \"WM to {baseName}\"",
                $"midi1-loopback create --name \"WM from {baseName}\""
            };

            return RunMidiCommands(args);
        }

        private bool CreateMidi2Endpoints(string baseName)
        {
            string[] args =
            {
                $"loopback create --root-name \"{baseName}\""
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
            int count = midiOutGetNumDevs();
            string toName = "WM to " + baseName;
            string fromName = "WM from " + baseName;

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
            int count = midiOutGetNumDevs();
            string sideAName = endpointName + " (A)";
            string sideBName = endpointName + " (B)";

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

        private void create_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PortName.Text))
            {
                MessageBox.Show("Please enter a name.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string trimmedName = PortName.Text.Trim();

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

                if (CreateMidi1Endpoints(trimmedName))
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
                    MessageBox.Show(
                        "The endpoints could not be created. Ensure you have administrator rights and the MIDI service is running.\r\n\r\n" +
                        "If this continues, install the Windows MIDI Services SDK (includes the MIDI Console `midi.exe`).\r\n" +
                        "You can install it with: winget install Microsoft.WindowsMIDIServicesSDK",
                        "Creation Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
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

            if (CreateMidi2Endpoints(trimmedName))
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
                MessageBox.Show(
                    "The endpoint could not be created. Ensure you have administrator rights and the MIDI service is running.\r\n\r\n" +
                    "If this continues, install the Windows MIDI Services SDK (includes the MIDI Console `midi.exe`).\r\n" +
                    "You can install it with: winget install Microsoft.WindowsMIDIServicesSDK",
                    "Creation Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            string infoText =
                "Windows MIDI Port Creator by Phil Pendlebury\r\n" +
                "www.everythingcreative.biz\r\n\r\n" +
                "Creating MIDI 1.0 endpoints:\r\n" +
                "Enter the desired name.\r\n" +
                "Two ports will be created: 'WM to' and 'WM from'.\r\n\r\n" +
                "Creating MIDI 2.0 endpoints:\r\n" +
                "These are bi-directional, so only one port will be created.\r\n" +
                "The preview shows '(A)' and '(B)' for clarity, as this is how they appear in some environments.\r\n\r\n" +
                "Create:\r\n" +
                "Creates endpoint(s) using the entered name and selected MIDI version.\r\n" +
                "After you have successfully created your port(s), you can create another using the same process.\r\n\r\n" +
                "Quit:\r\n" +
                "Closes the application.";

            MessageBox.Show(infoText, "About Windows MIDI Port Creator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
