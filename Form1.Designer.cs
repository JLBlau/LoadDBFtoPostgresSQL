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
            this.dbfPath = new System.Windows.Forms.TextBox();
            this.selectFolder = new System.Windows.Forms.Button();
            this.sqlServerLabel = new System.Windows.Forms.Label();
            this.sqlServerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.sqlUserName = new System.Windows.Forms.TextBox();
            this.sqlPasswordLabel = new System.Windows.Forms.Label();
            this.sqlPassword = new System.Windows.Forms.TextBox();
            this.sqlDBLabel = new System.Windows.Forms.Label();
            this.newSQLDBName = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.lbMessages = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // DBFLabel
            // 
            this.DBFLabel.AutoSize = true;
            this.DBFLabel.Location = new System.Drawing.Point(14, 30);
            this.DBFLabel.Name = "DBFLabel";
            this.DBFLabel.Size = new System.Drawing.Size(142, 25);
            this.DBFLabel.TabIndex = 0;
            this.DBFLabel.Text = "DBF Location";
            // 
            // dbfPath
            // 
            this.dbfPath.Location = new System.Drawing.Point(237, 30);
            this.dbfPath.Name = "dbfPath";
            this.dbfPath.Size = new System.Drawing.Size(224, 31);
            this.dbfPath.TabIndex = 1;
            this.dbfPath.Text = "C:\\Data\\HonoreKin\\AD2014\\DBFF";
            // 
            // selectFolder
            // 
            this.selectFolder.Location = new System.Drawing.Point(520, 30);
            this.selectFolder.Name = "selectFolder";
            this.selectFolder.Size = new System.Drawing.Size(120, 31);
            this.selectFolder.TabIndex = 2;
            this.selectFolder.Text = "Select Folder";
            this.selectFolder.UseVisualStyleBackColor = true;
            this.selectFolder.Click += new System.EventHandler(this.selectFolder_Click);
            // 
            // sqlServerLabel
            // 
            this.sqlServerLabel.AutoSize = true;
            this.sqlServerLabel.Location = new System.Drawing.Point(14, 91);
            this.sqlServerLabel.Name = "sqlServerLabel";
            this.sqlServerLabel.Size = new System.Drawing.Size(123, 25);
            this.sqlServerLabel.TabIndex = 3;
            this.sqlServerLabel.Text = "SQL Server";
            // 
            // sqlServerName
            // 
            this.sqlServerName.Location = new System.Drawing.Point(237, 88);
            this.sqlServerName.Name = "sqlServerName";
            this.sqlServerName.Size = new System.Drawing.Size(224, 31);
            this.sqlServerName.TabIndex = 4;
            this.sqlServerName.Text = "SQL1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 140);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "SQL User";
            // 
            // sqlUserName
            // 
            this.sqlUserName.Location = new System.Drawing.Point(237, 140);
            this.sqlUserName.Name = "sqlUserName";
            this.sqlUserName.Size = new System.Drawing.Size(224, 31);
            this.sqlUserName.TabIndex = 6;
            this.sqlUserName.Text = "SA";
            // 
            // sqlPasswordLabel
            // 
            this.sqlPasswordLabel.AutoSize = true;
            this.sqlPasswordLabel.Location = new System.Drawing.Point(14, 193);
            this.sqlPasswordLabel.Name = "sqlPasswordLabel";
            this.sqlPasswordLabel.Size = new System.Drawing.Size(154, 25);
            this.sqlPasswordLabel.TabIndex = 7;
            this.sqlPasswordLabel.Text = "SQL Password";
            // 
            // sqlPassword
            // 
            this.sqlPassword.Location = new System.Drawing.Point(237, 190);
            this.sqlPassword.Name = "sqlPassword";
            this.sqlPassword.Size = new System.Drawing.Size(224, 31);
            this.sqlPassword.TabIndex = 8;
            this.sqlPassword.Text = "z6@A*U23Fg7w";
            this.sqlPassword.UseSystemPasswordChar = true;
            // 
            // sqlDBLabel
            // 
            this.sqlDBLabel.AutoSize = true;
            this.sqlDBLabel.Location = new System.Drawing.Point(12, 244);
            this.sqlDBLabel.Name = "sqlDBLabel";
            this.sqlDBLabel.Size = new System.Drawing.Size(199, 25);
            this.sqlDBLabel.TabIndex = 9;
            this.sqlDBLabel.Text = "New SQL DB Name";
            // 
            // newSQLDBName
            // 
            this.newSQLDBName.Location = new System.Drawing.Point(237, 244);
            this.newSQLDBName.Name = "newSQLDBName";
            this.newSQLDBName.Size = new System.Drawing.Size(224, 31);
            this.newSQLDBName.TabIndex = 10;
            this.newSQLDBName.Text = "HonoreKin_AD20";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(520, 227);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(165, 48);
            this.startButton.TabIndex = 11;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // lbMessages
            // 
            this.lbMessages.FormattingEnabled = true;
            this.lbMessages.ItemHeight = 25;
            this.lbMessages.Location = new System.Drawing.Point(19, 340);
            this.lbMessages.Name = "lbMessages";
            this.lbMessages.Size = new System.Drawing.Size(675, 229);
            this.lbMessages.TabIndex = 12;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 629);
            this.Controls.Add(this.lbMessages);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.newSQLDBName);
            this.Controls.Add(this.sqlDBLabel);
            this.Controls.Add(this.sqlPassword);
            this.Controls.Add(this.sqlPasswordLabel);
            this.Controls.Add(this.sqlUserName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sqlServerName);
            this.Controls.Add(this.sqlServerLabel);
            this.Controls.Add(this.selectFolder);
            this.Controls.Add(this.dbfPath);
            this.Controls.Add(this.DBFLabel);
            this.Name = "Form1";
            this.Text = "MainForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DBFLabel;
        private System.Windows.Forms.TextBox dbfPath;
        private System.Windows.Forms.Button selectFolder;
        private System.Windows.Forms.Label sqlServerLabel;
        private System.Windows.Forms.TextBox sqlServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sqlUserName;
        private System.Windows.Forms.Label sqlPasswordLabel;
        private System.Windows.Forms.TextBox sqlPassword;
        private System.Windows.Forms.Label sqlDBLabel;
        private System.Windows.Forms.TextBox newSQLDBName;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.ListBox lbMessages;
    }
}

