namespace LoadFoxProDBToSQL
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
            this.DBFLabel = new System.Windows.Forms.Label();
            this.dbPath = new System.Windows.Forms.TextBox();
            this.selectFolder = new System.Windows.Forms.Button();
            this.sqlServerLabel = new System.Windows.Forms.Label();
            this.serverName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sqlUserName = new System.Windows.Forms.TextBox();
            this.sqlPasswordLabel = new System.Windows.Forms.Label();
            this.sqlPassword = new System.Windows.Forms.TextBox();
            this.sqlDBLabel = new System.Windows.Forms.Label();
            this.newSQLDBName = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.accessButton = new System.Windows.Forms.RadioButton();
            this.foxProButton1 = new System.Windows.Forms.RadioButton();
            this.dbaseButton1 = new System.Windows.Forms.RadioButton();
            this.serverButton1 = new System.Windows.Forms.RadioButton();
            this.serverButton2 = new System.Windows.Forms.RadioButton();
            this.serverBox = new System.Windows.Forms.GroupBox();
            this.messageBox = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.serverBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // DBFLabel
            // 
            this.DBFLabel.AutoSize = true;
            this.DBFLabel.Location = new System.Drawing.Point(14, 58);
            this.DBFLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.DBFLabel.Name = "DBFLabel";
            this.DBFLabel.Size = new System.Drawing.Size(138, 20);
            this.DBFLabel.TabIndex = 0;
            this.DBFLabel.Text = "DBF /DB Location";
            // 
            // dbPath
            // 
            this.dbPath.Location = new System.Drawing.Point(156, 58);
            this.dbPath.Margin = new System.Windows.Forms.Padding(2);
            this.dbPath.Name = "dbPath";
            this.dbPath.Size = new System.Drawing.Size(815, 26);
            this.dbPath.TabIndex = 1;
            // 
            // selectFolder
            // 
            this.selectFolder.Location = new System.Drawing.Point(986, 51);
            this.selectFolder.Margin = new System.Windows.Forms.Padding(2);
            this.selectFolder.Name = "selectFolder";
            this.selectFolder.Size = new System.Drawing.Size(124, 40);
            this.selectFolder.TabIndex = 2;
            this.selectFolder.Text = "Select Folder";
            this.selectFolder.UseVisualStyleBackColor = true;
            this.selectFolder.Click += new System.EventHandler(this.selectFolder_Click);
            // 
            // sqlServerLabel
            // 
            this.sqlServerLabel.AutoSize = true;
            this.sqlServerLabel.Location = new System.Drawing.Point(20, 107);
            this.sqlServerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.sqlServerLabel.Name = "sqlServerLabel";
            this.sqlServerLabel.Size = new System.Drawing.Size(101, 20);
            this.sqlServerLabel.TabIndex = 3;
            this.sqlServerLabel.Text = "Server Name";
            // 
            // serverName
            // 
            this.serverName.Location = new System.Drawing.Point(156, 104);
            this.serverName.Margin = new System.Windows.Forms.Padding(2);
            this.serverName.Name = "serverName";
            this.serverName.Size = new System.Drawing.Size(815, 26);
            this.serverName.TabIndex = 4;
            this.serverName.Text = "pg2.bitwerx.com";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 146);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "UserName";
            // 
            // sqlUserName
            // 
            this.sqlUserName.Location = new System.Drawing.Point(156, 146);
            this.sqlUserName.Margin = new System.Windows.Forms.Padding(2);
            this.sqlUserName.Name = "sqlUserName";
            this.sqlUserName.Size = new System.Drawing.Size(815, 26);
            this.sqlUserName.TabIndex = 6;
            this.sqlUserName.Text = "postgres";
            // 
            // sqlPasswordLabel
            // 
            this.sqlPasswordLabel.AutoSize = true;
            this.sqlPasswordLabel.Location = new System.Drawing.Point(20, 188);
            this.sqlPasswordLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.sqlPasswordLabel.Name = "sqlPasswordLabel";
            this.sqlPasswordLabel.Size = new System.Drawing.Size(78, 20);
            this.sqlPasswordLabel.TabIndex = 7;
            this.sqlPasswordLabel.Text = "Password";
            // 
            // sqlPassword
            // 
            this.sqlPassword.Location = new System.Drawing.Point(156, 186);
            this.sqlPassword.Margin = new System.Windows.Forms.Padding(2);
            this.sqlPassword.Name = "sqlPassword";
            this.sqlPassword.Size = new System.Drawing.Size(815, 26);
            this.sqlPassword.TabIndex = 8;
            this.sqlPassword.Text = "KjpQ}A#V}Lku)HZWx%-L7i.B";
            this.sqlPassword.UseSystemPasswordChar = true;
            // 
            // sqlDBLabel
            // 
            this.sqlDBLabel.AutoSize = true;
            this.sqlDBLabel.Location = new System.Drawing.Point(19, 229);
            this.sqlDBLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.sqlDBLabel.Name = "sqlDBLabel";
            this.sqlDBLabel.Size = new System.Drawing.Size(116, 20);
            this.sqlDBLabel.TabIndex = 9;
            this.sqlDBLabel.Text = "Dest DB Name";
            // 
            // newSQLDBName
            // 
            this.newSQLDBName.Location = new System.Drawing.Point(156, 229);
            this.newSQLDBName.Margin = new System.Windows.Forms.Padding(2);
            this.newSQLDBName.Name = "newSQLDBName";
            this.newSQLDBName.Size = new System.Drawing.Size(815, 26);
            this.newSQLDBName.TabIndex = 10;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(996, 223);
            this.startButton.Margin = new System.Windows.Forms.Padding(2);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(124, 38);
            this.startButton.TabIndex = 11;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.accessButton);
            this.groupBox1.Controls.Add(this.foxProButton1);
            this.groupBox1.Controls.Add(this.dbaseButton1);
            this.groupBox1.Location = new System.Drawing.Point(24, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(417, 50);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source Data Type";
            // 
            // accessButton
            // 
            this.accessButton.AutoSize = true;
            this.accessButton.Location = new System.Drawing.Point(271, 19);
            this.accessButton.Name = "accessButton";
            this.accessButton.Size = new System.Drawing.Size(113, 24);
            this.accessButton.TabIndex = 3;
            this.accessButton.TabStop = true;
            this.accessButton.Text = "Access DB";
            this.accessButton.UseVisualStyleBackColor = true;
            this.accessButton.CheckedChanged += new System.EventHandler(this.accessButton_CheckedChanged);
            // 
            // foxProButton1
            // 
            this.foxProButton1.AutoSize = true;
            this.foxProButton1.Location = new System.Drawing.Point(143, 20);
            this.foxProButton1.Name = "foxProButton1";
            this.foxProButton1.Size = new System.Drawing.Size(121, 24);
            this.foxProButton1.TabIndex = 2;
            this.foxProButton1.TabStop = true;
            this.foxProButton1.Text = "FoxPro DBF";
            this.foxProButton1.UseVisualStyleBackColor = true;
            this.foxProButton1.CheckedChanged += new System.EventHandler(this.foxProButton1_CheckedChanged);
            // 
            // dbaseButton1
            // 
            this.dbaseButton1.AutoSize = true;
            this.dbaseButton1.Location = new System.Drawing.Point(16, 20);
            this.dbaseButton1.Name = "dbaseButton1";
            this.dbaseButton1.Size = new System.Drawing.Size(118, 24);
            this.dbaseButton1.TabIndex = 0;
            this.dbaseButton1.TabStop = true;
            this.dbaseButton1.Text = "Dbase DBF";
            this.dbaseButton1.UseVisualStyleBackColor = true;
            this.dbaseButton1.CheckedChanged += new System.EventHandler(this.dbaseButton1_CheckedChanged);
            // 
            // serverButton1
            // 
            this.serverButton1.AutoSize = true;
            this.serverButton1.Location = new System.Drawing.Point(6, 20);
            this.serverButton1.Name = "serverButton1";
            this.serverButton1.Size = new System.Drawing.Size(116, 24);
            this.serverButton1.TabIndex = 14;
            this.serverButton1.TabStop = true;
            this.serverButton1.Text = "SQL Server";
            this.serverButton1.UseVisualStyleBackColor = true;
            // 
            // serverButton2
            // 
            this.serverButton2.AutoSize = true;
            this.serverButton2.Location = new System.Drawing.Point(199, 20);
            this.serverButton2.Name = "serverButton2";
            this.serverButton2.Size = new System.Drawing.Size(112, 24);
            this.serverButton2.TabIndex = 15;
            this.serverButton2.TabStop = true;
            this.serverButton2.Text = "PostreSQL";
            this.serverButton2.UseVisualStyleBackColor = true;
            // 
            // serverBox
            // 
            this.serverBox.Controls.Add(this.serverButton1);
            this.serverBox.Controls.Add(this.serverButton2);
            this.serverBox.Location = new System.Drawing.Point(596, 3);
            this.serverBox.Name = "serverBox";
            this.serverBox.Size = new System.Drawing.Size(375, 50);
            this.serverBox.TabIndex = 16;
            this.serverBox.TabStop = false;
            this.serverBox.Text = "Destination Server Type";
            // 
            // messageBox
            // 
            this.messageBox.Location = new System.Drawing.Point(14, 266);
            this.messageBox.Name = "messageBox";
            this.messageBox.ReadOnly = true;
            this.messageBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.messageBox.ShortcutsEnabled = false;
            this.messageBox.Size = new System.Drawing.Size(1369, 385);
            this.messageBox.TabIndex = 17;
            this.messageBox.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1395, 749);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.serverBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.newSQLDBName);
            this.Controls.Add(this.sqlDBLabel);
            this.Controls.Add(this.sqlPassword);
            this.Controls.Add(this.sqlPasswordLabel);
            this.Controls.Add(this.sqlUserName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.serverName);
            this.Controls.Add(this.sqlServerLabel);
            this.Controls.Add(this.selectFolder);
            this.Controls.Add(this.dbPath);
            this.Controls.Add(this.DBFLabel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.serverBox.ResumeLayout(false);
            this.serverBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DBFLabel;
        private System.Windows.Forms.TextBox dbPath;
        private System.Windows.Forms.Button selectFolder;
        private System.Windows.Forms.Label sqlServerLabel;
        private System.Windows.Forms.TextBox serverName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sqlUserName;
        private System.Windows.Forms.Label sqlPasswordLabel;
        private System.Windows.Forms.TextBox sqlPassword;
        private System.Windows.Forms.Label sqlDBLabel;
        private System.Windows.Forms.TextBox newSQLDBName;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton foxProButton1;
        private System.Windows.Forms.RadioButton dbaseButton1;
        private System.Windows.Forms.RadioButton serverButton1;
        private System.Windows.Forms.RadioButton serverButton2;
        private System.Windows.Forms.GroupBox serverBox;
        private System.Windows.Forms.RichTextBox messageBox;
        private System.Windows.Forms.RadioButton accessButton;
    }
}

