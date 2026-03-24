// Copyright (c) 2026 Phil Pendlebury
// Everything Creative
// Licensed under MIT

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CreateMIDI
{
    public partial class Form1 : Form
    {
        private const int MaxEndpointNameLength = 64;
        private const string CreatedPortsFileName = "Created MIDI Ports.txt";
        private const char CreatedPortEntrySeparator = '|';
        private const string InfoButtonIconResourceName = "InfoButtonIcon";
        private const string PortsButtonIconResourceName = "PortsButtonIcon";
        private const string InfoButtonIconFileName = "icon_info.png";
        private const string PortsButtonIconFileName = "icon_ports.png";
        private const int SmallActionButtonLogicalSize = 28;
        private const int SmallActionImagePaddingLogicalSize = 6;
        private const string StartupRecreateTaskName = "CreateMIDI Recreate Ports";
        private const string StartupArgument = "-startup";
        private bool _isCreating;
        private bool _isUpdatingStartupCheckbox;
        private Panel _mainContentPanel;
        private ToolTip _actionButtonToolTip;

        // Initialize the form, apply the app icon, and show MIDI service status.
        public Form1()
        {
            InitializeComponent();
            checkBox1.Font = btnGetLoopMIDI.Font;
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeResponsiveLayout();
            ApplySmallActionButtonIcons();
            InitializeActionButtonToolTips();
            InitializeStartupTaskCheckbox();
            ApplyExecutableIcon();
            lblVersion.Text = GetDisplayVersionText();

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

            PerformResponsiveLayout();
        }

        private void InitializeResponsiveLayout()
        {
            _mainContentPanel = new Panel();
            _mainContentPanel.Dock = DockStyle.Fill;
            _mainContentPanel.Margin = Padding.Empty;
            _mainContentPanel.Padding = Padding.Empty;

            SuspendLayout();
            Controls.Add(_mainContentPanel);

            Control[] responsiveControls =
            {
                label1,
                PortName,
                label2,
                lblPortsWillBeCreated,
                lblToPreview,
                lblFromPreview,
                lblToSuffix,
                lblFromSuffix,
                create,
                quit,
                lblMidiStatus,
                lblEndpointVersion,
                cmbEndpointVersion,
                btnInfo,
                btnPorts,
                btnGetLoopMIDI,
                lblVersion,
                checkBox1
            };

            for (int i = 0; i < responsiveControls.Length; i++)
            {
                responsiveControls[i].Anchor = AnchorStyles.Top | AnchorStyles.Left;
                _mainContentPanel.Controls.Add(responsiveControls[i]);
            }

            Resize += ResponsiveLayoutHost_Resize;
            _mainContentPanel.Resize += ResponsiveLayoutHost_Resize;
            ResumeLayout(true);
        }

        private void ResponsiveLayoutHost_Resize(object sender, EventArgs e)
        {
            PerformResponsiveLayout();
        }

        private int ScaleLogicalPixels(int logicalPixels)
        {
            return (int)Math.Round(logicalPixels * (DeviceDpi / 96f));
        }

        private Size ScaleLogicalSize(int width, int height)
        {
            return new Size(ScaleLogicalPixels(width), ScaleLogicalPixels(height));
        }

        private void PerformResponsiveLayout()
        {
            if (_mainContentPanel == null)
            {
                return;
            }

            int clientWidth = _mainContentPanel.ClientSize.Width;
            int clientHeight = _mainContentPanel.ClientSize.Height;
            if (clientWidth <= 0 || clientHeight <= 0)
            {
                return;
            }

            int margin = ScaleLogicalPixels(24);
            int smallGap = ScaleLogicalPixels(4);
            int sectionGap = ScaleLogicalPixels(11);
            int rowGap = ScaleLogicalPixels(6);
            int controlGap = ScaleLogicalPixels(4);
            int footerGap = ScaleLogicalPixels(12);
            int contentWidth = Math.Max(ScaleLogicalPixels(180), clientWidth - (margin * 2));

            _mainContentPanel.SuspendLayout();
            try
            {
                Size smallButtonSize = ScaleLogicalSize(SmallActionButtonLogicalSize, SmallActionButtonLogicalSize);
                btnInfo.Size = smallButtonSize;
                btnPorts.Size = smallButtonSize;

                label1.Location = new Point(margin, margin);

                btnInfo.Location = new Point(clientWidth - margin - btnInfo.Width, margin);
                btnPorts.Location = new Point(btnInfo.Left - smallGap - btnPorts.Width, margin);

                PortName.Location = new Point(margin, label1.Bottom + controlGap);
                PortName.Width = contentWidth;

                lblEndpointVersion.Location = new Point(margin, PortName.Bottom + sectionGap);
                cmbEndpointVersion.Location = new Point(margin, lblEndpointVersion.Bottom + controlGap);
                cmbEndpointVersion.Width = contentWidth;

                lblPortsWillBeCreated.MaximumSize = new Size(contentWidth, 0);
                lblPortsWillBeCreated.Location = new Point(margin, cmbEndpointVersion.Bottom + sectionGap);

                int previewIndent = ScaleLogicalPixels(20);
                int previewWidth = Math.Max(ScaleLogicalPixels(140), contentWidth - previewIndent);
                lblToPreview.MaximumSize = new Size(previewWidth, 0);
                lblToPreview.Location = new Point(margin + previewIndent, lblPortsWillBeCreated.Bottom + rowGap);

                lblFromPreview.MaximumSize = new Size(previewWidth, 0);
                lblFromPreview.Location = new Point(margin + previewIndent, lblToPreview.Bottom + ScaleLogicalPixels(3));

                PositionPreviewSuffix();

                int versionWidth = ScaleLogicalPixels(120);
                int versionHeight = Math.Max(lblVersion.PreferredHeight, ScaleLogicalPixels(19));
                lblVersion.Size = new Size(versionWidth, versionHeight);

                int statusWidth = Math.Max(ScaleLogicalPixels(140), clientWidth - (margin * 2) - versionWidth - rowGap);
                lblMidiStatus.MaximumSize = new Size(statusWidth, 0);

                int footerHeight = Math.Max(lblMidiStatus.PreferredHeight, lblVersion.Height);
                int footerY = clientHeight - margin - footerHeight;

                lblMidiStatus.Location = new Point(margin, footerY + ((footerHeight - lblMidiStatus.PreferredHeight) / 2));
                lblVersion.Location = new Point(clientWidth - margin - lblVersion.Width, footerY + ((footerHeight - lblVersion.Height) / 2));

                checkBox1.MaximumSize = new Size(contentWidth, 0);
                checkBox1.Location = new Point(margin, footerY - footerGap - checkBox1.PreferredSize.Height);

                int actionAreaBottom = checkBox1.Top - rowGap;
                LayoutActionButtons(margin, actionAreaBottom, contentWidth);
            }
            finally
            {
                _mainContentPanel.ResumeLayout();
            }
        }

        private void LayoutActionButtons(int left, int bottom, int availableWidth)
        {
            Button[] buttons = { create, btnGetLoopMIDI, quit };
            int[] buttonWidths =
            {
                ScaleLogicalPixels(130),
                ScaleLogicalPixels(156),
                ScaleLogicalPixels(130)
            };

            int buttonHeight = ScaleLogicalPixels(45);
            int gap = ScaleLogicalPixels(6);
            List<List<int>> rows = new List<List<int>>();
            List<int> currentRow = new List<int>();
            int currentRowWidth = 0;

            for (int i = 0; i < buttons.Length; i++)
            {
                int requiredWidth = buttonWidths[i] + (currentRow.Count > 0 ? gap : 0);
                if (currentRow.Count > 0 && currentRowWidth + requiredWidth > availableWidth)
                {
                    rows.Add(currentRow);
                    currentRow = new List<int>();
                    currentRowWidth = 0;
                    requiredWidth = buttonWidths[i];
                }

                currentRow.Add(i);
                currentRowWidth += requiredWidth;
            }

            if (currentRow.Count > 0)
            {
                rows.Add(currentRow);
            }

            int totalHeight = (rows.Count * buttonHeight) + ((rows.Count - 1) * gap);
            int y = bottom - totalHeight;

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                List<int> row = rows[rowIndex];
                int x = left;

                for (int itemIndex = 0; itemIndex < row.Count; itemIndex++)
                {
                    int buttonIndex = row[itemIndex];
                    buttons[buttonIndex].SetBounds(x, y, buttonWidths[buttonIndex], buttonHeight);
                    x += buttonWidths[buttonIndex] + gap;
                }

                y += buttonHeight + gap;
            }
        }

        private void PositionPreviewSuffix()
        {
            if (!lblToSuffix.Visible)
            {
                return;
            }

            int suffixWidth = TextRenderer.MeasureText(lblToSuffix.Text, lblToSuffix.Font).Width;
            int rightAlignedStart = PortName.Right - suffixWidth;
            lblToSuffix.Location = new Point(rightAlignedStart, lblToPreview.Top);
        }

        // Return a short display version from the assembly file version.
        private static string GetDisplayVersionText()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);

            if (attributes.Length > 0)
            {
                AssemblyFileVersionAttribute fileVersionAttribute = attributes[0] as AssemblyFileVersionAttribute;
                Version version;
                if (fileVersionAttribute != null && Version.TryParse(fileVersionAttribute.Version, out version))
                {
                    return string.Format("v{0}.{1}.{2}", version.Major, version.Minor, version.Build);
                }
            }

            Version assemblyVersion = assembly.GetName().Version;
            if (assemblyVersion != null)
            {
                return string.Format("v{0}.{1}.{2}", assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build);
            }

            return "v1.0.0";
        }

        // Check whether the Windows MIDI service is currently available.
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

        private static async Task<bool> WaitForMidiServiceAsync(TimeSpan timeout)
        {
            DateTime deadline = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < deadline)
            {
                if (IsMidiServiceRunning())
                {
                    return true;
                }

                await Task.Delay(2000).ConfigureAwait(false);
            }

            return IsMidiServiceRunning();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        // Use the executable's icon for the main window when possible.
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

        // Configure compact icon-only buttons for info and created ports.
        private void ApplySmallActionButtonIcons()
        {
            try
            {
                Size buttonSize = ScaleLogicalSize(SmallActionButtonLogicalSize, SmallActionButtonLogicalSize);
                int imagePadding = ScaleLogicalPixels(SmallActionImagePaddingLogicalSize);
                Size imageSize = new Size(
                    Math.Max(1, buttonSize.Width - imagePadding),
                    Math.Max(1, buttonSize.Height - imagePadding));

                btnInfo.Size = buttonSize;
                btnInfo.Padding = Padding.Empty;
                Image infoImage = GetButtonImage(InfoButtonIconResourceName, InfoButtonIconFileName, imageSize);
                if (infoImage != null)
                {
                    btnInfo.Image = infoImage;
                }
                btnInfo.ImageAlign = ContentAlignment.MiddleCenter;
                btnInfo.AccessibleName = "Info";
                btnInfo.Text = btnInfo.Image == null ? "i" : string.Empty;

                btnPorts.Size = buttonSize;
                btnPorts.Padding = Padding.Empty;
                Image portsImage = GetButtonImage(PortsButtonIconResourceName, PortsButtonIconFileName, imageSize);
                if (portsImage != null)
                {
                    btnPorts.Image = portsImage;
                }
                btnPorts.ImageAlign = ContentAlignment.MiddleCenter;
                btnPorts.AccessibleName = "Created Ports";
                btnPorts.Text = btnPorts.Image == null ? "P" : string.Empty;
            }
            catch
            {
                btnInfo.Text = "i";
                btnPorts.Text = "P";
            }
        }

        private void InitializeActionButtonToolTips()
        {
            _actionButtonToolTip = components != null ? new ToolTip(components) : new ToolTip();
            _actionButtonToolTip.ShowAlways = true;
            _actionButtonToolTip.SetToolTip(btnInfo, "Open readme file");
            _actionButtonToolTip.SetToolTip(btnPorts, "View list of created ports");
        }

        private void InitializeStartupTaskCheckbox()
        {
            _isUpdatingStartupCheckbox = true;
            try
            {
                checkBox1.Checked = StartupRecreateTaskExists();
            }
            finally
            {
                _isUpdatingStartupCheckbox = false;
            }
        }

        private static bool StartupRecreateTaskExists()
        {
            int exitCode;
            string stdOut;
            string stdErr;
            if (!TryRunProcess("schtasks.exe", "/Query /TN \"" + StartupRecreateTaskName + "\"", out exitCode, out stdOut, out stdErr))
            {
                return false;
            }

            return exitCode == 0;
        }

        private static bool TryCreateStartupRecreateTask(out string errorDetails)
        {
            string runAsUser = GetCurrentUserForTaskScheduler();
            string xmlPath = Path.Combine(Path.GetTempPath(), "CreateMIDI.StartupTask." + Guid.NewGuid().ToString("N") + ".xml");

            try
            {
                string taskXml = BuildStartupTaskXml(runAsUser, Application.ExecutablePath, StartupArgument);
                File.WriteAllText(xmlPath, taskXml, Encoding.Unicode);

                string arguments = "/Create /TN \"" + StartupRecreateTaskName + "\" /XML \"" + xmlPath + "\" /F";

                int exitCode;
                string stdOut;
                string stdErr;
                if (!TryRunProcess("schtasks.exe", arguments, out exitCode, out stdOut, out stdErr))
                {
                    errorDetails = "Unable to start Task Scheduler command.";
                    return false;
                }

                if (exitCode == 0)
                {
                    errorDetails = string.Empty;
                    return true;
                }

                errorDetails = BuildCommandErrorDetails("schtasks.exe " + arguments, exitCode, stdOut, stdErr);
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(xmlPath))
                    {
                        File.Delete(xmlPath);
                    }
                }
                catch
                {
                    // Ignore temp file cleanup failures.
                }
            }
        }

        private static string GetCurrentUserForTaskScheduler()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity != null && !string.IsNullOrWhiteSpace(identity.Name))
            {
                return identity.Name;
            }

            string domain = Environment.UserDomainName;
            string user = Environment.UserName;
            if (!string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(user))
            {
                return domain + "\\" + user;
            }

            return user;
        }

        private static string BuildStartupTaskXml(string runAsUser, string executablePath, string startupArgument)
        {
            string escapedUser = System.Security.SecurityElement.Escape(runAsUser) ?? string.Empty;
            string escapedExe = System.Security.SecurityElement.Escape(executablePath) ?? string.Empty;
            string escapedArg = System.Security.SecurityElement.Escape(startupArgument) ?? string.Empty;

            return
                "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" +
                "<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">" +
                "<RegistrationInfo>" +
                "<Author>" + escapedUser + "</Author>" +
                "<Description>Recreate tracked MIDI ports at user logon.</Description>" +
                "</RegistrationInfo>" +
                "<Triggers>" +
                "<LogonTrigger>" +
                "<Enabled>true</Enabled>" +
                "<UserId>" + escapedUser + "</UserId>" +
                "</LogonTrigger>" +
                "</Triggers>" +
                "<Principals>" +
                "<Principal id=\"Author\">" +
                "<UserId>" + escapedUser + "</UserId>" +
                "<LogonType>InteractiveToken</LogonType>" +
                "<RunLevel>HighestAvailable</RunLevel>" +
                "</Principal>" +
                "</Principals>" +
                "<Settings>" +
                "<MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>" +
                "<DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>" +
                "<StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>" +
                "<AllowHardTerminate>true</AllowHardTerminate>" +
                "<StartWhenAvailable>true</StartWhenAvailable>" +
                "<RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>" +
                "<IdleSettings><StopOnIdleEnd>false</StopOnIdleEnd><RestartOnIdle>false</RestartOnIdle></IdleSettings>" +
                "<AllowStartOnDemand>true</AllowStartOnDemand>" +
                "<Enabled>true</Enabled>" +
                "<Hidden>false</Hidden>" +
                "<RunOnlyIfIdle>false</RunOnlyIfIdle>" +
                "<WakeToRun>false</WakeToRun>" +
                "<ExecutionTimeLimit>PT10M</ExecutionTimeLimit>" +
                "<Priority>7</Priority>" +
                "</Settings>" +
                "<Actions Context=\"Author\">" +
                "<Exec><Command>" + escapedExe + "</Command><Arguments>" + escapedArg + "</Arguments></Exec>" +
                "</Actions>" +
                "</Task>";
        }

        private static bool TryDeleteStartupRecreateTask(out string errorDetails)
        {
            if (!StartupRecreateTaskExists())
            {
                errorDetails = string.Empty;
                return true;
            }

            int exitCode;
            string stdOut;
            string stdErr;
            string arguments = "/Delete /TN \"" + StartupRecreateTaskName + "\" /F";

            if (!TryRunProcess("schtasks.exe", arguments, out exitCode, out stdOut, out stdErr))
            {
                errorDetails = "Unable to start Task Scheduler command.";
                return false;
            }

            if (exitCode == 0)
            {
                errorDetails = string.Empty;
                return true;
            }

            errorDetails = BuildCommandErrorDetails("schtasks.exe " + arguments, exitCode, stdOut, stdErr);
            return false;
        }

        private static bool TryRunProcess(string fileName, string arguments, out int exitCode, out string stdOut, out string stdErr)
        {
            exitCode = -1;
            stdOut = string.Empty;
            stdErr = string.Empty;

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    process.Start();
                    stdOut = process.StandardOutput.ReadToEnd();
                    stdErr = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    exitCode = process.ExitCode;
                    return true;
                }
            }
            catch (Exception ex)
            {
                stdErr = ex.Message;
                return false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingStartupCheckbox)
            {
                return;
            }

            bool targetState = checkBox1.Checked;
            string errorDetails;

            bool success = targetState
                ? TryCreateStartupRecreateTask(out errorDetails)
                : TryDeleteStartupRecreateTask(out errorDetails);

            if (success)
            {
                return;
            }

            _isUpdatingStartupCheckbox = true;
            try
            {
                checkBox1.Checked = !targetState;
            }
            finally
            {
                _isUpdatingStartupCheckbox = false;
            }

            MessageBox.Show(
                "Unable to update startup restore task.\r\n\r\n" + errorDetails,
                "Task Scheduler Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static Image GetButtonImage(string resourceName, string fileName, Size targetSize)
        {
            Image resourceImage = GetButtonImageResource(resourceName);
            if (resourceImage != null)
            {
                return ResizeImageToFit(resourceImage, targetSize);
            }

            Image fileImage = GetButtonImageFile(fileName);
            if (fileImage != null)
            {
                return ResizeImageToFit(fileImage, targetSize);
            }

            return null;
        }

        private static Bitmap ResizeImageToFit(Image image, Size targetSize)
        {
            using (Bitmap source = TrimTransparentBounds(image))
            {
                float ratio = Math.Min((float)targetSize.Width / source.Width, (float)targetSize.Height / source.Height);
                int width = Math.Max(1, (int)Math.Round(source.Width * ratio));
                int height = Math.Max(1, (int)Math.Round(source.Height * ratio));
                int x = (targetSize.Width - width) / 2;
                int y = (targetSize.Height - height) / 2;

                Bitmap resized = new Bitmap(targetSize.Width, targetSize.Height);
                using (Graphics graphics = Graphics.FromImage(resized))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(source, new Rectangle(x, y, width, height));
                }

                return resized;
            }
        }

        private static Bitmap TrimTransparentBounds(Image image)
        {
            Bitmap bitmap = image as Bitmap ?? new Bitmap(image);
            bool createdBitmap = !(image is Bitmap);

            int left = bitmap.Width;
            int top = bitmap.Height;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).A == 0)
                    {
                        continue;
                    }

                    if (x < left) left = x;
                    if (y < top) top = y;
                    if (x > right) right = x;
                    if (y > bottom) bottom = y;
                }
            }

            if (right < left || bottom < top)
            {
                Bitmap transparent = new Bitmap(bitmap.Width, bitmap.Height);
                if (createdBitmap)
                {
                    bitmap.Dispose();
                }

                return transparent;
            }

            Rectangle bounds = Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
            Bitmap trimmed = bitmap.Clone(bounds, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            if (createdBitmap)
            {
                bitmap.Dispose();
            }

            return trimmed;
        }

        private static Image GetButtonImageFile(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (Image image = Image.FromFile(filePath))
            {
                return new Bitmap(image);
            }
        }

        private static Image GetButtonImageResource(string resourceName)
        {
            object resource = Properties.Resources.ResourceManager.GetObject(resourceName);

            if (resource is Bitmap bitmap)
            {
                return bitmap;
            }

            if (resource is Icon icon)
            {
                return icon.ToBitmap();
            }

            return resource as Image;
        }

        // UI helpers for endpoint mode selection and preview text.
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
                    lblFromPreview.Text = "(A) (B) are labels shown by some DAW hosts";
                    lblToPreview.ForeColor = SystemColors.ControlText;
                    lblFromPreview.ForeColor = Color.DarkGreen;

                    lblToSuffix.Text = "(A) (B)";
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

            PerformResponsiveLayout();
        }

        private void SetCreationInProgress(bool isCreating)
        {
            _isCreating = isCreating;
            UseWaitCursor = isCreating;
            UpdatePreviewAndCreateButton();
        }

        // Refresh the preview when the entered name or endpoint version changes.
        private void PortName_TextChanged(object sender, EventArgs e)
        {
            UpdatePreviewAndCreateButton();
        }

        private void cmbEndpointVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreviewAndCreateButton();
        }

        // Validate endpoint names before calling the MIDI tools.
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

        private static string GetCreatedPortsFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CreatedPortsFileName);
        }

        private static string BuildCreatedPortEntry(string portName, int midiType)
        {
            return portName.Trim() + CreatedPortEntrySeparator + midiType.ToString();
        }

        private static bool IsCreatedPortCommentLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string trimmed = line.TrimStart();
            return trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("#", StringComparison.Ordinal);
        }

        private static void RememberCreatedPort(string portName, int midiType)
        {
            try
            {
                string entry = BuildCreatedPortEntry(portName, midiType);
                string filePath = GetCreatedPortsFilePath();

                if (File.Exists(filePath))
                {
                    string[] existingLines = File.ReadAllLines(filePath);
                    for (int i = 0; i < existingLines.Length; i++)
                    {
                        if (IsCreatedPortCommentLine(existingLines[i]))
                            continue;

                        string existingLine = existingLines[i].Trim();
                        if (existingLine.Length == 0)
                            continue;

                        if (string.Equals(existingLine, entry, StringComparison.OrdinalIgnoreCase))
                            return;
                    }
                }

                using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    writer.WriteLine(entry);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to store created port entry: " + ex.Message);
            }
        }

        // Locate midi.exe from PATH first, then try common install folders.
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

        // Hold the result of running midi.exe commands.
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

        // Build readable command error text for failed MIDI tool calls.
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

        // Query midi.exe for information about existing endpoints.
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

        // Run one or more midi.exe commands to create endpoints.
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

        // Create MIDI 1.0 or MIDI 2.0 endpoints using the selected naming rules.
        private static CommandRunResult CreateMidi1Endpoints(string baseName)
        {
            string[] args =
            {
                $"midi1-loopback create --name \"WM to {baseName}\"",
                $"midi1-loopback create --name \"WM from {baseName}\""
            };

            return RunMidiCommands(args);
        }

        private static CommandRunResult CreateMidi2Endpoints(string baseName)
        {
            string[] args =
            {
                $"loopback create --root-name \"{baseName}\""
            };

            return RunMidiCommands(args);
        }

        private static CommandRunResult CreateMidi1EndpointWithExactName(string endpointName)
        {
            string escapedName = endpointName.Replace("\"", "\\\"");
            string[] args =
            {
                "midi1-loopback create --name \"" + escapedName + "\""
            };

            return RunMidiCommands(args);
        }

        // Native WinMM calls used as a fallback when checking existing MIDI outputs.
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

        // Check for duplicate endpoint names before creating new ones.
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

        // Handle manual endpoint creation from the main Create button.
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
                        RememberCreatedPort("WM to " + trimmedName, 1);
                        RememberCreatedPort("WM from " + trimmedName, 1);

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
                    RememberCreatedPort(trimmedName, 2);

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

        // Show a friendly failure message when endpoint creation does not succeed.
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

        // Open the created ports tracking file for quick review.
        private void btnPorts_Click(object sender, EventArgs e)
        {
            string createdPortsPath = GetCreatedPortsFilePath();

            if (!File.Exists(createdPortsPath))
            {
                MessageBox.Show(
                    "The file '" + CreatedPortsFileName + "' has not been created yet.",
                    "Ports File Missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createdPortsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to open '" + CreatedPortsFileName + "'.\r\n\r\n" + ex.Message,
                    "Open Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // Open the bundled readme file for quick help.
        private void btnInfo_Click(object sender, EventArgs e)
        {
            string readmePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MIDI Port Creator README.txt");

            if (!File.Exists(readmePath))
            {
                MessageBox.Show(
                    "The help file 'MIDI Port Creator README.txt' was not found next to the application.\r\n\r\n" +
                    "Please ensure MIDI Port Creator README.txt is included with the distributed files.",
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
                    "Unable to open MIDI Port Creator README.txt.\r\n\r\n" + ex.Message,
                    "Open Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // Close the application.
        private void quit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Migrate existing loopMIDI port names into Windows MIDI endpoints.
        private async void button1_Click(object sender, EventArgs e)
        {
            const string loopMidiPortsRegistryPath = @"Software\Tobias Erichsen\loopMIDI\Ports";
            int migratedCount = 0;
            int skippedExistingCount = 0;
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

                        if (EndpointExists(portName))
                        {
                            skippedExistingCount++;
                            continue;
                        }

                        CommandRunResult result = await Task.Run(() => CreateMidi1EndpointWithExactName(portName));
                        if (!result.Success)
                        {
                            ShowCreationFailedMessage("endpoint", result.ErrorDetails);
                            return;
                        }

                        RememberCreatedPort(portName, 1);
                        migratedCount++;
                        migratedPortNames.Add(portName);
                    }
                }

                if (migratedCount == 0)
                {
                    string message = skippedExistingCount > 0
                        ? "All loopMIDI ports already exist. No new ports were imported."
                        : "No loopMIDI Ports were found";

                    MessageBox.Show(
                        message,
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

                if (skippedExistingCount > 0)
                {
                    successMessage.AppendLine();
                    successMessage.AppendLine(skippedExistingCount + " existing port(s) were skipped.");
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

        // Recreate MIDI ports from the tracking file (used with -startup flag for automatic restoration).
        public static void RecreatePortsFromCsv()
        {
            RecreatePortsFromCsvAsync().GetAwaiter().GetResult();
        }

        private static async Task RecreatePortsFromCsvAsync()
        {
            string createdPortsPath = GetCreatedPortsFilePath();

            if (!File.Exists(createdPortsPath))
            {
                Debug.WriteLine("Created ports file not found: " + createdPortsPath);
                return;
            }

            if (!await WaitForMidiServiceAsync(TimeSpan.FromMinutes(2)).ConfigureAwait(false))
            {
                Debug.WriteLine("MIDI service did not reach running state before timeout. Startup restore skipped.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(createdPortsPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (IsCreatedPortCommentLine(lines[i]))
                        continue;

                    string line = lines[i].Trim();
                    if (line.Length == 0)
                        continue;

                    string[] parts = line.Split(new[] { CreatedPortEntrySeparator }, StringSplitOptions.None);
                    if (parts.Length != 2)
                    {
                        Debug.WriteLine("Invalid port entry format: " + line);
                        continue;
                    }

                    string portName = parts[0].Trim();
                    string midiTypeStr = parts[1].Trim();

                    int midiType;
                    if (!int.TryParse(midiTypeStr, out midiType))
                    {
                        Debug.WriteLine("Invalid MIDI type in entry: " + line);
                        continue;
                    }

                    CommandRunResult result;

                    if (midiType == 1)
                    {
                        // MIDI 1.0 ports imported from loopMIDI are stored as exact names (no WM to/from prefix)
                        // Check if port already exists before attempting to create
                        if (EndpointExists(portName))
                        {
                            Debug.WriteLine("Port already exists, skipping: " + portName);
                            continue;
                        }

                        result = await Task.Run(() => CreateMidi1EndpointWithExactName(portName)).ConfigureAwait(false);
                    }
                    else if (midiType == 2)
                    {
                        // MIDI 2.0 ports use the base name without suffixes
                        // Check if port already exists before attempting to create
                        if (EndpointExists(portName))
                        {
                            Debug.WriteLine("Port already exists, skipping: " + portName);
                            continue;
                        }

                        result = await Task.Run(() => CreateMidi2Endpoints(portName)).ConfigureAwait(false);
                    }
                    else
                    {
                        Debug.WriteLine("Unknown MIDI type: " + midiType);
                        continue;
                    }

                    if (!result.Success)
                    {
                        Debug.WriteLine("Failed to recreate port: " + portName + " - " + result.ErrorDetails);
                    }
                    else
                    {
                        Debug.WriteLine("Successfully recreated port: " + portName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error recreating ports from file: " + ex.Message);
            }
        }
    }
}
