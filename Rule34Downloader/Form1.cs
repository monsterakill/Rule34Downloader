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

namespace Rule34Downloader
{
    public partial class Form1 : Form
    {
        BackgroundWorker Worker;
        string[] duplicateFiles = new string[] { };
        List<string> duplicatesNames = new List<string>();

        private const string ConnectionString = "Data Source=D:\\Program Files\\DB Browser for SQLite\\Rule34ImagesDB.db";
        private readonly List<Artist> artistsLocal = new List<Artist>();
        private readonly List<Artist> artistsDB = new List<Artist>();

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Worker.RunWorkerAsync();
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            // This function fires on the UI thread so it's safe to edit

            // the UI control directly, no funny business with Control.Invoke :)

            // Update the progressBar with the integer supplied to us from the

            // ReportProgress() function. 
            var defaultValue = 0;
            progressBar1.Maximum = 100;
            if (e.UserState != null)
            {
                LogField.AppendText(e.UserState.ToString());
            }

            progressBar1.Value = e.ProgressPercentage;
            TaskbarProgress.SetValue(Handle, progressBar1.Value, progressBar1.Maximum);

            //while (progressBar1.Value == progressBar1.Maximum && !ContainsFocus)
            //{
            //    TaskbarProgress.SetValue(Handle, 33, 99);
            //    System.Threading.Thread.Sleep(1000);
            //    TaskbarProgress.SetValue(Handle, 66, 99);
            //    System.Threading.Thread.Sleep(1000);
            //    TaskbarProgress.SetValue(Handle, 99, 99);
            //    System.Threading.Thread.Sleep(1000);
            //    TaskbarProgress.SetValue(Handle, 66, 99);
            //    System.Threading.Thread.Sleep(1000);
            //    TaskbarProgress.SetValue(Handle, 33, 99);
            //    System.Threading.Thread.Sleep(1000);
            //    TaskbarProgress.SetValue(Handle, 0, 99);
            //    System.Threading.Thread.Sleep(1000);
            //}
            //if(progressBar1.Value == progressBar1.Maximum)
            //{

            //}
            //lblStatus.Text = "Processing......" + progressBar1.Value.ToString() + "%";

        }

        //protected override void OnDeactivate(EventArgs e)
        //{
        //    LogField.AppendText("OnDeactivate");
        //}

        //protected override void OnActivated(EventArgs e)
        //{
        //    LogField.AppendText("OnActivated");
        //}

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
                finalURL = RemoveArtistsFromSearch($"https://rule34.xxx/index.php?page=post&s=list&tags={textBox1.Text}");
            }
            else
            {
                finalURL = $"https://rule34.xxx/index.php?page=post&s=list&tags={textBox1.Text}";
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

            if (!Directory.Exists($"D:\\Tests\\{textBox1.Text}"))
            {
                Directory.CreateDirectory($"D:\\Tests\\{textBox1.Text}");
                Worker.ReportProgress(progressBarPercentInt, $"Creating Directory: D:\\Tests\\{textBox1.Text}" + Environment.NewLine);
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
                System.Threading.Thread.Sleep(10000);

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
                    System.Threading.Thread.Sleep(r.Next(5000, 10000));

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
                            if (CheckForDuplicate(fileNameReg.ToString()) || CheckForDuplicateFromDB(new Artist { ArtistName = textBox1.Text, ImageNumber = fileNameReg.ToString() }))
                            {
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
                                $"D:\\Tests\\{textBox1.Text}\\{fileNameReg}.mp4"
                            );
                        }

                        AddArtist(new Artist { ArtistName = textBox1.Text, ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString() });
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
                            if (CheckForDuplicate(fileNameReg) || CheckForDuplicateFromDB(new Artist { ArtistName = textBox1.Text, ImageNumber = fileNameReg.ToString() }))
                            {
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
                            //wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                            wc.DownloadFileAsync(
                                // Param1 = Link of file
                                new System.Uri(urlSimple),
                                // Param2 = Path to save
                                $"D:\\Tests\\{textBox1.Text}\\{fileNameReg}.{format}"
                            );
                        }

                        AddArtist(new Artist { ArtistName = textBox1.Text, ImageNumber = fileNameReg.ToString(), CreatedOn = DateTime.Now.ToShortDateString() });
                    }
                    progressBarPercent = (progressBarValue / (double)progressBarMaximumValue) * 100;

                    Worker.ReportProgress(progressBarPercentInt = (int)progressBarPercent);
                }
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
                    System.Threading.Thread.Sleep(3000);
                    doc = web.Load(searchArtist);
                    artists = doc.DocumentNode.SelectNodes("//li[@class='tag-type-artist']");
                    if (artists != null) break;
                }
            }

            foreach (var artist in artists)
            {
                var fixedArtistName = "";
                var removeSymbolsFromArtistNames = artist.InnerText.Substring(6).Substring(0, artist.InnerText.Substring(6).IndexOf("\n") - 1);
                if (removeSymbolsFromArtistNames.Contains(" "))
                {
                    removeSymbolsFromArtistNames = removeSymbolsFromArtistNames.Replace(" ", "_");
                }
                if (removeSymbolsFromArtistNames != textBox1.Text)
                {
                    artistsStringList += $"+-{removeSymbolsFromArtistNames}";
                }

            }
            searchArtist += artistsStringList;

            //Timeout
            System.Threading.Thread.Sleep(5000);

            doc = web.Load(searchArtist);
            artists = doc.DocumentNode.SelectNodes("//li[@class='tag-type-artist']");


            if (artists == null)
            {
                for (int y = 0; y < retryCount; y++)
                {
                    Worker.ReportProgress(0, $"Artists Cycle is empty. Connection Retry: {y}" + Environment.NewLine);
                    //Timeout
                    System.Threading.Thread.Sleep(3000);
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
            string FinalString;
            int Pos1 = 0;
            int Pos2 = 0;
            if (!lastIndexof)
            {
                Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos1 = STR.LastIndexOf(FirstString) + FirstString.Length;
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        public bool CheckForDuplicate(string fileName)
        {
            if (duplicateFiles.Any())
            {
                return duplicatesNames.Contains(fileName);
            }
            else
            {
                duplicateFiles = Directory.GetFiles($"D:\\Tests\\{textBox1.Text}\\");
                foreach (var dupl in duplicateFiles)
                {
                    duplicatesNames.Add(Path.GetFileNameWithoutExtension(dupl));
                }
                return duplicatesNames.Contains(fileName);
            }
        }

        public void AddArtist(Artist artist, bool checkDuplicate = true)
        {
            if (CheckForDuplicateFromDB(artist))
            {
                Worker.ReportProgress(0, $"This Image Already Found in DB: {artist.ArtistName} - {artist.ImageNumber}" + Environment.NewLine);
                return;
            }

            var queryString = "insert into Images(ArtistName, ImageNumber, CreatedOn)" +
                                " values(@artistName, @imageNumber, @createdOn);";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                //1. Add the new participant to the database
                var command = new SQLiteCommand(queryString, connection);
                command.Parameters.AddWithValue("@artistName", artist.ArtistName);
                command.Parameters.AddWithValue("@imageNumber", artist.ImageNumber);
                command.Parameters.AddWithValue("@createdOn", artist.CreatedOn);

                command.ExecuteScalar();
            }
        }

        public void LoadArtists()
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
                        string artistName = (string)sqlReader["ArtistName"];
                        string imageNumber = (string)sqlReader["ImageNumber"];
                        string createdOn = (string)sqlReader["CreatedOn"];

                        Artist artist = new Artist()
                        {
                            ArtistName = artistName,
                            ImageNumber = imageNumber,
                            CreatedOn = createdOn
                        };
                        artistsDB.Add(artist);
                    }
                }
            }
        }

        public bool CheckForDuplicateFromDB(Artist artist)
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

        public void ReadLoaclArtists()
        {
            CheckForDuplicateFromDB(new Artist { ArtistName = "TEST", ImageNumber = "4164000" });

            CheckForDuplicateFromDB(new Artist { ArtistName = "zly", ImageNumber = "4164000" });

            artistsLocal.Clear();

            var progressBarValue = 0;
            var progressBarMaximumValue = Directory.GetDirectories(@"D:\\Tests").Length;
            var progressBarPercent = 0.0;

            foreach (var dir in Directory.GetDirectories(@"D:\\Tests"))
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

        //public void CreateArtistDB()
        //{
        //    var queryString = "insert into Artists(ArtistName, ImageNumber, CreatedOn)" +
        //                        " values(@artistName, @imageNumber, @createdOn);";

        //    using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
        //    {
        //        connection.Open();

        //        //1. Add the new participant to the database
        //        var command = new SQLiteCommand(queryString, connection);
        //        command.Parameters.AddWithValue("@artistName", artist.ArtistName);
        //        command.Parameters.AddWithValue("@imageNumber", artist.ImageNumber);
        //        command.Parameters.AddWithValue("@createdOn", artist.CreatedOn);

        //        command.ExecuteScalar();

        //        //2. Add the new participants to the local collection
        //        artists.Add(artist);
        //    }
        //}

        private void button2_Click(object sender, EventArgs e)
        {
            ReadLoaclArtists();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            WriteLocalArtists();
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
        public string ArtistName { get; set; }

        public string ImageNumber { get; set; }

        public string CreatedOn { get; set; }
    }
}
