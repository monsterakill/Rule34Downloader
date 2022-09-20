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
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Activities;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace Rule34Downloader
{
    public partial class Form1 : Form
    {
        BackgroundWorker Worker;
        string[] duplicateFiles = new string[] { };
        List<string> duplicatesNames = new List<string>();
        List<string> gifConvertPathSequence = new List<string>();
        bool convertSucess = true;
        List<string> downloadArtists = new List<string>();

        private string ConnectionString = string.Empty;
        private string ImageFolderPath = string.Empty;
        private string FFMPEGEXEPath = string.Empty;
        private byte[] OriginalFFMPEGEXEHash = new byte[] { 0x1f, 0x20, 0x16, 0xf3, 0x80, 0x1b, 0xdd, 0x36, 0xd3, 0x66, 0xb0, 0xcd, 0x69, 0xde, 0x88, 0xb9 };
        private readonly List<Artist> artistsLocal = new List<Artist>();
        private readonly List<Artist> artistsDB = new List<Artist>();
        private const string CreateTableQuery = @"CREATE TABLE ""Images"" (""PK"" INTEGER NOT NULL UNIQUE, ""ArtistName"" TEXT, ""ImageNumber"" TEXT, ""CreatedOn""	TEXT, ""ImageTags""	TEXT, ""Source"" TEXT, ""Score"" INTEGER, ""UpdatedOn"" TEXT, PRIMARY KEY(""PK"" AUTOINCREMENT))";
        private const string DatabaseFile = "Rule34ImagesDB.db";
        private List<string> artistListForFilterOriginal = new List<string>();

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

            InitializeWebView();
        }

        //private async void Form1_Load_1(object sender, EventArgs e)
        //{
        //    //webView21.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        //    await InitializeAsync();
        //    //webView21.NavigateToString("https://speed.cd");
        //}

        //private async Task InitializeAsync()
        //{
        //    Debug.WriteLine("InitializeAsync");
        //    await webView21.EnsureCoreWebView2Async(null);
        //    Debug.WriteLine("WebView2 Runtime version: " + webView21.CoreWebView2.Environment.BrowserVersionString);
        //}

        private async void InitializeWebView()
        {
            await browser.EnsureCoreWebView2Async(null);
            //var webview2Environment = await CoreWebView2Environment.CreateAsync(string.Empty, null, null);
            //var webview2Controller = await webview2Environment.CreateCoreWebView2ControllerAsync(this.Handle);
            //var webView = webView21.CoreWebView2;
            //var bw = new BackgroundWorker();
            //bw.WorkerReportsProgress = true;
            //bw.DoWork += delegate
            //{

            //};
            //bw.ProgressChanged += delegate (object bwsender, ProgressChangedEventArgs pwe)
            //{

            //};
            //bw.RunWorkerCompleted += delegate
            //{

            //};
            //bw.RunWorkerAsync();
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
            DownloadArtists();
        }

        private void DownloadArtists()
        {
            var breakout = false;
            //Prepare Data For The First Time for Multiple Artists Download
            if (!textBox1.Text.ToLower().Contains(';') && !string.IsNullOrEmpty(textBox1.Text.ToLower())) { Invoke(new Action(() => { textBox1.Text = $"{textBox1.Text};"; })); };
            downloadArtists = textBox1.Text.ToLower().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (downloadArtists.Any())
            {
                foreach (var downloadArtistName in downloadArtists)
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
                        finalURL = RemoveArtistsFromSearch($"https://rule34.xxx/index.php?page=post&s=list&tags={downloadArtistName}", downloadArtistName);
                    }
                    else
                    {
                        finalURL = $"https://rule34.xxx/index.php?page=post&s=list&tags={downloadArtistName}";
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

                    CheckArtistDirectory(downloadArtistName);

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
                                Thread.Sleep(10000);
                                doc = web.Load(PagingURL);
                                thumbs = doc.DocumentNode.SelectNodes("//span[@class='thumb']");
                                if (thumbs != null) break;
                                
                            }
                            //If Artist not Found Continue download others
                            if (thumbs == null) continue;
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

                            //Worker.ReportProgress(progressBarPercentInt);

                            progressBarValue++;

                            var r = new Random();
                            //Timeout
                            Thread.Sleep(r.Next(5000, 10000));

                            linktoVideo = video.OuterHtml.Substring(video.OuterHtml.IndexOf("index.php")).Substring(0, video.OuterHtml.Substring(video.OuterHtml.IndexOf("index.php")).IndexOf("\""));

                            var result = DownloadContentFromLinkOrId(linktoVideo, progressBarValue, progressBarMaximumValue, downloadArtistName);

                            if(result == ReturnType.Continue)
                            {
                                continue;
                            }
                            else if(result == ReturnType.Break)
                            {
                                breakout = true;
                                break;
                            }

                            Worker.ReportProgress(ProgressBarPercent(progressBarValue, (double)progressBarMaximumValue));
                        }
                        //Check Images after download

                        CheckImagesAfterDownload($"{ImageFolderPath}\\{downloadArtistName}");

                        if (breakout) { breakout = false; break; };
                    }
                    //!!! TO DO. CHECK FILE is not 0 bytes in folder then REDOWNLOAD IT
                    Invoke(new Action(() => { textBox1.Text = textBox1.Text.Replace($"{downloadArtistName};", ""); }));
                }
            }
            else
            {
                return;
            }
            DownloadArtists();
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

        public string RemoveArtistsFromSearch(string url, string artistName)
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
                if (removeSymbolsFromArtistNames != artistName)
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
                return RemoveArtistsFromSearch(searchArtist, artistName);
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

        public bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
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
                        if(node.InnerText != "?")
                        {
                            tags.Add(WebUtility.HtmlDecode(node.InnerText.Replace(' ', '_')));
                        }  
                    }
                }
                return string.Join("|", tags.Distinct().ToList());
            }
            catch
            {
                return string.Empty;
            }
        }

        public int GetScore(HtmlAgilityPack.HtmlDocument doc, string imageId)
        {
            var score = 0;
            try
            {
                if (int.TryParse(doc.DocumentNode.SelectNodes($"//span[@id='psc{imageId}']").FirstOrDefault().InnerText, out score))
                {
                    return score;
                }
                else
                {
                    return score;
                }
            }
            catch
            {
                return 0;
            }
        }

        public bool CheckForDuplicate(string fileName, string artistName)
        {
            if (duplicateFiles.Any())
            {
                return duplicatesNames.Contains(fileName);
            }
            else
            {
                duplicateFiles = Directory.GetFiles($"{ImageFolderPath}\\{artistName}\\");
                foreach (var dupl in duplicateFiles)
                {
                    duplicatesNames.Add(Path.GetFileNameWithoutExtension(dupl));
                }
                return duplicatesNames.Contains(fileName);
            }
        }

        public bool AddArtist(Artist artist)
        {
            //Don't Write In DB
            if (checkBox7.Checked) return false;

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                if (CheckForDuplicateFromDB(artist))
                {
                    Worker.ReportProgress(-1, $"This Image Already Found in DB: {artist.ArtistName} - {artist.ImageNumber}" + Environment.NewLine);
                    return false;
                }

                var queryString = "insert into Images(ArtistName, ImageNumber, CreatedOn, ImageTags, Source, Score)" +
                                    " values(@artistName, @imageNumber, @createdOn, @imageTags, @source, @score);";

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(queryString, connection);
                    command.Parameters.AddWithValue("@artistName", artist.ArtistName);
                    command.Parameters.AddWithValue("@imageNumber", artist.ImageNumber);
                    command.Parameters.AddWithValue("@createdOn", artist.CreatedOn);
                    command.Parameters.AddWithValue("@imageTags", artist.ImageTags);
                    command.Parameters.AddWithValue("@source", artist.Source);
                    command.Parameters.AddWithValue("@score", artist.Score);

                    command.ExecuteScalar();
                }
                return true;
            }
            return false;
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

        public void UpdateArtistTagsAndScore(Artist artist)
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                var queryString = "UPDATE Images SET ImageTags = @imageTags, Score = @score, UpdatedOn = @updatedOn WHERE ImageNumber = @imageNumber;";

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(queryString, connection);
                    command.Parameters.AddWithValue("@imageTags", artist.ImageTags);
                    command.Parameters.AddWithValue("@imageNumber", artist.ImageNumber);
                    command.Parameters.AddWithValue("@score", artist.Score);
                    command.Parameters.AddWithValue("@updatedOn", artist.UpdatedOn);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Artist> LoadArtists(bool distinct = false)
        {
            var artistsList = new List<Artist>();
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                string stringSql;
                if (distinct)
                {
                    stringSql = "SELECT DISTINCT ArtistName, Source FROM Images";
                }
                else
                {
                    stringSql = "SELECT * FROM Images";
                }
                

                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SQLiteCommand(stringSql, connection);

                    using (SQLiteDataReader sqlReader = command.ExecuteReader())
                    {
                        while (sqlReader.Read())
                        {
                            long primaryKey = ColumnExists(sqlReader, "PK") ? (long)sqlReader["PK"] : 0;
                            string artistName = ColumnExists(sqlReader, "ArtistName") ? (string)sqlReader["ArtistName"] : "";
                            string imageNumber = ColumnExists(sqlReader, "ImageNumber") ? (string)sqlReader["ImageNumber"] : "";
                            string createdOn = ColumnExists(sqlReader, "CreatedOn") ? sqlReader["CreatedOn"] as string : "";
                            string imageTags = ColumnExists(sqlReader, "ImageTags") ? sqlReader["ImageTags"] as string : "";
                            string source = ColumnExists(sqlReader, "Source") ? sqlReader["Source"] as string : "";
                            long score = ColumnExists(sqlReader, "Score") ? sqlReader["Score"] == DBNull.Value ? 0 : (long)sqlReader["Score"] : 0;
                            string updatedOn = ColumnExists(sqlReader, "UpdatedOn") ? sqlReader["UpdatedOn"] as string : "";

                            Artist artist = new Artist()
                            {
                                PrimaryKey = primaryKey,
                                ArtistName = artistName,
                                ImageNumber = imageNumber,
                                CreatedOn = createdOn,
                                ImageTags = imageTags,
                                Source = source,
                                Score = score,
                                UpdatedOn = updatedOn
                            };
                            artistsList.Add(artist);
                        }
                    }
                }
            }
            return artistsList;
        }

        public bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
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

        public bool ImageIsValid(string path)
        {
            try
            {
                using (var bmp = new Bitmap(path))
                {
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int ProgressBarPercent(int progressBarValue, double progressBarMaximumValue)
        {
            var progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;

            return (int)progressBarPercent;
        }

        public ReturnType DownloadContentFromLinkOrId(string linkOrId, int progressBarValue = 0, double progressBarMaximumValue = 0.0, string downloadArtistName = "", bool folderFilesRedownload = false)
        {
            var web = new HtmlAgilityPack.HtmlWeb();
            string url = $"https://rule34.xxx/{linkOrId}";
            if (long.TryParse(linkOrId, out long result))
            {
                url = $"https://rule34.xxx/index.php?page=post&s=view&id={linkOrId}";
            }

            HtmlAgilityPack.HtmlDocument document = web.Load(url);

            if (string.IsNullOrEmpty(downloadArtistName))
            {
                var artistListWithImageCount = new Dictionary<string, int>();
                var nodes = document.DocumentNode.SelectNodes("//li[@class='tag-type-artist tag']");
                foreach (var artists in nodes)
                {
                    var artistWithImageCount = Regex.Replace(artists.InnerText, @"\n|\?", " ").Trim().Split(' ');
                    artistListWithImageCount.Add(artistWithImageCount[0], int.Parse(artistWithImageCount[1]));


                }
                //Set artist with Max Image Count
                //!!! Choose Menu
                var artistWithMaxImages = artistListWithImageCount.Values.Max();
                downloadArtistName = artistListWithImageCount.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

                CheckArtistDirectory(downloadArtistName);
            }
            if (folderFilesRedownload)
            {
                var r = new Random();
                //Timeout
                Thread.Sleep(r.Next(5000, 10000));
                if (document.DocumentNode.SelectNodes("//img[@id='image']") != null)
                {
                    var urlSimple = document.DocumentNode.SelectSingleNode("//div[@class='link-list']/ul/li[3]/a").Attributes[0].Value;
                    if (urlSimple.Contains("saucenao") || urlSimple.Contains("index.php"))
                    {
                        urlSimple = document.DocumentNode.SelectSingleNode("//div[@class='link-list']/ul/li[2]/a").Attributes[0].Value;
                    }
                    var fileNameReg = urlSimple.Substring(urlSimple.LastIndexOf('?') + 1);
                    var format = Between(urlSimple, ".", "?", true);

                    Worker.ReportProgress(-1, $"Download Image: {fileNameReg}.{format}" + Environment.NewLine);
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                        wc.QueryString.Add("fileFormat", format);
                        wc.QueryString.Add("gifConvert", ConvertGifs.Checked.ToString());
                        wc.QueryString.Add("filePath", $"{ImageFolderPath}\\{downloadArtistName}\\{fileNameReg}.{format}");
                        wc.DownloadFileAsync(new Uri(urlSimple), $"{ImageFolderPath}\\{downloadArtistName}\\{fileNameReg}.{format}");
                    }
                    return ReturnType.True;
                }
            }

            //Video Found
            if (document.DocumentNode.SelectNodes("//source[@type='video/mp4']") != null)
            {
                string metascore = document.DocumentNode.SelectNodes("//source[@type='video/mp4']")[0].OuterHtml;

                string urlSimple = metascore.Substring(metascore.IndexOf("https")).Substring(0, metascore.Substring(metascore.IndexOf("https")).IndexOf("mp4") + 3);

                var fileName = metascore.Substring(metascore.IndexOf("?") + 1).Substring(0, metascore.Substring(metascore.IndexOf("?") + 1).IndexOf("t") + 1);

                int fileNameReg = int.Parse(Regex.Match(fileName, @"\d+").Value);

                if (checkBox3.Checked)
                {
                    if (CheckForDuplicate(fileNameReg.ToString(), downloadArtistName))
                    {
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue), $"File Already Found With Id : {fileNameReg}" + Environment.NewLine);
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue));

                        if (checkBox2.Checked) { return ReturnType.Break; };
                        return ReturnType.Continue;
                    }
                }
                else
                {
                    //!!!Check duplicate from db and directory can be improved by checking last file by artist and then download only those who is missing and then Recalculate Progress bar for those.
                    if (CheckForDuplicate(fileNameReg.ToString(), downloadArtistName) || CheckForDuplicateFromDB(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString() }))
                    {
                        if (!CheckForDuplicateFromDB(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString() }))
                        {
                            //Write Image in DB If not Found in DB but found in Local Folder
                            AddArtist(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document), Source = new Uri(url).Host, Score = GetScore(document, fileNameReg.ToString()) });
                        }
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue), $"File Already Found With Id : {fileNameReg}" + Environment.NewLine);
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue));

                        if (checkBox2.Checked) { return ReturnType.Break; };
                        return ReturnType.Continue;
                    }
                }

                Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue), $"Download Video: {fileNameReg}.mp4" + Environment.NewLine);

                using (WebClient wc = new WebClient())
                {
                    //wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileAsync(
                        // Param1 = Link of file
                        new System.Uri(urlSimple),
                        // Param2 = Path to save
                        $"{ImageFolderPath}\\{downloadArtistName}\\{fileNameReg}.mp4"
                    );
                }

                AddArtist(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document), Source = new Uri(url).Host, Score = GetScore(document, fileNameReg.ToString()) });
                return ReturnType.True;
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
                    if (CheckForDuplicate(fileNameReg, downloadArtistName))
                    {
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue), $"File Already Found With Id : {fileNameReg}" + Environment.NewLine);
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue));

                        if (checkBox2.Checked) { return ReturnType.Break; };
                        return ReturnType.Continue;
                    }
                }
                else
                {
                    if (CheckForDuplicate(fileNameReg, downloadArtistName) || CheckForDuplicateFromDB(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString() }))
                    {
                        if (!CheckForDuplicateFromDB(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString() }))
                        {
                            //Write Image in DB If not Found in DB but found in Local Folder
                            AddArtist(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document), Source = new Uri(url).Host, Score = GetScore(document, fileNameReg.ToString()) });
                        }
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue), $"File Already Found With Id : {fileNameReg}" + Environment.NewLine);
                        Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue));

                        if (checkBox2.Checked) { return ReturnType.Break; };
                        return ReturnType.Continue;
                    }
                }

                Worker.ReportProgress(ProgressBarPercent(progressBarValue, progressBarMaximumValue), $"Download Image: {fileNameReg}.{format}" + Environment.NewLine);
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                    wc.QueryString.Add("fileFormat", format);
                    wc.QueryString.Add("gifConvert", ConvertGifs.Checked.ToString());
                    wc.QueryString.Add("filePath", $"{ImageFolderPath}\\{downloadArtistName}\\{fileNameReg}.{format}");
                    wc.DownloadFileAsync(new Uri(urlSimple), $"{ImageFolderPath}\\{downloadArtistName}\\{fileNameReg}.{format}");
                }

                AddArtist(new Artist { ArtistName = downloadArtistName, ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = GetTags(document), Source = new Uri(url).Host, Score = GetScore(document, fileNameReg.ToString()) });
                return ReturnType.True;
            }
            return ReturnType.False;
        }

        private void CheckArtistDirectory(string downloadArtistName)
        {
            if (!Directory.Exists($"{ImageFolderPath}\\{downloadArtistName}"))
            {
                Directory.CreateDirectory($"{ImageFolderPath}\\{downloadArtistName}");
                Worker.ReportProgress(-1, $"Creating Directory: {ImageFolderPath}\\{downloadArtistName}" + Environment.NewLine);
            }
        }

        private void CheckImagesAfterDownload(string artistPath)
        {
            if (Directory.Exists(artistPath))
            {
                var filters = new string[] { "jpg", "jpeg", "png" };
                var files = GetFilesFrom(artistPath, filters, false);
                foreach (var file in files)
                {
                    Worker.ReportProgress(-1, $"Checking file {file}" + Environment.NewLine);
                    var imageValid = ImageIsValid(file.FullName);
                    if (!imageValid)
                    {
                        Worker.ReportProgress(-1, $"File {file.FullName} is CORRUPTED" + Environment.NewLine);
                        DownloadContentFromLinkOrId(Path.GetFileNameWithoutExtension(file.FullName), downloadArtistName: file.Directory.Name, folderFilesRedownload:true);
                    }
                    else
                    {
                        Worker.ReportProgress(-1, $"File {file.FullName} is VALID" + Environment.NewLine);
                    }
                }
            }
            else
            {
                Worker.ReportProgress(-1, $"{artistPath} is not a valid file or directory" + Environment.NewLine);
            }
        }

        public static List<FileInfo> GetFilesFrom(string searchFolder, string[] filters, bool isRecursive)
        {
            List<FileInfo> filesFound = new List<FileInfo>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                var fileFilter = Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), searchOption);
                foreach(var file in fileFilter)
                {
                    filesFound.Add(new FileInfo(file));
                }
            }
            return filesFound;
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
            if (!string.IsNullOrEmpty(Properties.Settings.Default.FFMPEGPath) && pathType == PathType.All)
            {
                FFMPEGPath.Text = Properties.Settings.Default.FFMPEGPath;
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

            if (pathType == PathType.FFMPEGPath || pathType == PathType.All)
            {
                if (string.IsNullOrEmpty(FFMPEGPath.Text))
                {
                    pictureBox3.Image = pictureBox3.ErrorImage;
                }
                else
                {
                    pictureBox3.Image = pictureBox3.InitialImage;
                    FFMPEGEXEPath = FFMPEGPath.Text;
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
                //CHeck is COnverter is Busy
                if (convertSucess)
                {
                    convertSucess = false;

                    var command = $"{ffmpegPath} -i {path} -c:v libvpx-vp9 -b:v 0 -crf 30 -an -f webm -passlogfile {Directory.GetCurrentDirectory()}\\ffmpeg2pass.log";
                    command = $"{command} -pass 1 -y NUL && {command} -pass 2 {Path.ChangeExtension(path, null)}.webm";

                    var bw = new BackgroundWorker();
                    bw.WorkerReportsProgress = true;
                    bw.DoWork += delegate
                    {
                        Invoke(new Action(() => { gifTotalProgress.Text = gifConvertPathSequence.Count.ToString(); }));
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
                        if (File.Exists(path) && DeleteOriginalGif.Checked)
                        {
                            File.Delete(path);
                        }
                        convertSucess = true;
                        gifConvertPathSequence.Remove(path);
                        //Gif Progress
                        var gProgress = Convert.ToInt32(gifCurrentProgress.Text);
                        gProgress += 1;
                        Invoke(new Action(() => { gifCurrentProgress.Text = gProgress.ToString(); }));
                        if (gifConvertPathSequence.Count == 0)
                        {
                            Invoke(new Action(() => { gifTotalProgress.Text = gifConvertPathSequence.Count.ToString(); }));
                        }
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
            if (!File.Exists($"{ImageFolderPath}\\{DatabaseFile}"))
            {
                SQLiteConnection.CreateFile($"{ImageFolderPath}\\{DatabaseFile}");

                using (var connection = new SQLiteConnection($"Data Source={ImageFolderPath}\\{DatabaseFile}"))
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

                //Get the path of specified file
                DBPath.Text = $"{ImageFolderPath}\\{DatabaseFile}";

                CheckPaths(PathType.DBPath);

                //Save as Default Setting
                Properties.Settings.Default.DBConnectionPath = DBPath.Text;
                Properties.Settings.Default.Save();
            }



            //SQLiteConnection dbConnection = new SQLiteConnection("Data Source=Rule34ImagesDB.db");

            //dbConnection.Open();

            //SQLiteCommand command = new SQLiteCommand(CreateTableQuery, dbConnection);

            //command.ExecuteNonQuery();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            //Clear List
            listBox1.Items.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.Refresh();

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

            //Source
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Source";
            dt.Columns.Add(column);

            //Score
            column = new DataColumn();
            column.DataType = Type.GetType("System.Int64");
            column.ColumnName = "Score";
            dt.Columns.Add(column);

            //UpdatedOn
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "UpdatedOn";
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
                row["Source"] = artist.Source;
                row["Score"] = artist.Score;
                row["UpdatedOn"] = artist.UpdatedOn;
                dt.Rows.Add(row);
            }
            // Create a DataView using the DataTable.
            dataGridView1.DataSource = dt;

            dataGridView1.Columns["ImageTags"].Width = 300;

            //Sort
            var dv = new DataView(dt)
            {
                Sort = "ArtistName ASC"
            };
            
            //Get Artists Distinct
            var distinctArtists = dv.ToTable(true, "ArtistName");

            //Fill listBox
            foreach (DataRow r in distinctArtists.Rows)
            {
                listBox1.Items.Add(r["ArtistName"]);
            }
            artistListForFilterOriginal = listBox1.Items.Cast<string>().ToList();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //!!! Check if Image has Been deleted from SIte Set DELETED Tag in DB
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
                            else
                            {
                                //!!!DELETED IMAGES
                                ////Write inside LogField and Select Currently Update Grid Row
                                //dataGridView1.ClearSelection();

                                //progressBarPercent = (forEachIteration / (double)artists.Count) * 100;
                                //if (dataGridView1.RowCount > 0)
                                //{
                                //    var nRowIndex = dataGridView1.Rows.Cast<DataGridViewRow>().Where(imageNumber => imageNumber.Cells["ImageNumber"].Value.ToString().Equals(artist.ImageNumber)).First().Index;
                                //    //Write inside LogField

                                //    bw.ReportProgress(progressBarPercentInt = (int)progressBarPercent, new Tuple<string, int>($"{Environment.NewLine}{artist.ArtistName}({artist.ImageNumber}) {Environment.NewLine}{artist.ImageTags}", nRowIndex));
                                //}
                                //else
                                //{
                                //    bw.ReportProgress(progressBarPercentInt = (int)progressBarPercent, new Tuple<string, int>($"{Environment.NewLine}{artist.ArtistName}({artist.ImageNumber}) {Environment.NewLine}{artist.ImageTags}", -1));
                                //}
                                //forEachIteration++;

                                //if (!string.IsNullOrEmpty(artist.ImageTags))
                                //{
                                //    AddArtistTags(artist);
                                //}
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

            if (radioButton2.Checked)
            {
                var bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += delegate
                {
                    //Shuffle List if Safe mode || Direct from DB List
                    var artists = checkBox4.Checked ? LoadArtists().Where(imageTag => string.IsNullOrEmpty(imageTag.UpdatedOn) && imageTag.Source == "rule34.xxx").OrderBy(image => r.Next()).ToList() : LoadArtists().Where(imageTag => string.IsNullOrEmpty(imageTag.UpdatedOn) && imageTag.Source == "rule34.xxx").OrderBy(image => image.ImageNumber).ToList();
                    foreach (var artist in artists)
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
                            var imageTags = GetTags(document);
                            var score = GetScore(document, artist.ImageNumber);

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

                            if ((!string.IsNullOrEmpty(imageTags) && artist.ImageTags != imageTags) || (score > 0 && artist.Score != score))
                            {
                                artist.ImageTags = imageTags;
                                artist.Score = score;
                                artist.UpdatedOn = DateTime.Now.ToShortDateString();
                                UpdateArtistTagsAndScore(artist);
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
        private async void button7_Click(object sender, EventArgs e)
        {
            //gifConvertPathSequence.Add("D:\\Tests\\batartcave\\5854964.gif");
            //gifConvertPathSequence.Add("D:\\Tests\\batartcave\\5844614.gif");
            //gifConvertPathSequence.Add("D:\\Tests\\batartcave\\5839842.gif");
            //GifToWebmConverter("D:\\Tests\\batartcave\\5844614.gif");
            //GifToWebmConverter("D:\\Tests\\batartcave\\5839842.gif");
            //DBPath.Text = string.Empty;


            // You can also use the Source property
            //wvc.NavigateToString("https://speed.cd");
            //webView21.NavigateToString("https://speed.cd");
            //webView21.CoreWebView2.Navigate("https://speed.cd");
            //string html = await webView21.ExecuteScriptAsync("(function() {  document.querySelectorAll('input[type=text]')[0].value = 'MyUsername@gmail.com';document.querySelectorAll('input[type=password]')[0].value = 'MyPassword'; })()");

            //CHeck if already logged ? 
            //var test = browser.CreationProperties.UserDataFolder;


            //LAST PAGE 
            //var test = document.evaluate("//a[text()='Last']", document, null, XPathResult.ANY_TYPE, null).iterateNext();
            //OR
            //var test = document.querySelectorAll('a');
            //var test2 = [...test].filter(e => e.innerText == "Last")[0];
            //console.log(test2);

            var loggedInCheck = await browser.ExecuteScriptAsync($"document.querySelectorAll('section[id=Logged_in_as_{UserName.Text}head]')[0];");
            if(loggedInCheck != "null")
            {
                //browser.CoreWebView2.Navigate(urlSimple);
                //Thread.Sleep(new Random().Next(5000, 10000));

                var pageCountElement = await browser.ExecuteScriptAsync($"var elem = document.querySelectorAll('a'); [...elem].filter(e => e.innerText == 'Last')[0].pathname;");
                var pageCount = int.Parse(new string(pageCountElement.Substring(pageCountElement.LastIndexOf('/')).Where(char.IsDigit).ToArray()));

                //if(pageCount > 1)
                //{
                //    var url = await browser.ExecuteScriptAsync("document.location.href;");
                //    var urlFixed = url.Replace("\"", "");
                //    urlFixed = urlFixed.Substring(0, urlFixed.LastIndexOf('/'));
                //    browser.CoreWebView2.Navigate($"{urlFixed}/{i}");
                //}

                //for(int i = 1; i <= pageCount; i++)
                //{
                //    if(i > 1)
                //    {
                //        var url = await browser.ExecuteScriptAsync("document.location.href;");
                //        var urlFixed = url.Replace("\"", "");
                //        urlFixed = urlFixed.Substring(0, urlFixed.LastIndexOf('/'));
                //        browser.CoreWebView2.Navigate($"{urlFixed}/{i}");
                //    }
                //    Thread.Sleep(new Random().Next(5000, 10000));


                //}

                var thumbs = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb').length;");
                var pagePostsInfo = new List<PostInfo>();
                //Data Prepare

                for (int y = 0; y < int.Parse(thumbs); y++)
                {
                    var artistName = await browser.ExecuteScriptAsync($"document.querySelectorAll('.tagit-label')[0].innerText;");
                    var postId = JsonConvert.DeserializeObject<string>(await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{y}].dataset.postId;"));
                    var fileFormat = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{y}].dataset.mime;");
                    var tags = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{y}].dataset.tags;");
                    var postSource = await browser.ExecuteScriptAsync($"document.querySelectorAll('#thumb_{postId}')[{0}].src;");
                    pagePostsInfo.Add(new PostInfo
                    {
                        ArtistName = JsonConvert.DeserializeObject<string>(artistName).ToLower(),
                        Id = postId,
                        FileFormat = JsonConvert.DeserializeObject<string>(fileFormat).Substring(fileFormat.LastIndexOf('/')),
                        Tags = JsonConvert.DeserializeObject<string>(tags).Replace(' ', '|'),
                        Source = JsonConvert.DeserializeObject<string>(postSource)
                    });
                    pagePostsInfo = pagePostsInfo.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                }

                foreach (var post in pagePostsInfo)
                {
                    //Change default browser Save Folder
                    var text = File.ReadAllText("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences");
                    var parseResponse = JToken.Parse(text);
                    var defDirectory = parseResponse.SelectToken("$.download.default_directory");
                    if (defDirectory != null && !defDirectory.Contains($"{pagePostsInfo[0].ArtistName}"))
                    {
                        defDirectory.Replace($"{ImageFolderPath}\\{pagePostsInfo[0].ArtistName}");
                        FileInfo fileInfo = new FileInfo("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences")
                        {
                            IsReadOnly = false
                        };
                        File.WriteAllText("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences", parseResponse.ToString());
                        fileInfo.IsReadOnly = true;
                    }

                    var imageId = Between(post.Source, "_thumbs/", "/thumb");
                    var urlSimple = $"https://rule34hentai.net/_images/{imageId}/{post.Id}";

                    //Timeout
                    //Thread.Sleep(new Random().Next(3000, 4000));
                    //var urlSimple = $"https://rule34hentai.net/post/view/{post.Id}";

                    //browser.CoreWebView2.DownloadStarting += OnDownloadStarting;
                    browser.CoreWebView2.Navigate(urlSimple);

                    //browser.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;


                    //browser.CoreWebView2.NavigationCompleted += async delegate (object obj, CoreWebView2NavigationCompletedEventArgs eventArgs)
                    //{
                    //    if (eventArgs.IsSuccess)
                    //    {
                    //        var testfunc = await browser.ExecuteScriptAsync($"(function () {{ " +
                    //            $"const link = document.createElement('a');" +
                    //            $"link.href = '{urlSimple}';" +
                    //            $"link.download = '{post.Id}.{post.FileFormat}';" +
                    //            $"link.click();" +
                    //            $"}})()");
                    //    }
                    //    //Timeout
                    //    Thread.Sleep(new Random().Next(5000, 10000));

                    //};

                    //Timeout
                    Thread.Sleep(new Random().Next(5000, 10000));

                    if (CheckForDuplicate(post.Id, post.ArtistName))
                    {
                        Worker.ReportProgress(-1, $"File Already Found With Id : {post.Id}" + Environment.NewLine);
                        continue;
                    }

                    var testfunc = await browser.ExecuteScriptAsync($"(function () {{ " +
                        $"const link = document.createElement('a');" +
                        $"link.href = '{urlSimple}';" +
                        $"link.download = '{post.Id}.{post.FileFormat}';" +
                        $"link.click();" +
                        $"}})()");

                    AddArtist(new Artist { ArtistName = post.ArtistName, ImageNumber = post.Id, CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = post.Tags, Source = new Uri(urlSimple).Host });
                    //    //function download(url, filename)
                    //    //{
                    //    //    fetch(url)
                    //    //        .then(response => response.blob())
                    //    //        .then(blob =>
                    //    //        {
                    //    //            const link = document.createElement("a");
                    //    //            link.href = URL.createObjectURL(blob);
                    //    //            link.download = filename;
                    //    //            link.click();
                    //    //        })
                    //    //    .catch (console.error);
                    //    //}
                    //    //download(urlSimple, "{postId}.{fileFormat}")
                    //function download(url, filename)
                    //{
                    //    const link = document.createElement("a");
                    //    link.href = url;
                    //    link.download = filename;
                    //    link.click();
                    //}
                    //download('rule34hentai.net/_images/ac57d67b95e6cda0b55b237a2612645f/413669', '413669.mp4')
                    //Timeout
                    //Thread.Sleep(new Random().Next(3000, 4000));

                }



                //for (int i = 0; i <= int.Parse("0"); i++)
                //{
                //    var artistName = await browser.ExecuteScriptAsync($"document.querySelectorAll('.tagit-label')[0].innerText;");
                //    artistName = JsonConvert.DeserializeObject<string>(artistName).ToLower();

                //    var postId = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{i}].dataset.postId;");
                //    postId = JsonConvert.DeserializeObject<string>(postId);

                //    var fileFormat = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{i}].dataset.mime;");
                //    fileFormat = JsonConvert.DeserializeObject<string>(fileFormat);
                //    fileFormat = fileFormat.Substring(fileFormat.LastIndexOf('/') + 1);

                //    var tags = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{i}].dataset.tags;");
                //    tags = JsonConvert.DeserializeObject<string>(tags);

                //    var postSource = await browser.ExecuteScriptAsync($"document.querySelectorAll('#thumb_{postId}')[{i}].src;");
                //    postSource = JsonConvert.DeserializeObject<string>(postSource);

                //    var imageId =  Between(postSource, "_thumbs/", "/thumb");

                //    //var urlSimple = $"https://rule34hentai.net/_images/{imageId}/{postId}";

                //    var urlSimple = $"https://rule34hentai.net/post/view/{postId}";

                //    //browser.EnsureCoreWebView2Async().AsTask().ContinueWith(async (task) =>
                //    //{
                //    //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                //    //    () =>
                //    //    {
                //    //        wv2.CoreWebView2.DownloadStarting += OnDownloadStarting;
                //    //        wv2.CoreWebView2.Navigate("http://demo.borland.com/testsite/downloads/downloadfile.php?file=dotNetFx40_Full_x86_x64.exe&cd=attachment+filename");
                //    //    });
                //    //});

                //    browser.CoreWebView2.DownloadStarting += OnDownloadStarting;
                //    browser.CoreWebView2.Navigate(urlSimple);

                //    //function download(url = "https://rule34hentai.net/_images/ac57d67b95e6cda0b55b237a2612645f/413669", filename = "413669.webm")
                //    //{
                //    //    fetch(url)
                //    //        .then(response => response.blob())
                //    //        .then(blob =>
                //    //        {
                //    //            const link = document.createElement("a");
                //    //            link.href = URL.createObjectURL(blob);
                //    //            link.download = filename;
                //    //            link.click();
                //    //        })
                //    //    .catch (console.error);
                //    //    }
                //    //    download(urlSimple, "{postId}.{fileFormat}")

                //    var text = File.ReadAllText("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences");
                //    //var sponsors = JsonConvert.DeserializeObject<object>(text);
                //    var parseResponse = JToken.Parse(text);
                //    var defDirectory = parseResponse.SelectToken("$.download.default_directory");
                //    if (defDirectory != null)
                //    {
                //        defDirectory.Replace($"{ImageFolderPath}\\{artistName}");
                //        FileInfo fileInfo = new FileInfo("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences")
                //        {
                //            IsReadOnly = false
                //        };
                //        File.WriteAllText("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences", parseResponse.ToString());
                //        fileInfo.IsReadOnly = true;
                //    }

                //    var testfunc = await browser.ExecuteScriptAsync($"document.getElementsByClassName('image_button_set')[0].click();");
                //    //Timeout
                //    Thread.Sleep(new Random().Next(5000, 10000));


                //    if (true)
                //    {
                //        //Rewrite Names in FOlder
                //    }


                //    //function download(url, filename)
                //    //{
                //    //    fetch(url)
                //    //        .then(response => response.blob())
                //    //        .then(blob =>
                //    //        {
                //    //            const link = document.createElement("a");
                //    //            link.href = URL.createObjectURL(blob);
                //    //            link.download = filename;
                //    //            link.click();
                //    //        })
                //    //    .catch (console.error);
                //    //}
                //    //download(urlSimple, "{postId}.{fileFormat}")

                //    //sponsors["download"]["default_directory"] = "";


                //    //D:\Projects\Rule34Downloader\Rule34Downloader\bin\Debug\Rule34Downloader.exe.WebView2\EBWebView\Default\Preferences


                //    //"download":{"default_directory":"D:\\Tests","directory_upgrade":true}
                //    //using (WebClient wc = new WebClient())
                //    //{
                //    //    //wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                //    //    wc.DownloadFileAsync(
                //    //        // Param1 = Link of file
                //    //        new System.Uri(urlSimple),
                //    //        // Param2 = Path to save
                //    //        $"{ImageFolderPath}\\{artistName}TEST\\{postId}.{fileFormat}"
                //    //    );
                //    //}
                //    //Thread.Sleep(5000);
                //}

            }
            else
            {

            }
            //document.querySelectorAll(".thumb")[0].dataset.postId
            //document.querySelectorAll(".thumb")[0].dataset.mime
            //document.querySelectorAll(".thumb")[0].dataset.tags
            //document.querySelectorAll("#thumb_505321")[0].src

            //https://rule34hentai.net/_images/788c504cd91d1a190cc4f22d0bdc4e59/


            //var test2 = await browser.ExecuteScriptAsync($"document.documentElement.outerHTML;");

            //var prepareLoginForm = await browser.ExecuteScriptAsync("(function() {" +
            //    "var loginSelector = document.querySelector('#Loginhead');" +
            //    "document.body.innerHTML = ''; " +
            //    "document.body.appendChild(loginSelector);" +
            //    $"document.querySelectorAll('input[type=text]')[0].value = '{UserName.Text}';" +
            //    $"document.querySelectorAll('input[type=password]')[0].value = '{Password.Text}';" +
            //    "document.querySelectorAll('input[type=submit]')[0].click(); })()");

            //browser.
            //await webView21.ExecuteScriptAsync("(function() {  document.querySelectorAll('input[type=text]')[0] = 'MyUsername@gmail.com';document.querySelectorAll('input[type=password]')[0] = 'MyPassword'; })()");

            //var text = html;
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            
        }

        private void OnDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs args)
        {
            Trace.WriteLine("DownloadStarting");
            var downloadOp = args.DownloadOperation;
            args.ResultFilePath = $"{ImageFolderPath}\\rastafariansfmTEST\\test.webm";
            args.DownloadOperation.StateChanged += (sender2, args2) =>
            {
                var state = downloadOp.State;
                switch (state)
                {
                    case CoreWebView2DownloadState.InProgress:
                        Trace.WriteLine("Download StateChanged: InProgress");
                        break;
                    case CoreWebView2DownloadState.Completed:
                        Trace.WriteLine("Download StateChanged: Completed");
                        break;
                    case CoreWebView2DownloadState.Interrupted:
                        Trace.WriteLine("Download StateChanged: Interrupted, reason: " + downloadOp.InterruptReason);
                        break;
                }
            };
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();

            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                foreach (string str in artistListForFilterOriginal)
                {
                    if (str.ToLower().Contains(SearchBox.Text))
                    {
                        listBox1.Items.Add(str);
                    }
                }
            }
            else
            {
                foreach (string str in artistListForFilterOriginal)
                {
                    listBox1.Items.Add(str);
                }
            }
            listBox1.EndUpdate();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if(textBox1.Text.Length > 0)
                {
                    textBox1.Text += $"{listBox1.Items[index].ToString()};";
                }
                else
                {
                    textBox1.Text = $"{listBox1.Items[index].ToString()};";
                }
            }
        }

        private void FFMPEGPathExplorer_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "d:\\";
                openFileDialog.Filter = "FFMPEG (*.exe)|*.exe";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    byte[] inputHash;
                    using (var md5 = MD5.Create())
                    using (var stream = File.OpenRead(openFileDialog.FileName))
                    {
                        inputHash = md5.ComputeHash(stream);
                    }
                    if (inputHash.SequenceEqual(OriginalFFMPEGEXEHash))
                    {
                        //Get the path of specified file
                        FFMPEGPath.Text = openFileDialog.FileName;

                        CheckPaths(PathType.FFMPEGPath);

                        //Save as Default Setting
                        Properties.Settings.Default.FFMPEGPath = FFMPEGPath.Text;
                        Properties.Settings.Default.Save();

                        //Disable Gif Converter
                        ConvertGifs.Checked = true;
                        DeleteOriginalGif.Checked = true;

                        ConvertGifs.Enabled = true;
                        DeleteOriginalGif.Enabled = true;
                    }
                    else
                    {
                        //Get the path of specified file
                        FFMPEGPath.Text = string.Empty;

                        CheckPaths(PathType.FFMPEGPath);

                        //Disable Gif Converter
                        ConvertGifs.Checked = false;
                        DeleteOriginalGif.Checked = false;

                        ConvertGifs.Enabled = false;
                        DeleteOriginalGif.Enabled = false;

                    }
                }
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private async void browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var defaultPage = "https://rule34hentai.net/";

            var loggedInCheck = await browser.ExecuteScriptAsync($"document.querySelectorAll('section[id=Logged_in_as_{UserName.Text}head]')[0];");
            if (loggedInCheck != "null")
            {
                var pageCountElement = await browser.ExecuteScriptAsync($"var elem = document.querySelectorAll('a'); [...elem].filter(e => e.innerText == 'Last')[0].pathname;");
                var lastPage = await browser.ExecuteScriptAsync($"var elem = document.querySelectorAll('a'); [...elem].filter(e => e.innerText == 'Random')[0].pathname;");
                var pageCount = 0;
                if (pageCountElement == "null" && lastPage == "null")
                {
                    return;
                }
                else if(pageCountElement != "null")
                {
                    pageCount = int.Parse(new string(pageCountElement.Substring(pageCountElement.LastIndexOf('/')).Where(char.IsDigit).ToArray()));
                }
                
                var url = await browser.ExecuteScriptAsync("document.location.href;");
                if (JsonConvert.DeserializeObject<string>(url).ToLower() == defaultPage.ToLower()) return;
                var artistNameFromUrl = url.Split('/')[5];
                var currentPage = int.Parse(new string(url.Substring(url.LastIndexOf('/')).Where(char.IsDigit).ToArray()));
                

                var thumbs = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb').length;");
                var pagePostsInfo = new List<PostInfo>();
                //Data Prepare

                for (int y = 0; y < int.Parse(thumbs); y++)
                {
                    var artistName = await browser.ExecuteScriptAsync($"document.querySelectorAll('.tagit-label')[0].innerText;");
                    var postId = JsonConvert.DeserializeObject<string>(await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{y}].dataset.postId;"));
                    var fileFormat = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{y}].dataset.mime;");
                    var tags = await browser.ExecuteScriptAsync($"document.querySelectorAll('.thumb')[{y}].dataset.tags;");
                    var postSource = await browser.ExecuteScriptAsync($"document.querySelectorAll('#thumb_{postId}')[{0}].src;");

                    if(artistName == "null" && artistNameFromUrl.Length > 1) { artistName = artistNameFromUrl; };
                    //!!!Deser Problem Exception Need to fix
                    pagePostsInfo.Add(new PostInfo
                    {
                        ArtistName = JsonConvert.DeserializeObject<string>(artistName).ToLower(),
                        Id = postId,
                        FileFormat = JsonConvert.DeserializeObject<string>(fileFormat).Substring(fileFormat.LastIndexOf('/')),
                        Tags = JsonConvert.DeserializeObject<string>(tags).Replace(' ', '|'),
                        Source = JsonConvert.DeserializeObject<string>(postSource)
                    });
                    pagePostsInfo = pagePostsInfo.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                }

                foreach (var post in pagePostsInfo)
                {
                    if (!Directory.Exists($"{ImageFolderPath}\\{post.ArtistName}"))
                    {
                        Directory.CreateDirectory($"{ImageFolderPath}\\{post.ArtistName}");
                        Worker.ReportProgress(-1, $"Creating Directory: {ImageFolderPath}\\{post.ArtistName}" + Environment.NewLine);
                    }
                    //Change default browser Save Folder
                    var text = File.ReadAllText("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences");
                    var parseResponse = JToken.Parse(text);
                    var defDirectory = parseResponse.SelectToken("$.download.default_directory");
                    if (defDirectory != null && !defDirectory.Contains($"{pagePostsInfo[0].ArtistName}"))
                    {
                        defDirectory.Replace($"{ImageFolderPath}\\{pagePostsInfo[0].ArtistName}");
                        FileInfo fileInfo = new FileInfo("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences")
                        {
                            IsReadOnly = false
                        };
                        File.WriteAllText("D:\\Projects\\Rule34Downloader\\Rule34Downloader\\bin\\Debug\\Rule34Downloader.exe.WebView2\\EBWebView\\Default\\Preferences", parseResponse.ToString());
                        fileInfo.IsReadOnly = true;
                    }

                    var imageId = Between(post.Source, "_thumbs/", "/thumb");
                    var urlSimple = $"https://rule34hentai.net/_images/{imageId}/{post.Id}";

                    browser.CoreWebView2.Navigate(urlSimple);

                    //Timeout
                    Thread.Sleep(new Random().Next(5000, 10000));

                    if (CheckForDuplicate(post.Id, post.ArtistName))
                    {
                        Worker.ReportProgress(-1, $"File Already Found With Id : {post.Id}" + Environment.NewLine);
                        continue;
                    }
                    var artistAdded = AddArtist(new Artist { ArtistName = post.ArtistName, ImageNumber = post.Id, CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = post.Tags, Source = new Uri(urlSimple).Host });

                    if (!artistAdded)
                    {
                        browser.CoreWebView2.Navigate(defaultPage);
                        return;
                    }

                    var testfunc = await browser.ExecuteScriptAsync($"(function () {{ " +
                        $"const link = document.createElement('a');" +
                        $"link.href = '{urlSimple}';" +
                        $"link.download = '{post.Id}.{post.FileFormat}';" +
                        $"link.click();" +
                        $"}})()");

                    //AddArtist(new Artist { ArtistName = post.ArtistName, ImageNumber = post.Id, CreatedOn = DateTime.Now.ToShortDateString(), ImageTags = post.Tags, Source = new Uri(urlSimple).Host });
                }
                if (currentPage <= pageCount)
                {
                    var urlFixed = url.Replace("\"", "").Substring(0, url.Replace("\"", "").LastIndexOf("/"));
                    browser.CoreWebView2.Navigate($"{urlFixed}/{currentPage + 1}");
                }
                else
                {
                    return;
                }
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            browser.CoreWebView2.Navigate("https://rule34hentai.net/");
        }

        private void DownloadAll_Click(object sender, EventArgs e)
        {
            var allArtists = "";
            Random rnd = new Random();
            var shuffledArtists = LoadArtists(true).OrderBy(x => rnd.Next()).ToList();
            foreach (var artist in shuffledArtists)
            {
                if (artist.Source != "rule34hentai.net")
                {
                    allArtists += $"{artist.ArtistName};";
                }
            }
            Invoke(new Action(() => { textBox1.Text = allArtists; }));
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //GifToWebmConverter("C:\\Users\\Admin\\Desktop\\webp\\1.gif");
            //GifToWebmConverter("C:\\Users\\Admin\\Desktop\\webp\\2.gif");
            //GifToWebmConverter("C:\\Users\\Admin\\Desktop\\webp\\3.gif");
            //var test = ImageIsValid("C:\\Users\\Admin\\Desktop\\ezgif-4-10b738d53a25.jpg");

            //DownloadContentFromLinkOrId("6711198");
            CheckImagesAfterDownload("D:\\Tests\\prywinko");
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

        public string Source { get; set; }

        public long Score { get; set; }

        [DefaultValue("")]
        public string UpdatedOn { get; set; }
    }

    public class PostInfo
    {
        public string Id { get; set; }
        public string Tags { get; set; }
        public string FileFormat { get; set; }
        public string ArtistName { get; set; }
        public string Source { get; set; }

    }

    public enum PathType
    {
        All = 0,
        ImagePath = 1,
        DBPath = 2,
        FFMPEGPath = 3
    }

    public enum ReturnType
    {
        Break = 0,
        Continue = 1,
        True = 2,
        False = 3
    }
}
