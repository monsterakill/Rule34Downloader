using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace Rule34Downloader
{
    public partial class Form1 : Form
    {
        BackgroundWorker Worker;
        string[] duplicateFiles = new string[] { };
        List<string> duplicatesNames = new List<string>();
        List<string> gifConvertPathSequence = new List<string>();
        bool convertSucess = true;

        private string ConnectionString = string.Empty;
        private string ImageFolderPath = string.Empty;
        private readonly List<Artist> artistsLocal = new List<Artist>();
        private readonly List<Artist> artistsDB = new List<Artist>();
        private const string CreateTableQuery = @"CREATE TABLE IF NOT EXISTS [Images] (
                                               [PK] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                                               [ArtistName] TEXT,
                                               [ImageNumber] TEXT,
                                               [CreatedOn] TEXT,
                                               [ImageTags] TEXT    
                                               )";
        private const string DatabaseFile = "Rule34ImagesDB.db";
        

        public Form1()
        {
            InitializeComponent();

            Worker = new BackgroundWorker();
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.ProgressChanged += new ProgressChangedEventHandler
                    (Worker_ProgressChanged);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (Worker_RunWorkerCompleted);
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            CheckPaths(PathType.All);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ImageFolderPath))
            {
                button1.Enabled = false;
                textBox1.Enabled = false;
                Worker.RunWorkerAsync();
            }
            else
            {
                LogField.AppendText("Error: Select Image Save Folder!" + Environment.NewLine);
            }
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            // This function fires on the UI thread so it's safe to edit

            // the UI control directly, no funny business with Control.Invoke :)

            // Update the progressBar with the integer supplied to us from the

            // ReportProgress() function. 
            progressBar1.Maximum = 100;
            if (e.UserState != null)
            {
                LogField.AppendText(e.UserState.ToString());
            }
            if (e.ProgressPercentage != -1)
            {
                progressBar1.Value = e.ProgressPercentage;
            }
            TaskbarProgress.SetValue(Handle, progressBar1.Value, progressBar1.Maximum);
        }

        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //// The background process is complete. We need to inspect
            //// our response to see if an error occurred, a cancel was
            //// requested or if we completed successfully.  
            //if (e.Cancelled)
            //{
            //    lblStatus.Text = "Task Cancelled.";
            //}

            //// Check to see if an error occurred in the background process.

            //else if (e.Error != null)
            //{
            //    lblStatus.Text = "Error while performing background operation.";
            //}
            //else
            //{
            //    // Everything completed normally.
            //    lblStatus.Text = "Task Completed...";
            //}

            ////Change the status of the buttons on the UI accordingly
            //btnStartAsyncOperation.Enabled = true;
            ExtensionMethods.FlashNotification(this);
            button1.Enabled = true;
            textBox1.Enabled = true;
        }

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var finalURL = "";
            var progressBarValue = 0;
            var progressBarMaximumValue = 0;
            var progressBarPercent = 0.0;
            var progressBarPercentInt = 0;
            var correctedProgressBar = false;
            //Clear Duplicate Names
            duplicatesNames.Clear();
            duplicateFiles = new string[] { };
            if (checkBox1.Checked)
            {
                finalURL = RemoveArtistsFromSearch($"https://rule34.xxx/index.php?page=post&s=list&tags={textBox1.Text.ToLower()}");
            }
            else
            {
                finalURL = $"https://rule34.xxx/index.php?page=post&s=list&tags={textBox1.Text.ToLower()}";
            }

            Worker.ReportProgress(progressBarPercentInt, $"Url: {finalURL}" + Environment.NewLine);

            var web = new HtmlAgilityPack.HtmlWeb();

            HtmlAgilityPack.HtmlDocument doc = web.Load(finalURL);

            var pageCount = 1;
            var getLastPage = "";

            if (doc.DocumentNode.SelectNodes("//a[@alt='last page']") != null)
            {
                getLastPage = doc.DocumentNode.SelectNodes("//a[@alt='last page']")[0].OuterHtml;

                pageCount = (int.Parse(Between(getLastPage, "pid=", "\"")) / 42) + 1;
            }

            Worker.ReportProgress(progressBarPercentInt, $"Page count:{pageCount}" + Environment.NewLine);

            if (!Directory.Exists($"{ImageFolderPath}\\{textBox1.Text.ToLower()}"))
            {
                Directory.CreateDirectory($"{ImageFolderPath}\\{textBox1.Text.ToLower()}");
                Worker.ReportProgress(progressBarPercentInt, $"Creating Directory: {ImageFolderPath}\\{textBox1.Text.ToLower()}" + Environment.NewLine);
            }

            var skipPages = 0;
            if (!string.IsNullOrEmpty(textBox2.Text))
            {
                skipPages = int.Parse(textBox2.Text);
            }

            for (int i = skipPages; i < pageCount; i++)
            {
                var PagingURL = $"{finalURL}&pid={i * 42}";

                if (pageCount > 1)
                {
                    progressBarMaximumValue = pageCount * 42;
                }

                Worker.ReportProgress(progressBarPercentInt, $"Current Download Page URL: {PagingURL}" + Environment.NewLine);
                //Timeout
                Thread.Sleep(10000);

                doc = web.Load(PagingURL);

                var thumbs = doc.DocumentNode.SelectNodes("//span[@class='thumb']");
                var linktoVideo = "";
                var retryCount = 3;
                if (thumbs == null)
                {
                    for (int y = 0; y < retryCount; y++)
                    {
                        Worker.ReportProgress(progressBarPercentInt, $"Thumbs is empty Connection Retry: {y}" + Environment.NewLine);
                        //Timeout
                        System.Threading.Thread.Sleep(10000);
                        doc = web.Load(PagingURL);
                        thumbs = doc.DocumentNode.SelectNodes("//span[@class='thumb']");
                        if (thumbs != null) break;
                    }
                }

                foreach (var video in thumbs)
                {
                    if (pageCount <= 1)
                    {
                        progressBarMaximumValue = thumbs.Count;
                    }
                    else if (pageCount > 1 && thumbs.Count < 42 && !correctedProgressBar)
                    {
                        progressBarMaximumValue = progressBarMaximumValue - (42 - thumbs.Count);
                        correctedProgressBar = true;
                    }

                    Worker.ReportProgress(progressBarPercentInt);

                    progressBarValue++;

                    var r = new Random();
                    //Timeout
                    Thread.Sleep(r.Next(5000, 10000));

                    linktoVideo = video.OuterHtml.Substring(video.OuterHtml.IndexOf("index.php")).Substring(0, video.OuterHtml.Substring(video.OuterHtml.IndexOf("index.php")).IndexOf("\""));

                    string url = $"https://rule34.xxx/{linktoVideo}";
                    HtmlAgilityPack.HtmlDocument document = web.Load(url);

                    //Video Found
                    if (document.DocumentNode.SelectNodes("//source[@type='video/mp4']") != null)
                    {
                        string metascore = document.DocumentNode.SelectNodes("//source[@type='video/mp4']")[0].OuterHtml;

                        string urlSimple = metascore.Substring(metascore.IndexOf("https")).Substring(0, metascore.Substring(metascore.IndexOf("https")).IndexOf("mp4") + 3);

                        var fileName = metascore.Substring(metascore.IndexOf("?") + 1).Substring(0, metascore.Substring(metascore.IndexOf("?") + 1).IndexOf("t") + 1);

                        int fileNameReg = int.Parse(Regex.Match(fileName, @"\d+").Value);

                        if (checkBox3.Checked)
                        {
                            if (CheckForDuplicate(fileNameReg.ToString()))
                            {
                                Worker.ReportProgress(progressBarPercentInt, $"File Already Found With Id : {fileNameReg.ToString()}" + Environment.NewLine);
                                progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;
                                Worker.ReportProgress(progressBarPercentInt = (int)progressBarPercent);
                                if (checkBox2.Checked) return;
                                continue;
                            }
                        }
                        else
                        {
                            if (CheckForDuplicate(fileNameReg.ToString()) || CheckForDuplicateFromDB(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString() }))
                            {
                                if (!CheckForDuplicateFromDB(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString() }))
                                {
                                    //Write Image in DB If not Found in DB but found in Local Folder
                                    AddArtist(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document) });
                                }
                                Worker.ReportProgress(progressBarPercentInt, $"File Already Found With Id : {fileNameReg.ToString()}" + Environment.NewLine);
                                progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;
                                Worker.ReportProgress(progressBarPercentInt = (int)progressBarPercent);
                                if (checkBox2.Checked) return;
                                continue;
                            }
                        }

                        Worker.ReportProgress(progressBarPercentInt, $"Download Video: {fileNameReg}.mp4" + Environment.NewLine);

                        using (WebClient wc = new WebClient())
                        {
                            //wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                            wc.DownloadFileAsync(
                                // Param1 = Link of file
                                new System.Uri(urlSimple),
                                // Param2 = Path to save
                                $"{ImageFolderPath}\\{textBox1.Text.ToLower()}\\{fileNameReg}.mp4"
                            );
                        }

                        AddArtist(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document) });
                    }
                    else if (document.DocumentNode.SelectNodes("//img[@id='image']") != null)
                    {
                        var urlSimple = document.DocumentNode.SelectSingleNode("//div[@class='link-list']/ul/li[3]/a").Attributes[0].Value;
                        if (urlSimple.Contains("saucenao") || urlSimple.Contains("index.php"))
                        {
                            urlSimple = document.DocumentNode.SelectSingleNode("//div[@class='link-list']/ul/li[2]/a").Attributes[0].Value;
                        }
                        var fileNameReg = urlSimple.Substring(urlSimple.LastIndexOf('?') + 1);
                        var format = Between(urlSimple, ".", "?", true);

                        if (checkBox3.Checked)
                        {
                            if (CheckForDuplicate(fileNameReg))
                            {
                                Worker.ReportProgress(progressBarPercentInt, $"File Already Found With Id : {fileNameReg}" + Environment.NewLine);
                                progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;
                                Worker.ReportProgress(progressBarPercentInt = (int)progressBarPercent);

                                if (checkBox2.Checked) return;
                                continue;
                            }
                        }
                        else
                        {
                            if (CheckForDuplicate(fileNameReg) || CheckForDuplicateFromDB(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString() }))
                            {
                                if (!CheckForDuplicateFromDB(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString() }))
                                {
                                    //Write Image in DB If not Found in DB but found in Local Folder
                                    AddArtist(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document) });
                                }
                                Worker.ReportProgress(progressBarPercentInt, $"File Already Found With Id : {fileNameReg}" + Environment.NewLine);
                                progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;
                                Worker.ReportProgress(progressBarPercentInt = (int)progressBarPercent);

                                if (checkBox2.Checked) return;
                                continue;
                            }
                        }

                        Worker.ReportProgress(progressBarPercentInt, $"Download Image: {fileNameReg}.{format}" + Environment.NewLine);
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                            wc.QueryString.Add("fileFormat", format);
                            wc.QueryString.Add("gifConvert", checkBox5.Checked.ToString());
                            wc.QueryString.Add("filePath", $"{ImageFolderPath}\\{textBox1.Text.ToLower()}\\{fileNameReg}.{format}");
                            wc.DownloadFileAsync(new Uri(urlSimple), $"{ImageFolderPath}\\{textBox1.Text.ToLower()}\\{fileNameReg}.{format}");
                        }

                        AddArtist(new Artist { ArtistName = textBox1.Text.ToLower(), ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document) }); 
                    }
                    progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;

                    Worker.ReportProgress(progressBarPercentInt = (int)progressBarPercent);
                }
            }
        }

        private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
        {
            var fileFormat = ((WebClient)(sender)).QueryString["fileFormat"];
            var gifConvert = bool.Parse(((WebClient)(sender)).QueryString["gifConvert"]);
            var filePath = ((WebClient)(sender)).QueryString["filePath"];
            //Convert Gif to Webm
            if (fileFormat == "gif" && gifConvert && File.Exists(filePath))
            {
                GifToWebmConverter(filePath);
            }
        }

        public string RemoveArtistsFromSearch(string url)
        {
            var searchArtist = url;
            var web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(searchArtist);
            var artists = doc.DocumentNode.SelectNodes("//li[@class='tag-type-artist']");
            var artistsStringList = "";

            var retryCount = 3;
            if (artists == null)
            {
                for (int y = 0; y < retryCount; y++)
                {
                    Worker.ReportProgress(0, $"Artists is empty. Connection Retry: {y}" + Environment.NewLine);
                    //Timeout
                    Thread.Sleep(3000);
                    doc = web.Load(searchArtist);
                    artists = doc.DocumentNode.SelectNodes("//li[@class='tag-type-artist']");
                    if (artists != null) break;
                }
            }

            foreach (var artist in artists)
            {
                var removeSymbolsFromArtistNames = artist.InnerText.Substring(6).Substring(0, artist.InnerText.Substring(6).IndexOf("\n") - 1);
                if (removeSymbolsFromArtistNames.Contains(" "))
                {
                    removeSymbolsFromArtistNames = removeSymbolsFromArtistNames.Replace(" ", "_");
                }
                if (removeSymbolsFromArtistNames != textBox1.Text.ToLower())
                {
                    artistsStringList += $"+-{removeSymbolsFromArtistNames}";
                }

            }
            searchArtist += artistsStringList;

            //Timeout
            Thread.Sleep(5000);

            doc = web.Load(searchArtist);
            artists = doc.DocumentNode.SelectNodes("//li[@class='tag-type-artist']");


            if (artists == null)
            {
                for (int y = 0; y < retryCount; y++)
                {
                    Worker.ReportProgress(0, $"Artists Cycle is empty. Connection Retry: {y}" + Environment.NewLine);
                    //Timeout
                    Thread.Sleep(3000);
                    doc = web.Load(searchArtist);
                    artists = doc.DocumentNode.SelectNodes("//li[@class='tag-type-artist']");
                    if (artists != null) break;
                }
            }

            if (artists.Count > 1)
            {
                return RemoveArtistsFromSearch(searchArtist);
            }

            return searchArtist;
        }

        public string Between(string STR, string FirstString, string LastString, bool lastIndexof = false)
        {
            if(STR == null) return string.Empty;
            string FinalString;
            int Pos1 = 0;
            int Pos2 = 0;
            if (!lastIndexof)
            {
                Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                Pos2 = STR.IndexOf(LastString, Pos1);
                if (Pos2 == -1) return string.Empty;
            }
            else
            {
                Pos1 = STR.LastIndexOf(FirstString) + FirstString.Length;
                Pos2 = STR.IndexOf(LastString, Pos1);
            }

            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        public string GetUntilOrEmpty(string text, string stopAt = "-")
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }
            return string.Empty;
        }

        public string GetTags(HtmlAgilityPack.HtmlDocument doc)
        {
            try
            {
                var tags = new List<string>();
                foreach (HtmlAgilityPack.HtmlNode node in doc.DocumentNode.SelectNodes("//li[contains(@class, 'tag')]/a"))
                {
                    if (node.InnerText.Contains('(') && node.InnerText.Contains(')'))
                    {
                        //Need better solution
                        tags.Add(WebUtility.HtmlDecode(GetUntilOrEmpty(node.InnerText, "(").Replace(' ', '_').TrimEnd('_')));
                        tags.Add(WebUtility.HtmlDecode(node.InnerText.Split('(', ')')[1].Replace(' ', '_')));
                    }
                    else
                    {
                        tags.Add(WebUtility.HtmlDecode(node.InnerText.Replace(' ', '_')));
                    }
                }
                return string.Join("|", tags.Distinct().ToList());
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool CheckForDuplicate(string fileName)
        {
            if (duplicateFiles.Any())
            {
                return duplicatesNames.Contains(fileName);
            }
            else
            {
                duplicateFiles = Directory.GetFiles($"{ImageFolderPath}\\{textBox1.Text.ToLower()}\\");
                foreach (var dupl in duplicateFiles)
                {
                    duplicatesNames.Add(Path.GetFileNameWithoutExtension(dupl));
                }
                return duplicatesNames.Contains(fileName);
            }
        }

        public void AddArtist(Artist artist)
        {
            //Don't Write In DB
            if (checkBox7.Checked) return;

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                if (CheckForDuplicateFromDB(artist))
                {
                    Worker.ReportProgress(-1, $"This Image Already Found in DB: {artist.ArtistName} - {artist.ImageNumber}" + Environment.NewLine);
                    return;
                }

                var queryString = "insert into Images(ArtistName, ImageNumber, CreatedOn, ImageTags)" +
                                    " values(@artistName, @imageNumber, @createdOn, @imageTags);";

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(queryString, connection);
                    command.Parameters.AddWithValue("@artistName", artist.ArtistName);
                    command.Parameters.AddWithValue("@imageNumber", artist.ImageNumber);
                    command.Parameters.AddWithValue("@createdOn", artist.CreatedOn);
                    command.Parameters.AddWithValue("@imageTags", artist.ImageTags);

                    command.ExecuteScalar();
                }
            }
        }

        public void AddArtistTags(Artist artist)
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                var queryString = "UPDATE Images SET ImageTags = @imageTags WHERE ImageNumber = @imageNumber;";

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(queryString, connection);
                    command.Parameters.AddWithValue("@imageTags", artist.ImageTags);
                    command.Parameters.AddWithValue("@imageNumber", artist.ImageNumber);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Artist> LoadArtists()
        {
            var artistsList = new List<Artist>();
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                const string stringSql = "SELECT * FROM Images";

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(stringSql, connection);

                    using (SQLiteDataReader sqlReader = command.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            long primaryKey = (long)sqlReader["PK"];
                            string artistName = (string)sqlReader["ArtistName"];
                            string imageNumber = (string)sqlReader["ImageNumber"];
                            string createdOn = sqlReader["CreatedOn"] as string;
                            string imageTags = sqlReader["ImageTags"] as string;

                            Artist artist = new Artist()
                            {
                                PrimaryKey = primaryKey,
                                ArtistName = artistName,
                                ImageNumber = imageNumber,
                                CreatedOn = createdOn,
                                ImageTags = imageTags
                            };
                            artistsList.Add(artist);
                        }
                    }
                }
            }
            return artistsList;
        }

        public bool CheckForDuplicateFromDB(Artist artist)
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                const string stringSql = "SELECT * FROM Images WHERE ArtistName = @artistName and ImageNumber = @imageNumber";

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(stringSql, connection);

                    command.Parameters.Add(new SQLiteParameter("@artistName", artist.ArtistName));
                    command.Parameters.Add(new SQLiteParameter("@imageNumber", artist.ImageNumber));

                    using (SQLiteDataReader sqlReader = command.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            return sqlReader.HasRows;
                        }
                        return sqlReader.HasRows;
                    }
                }
            }
            return false;
        }

        public void ReadLoaclArtists()
        {
            artistsLocal.Clear();

            var progressBarValue = 0;
            var progressBarMaximumValue = Directory.GetDirectories($"{ImageFolderPath}").Length;
            var progressBarPercent = 0.0;

            foreach (var dir in Directory.GetDirectories($"{ImageFolderPath}"))
            {
                var artistName = new DirectoryInfo(dir).Name;

                foreach (string file in Directory.GetFiles(dir))
                {
                    FileInfo fi = new FileInfo(file);
                    var pa = Path.GetFileNameWithoutExtension(file);


                    Artist artist = new Artist()
                    {
                        ArtistName = artistName,
                        ImageNumber = pa,
                        CreatedOn = fi.CreationTime.ToShortDateString()
                    };

                    artistsLocal.Add(artist);
                }

                progressBarValue++;
                progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;
                progressBar1.Value = (int)progressBarPercent;
                TaskbarProgress.SetValue(Handle, progressBarPercent, progressBarMaximumValue);
            }
        }

        public void WriteLocalArtists()
        {
            var progressBarValue = 0;
            var progressBarMaximumValue = artistsLocal.Count;
            var progressBarPercent = 0.0;

            if (artistsLocal.Any())
            {
                foreach (var localArtist in artistsLocal)
                {
                    AddArtist(localArtist);

                    progressBarValue++;
                    progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;
                    progressBar1.Value = (int)progressBarPercent;
                    TaskbarProgress.SetValue(Handle, progressBarPercent, progressBarMaximumValue);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReadLoaclArtists();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            WriteLocalArtists();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "d:\\";
                openFileDialog.Filter = "DB File (*.db)|*.db";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    DBPath.Text = openFileDialog.FileName;

                    CheckPaths(PathType.DBPath);

                    //Save as Default Setting
                    Properties.Settings.Default.DBConnectionPath = DBPath.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog openFolderDialog = new FolderBrowserDialog())
            {
                if (openFolderDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(openFolderDialog.SelectedPath))
                {
                    //Get the ImagePath
                    ImagePath.Text = openFolderDialog.SelectedPath;

                    CheckPaths(PathType.ImagePath);

                    //Save as Default Setting
                    Properties.Settings.Default.ImageSavePath = ImagePath.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = pictureBox1.ErrorImage;
        }

        private void CheckPaths(PathType pathType)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DBConnectionPath) && pathType == PathType.All)
            {
                DBPath.Text = Properties.Settings.Default.DBConnectionPath;
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ImageSavePath) && pathType == PathType.All)
            {
                ImagePath.Text = Properties.Settings.Default.ImageSavePath;
            }

            if (pathType == PathType.DBPath || pathType == PathType.All)
            {
                if (string.IsNullOrEmpty(DBPath.Text))
                {
                    pictureBox1.Image = pictureBox1.ErrorImage;
                }
                else
                {
                    pictureBox1.Image = pictureBox1.InitialImage;
                    ConnectionString = $"Data Source={DBPath.Text}";
                }
            }

            if (pathType == PathType.ImagePath || pathType == PathType.All)
            {
                if (string.IsNullOrEmpty(ImagePath.Text))
                {
                    pictureBox2.Image = pictureBox2.ErrorImage;
                }
                else
                {
                    pictureBox2.Image = pictureBox2.InitialImage;
                    ImageFolderPath = ImagePath.Text;
                }
            }
        }

        public bool GifToWebmConverter(string filePath = "")
        {
            //Add New Path In List
            if(!string.IsNullOrEmpty(filePath)) gifConvertPathSequence.Add(filePath);

            //Return If Converter Working Now
            if (!convertSucess) return false;

            var iteration = "";
            var progressBarPercent = 0.0d;
            var progressBarPercentInt = 0;
            var intList = new List<int>() { 0 };
            var intMax = 0.0d;

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var ffmpegPath = $"{Directory.GetCurrentDirectory()}\\ffmpeg\\ffmpeg.exe";

            foreach (var path in gifConvertPathSequence)
            {
                if (convertSucess)
                {
                    convertSucess = false;

                    var command = $"{ffmpegPath} -i {path} -c:v libvpx-vp9 -b:v 0 -crf 30 -an -f webm -passlogfile {Directory.GetCurrentDirectory()}\\ffmpeg2pass.log";
                    command = $"{command} -pass 1 -y NUL && {command} -pass 2 {Path.ChangeExtension(path, null)}.webm";

                    var bw = new BackgroundWorker();
                    bw.WorkerReportsProgress = true;
                    bw.DoWork += delegate
                    {
                        var proc = new Process()
                        {
                            StartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                            {
                                UseShellExecute = false,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };
                        proc.ErrorDataReceived += new DataReceivedEventHandler((a, b) =>
                        {
                            Debug.WriteLine(b.Data);
                            iteration = Between(b.Data, "frame=", "fps=");
                            if (!string.IsNullOrEmpty(iteration))
                            {
                                if (intList.Any() && int.Parse(iteration) < intList.Max())
                                {
                                    //Get Max Iteration From Gif Read
                                    intMax = double.Parse(intList.Max().ToString());
                                    //Clear List Wwen Max Iteration From Gif Found
                                    intList.Clear();
                                }
                                if (intList.Any())
                                {
                                    intList.Add(int.Parse(iteration));
                                }
                                else
                                {
                                    //Fill Progress bar only for 90% then wait File Save
                                    progressBarPercent = (int.Parse(iteration) / intMax) * 90;
                                }
                            }
                            bw.ReportProgress(progressBarPercentInt = (int)progressBarPercent);
                        });

                        proc.Start();
                        proc.BeginErrorReadLine();
                        proc.WaitForExit();

                        //Report Progress After File Has Been Saved
                        bw.ReportProgress(100);

                        //File.Delete($"{Path.ChangeExtension(filePath, null)}.webm");
                    };
                    bw.ProgressChanged += delegate (object bwsender, ProgressChangedEventArgs pwe)
                    {
                        progressBar2.Maximum = 100;
                        if (pwe.ProgressPercentage != -1)
                        {
                            //Sync Thread
                            Invoke(new Action(() => { progressBar2.Value = pwe.ProgressPercentage; }));
                        }
                    };
                    bw.RunWorkerCompleted += delegate
                    {
                        //Delete Old File
                        if (File.Exists(path) && checkBox6.Checked)
                        {
                            File.Delete(path);
                            convertSucess = true;
                            gifConvertPathSequence.Remove(path);
                        }
                        convertSucess = true;
                        gifConvertPathSequence.Remove(path);
                        //Wait 100% Progress Bar ANimation (Just For Better Visualization)
                        Thread.Sleep(1000);
                        //Recursive Call 
                        GifToWebmConverter();
                    };
                    bw.RunWorkerAsync();
                }
            }
            return convertSucess;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!File.Exists(DatabaseFile))
            {
                SQLiteConnection.CreateFile(DatabaseFile);

                using (var connection = new SQLiteConnection($"Data Source={DatabaseFile}"))
                {
                    // Create a database command
                    using (var command = new SQLiteCommand(connection))
                    {
                        connection.Open();

                        // Create the table
                        command.CommandText = CreateTableQuery;
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
            }



            //SQLiteConnection dbConnection = new SQLiteConnection("Data Source=Rule34ImagesDB.db");

            //dbConnection.Open();

            //SQLiteCommand command = new SQLiteCommand(CreateTableQuery, dbConnection);

            //command.ExecuteNonQuery();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            DataColumn column;
            DataRow row;

            //Primary Key
            column = new DataColumn();
            column.DataType = Type.GetType("System.Int64");
            column.ColumnName = "PK";
            dt.Columns.Add(column);

            //ArtistName
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "ArtistName";
            dt.Columns.Add(column);

            //ImageNumber
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "ImageNumber";
            dt.Columns.Add(column);

            //CreatedOn
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "CreatedOn";
            dt.Columns.Add(column);

            //ImageTags
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "ImageTags";
            dt.Columns.Add(column);

            // Create new DataRow objects and add to DataTable.
            foreach (var artist in LoadArtists())
            {
                row = dt.NewRow();
                row["PK"] = artist.PrimaryKey;
                row["ArtistName"] = artist.ArtistName;
                row["ImageNumber"] = artist.ImageNumber;
                row["CreatedOn"] = artist.CreatedOn;
                row["ImageTags"] = artist.ImageTags;
                dt.Rows.Add(row);
            }
            // Create a DataView using the DataTable.
            dataGridView1.DataSource = dt;

            dataGridView1.Columns["ImageTags"].Width = 300;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            checkBox4.Enabled = false;
            button6.Enabled = false;
            button1.Enabled = false;
            var r = new Random();
            //var logText = "";
            //var gridRowIndex = 0;
            var progressBarPercent = 0.0d;
            var progressBarPercentInt = 0;
            var forEachIteration = 0;

            if (radioButton1.Checked)
            {
                var bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += delegate
                {
                    //Shuffle List if Safe mode || Direct from DB List
                    var artists = checkBox4.Checked ? LoadArtists().Where(imageTag => string.IsNullOrEmpty(imageTag.ImageTags)).OrderBy(image => r.Next()).ToList() : LoadArtists().Where(imageTag => string.IsNullOrEmpty(imageTag.ImageTags)).OrderBy(image => image.ImageNumber).ToList();
                    foreach (var artist in artists)
                    {
                        if (string.IsNullOrEmpty(artist.ImageTags))
                        {
                            var tagUrl = $"https://rule34.xxx/index.php?page=post&s=view&id={artist.ImageNumber}";
                            //Timeout for Safe || Unsafe
                            if (checkBox4.Checked)
                            {
                                Thread.Sleep(r.Next(5000, 10000));
                            }
                            else
                            {
                                //Be Gentle
                                Thread.Sleep(r.Next(500, 1500));
                            }

                            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlWeb().Load(tagUrl);
                            if (document.DocumentNode.SelectNodes("//div[@id='stats']") != null)
                            {
                                artist.ImageTags = GetTags(document);

                                //Write inside LogField and Select Currently Update Grid Row
                                dataGridView1.ClearSelection();

                                progressBarPercent = (forEachIteration / (double)artists.Count) * 100;
                                if (dataGridView1.RowCount > 0)
                                {
                                    var nRowIndex = dataGridView1.Rows.Cast<DataGridViewRow>().Where(imageNumber => imageNumber.Cells["ImageNumber"].Value.ToString().Equals(artist.ImageNumber)).First().Index;
                                    //Write inside LogField

                                    bw.ReportProgress(progressBarPercentInt = (int)progressBarPercent, new Tuple<string, int>($"{Environment.NewLine}{artist.ArtistName}({artist.ImageNumber}) {Environment.NewLine}{artist.ImageTags}", nRowIndex));
                                }
                                else
                                {
                                    bw.ReportProgress(progressBarPercentInt = (int)progressBarPercent, new Tuple<string, int>($"{Environment.NewLine}{artist.ArtistName}({artist.ImageNumber}) {Environment.NewLine}{artist.ImageTags}", -1));
                                }
                                forEachIteration++;

                                if (!string.IsNullOrEmpty(artist.ImageTags))
                                {
                                    AddArtistTags(artist);
                                }
                            }
                        }
                    }
                };
                bw.ProgressChanged += delegate (object bwsender, ProgressChangedEventArgs bwe)
                {
                    var args = (Tuple<string, int>)bwe.UserState;

                    progressBar1.Maximum = 100;
                    if (bwe.ProgressPercentage != -1)
                    {
                        progressBar1.Value = bwe.ProgressPercentage;
                    }
                    TaskbarProgress.SetValue(Handle, progressBar1.Value, progressBar1.Maximum);

                    if (!string.IsNullOrEmpty(args.Item1))
                    {
                        LogField.AppendText(args.Item1);
                    }

                    if (args.Item2 >= 0)
                    {
                        dataGridView1.CurrentCell = dataGridView1.Rows[args.Item2].Cells[0];
                    }
                };
                bw.RunWorkerCompleted += delegate
                {
                    checkBox4.Enabled = true;
                    button6.Enabled = true;
                    button1.Enabled = true;
                };
                bw.RunWorkerAsync();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
            }
        }

        //For Tests
        private void button7_Click(object sender, EventArgs e)
        {
            //gifConvertPathSequence.Add("D:\\Tests\\batartcave\\5854964.gif");
            //gifConvertPathSequence.Add("D:\\Tests\\batartcave\\5844614.gif");
            //gifConvertPathSequence.Add("D:\\Tests\\batartcave\\5839842.gif");
            //GifToWebmConverter("D:\\Tests\\batartcave\\5844614.gif");
            //GifToWebmConverter("D:\\Tests\\batartcave\\5839842.gif");
        }
    }
    public static class TaskbarProgress
    {
        public enum TaskbarStates
        {
            NoProgress = 0,
            Indeterminate = 0x1,
            Normal = 0x2,
            Error = 0x4,
            Paused = 0x8
        }

        [ComImport()]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig]
            void HrInit();
            [PreserveSig]
            void AddTab(IntPtr hwnd);
            [PreserveSig]
            void DeleteTab(IntPtr hwnd);
            [PreserveSig]
            void ActivateTab(IntPtr hwnd);
            [PreserveSig]
            void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig]
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

            // ITaskbarList3
            [PreserveSig]
            void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
            [PreserveSig]
            void SetProgressState(IntPtr hwnd, TaskbarStates state);
        }

        [ComImport()]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance
        {
        }

        private static ITaskbarList3 taskbarInstance = (ITaskbarList3)new TaskbarInstance();
        private static bool taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);

        public static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
        {
            if (taskbarSupported) taskbarInstance.SetProgressState(windowHandle, taskbarState);
        }

        public static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
        {
            if (taskbarSupported) taskbarInstance.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
        }
    }

    public static class ExtensionMethods
    {
        // To support flashing.
        [DllImport("user32.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
        private const uint FLASHW_ALL = 3;

        // Flash continuously until the window comes to the foreground. 
        private const uint FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        /// <summary>
        /// Send form taskbar notification, the Window will flash until get's focus
        /// <remarks>
        /// This method allows to Flash a Window, signifying to the user that some major event occurred within the application that requires their attention. 
        /// </remarks>
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static bool FlashNotification(this Form form)
        {
            IntPtr hWnd = form.Handle;
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            fInfo.uCount = uint.MaxValue;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }
    }

    public class Artist
    {
        public long PrimaryKey { get; set; }

        public string ArtistName { get; set; }

        public string ImageNumber { get; set; }

        [DefaultValue("")]
        public string CreatedOn { get; set; }

        [DefaultValue("")]
        public string ImageTags { get; set; }
    }

    public enum PathType
    {
        All = 0,
        ImagePath = 1,
        DBPath = 2
    }
}
