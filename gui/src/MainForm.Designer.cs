namespace ov7675
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            ov7675PictureBox = new PictureBox();
            comPortLabel = new Label();
            comPortComboBox = new ComboBox();
            connectButton = new Button();
            disconnectButton = new Button();
            statusTextBox = new TextBox();
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            savePictureToolStripMenuItem = new ToolStripMenuItem();
            storeForCToolStripMenuItem = new ToolStripMenuItem();
            debugPictureBox = new PictureBox();
            psocEdgeNNOutputTextBox = new TextBox();
            mainSplitContainer = new SplitContainer();
            pictureSplitContainer = new SplitContainer();
            ov7675IconPictureBox = new PictureBox();
            pse84PictureBox = new PictureBox();
            rightPartSplitContainer = new SplitContainer();
            psocEdgeHardwarePictureBox = new PictureBox();
            psocEdgePredictionLabel = new Label();
            predictionSignalsView = new ov7675.Views.RawRadarSignalsView();
            ((System.ComponentModel.ISupportInitialize)ov7675PictureBox).BeginInit();
            mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)debugPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureSplitContainer).BeginInit();
            pictureSplitContainer.Panel1.SuspendLayout();
            pictureSplitContainer.Panel2.SuspendLayout();
            pictureSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ov7675IconPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pse84PictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightPartSplitContainer).BeginInit();
            rightPartSplitContainer.Panel1.SuspendLayout();
            rightPartSplitContainer.Panel2.SuspendLayout();
            rightPartSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)psocEdgeHardwarePictureBox).BeginInit();
            SuspendLayout();
            // 
            // ov7675PictureBox
            // 
            ov7675PictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ov7675PictureBox.BorderStyle = BorderStyle.FixedSingle;
            ov7675PictureBox.Location = new Point(6, 53);
            ov7675PictureBox.Margin = new Padding(3, 2, 3, 2);
            ov7675PictureBox.Name = "ov7675PictureBox";
            ov7675PictureBox.Size = new Size(377, 259);
            ov7675PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            ov7675PictureBox.TabIndex = 2;
            ov7675PictureBox.TabStop = false;
            // 
            // comPortLabel
            // 
            comPortLabel.AutoSize = true;
            comPortLabel.Location = new Point(10, 26);
            comPortLabel.Name = "comPortLabel";
            comPortLabel.Size = new Size(63, 15);
            comPortLabel.TabIndex = 5;
            comPortLabel.Text = "COM port:";
            // 
            // comPortComboBox
            // 
            comPortComboBox.FormattingEnabled = true;
            comPortComboBox.Location = new Point(83, 23);
            comPortComboBox.Margin = new Padding(3, 2, 3, 2);
            comPortComboBox.Name = "comPortComboBox";
            comPortComboBox.Size = new Size(133, 23);
            comPortComboBox.TabIndex = 6;
            // 
            // connectButton
            // 
            connectButton.Location = new Point(220, 22);
            connectButton.Margin = new Padding(3, 2, 3, 2);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(82, 22);
            connectButton.TabIndex = 7;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;
            connectButton.Click += connectButton_Click;
            // 
            // disconnectButton
            // 
            disconnectButton.Enabled = false;
            disconnectButton.Location = new Point(308, 22);
            disconnectButton.Margin = new Padding(3, 2, 3, 2);
            disconnectButton.Name = "disconnectButton";
            disconnectButton.Size = new Size(82, 22);
            disconnectButton.TabIndex = 8;
            disconnectButton.Text = "Disconnect";
            disconnectButton.UseVisualStyleBackColor = true;
            disconnectButton.Click += disconnectButton_Click;
            // 
            // statusTextBox
            // 
            statusTextBox.Location = new Point(402, 23);
            statusTextBox.Margin = new Padding(3, 2, 3, 2);
            statusTextBox.Name = "statusTextBox";
            statusTextBox.ReadOnly = true;
            statusTextBox.Size = new Size(110, 23);
            statusTextBox.TabIndex = 9;
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.BackColor = Color.FromArgb(224, 224, 224);
            mainMenuStrip.ImageScalingSize = new Size(20, 20);
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(5, 2, 0, 2);
            mainMenuStrip.Size = new Size(898, 24);
            mainMenuStrip.TabIndex = 11;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { savePictureToolStripMenuItem, storeForCToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // savePictureToolStripMenuItem
            // 
            savePictureToolStripMenuItem.Name = "savePictureToolStripMenuItem";
            savePictureToolStripMenuItem.Size = new Size(138, 22);
            savePictureToolStripMenuItem.Text = "Save picture";
            savePictureToolStripMenuItem.Click += savePictureToolStripMenuItem_Click;
            // 
            // storeForCToolStripMenuItem
            // 
            storeForCToolStripMenuItem.Name = "storeForCToolStripMenuItem";
            storeForCToolStripMenuItem.Size = new Size(138, 22);
            storeForCToolStripMenuItem.Text = "Store for C";
            storeForCToolStripMenuItem.Click += storeForCToolStripMenuItem_Click;
            // 
            // debugPictureBox
            // 
            debugPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            debugPictureBox.BorderStyle = BorderStyle.FixedSingle;
            debugPictureBox.Location = new Point(6, 54);
            debugPictureBox.Name = "debugPictureBox";
            debugPictureBox.Size = new Size(377, 260);
            debugPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            debugPictureBox.TabIndex = 12;
            debugPictureBox.TabStop = false;
            // 
            // psocEdgeNNOutputTextBox
            // 
            psocEdgeNNOutputTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            psocEdgeNNOutputTextBox.Location = new Point(169, 300);
            psocEdgeNNOutputTextBox.Name = "psocEdgeNNOutputTextBox";
            psocEdgeNNOutputTextBox.ReadOnly = true;
            psocEdgeNNOutputTextBox.Size = new Size(316, 23);
            psocEdgeNNOutputTextBox.TabIndex = 13;
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainSplitContainer.Location = new Point(10, 50);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(pictureSplitContainer);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(rightPartSplitContainer);
            mainSplitContainer.Size = new Size(878, 640);
            mainSplitContainer.SplitterDistance = 387;
            mainSplitContainer.TabIndex = 14;
            // 
            // pictureSplitContainer
            // 
            pictureSplitContainer.Dock = DockStyle.Fill;
            pictureSplitContainer.Location = new Point(0, 0);
            pictureSplitContainer.Name = "pictureSplitContainer";
            pictureSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // pictureSplitContainer.Panel1
            // 
            pictureSplitContainer.Panel1.Controls.Add(ov7675PictureBox);
            pictureSplitContainer.Panel1.Controls.Add(ov7675IconPictureBox);
            // 
            // pictureSplitContainer.Panel2
            // 
            pictureSplitContainer.Panel2.Controls.Add(debugPictureBox);
            pictureSplitContainer.Panel2.Controls.Add(pse84PictureBox);
            pictureSplitContainer.Size = new Size(387, 640);
            pictureSplitContainer.SplitterDistance = 314;
            pictureSplitContainer.TabIndex = 0;
            // 
            // ov7675IconPictureBox
            // 
            ov7675IconPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ov7675IconPictureBox.BackColor = Color.White;
            ov7675IconPictureBox.BorderStyle = BorderStyle.FixedSingle;
            ov7675IconPictureBox.Image = kit_pse84_ai_streaming.Properties.Resources.ov7675;
            ov7675IconPictureBox.Location = new Point(6, 2);
            ov7675IconPictureBox.Margin = new Padding(3, 2, 3, 2);
            ov7675IconPictureBox.Name = "ov7675IconPictureBox";
            ov7675IconPictureBox.Size = new Size(377, 47);
            ov7675IconPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            ov7675IconPictureBox.TabIndex = 0;
            ov7675IconPictureBox.TabStop = false;
            // 
            // pse84PictureBox
            // 
            pse84PictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pse84PictureBox.BackColor = Color.White;
            pse84PictureBox.BorderStyle = BorderStyle.FixedSingle;
            pse84PictureBox.Image = kit_pse84_ai_streaming.Properties.Resources.pse84;
            pse84PictureBox.Location = new Point(6, 2);
            pse84PictureBox.Margin = new Padding(3, 2, 3, 2);
            pse84PictureBox.Name = "pse84PictureBox";
            pse84PictureBox.Size = new Size(377, 47);
            pse84PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pse84PictureBox.TabIndex = 0;
            pse84PictureBox.TabStop = false;
            // 
            // rightPartSplitContainer
            // 
            rightPartSplitContainer.Dock = DockStyle.Fill;
            rightPartSplitContainer.Location = new Point(0, 0);
            rightPartSplitContainer.Margin = new Padding(3, 2, 3, 2);
            rightPartSplitContainer.Name = "rightPartSplitContainer";
            rightPartSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // rightPartSplitContainer.Panel1
            // 
            rightPartSplitContainer.Panel1.Controls.Add(psocEdgeHardwarePictureBox);
            // 
            // rightPartSplitContainer.Panel2
            // 
            rightPartSplitContainer.Panel2.Controls.Add(psocEdgePredictionLabel);
            rightPartSplitContainer.Panel2.Controls.Add(predictionSignalsView);
            rightPartSplitContainer.Panel2.Controls.Add(psocEdgeNNOutputTextBox);
            rightPartSplitContainer.Size = new Size(487, 640);
            rightPartSplitContainer.SplitterDistance = 314;
            rightPartSplitContainer.SplitterWidth = 3;
            rightPartSplitContainer.TabIndex = 0;
            // 
            // psocEdgeHardwarePictureBox
            // 
            psocEdgeHardwarePictureBox.BackColor = Color.White;
            psocEdgeHardwarePictureBox.BorderStyle = BorderStyle.FixedSingle;
            psocEdgeHardwarePictureBox.Dock = DockStyle.Fill;
            psocEdgeHardwarePictureBox.Image = kit_pse84_ai_streaming.Properties.Resources.psoc_edge_hardware;
            psocEdgeHardwarePictureBox.InitialImage = kit_pse84_ai_streaming.Properties.Resources.psoc_edge_hardware;
            psocEdgeHardwarePictureBox.Location = new Point(0, 0);
            psocEdgeHardwarePictureBox.Margin = new Padding(3, 2, 3, 2);
            psocEdgeHardwarePictureBox.Name = "psocEdgeHardwarePictureBox";
            psocEdgeHardwarePictureBox.Size = new Size(487, 314);
            psocEdgeHardwarePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            psocEdgeHardwarePictureBox.TabIndex = 0;
            psocEdgeHardwarePictureBox.TabStop = false;
            // 
            // psocEdgePredictionLabel
            // 
            psocEdgePredictionLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            psocEdgePredictionLabel.AutoSize = true;
            psocEdgePredictionLabel.Location = new Point(3, 303);
            psocEdgePredictionLabel.Name = "psocEdgePredictionLabel";
            psocEdgePredictionLabel.Size = new Size(126, 15);
            psocEdgePredictionLabel.TabIndex = 14;
            psocEdgePredictionLabel.Text = "PSOC Edge Prediction:";
            // 
            // predictionSignalsView
            // 
            predictionSignalsView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            predictionSignalsView.BorderStyle = BorderStyle.FixedSingle;
            predictionSignalsView.Location = new Point(3, 2);
            predictionSignalsView.Margin = new Padding(3, 2, 3, 2);
            predictionSignalsView.Name = "predictionSignalsView";
            predictionSignalsView.Size = new Size(482, 293);
            predictionSignalsView.TabIndex = 15;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(898, 700);
            Controls.Add(mainSplitContainer);
            Controls.Add(statusTextBox);
            Controls.Add(disconnectButton);
            Controls.Add(connectButton);
            Controls.Add(comPortComboBox);
            Controls.Add(comPortLabel);
            Controls.Add(mainMenuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(3, 2, 3, 2);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Rutronik - KIT_PSE84_AI streaming";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)ov7675PictureBox).EndInit();
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)debugPictureBox).EndInit();
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            pictureSplitContainer.Panel1.ResumeLayout(false);
            pictureSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureSplitContainer).EndInit();
            pictureSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ov7675IconPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)pse84PictureBox).EndInit();
            rightPartSplitContainer.Panel1.ResumeLayout(false);
            rightPartSplitContainer.Panel2.ResumeLayout(false);
            rightPartSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)rightPartSplitContainer).EndInit();
            rightPartSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)psocEdgeHardwarePictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private PictureBox ov7675PictureBox;
        private Label comPortLabel;
        private ComboBox comPortComboBox;
        private Button connectButton;
        private Button disconnectButton;
        private TextBox statusTextBox;
        private MenuStrip mainMenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem savePictureToolStripMenuItem;
        private PictureBox debugPictureBox;
        private ToolStripMenuItem storeForCToolStripMenuItem;
        private TextBox psocEdgeNNOutputTextBox;
        private SplitContainer mainSplitContainer;
        private SplitContainer pictureSplitContainer;
        private Label psocEdgePredictionLabel;
        private Views.RawRadarSignalsView predictionSignalsView;
        private SplitContainer rightPartSplitContainer;
        private PictureBox psocEdgeHardwarePictureBox;
        private PictureBox ov7675IconPictureBox;
        private PictureBox pse84PictureBox;
    }
}
