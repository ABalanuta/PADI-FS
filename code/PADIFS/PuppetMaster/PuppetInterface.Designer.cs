namespace PuppetMaster
{
    partial class PuppetInterface
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.TexboxLogStatus = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonAddMeta2 = new System.Windows.Forms.Button();
            this.buttonAddMeta1 = new System.Windows.Forms.Button();
            this.buttonAddMeta0 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.buttonAddData4 = new System.Windows.Forms.Button();
            this.buttonAddData3 = new System.Windows.Forms.Button();
            this.buttonAddData2 = new System.Windows.Forms.Button();
            this.buttonAddData1 = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.buttonAddClient4 = new System.Windows.Forms.Button();
            this.buttonAddClient3 = new System.Windows.Forms.Button();
            this.buttonAddClient2 = new System.Windows.Forms.Button();
            this.buttonAddClient1 = new System.Windows.Forms.Button();
            this.ButtonNextStep = new System.Windows.Forms.Button();
            this.ButtonRun = new System.Windows.Forms.Button();
            this.TextBoxManualCommand = new System.Windows.Forms.TextBox();
            this.ButtonLoadScript = new System.Windows.Forms.Button();
            this.ButtonExecute = new System.Windows.Forms.Button();
            this.TextBoxCommandList = new System.Windows.Forms.TextBox();
            this.LabelOpenFile = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ButtonAsyncExecute = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // TexboxLogStatus
            // 
            this.TexboxLogStatus.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.TexboxLogStatus.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.TexboxLogStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TexboxLogStatus.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TexboxLogStatus.ForeColor = System.Drawing.Color.Gold;
            this.TexboxLogStatus.Location = new System.Drawing.Point(9, 19);
            this.TexboxLogStatus.Multiline = true;
            this.TexboxLogStatus.Name = "TexboxLogStatus";
            this.TexboxLogStatus.ReadOnly = true;
            this.TexboxLogStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TexboxLogStatus.Size = new System.Drawing.Size(605, 331);
            this.TexboxLogStatus.TabIndex = 9;
            this.TexboxLogStatus.TextChanged += new System.EventHandler(this.TexboxLogStatus_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TexboxLogStatus);
            this.groupBox2.Location = new System.Drawing.Point(3, 263);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(620, 356);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "LogBox";
            this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonAddMeta2);
            this.groupBox3.Controls.Add(this.buttonAddMeta1);
            this.groupBox3.Controls.Add(this.buttonAddMeta0);
            this.groupBox3.Location = new System.Drawing.Point(629, 1);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(83, 169);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "MetaServers";
            // 
            // buttonAddMeta2
            // 
            this.buttonAddMeta2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddMeta2.Location = new System.Drawing.Point(6, 120);
            this.buttonAddMeta2.Name = "buttonAddMeta2";
            this.buttonAddMeta2.Size = new System.Drawing.Size(71, 44);
            this.buttonAddMeta2.TabIndex = 3;
            this.buttonAddMeta2.Text = "Meta 2";
            this.buttonAddMeta2.UseVisualStyleBackColor = true;
            this.buttonAddMeta2.Click += new System.EventHandler(this.buttonAddMeta2_Click);
            // 
            // buttonAddMeta1
            // 
            this.buttonAddMeta1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddMeta1.Location = new System.Drawing.Point(6, 70);
            this.buttonAddMeta1.Name = "buttonAddMeta1";
            this.buttonAddMeta1.Size = new System.Drawing.Size(71, 44);
            this.buttonAddMeta1.TabIndex = 2;
            this.buttonAddMeta1.Text = "Meta 1";
            this.buttonAddMeta1.UseVisualStyleBackColor = true;
            this.buttonAddMeta1.Click += new System.EventHandler(this.buttonAddMeta1_Click);
            // 
            // buttonAddMeta0
            // 
            this.buttonAddMeta0.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddMeta0.Location = new System.Drawing.Point(6, 20);
            this.buttonAddMeta0.Name = "buttonAddMeta0";
            this.buttonAddMeta0.Size = new System.Drawing.Size(71, 44);
            this.buttonAddMeta0.TabIndex = 1;
            this.buttonAddMeta0.Text = "Meta 0";
            this.buttonAddMeta0.UseVisualStyleBackColor = true;
            this.buttonAddMeta0.Click += new System.EventHandler(this.buttonAddMeta_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.buttonAddData4);
            this.groupBox4.Controls.Add(this.buttonAddData3);
            this.groupBox4.Controls.Add(this.buttonAddData2);
            this.groupBox4.Controls.Add(this.buttonAddData1);
            this.groupBox4.Location = new System.Drawing.Point(629, 176);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(83, 220);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "DataServers";
            // 
            // buttonAddData4
            // 
            this.buttonAddData4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddData4.Location = new System.Drawing.Point(6, 169);
            this.buttonAddData4.Name = "buttonAddData4";
            this.buttonAddData4.Size = new System.Drawing.Size(71, 44);
            this.buttonAddData4.TabIndex = 16;
            this.buttonAddData4.Text = "Data 4";
            this.buttonAddData4.UseVisualStyleBackColor = true;
            this.buttonAddData4.Click += new System.EventHandler(this.buttonAddData4_Click);
            // 
            // buttonAddData3
            // 
            this.buttonAddData3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddData3.Location = new System.Drawing.Point(6, 119);
            this.buttonAddData3.Name = "buttonAddData3";
            this.buttonAddData3.Size = new System.Drawing.Size(71, 44);
            this.buttonAddData3.TabIndex = 15;
            this.buttonAddData3.Text = "Data 3";
            this.buttonAddData3.UseVisualStyleBackColor = true;
            this.buttonAddData3.Click += new System.EventHandler(this.buttonAddData3_Click);
            // 
            // buttonAddData2
            // 
            this.buttonAddData2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddData2.Location = new System.Drawing.Point(6, 69);
            this.buttonAddData2.Name = "buttonAddData2";
            this.buttonAddData2.Size = new System.Drawing.Size(71, 44);
            this.buttonAddData2.TabIndex = 5;
            this.buttonAddData2.Text = "Data 2";
            this.buttonAddData2.UseVisualStyleBackColor = true;
            this.buttonAddData2.Click += new System.EventHandler(this.buttonAddData2_Click);
            // 
            // buttonAddData1
            // 
            this.buttonAddData1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddData1.Location = new System.Drawing.Point(6, 19);
            this.buttonAddData1.Name = "buttonAddData1";
            this.buttonAddData1.Size = new System.Drawing.Size(71, 44);
            this.buttonAddData1.TabIndex = 4;
            this.buttonAddData1.Text = "Data 1";
            this.buttonAddData1.UseVisualStyleBackColor = true;
            this.buttonAddData1.Click += new System.EventHandler(this.buttonAddData1_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.buttonAddClient4);
            this.groupBox5.Controls.Add(this.buttonAddClient3);
            this.groupBox5.Controls.Add(this.buttonAddClient2);
            this.groupBox5.Controls.Add(this.buttonAddClient1);
            this.groupBox5.Location = new System.Drawing.Point(629, 402);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(83, 220);
            this.groupBox5.TabIndex = 17;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Clients";
            // 
            // buttonAddClient4
            // 
            this.buttonAddClient4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddClient4.Location = new System.Drawing.Point(6, 169);
            this.buttonAddClient4.Name = "buttonAddClient4";
            this.buttonAddClient4.Size = new System.Drawing.Size(71, 44);
            this.buttonAddClient4.TabIndex = 16;
            this.buttonAddClient4.Text = "Client 4";
            this.buttonAddClient4.UseVisualStyleBackColor = true;
            this.buttonAddClient4.Click += new System.EventHandler(this.buttonAddClient4_Click);
            // 
            // buttonAddClient3
            // 
            this.buttonAddClient3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddClient3.Location = new System.Drawing.Point(6, 119);
            this.buttonAddClient3.Name = "buttonAddClient3";
            this.buttonAddClient3.Size = new System.Drawing.Size(71, 44);
            this.buttonAddClient3.TabIndex = 15;
            this.buttonAddClient3.Text = "Client 3";
            this.buttonAddClient3.UseVisualStyleBackColor = true;
            this.buttonAddClient3.Click += new System.EventHandler(this.buttonAddClient3_Click);
            // 
            // buttonAddClient2
            // 
            this.buttonAddClient2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddClient2.Location = new System.Drawing.Point(6, 69);
            this.buttonAddClient2.Name = "buttonAddClient2";
            this.buttonAddClient2.Size = new System.Drawing.Size(71, 44);
            this.buttonAddClient2.TabIndex = 5;
            this.buttonAddClient2.Text = "Client 2";
            this.buttonAddClient2.UseVisualStyleBackColor = true;
            this.buttonAddClient2.Click += new System.EventHandler(this.buttonAddClient2_Click);
            // 
            // buttonAddClient1
            // 
            this.buttonAddClient1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAddClient1.Location = new System.Drawing.Point(6, 19);
            this.buttonAddClient1.Name = "buttonAddClient1";
            this.buttonAddClient1.Size = new System.Drawing.Size(71, 44);
            this.buttonAddClient1.TabIndex = 4;
            this.buttonAddClient1.Text = "Client 1";
            this.buttonAddClient1.UseVisualStyleBackColor = true;
            this.buttonAddClient1.Click += new System.EventHandler(this.buttonAddClient1_Click);
            // 
            // ButtonNextStep
            // 
            this.ButtonNextStep.Enabled = false;
            this.ButtonNextStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonNextStep.ForeColor = System.Drawing.Color.DarkRed;
            this.ButtonNextStep.Location = new System.Drawing.Point(503, 120);
            this.ButtonNextStep.Name = "ButtonNextStep";
            this.ButtonNextStep.Size = new System.Drawing.Size(109, 44);
            this.ButtonNextStep.TabIndex = 2;
            this.ButtonNextStep.Text = "Next Step";
            this.ButtonNextStep.UseVisualStyleBackColor = true;
            this.ButtonNextStep.Click += new System.EventHandler(this.ButtonNextStep_Click);
            // 
            // ButtonRun
            // 
            this.ButtonRun.Enabled = false;
            this.ButtonRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonRun.ForeColor = System.Drawing.Color.Maroon;
            this.ButtonRun.Location = new System.Drawing.Point(503, 70);
            this.ButtonRun.Name = "ButtonRun";
            this.ButtonRun.Size = new System.Drawing.Size(109, 44);
            this.ButtonRun.TabIndex = 1;
            this.ButtonRun.Text = "Run";
            this.ButtonRun.UseVisualStyleBackColor = true;
            this.ButtonRun.Click += new System.EventHandler(this.ButtonRun_Click);
            // 
            // TextBoxManualCommand
            // 
            this.TextBoxManualCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxManualCommand.Location = new System.Drawing.Point(6, 221);
            this.TextBoxManualCommand.Name = "TextBoxManualCommand";
            this.TextBoxManualCommand.Size = new System.Drawing.Size(415, 29);
            this.TextBoxManualCommand.TabIndex = 4;
            this.TextBoxManualCommand.Text = "Manual Command";
            this.TextBoxManualCommand.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TextBoxManualCommand_MouseClick);
            this.TextBoxManualCommand.TextChanged += new System.EventHandler(this.TextBoxManualCommand_TextChanged);
            this.TextBoxManualCommand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBoxManualCommand_KeyPress);
            // 
            // ButtonLoadScript
            // 
            this.ButtonLoadScript.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonLoadScript.Location = new System.Drawing.Point(503, 16);
            this.ButtonLoadScript.Name = "ButtonLoadScript";
            this.ButtonLoadScript.Size = new System.Drawing.Size(109, 48);
            this.ButtonLoadScript.TabIndex = 0;
            this.ButtonLoadScript.Text = "Load Script";
            this.ButtonLoadScript.UseVisualStyleBackColor = true;
            this.ButtonLoadScript.Click += new System.EventHandler(this.ButtonLoadScript_Click);
            // 
            // ButtonExecute
            // 
            this.ButtonExecute.BackColor = System.Drawing.Color.DarkOliveGreen;
            this.ButtonExecute.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonExecute.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ButtonExecute.Location = new System.Drawing.Point(503, 218);
            this.ButtonExecute.Name = "ButtonExecute";
            this.ButtonExecute.Size = new System.Drawing.Size(109, 32);
            this.ButtonExecute.TabIndex = 5;
            this.ButtonExecute.Text = "Execute";
            this.ButtonExecute.UseVisualStyleBackColor = false;
            this.ButtonExecute.Click += new System.EventHandler(this.ButtonExecute_Click);
            // 
            // TextBoxCommandList
            // 
            this.TextBoxCommandList.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.TextBoxCommandList.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.TextBoxCommandList.ForeColor = System.Drawing.Color.Lime;
            this.TextBoxCommandList.Location = new System.Drawing.Point(6, 34);
            this.TextBoxCommandList.Multiline = true;
            this.TextBoxCommandList.Name = "TextBoxCommandList";
            this.TextBoxCommandList.ReadOnly = true;
            this.TextBoxCommandList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBoxCommandList.Size = new System.Drawing.Size(491, 181);
            this.TextBoxCommandList.TabIndex = 10;
            this.TextBoxCommandList.Text = "Command List";
            // 
            // LabelOpenFile
            // 
            this.LabelOpenFile.AutoSize = true;
            this.LabelOpenFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOpenFile.Location = new System.Drawing.Point(6, 16);
            this.LabelOpenFile.Name = "LabelOpenFile";
            this.LabelOpenFile.Size = new System.Drawing.Size(107, 15);
            this.LabelOpenFile.TabIndex = 6;
            this.LabelOpenFile.Text = "File not loaded ";
            this.LabelOpenFile.Click += new System.EventHandler(this.LabelOpenFile_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.ButtonAsyncExecute);
            this.groupBox1.Controls.Add(this.LabelOpenFile);
            this.groupBox1.Controls.Add(this.TextBoxCommandList);
            this.groupBox1.Controls.Add(this.ButtonExecute);
            this.groupBox1.Controls.Add(this.ButtonLoadScript);
            this.groupBox1.Controls.Add(this.TextBoxManualCommand);
            this.groupBox1.Controls.Add(this.ButtonRun);
            this.groupBox1.Controls.Add(this.ButtonNextStep);
            this.groupBox1.Location = new System.Drawing.Point(3, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(620, 256);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CommandBox";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // ButtonAsyncExecute
            // 
            this.ButtonAsyncExecute.BackColor = System.Drawing.Color.DarkRed;
            this.ButtonAsyncExecute.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonAsyncExecute.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ButtonAsyncExecute.Location = new System.Drawing.Point(427, 218);
            this.ButtonAsyncExecute.Name = "ButtonAsyncExecute";
            this.ButtonAsyncExecute.Size = new System.Drawing.Size(70, 32);
            this.ButtonAsyncExecute.TabIndex = 11;
            this.ButtonAsyncExecute.Text = "Async";
            this.ButtonAsyncExecute.UseVisualStyleBackColor = false;
            this.ButtonAsyncExecute.Click += new System.EventHandler(this.button1_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Green;
            this.button1.Location = new System.Drawing.Point(505, 168);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(109, 44);
            this.button1.TabIndex = 12;
            this.button1.Text = "Async Step";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // PuppetInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 631);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "PuppetInterface";
            this.Text = "Puppet Master - PADI FS";
            this.Load += new System.EventHandler(this.PuppetMaster_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox TexboxLogStatus;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button buttonAddMeta2;
        private System.Windows.Forms.Button buttonAddMeta1;
        private System.Windows.Forms.Button buttonAddMeta0;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button buttonAddData4;
        private System.Windows.Forms.Button buttonAddData3;
        private System.Windows.Forms.Button buttonAddData2;
        private System.Windows.Forms.Button buttonAddData1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button buttonAddClient4;
        private System.Windows.Forms.Button buttonAddClient3;
        private System.Windows.Forms.Button buttonAddClient2;
        private System.Windows.Forms.Button buttonAddClient1;
        private System.Windows.Forms.Button ButtonNextStep;
        private System.Windows.Forms.Button ButtonRun;
        private System.Windows.Forms.TextBox TextBoxManualCommand;
        private System.Windows.Forms.Button ButtonLoadScript;
        private System.Windows.Forms.Button ButtonExecute;
        private System.Windows.Forms.TextBox TextBoxCommandList;
        private System.Windows.Forms.Label LabelOpenFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ButtonAsyncExecute;
        private System.Windows.Forms.Button button1;
    }
}

