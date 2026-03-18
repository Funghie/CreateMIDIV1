namespace CreateMIDI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.PortName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPortsWillBeCreated = new System.Windows.Forms.Label();
            this.lblToPreview = new System.Windows.Forms.Label();
            this.lblFromPreview = new System.Windows.Forms.Label();
            this.lblToSuffix = new System.Windows.Forms.Label();
            this.lblFromSuffix = new System.Windows.Forms.Label();
            this.lblMidi2Reference = new System.Windows.Forms.Label();
            this.create = new System.Windows.Forms.Button();
            this.quit = new System.Windows.Forms.Button();
            this.lblMidiStatus = new System.Windows.Forms.Label();
            this.lblEndpointVersion = new System.Windows.Forms.Label();
            this.cmbEndpointVersion = new System.Windows.Forms.ComboBox();
            this.btnInfo = new System.Windows.Forms.Button();
            this.btnGetLoopMIDI = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.label1.Location = new System.Drawing.Point(24, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Endpoint Name";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // PortName
            // 
            this.PortName.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.PortName.Location = new System.Drawing.Point(24, 53);
            this.PortName.Name = "PortName";
            this.PortName.Size = new System.Drawing.Size(428, 32);
            this.PortName.TabIndex = 1;
            this.PortName.TextChanged += new System.EventHandler(this.PortName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(243, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 16);
            this.label2.TabIndex = 2;
            this.label2.Visible = false;
            // 
            // lblPortsWillBeCreated
            // 
            this.lblPortsWillBeCreated.AutoSize = true;
            this.lblPortsWillBeCreated.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPortsWillBeCreated.Location = new System.Drawing.Point(24, 161);
            this.lblPortsWillBeCreated.Name = "lblPortsWillBeCreated";
            this.lblPortsWillBeCreated.Size = new System.Drawing.Size(231, 23);
            this.lblPortsWillBeCreated.TabIndex = 8;
            this.lblPortsWillBeCreated.Text = "These ports will be created:";
            // 
            // lblToPreview
            // 
            this.lblToPreview.AutoSize = true;
            this.lblToPreview.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblToPreview.Location = new System.Drawing.Point(44, 188);
            this.lblToPreview.Name = "lblToPreview";
            this.lblToPreview.Size = new System.Drawing.Size(145, 23);
            this.lblToPreview.TabIndex = 3;
            this.lblToPreview.Text = "Waiting for Name";
            // 
            // lblFromPreview
            // 
            this.lblFromPreview.AutoSize = true;
            this.lblFromPreview.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFromPreview.Location = new System.Drawing.Point(44, 214);
            this.lblFromPreview.Name = "lblFromPreview";
            this.lblFromPreview.Size = new System.Drawing.Size(145, 23);
            this.lblFromPreview.TabIndex = 4;
            this.lblFromPreview.Text = "Waiting for Name";
            // 
            // lblToSuffix
            // 
            this.lblToSuffix.AutoSize = true;
            this.lblToSuffix.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblToSuffix.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblToSuffix.Location = new System.Drawing.Point(195, 188);
            this.lblToSuffix.Name = "lblToSuffix";
            this.lblToSuffix.Size = new System.Drawing.Size(31, 23);
            this.lblToSuffix.TabIndex = 13;
            this.lblToSuffix.Text = "(A)";
            this.lblToSuffix.Visible = false;
            // 
            // lblFromSuffix
            // 
            this.lblFromSuffix.AutoSize = true;
            this.lblFromSuffix.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblFromSuffix.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblFromSuffix.Location = new System.Drawing.Point(195, 214);
            this.lblFromSuffix.Name = "lblFromSuffix";
            this.lblFromSuffix.Size = new System.Drawing.Size(30, 23);
            this.lblFromSuffix.TabIndex = 14;
            this.lblFromSuffix.Text = "(B)";
            this.lblFromSuffix.Visible = false;
            // 
            // lblMidi2Reference
            // 
            this.lblMidi2Reference.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Italic);
            this.lblMidi2Reference.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblMidi2Reference.Location = new System.Drawing.Point(370, 188);
            this.lblMidi2Reference.Name = "lblMidi2Reference";
            this.lblMidi2Reference.Size = new System.Drawing.Size(82, 49);
            this.lblMidi2Reference.TabIndex = 12;
            this.lblMidi2Reference.Text = "(A) (B)\r\nare visual\r\nreference";
            this.lblMidi2Reference.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMidi2Reference.Visible = false;
            // 
            // create
            // 
            this.create.Enabled = false;
            this.create.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.create.Location = new System.Drawing.Point(24, 262);
            this.create.Name = "create";
            this.create.Size = new System.Drawing.Size(130, 45);
            this.create.TabIndex = 5;
            this.create.Text = "Create Ports";
            this.create.UseVisualStyleBackColor = true;
            this.create.Click += new System.EventHandler(this.create_Click);
            // 
            // quit
            // 
            this.quit.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.quit.Location = new System.Drawing.Point(322, 262);
            this.quit.Name = "quit";
            this.quit.Size = new System.Drawing.Size(130, 45);
            this.quit.TabIndex = 6;
            this.quit.Text = "Quit";
            this.quit.UseVisualStyleBackColor = true;
            this.quit.Click += new System.EventHandler(this.quit_Click);
            // 
            // lblMidiStatus
            // 
            this.lblMidiStatus.AutoSize = true;
            this.lblMidiStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMidiStatus.Location = new System.Drawing.Point(24, 333);
            this.lblMidiStatus.Name = "lblMidiStatus";
            this.lblMidiStatus.Size = new System.Drawing.Size(236, 20);
            this.lblMidiStatus.TabIndex = 7;
            this.lblMidiStatus.Text = "Checking Windows MIDI Services...";
            // 
            // lblEndpointVersion
            // 
            this.lblEndpointVersion.AutoSize = true;
            this.lblEndpointVersion.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblEndpointVersion.Location = new System.Drawing.Point(24, 96);
            this.lblEndpointVersion.Name = "lblEndpointVersion";
            this.lblEndpointVersion.Size = new System.Drawing.Size(157, 20);
            this.lblEndpointVersion.TabIndex = 9;
            this.lblEndpointVersion.Text = "MIDI Endpoint Version";
            // 
            // cmbEndpointVersion
            // 
            this.cmbEndpointVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEndpointVersion.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbEndpointVersion.FormattingEnabled = true;
            this.cmbEndpointVersion.Items.AddRange(new object[] {
            "MIDI 1.0",
            "MIDI 2.0"});
            this.cmbEndpointVersion.Location = new System.Drawing.Point(24, 118);
            this.cmbEndpointVersion.Name = "cmbEndpointVersion";
            this.cmbEndpointVersion.Size = new System.Drawing.Size(428, 31);
            this.cmbEndpointVersion.TabIndex = 10;
            this.cmbEndpointVersion.SelectedIndexChanged += new System.EventHandler(this.cmbEndpointVersion_SelectedIndexChanged);
            // 
            // btnInfo
            // 
            this.btnInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnInfo.Location = new System.Drawing.Point(428, 24);
            this.btnInfo.Name = "btnInfo";
            this.btnInfo.Size = new System.Drawing.Size(24, 24);
            this.btnInfo.TabIndex = 11;
            this.btnInfo.Text = "i";
            this.btnInfo.UseVisualStyleBackColor = true;
            this.btnInfo.Click += new System.EventHandler(this.btnInfo_Click);
            // 
            // btnGetLoopMIDI
            // 
            this.btnGetLoopMIDI.BackColor = System.Drawing.Color.LightBlue;
            this.btnGetLoopMIDI.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnGetLoopMIDI.Location = new System.Drawing.Point(160, 262);
            this.btnGetLoopMIDI.Name = "btnGetLoopMIDI";
            this.btnGetLoopMIDI.Size = new System.Drawing.Size(156, 45);
            this.btnGetLoopMIDI.TabIndex = 15;
            this.btnGetLoopMIDI.Text = "Import loopMIDI";
            this.btnGetLoopMIDI.UseVisualStyleBackColor = false;
            this.btnGetLoopMIDI.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 374);
            this.Controls.Add(this.btnGetLoopMIDI);
            this.Controls.Add(this.btnInfo);
            this.Controls.Add(this.cmbEndpointVersion);
            this.Controls.Add(this.lblEndpointVersion);
            this.Controls.Add(this.lblPortsWillBeCreated);
            this.Controls.Add(this.lblMidiStatus);
            this.Controls.Add(this.quit);
            this.Controls.Add(this.create);
            this.Controls.Add(this.lblMidi2Reference);
            this.Controls.Add(this.lblFromSuffix);
            this.Controls.Add(this.lblToSuffix);
            this.Controls.Add(this.lblFromPreview);
            this.Controls.Add(this.lblToPreview);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PortName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Windows MIDI Port Creator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PortName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblToPreview;
        private System.Windows.Forms.Label lblFromPreview;
        private System.Windows.Forms.Label lblToSuffix;
        private System.Windows.Forms.Label lblFromSuffix;
        private System.Windows.Forms.Label lblMidi2Reference;
        private System.Windows.Forms.Button create;
        private System.Windows.Forms.Button quit;
        private System.Windows.Forms.Label lblMidiStatus;
        private System.Windows.Forms.Label lblPortsWillBeCreated;
        private System.Windows.Forms.Label lblEndpointVersion;
        private System.Windows.Forms.ComboBox cmbEndpointVersion;
        private System.Windows.Forms.Button btnInfo;
        private System.Windows.Forms.Button btnGetLoopMIDI;
    }
}

