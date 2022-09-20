namespace Rule34Downloader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.LogField = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.DBRead = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.ImagePath = new System.Windows.Forms.TextBox();
            this.DBPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.CreateDB = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.ConvertGifs = new System.Windows.Forms.CheckBox();
            this.button7 = new System.Windows.Forms.Button();
            this.DeleteOriginalGif = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.FFMPEGPath = new System.Windows.Forms.TextBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.FFMPEGPathExplorer = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.ImagePathExplorer = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.DBPathExplorer = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.browser = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.UserName = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.DownloadAll = new System.Windows.Forms.Button();
            this.gifCurrentProgress = new System.Windows.Forms.Label();
            this.gifTotalProgress = new System.Windows.Forms.Label();
            this.gifSlash = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.browser)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(311, 227);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Download";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 198);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(374, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 12);
            this.textBox1.MaxLength = 100000;
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(373, 20);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(15, 61);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(31, 20);
            this.textBox2.TabIndex = 3;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(88, 64);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(126, 17);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "Remove Other Artists\r\n";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // LogField
            // 
            this.LogField.Location = new System.Drawing.Point(12, 89);
            this.LogField.MaxLength = 128000;
            this.LogField.Multiline = true;
            this.LogField.Name = "LogField";
            this.LogField.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogField.Size = new System.Drawing.Size(374, 103);
            this.LogField.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Skip Pages";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(88, 41);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(128, 17);
            this.checkBox2.TabIndex = 7;
            this.checkBox2.Text = "Stop When Duplicate";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // DBRead
            // 
            this.DBRead.Enabled = false;
            this.DBRead.Location = new System.Drawing.Point(697, 484);
            this.DBRead.Name = "DBRead";
            this.DBRead.Size = new System.Drawing.Size(75, 23);
            this.DBRead.TabIndex = 8;
            this.DBRead.Text = "LocalRead";
            this.DBRead.UseVisualStyleBackColor = true;
            this.DBRead.Click += new System.EventHandler(this.button2_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(697, 510);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "LocalWrite";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(220, 64);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(153, 17);
            this.checkBox3.TabIndex = 10;
            this.checkBox3.Text = "Igonre Duplicates From DB";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // ImagePath
            // 
            this.ImagePath.Location = new System.Drawing.Point(583, 20);
            this.ImagePath.Name = "ImagePath";
            this.ImagePath.ReadOnly = true;
            this.ImagePath.Size = new System.Drawing.Size(155, 20);
            this.ImagePath.TabIndex = 11;
            // 
            // DBPath
            // 
            this.DBPath.Location = new System.Drawing.Point(584, 60);
            this.DBPath.Name = "DBPath";
            this.DBPath.ReadOnly = true;
            this.DBPath.Size = new System.Drawing.Size(155, 20);
            this.DBPath.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(580, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Image Save Folder:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(580, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "DataBase Path:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // CreateDB
            // 
            this.CreateDB.Location = new System.Drawing.Point(12, 510);
            this.CreateDB.Name = "CreateDB";
            this.CreateDB.Size = new System.Drawing.Size(75, 23);
            this.CreateDB.TabIndex = 19;
            this.CreateDB.Text = "CreateDB";
            this.CreateDB.UseVisualStyleBackColor = true;
            this.CreateDB.Click += new System.EventHandler(this.button5_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 256);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(760, 193);
            this.dataGridView1.TabIndex = 20;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(697, 455);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 21;
            this.button5.Text = "LoadData";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click_1);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(616, 509);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 22;
            this.button6.Text = "UpdateTags";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(13, 455);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(109, 17);
            this.radioButton1.TabIndex = 23;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Write Empty Tags";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(128, 455);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(88, 17);
            this.radioButton2.TabIndex = 24;
            this.radioButton2.Text = "Rewrite Tags";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Location = new System.Drawing.Point(15, 478);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(99, 17);
            this.checkBox4.TabIndex = 25;
            this.checkBox4.Text = "Safe Download";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(552, 198);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(220, 23);
            this.progressBar2.TabIndex = 26;
            // 
            // ConvertGifs
            // 
            this.ConvertGifs.AutoSize = true;
            this.ConvertGifs.Checked = true;
            this.ConvertGifs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ConvertGifs.Location = new System.Drawing.Point(552, 175);
            this.ConvertGifs.Name = "ConvertGifs";
            this.ConvertGifs.Size = new System.Drawing.Size(81, 17);
            this.ConvertGifs.TabIndex = 27;
            this.ConvertGifs.Text = "ConvertGifs";
            this.ConvertGifs.UseVisualStyleBackColor = true;
            this.ConvertGifs.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(1030, 59);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 28;
            this.button7.Text = "Log In";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // DeleteOriginalGif
            // 
            this.DeleteOriginalGif.AutoSize = true;
            this.DeleteOriginalGif.Checked = true;
            this.DeleteOriginalGif.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DeleteOriginalGif.Location = new System.Drawing.Point(640, 174);
            this.DeleteOriginalGif.Name = "DeleteOriginalGif";
            this.DeleteOriginalGif.Size = new System.Drawing.Size(95, 17);
            this.DeleteOriginalGif.TabIndex = 29;
            this.DeleteOriginalGif.Text = "Delete Original";
            this.DeleteOriginalGif.UseVisualStyleBackColor = true;
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Location = new System.Drawing.Point(220, 41);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(108, 17);
            this.checkBox7.TabIndex = 30;
            this.checkBox7.Text = "Don\'t Write in DB";
            this.checkBox7.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(392, 38);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(154, 212);
            this.listBox1.TabIndex = 31;
            this.listBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseDoubleClick);
            // 
            // SearchBox
            // 
            this.SearchBox.Location = new System.Drawing.Point(392, 12);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(153, 20);
            this.SearchBox.TabIndex = 32;
            this.SearchBox.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(580, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "FFMPEG Path:";
            // 
            // FFMPEGPath
            // 
            this.FFMPEGPath.Location = new System.Drawing.Point(584, 100);
            this.FFMPEGPath.Name = "FFMPEGPath";
            this.FFMPEGPath.ReadOnly = true;
            this.FFMPEGPath.Size = new System.Drawing.Size(155, 20);
            this.FFMPEGPath.TabIndex = 33;
            // 
            // pictureBox3
            // 
            this.pictureBox3.ErrorImage = ((System.Drawing.Image)(resources.GetObject("pictureBox3.ErrorImage")));
            this.pictureBox3.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox3.InitialImage")));
            this.pictureBox3.Location = new System.Drawing.Point(558, 100);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(20, 20);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 36;
            this.pictureBox3.TabStop = false;
            // 
            // FFMPEGPathExplorer
            // 
            this.FFMPEGPathExplorer.Image = ((System.Drawing.Image)(resources.GetObject("FFMPEGPathExplorer.Image")));
            this.FFMPEGPathExplorer.Location = new System.Drawing.Point(743, 95);
            this.FFMPEGPathExplorer.Name = "FFMPEGPathExplorer";
            this.FFMPEGPathExplorer.Size = new System.Drawing.Size(25, 25);
            this.FFMPEGPathExplorer.TabIndex = 35;
            this.FFMPEGPathExplorer.UseVisualStyleBackColor = true;
            this.FFMPEGPathExplorer.Click += new System.EventHandler(this.FFMPEGPathExplorer_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.ErrorImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.ErrorImage")));
            this.pictureBox2.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.InitialImage")));
            this.pictureBox2.Location = new System.Drawing.Point(558, 20);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(20, 20);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 18;
            this.pictureBox2.TabStop = false;
            // 
            // ImagePathExplorer
            // 
            this.ImagePathExplorer.Image = ((System.Drawing.Image)(resources.GetObject("ImagePathExplorer.Image")));
            this.ImagePathExplorer.Location = new System.Drawing.Point(743, 15);
            this.ImagePathExplorer.Name = "ImagePathExplorer";
            this.ImagePathExplorer.Size = new System.Drawing.Size(25, 25);
            this.ImagePathExplorer.TabIndex = 17;
            this.ImagePathExplorer.UseVisualStyleBackColor = true;
            this.ImagePathExplorer.Click += new System.EventHandler(this.button4_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.ErrorImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.ErrorImage")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(558, 60);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // DBPathExplorer
            // 
            this.DBPathExplorer.Image = ((System.Drawing.Image)(resources.GetObject("DBPathExplorer.Image")));
            this.DBPathExplorer.Location = new System.Drawing.Point(743, 55);
            this.DBPathExplorer.Name = "DBPathExplorer";
            this.DBPathExplorer.Size = new System.Drawing.Size(25, 25);
            this.DBPathExplorer.TabIndex = 15;
            this.DBPathExplorer.UseVisualStyleBackColor = true;
            this.DBPathExplorer.Click += new System.EventHandler(this.button3_Click);
            // 
            // browser
            // 
            this.browser.CreationProperties = null;
            this.browser.DefaultBackgroundColor = System.Drawing.Color.White;
            this.browser.Location = new System.Drawing.Point(774, 89);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(463, 443);
            this.browser.Source = new System.Uri("https://rule34hentai.net/user_admin/login", System.UriKind.Absolute);
            this.browser.TabIndex = 37;
            this.browser.ZoomFactor = 1D;
            this.browser.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.browser_NavigationCompleted);
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(1005, 11);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(100, 20);
            this.UserName.TabIndex = 38;
            this.UserName.Text = "monsterakill";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(1005, 34);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.Size = new System.Drawing.Size(100, 20);
            this.Password.TabIndex = 39;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(775, 59);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 40;
            this.button3.Text = "Return";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // DownloadAll
            // 
            this.DownloadAll.Location = new System.Drawing.Point(12, 226);
            this.DownloadAll.Name = "DownloadAll";
            this.DownloadAll.Size = new System.Drawing.Size(102, 23);
            this.DownloadAll.TabIndex = 41;
            this.DownloadAll.Text = "Download All";
            this.DownloadAll.UseVisualStyleBackColor = true;
            this.DownloadAll.Click += new System.EventHandler(this.DownloadAll_Click);
            // 
            // gifCurrentProgress
            // 
            this.gifCurrentProgress.AutoSize = true;
            this.gifCurrentProgress.Location = new System.Drawing.Point(616, 231);
            this.gifCurrentProgress.Name = "gifCurrentProgress";
            this.gifCurrentProgress.Size = new System.Drawing.Size(13, 13);
            this.gifCurrentProgress.TabIndex = 42;
            this.gifCurrentProgress.Text = "0";
            // 
            // gifTotalProgress
            // 
            this.gifTotalProgress.AutoSize = true;
            this.gifTotalProgress.Location = new System.Drawing.Point(678, 232);
            this.gifTotalProgress.Name = "gifTotalProgress";
            this.gifTotalProgress.Size = new System.Drawing.Size(13, 13);
            this.gifTotalProgress.TabIndex = 43;
            this.gifTotalProgress.Text = "0";
            // 
            // gifSlash
            // 
            this.gifSlash.AutoSize = true;
            this.gifSlash.BackColor = System.Drawing.Color.Transparent;
            this.gifSlash.Location = new System.Drawing.Point(646, 231);
            this.gifSlash.Name = "gifSlash";
            this.gifSlash.Size = new System.Drawing.Size(12, 13);
            this.gifSlash.TabIndex = 44;
            this.gifSlash.Text = "/";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(775, 5);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 45;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1249, 545);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.gifSlash);
            this.Controls.Add(this.gifTotalProgress);
            this.Controls.Add(this.gifCurrentProgress);
            this.Controls.Add(this.DownloadAll);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.UserName);
            this.Controls.Add(this.browser);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.FFMPEGPathExplorer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.FFMPEGPath);
            this.Controls.Add(this.SearchBox);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.checkBox7);
            this.Controls.Add(this.DeleteOriginalGif);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.ConvertGifs);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.CreateDB);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.ImagePathExplorer);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.DBPathExplorer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.DBPath);
            this.Controls.Add(this.ImagePath);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.DBRead);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LogField);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.browser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox LogField;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button DBRead;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.TextBox ImagePath;
        private System.Windows.Forms.TextBox DBPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button DBPathExplorer;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button ImagePathExplorer;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button CreateDB;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.CheckBox ConvertGifs;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.CheckBox DeleteOriginalGif;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button FFMPEGPathExplorer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FFMPEGPath;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Microsoft.Web.WebView2.WinForms.WebView2 browser;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button DownloadAll;
        private System.Windows.Forms.Label gifCurrentProgress;
        private System.Windows.Forms.Label gifTotalProgress;
        private System.Windows.Forms.Label gifSlash;
        private System.Windows.Forms.Button button4;
    }
}

