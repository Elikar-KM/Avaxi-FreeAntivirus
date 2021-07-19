using IWshRuntimeLibrary;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using Shell32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Avaxi
{
    class RightClickItems
    {
        public string Place { get; set; }
        public string MenuName { get; set; }
        public string MenuCommand { get; set; }
    }
    [Serializable]
    class Stocksettings
    {
        public string name;
        public string file;
        public string directory;
        public string arguments;
        public int windowstate;
        
        public Stocksettings(string n, string f, string d, string a, int w)
        {
            this.name = n;
            this.file = f;
            this.directory = d;
            this.arguments = a;
            this.windowstate = w;
        }
    }
    public class StartupManager
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
    }
    class SoftwaresData
    {
        public int Id { get; set; }
        public string SoftwareName { get; set; }
        public string SoftwarePublisher { get; set; }
        public string SoftwareVersion { get; set; }
        public string SoftwareIcon { get; set; }
        public string InstallationDate { get; set; }
        public string RegistreKey { get; set; }
        public string SoftwareKeyName { get; set; }
    }
    class ScanDetails
    {
        public int SectionID { get; set; }
        public List<string> AllFiles { get; set; }
        public long Size { get; set; }
    }
    class ProblemDetails
    {
        public string Problem { get; set; }
        public string Data { get; set; }
        public string RegistryValue { get; set; }
        public string RegistreKey { get; set; }
        public string RegistryValueName { get; set; }
    }
    class RegistryScanDetails
    {
        public int SectionID { get; set; }
        public List<ProblemDetails> Problems { get; set; }
    }
    class ListViewItemComparer : IComparer
    {
        private int col;
        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            int returnVal = -1;
            returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
            ((ListViewItem)y).SubItems[col].Text);
            return returnVal;
        }
    }
    class MyRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected)
            {
                base.OnRenderButtonBackground(e);
            }
            else
            {
                Rectangle rectangle = new Rectangle(0, 0, e.Item.Size.Width - 1, e.Item.Size.Height - 1);
                LinearGradientBrush linGrBrush = new LinearGradientBrush(
         new Point(0, 0),
         new Point(rectangle.Width, rectangle.Height),
         ColorTranslator.FromHtml("#0f0c29"),
         ColorTranslator.FromHtml("#302b63"));

                ColorBlend cblend = new ColorBlend(3);
                var colors = new Color[3] { ColorTranslator.FromHtml("#F4F4F4"),
                ColorTranslator.FromHtml("#F4F4F4"),
                //ColorTranslator.FromHtml("#2f364f"),
                ColorTranslator.FromHtml("#F4F4F4") };
                cblend.Colors = colors;
                cblend.Positions = new float[3] { 0f, 0.5f, 1f };


                linGrBrush.InterpolationColors = cblend;
                e.Graphics.FillRectangle(linGrBrush, rectangle);

            }
        }
    }
    class Gradient
    {
        public static void FillGradient(int Width, int Height, PaintEventArgs e)
        {
            LinearGradientBrush linGrBrush = new LinearGradientBrush(
         new Point(0, 0),
         new Point(Width, Height),
         ColorTranslator.FromHtml("#0f0c29"),
         ColorTranslator.FromHtml("#302b63"));

            ColorBlend cblend = new ColorBlend(3);
            var colors = new Color[3] { ColorTranslator.FromHtml("#F4F4F4"),
                ColorTranslator.FromHtml("#313a52"),
                ColorTranslator.FromHtml("#F4F4F4") };
            cblend.Colors = colors;
            cblend.Positions = new float[3] { 0f, 0.5f, 1f };


            linGrBrush.InterpolationColors = cblend;

            try
            {
                Pen pen = new Pen(linGrBrush);
                e.Graphics.FillRectangle(linGrBrush, 0, 0, Width, Height);
                linGrBrush.Dispose();
                pen.Dispose();
            }
            catch { }
        }
    }
    class MainMethods
    {
        public static bool IsFileLocked(string filename)
        {
            bool Locked = false;
            try
            {
                FileStream fs =
                    System.IO.File.Open(filename, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException ex)
            {
                Locked = true;
                frmMain.PushLog(ex.Message);
            }
            return Locked;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static void FixedInvalidRegistryKey(RegistryKey MainKey, string SubKey)
        {
            Computer MyComputer = new Computer();
            string AllKeys = "";
            foreach (var key in SubKey.Split('\\'))
            {
                AllKeys += key;
                if (MainKey.OpenSubKey(AllKeys, true) == null)
                {
                    MainKey.CreateSubKey(AllKeys, true);
                }
                AllKeys += "\\";

            }

        }
        public static List<Registry> GetRegistrySettings()
        {
            List<Registry> Sections = new List<Registry>();
            Sections.Add(new Registry() { ID = 1, Enabled = true, Name = "Unused File Extensions" });
            Sections.Add(new Registry() { ID = 2, Enabled = true, Name = "Run at Startup" });
            Sections.Add(new Registry() { ID = 3, Enabled = true, Name = "Application Paths" });
            Sections.Add(new Registry() { ID = 4, Enabled = true, Name = "MUI Cache" });
            Sections.Add(new Registry() { ID = 5, Enabled = true, Name = "Unistaller" });
            Sections.Add(new Registry() { ID = 6, Enabled = true, Name = "Obsolete Application" });
            return Sections;
        }
        public static List<Registry> ReadRegistrySettins()
        {
            List<Registry> Sections = new List<Registry>();
            string Path = Application.StartupPath + "\\RegistreSettings.data";
            BinaryFormatter bin = new BinaryFormatter();
            Stream file = new FileStream(Path, FileMode.OpenOrCreate);
            Sections = (List<Registry>)bin.Deserialize(file);
            file.Close();
            return Sections;
        }
        public static void SaveRegistrySettins(List<Registry> Sections)
        {
            string Path = Application.StartupPath + "\\RegistreSettings.data";
            BinaryFormatter bin = new BinaryFormatter();
            Stream file = new FileStream(Path, FileMode.OpenOrCreate);
            bin.Serialize(file, Sections);
            file.Close();
        }
        public static void SaveCleanSettins(List<Section> Sections)
        {
            string Path = Application.StartupPath + "\\CleanSettings.data";
            BinaryFormatter bin = new BinaryFormatter();
            Stream file = new FileStream(Path, FileMode.OpenOrCreate);
            bin.Serialize(file, Sections);
            file.Close();
        }
        public static List<Section> ReadCleanSettins()
        {
            List<Section> Sections = new List<Section>();
            string Path = Application.StartupPath + "\\CleanSettings.data";
            BinaryFormatter bin = new BinaryFormatter();
            Stream file = new FileStream(Path, FileMode.OpenOrCreate);
            Sections = (List<Section>)bin.Deserialize(file);
            file.Close();
            return Sections;
        }
        public static void SaveProgramSettins(Settings ProgramSettings)
        {
            string Path = Application.StartupPath + "\\ProgramSettings.data";
            BinaryFormatter bin = new BinaryFormatter();
            Stream file = new FileStream(Path, FileMode.OpenOrCreate);
            bin.Serialize(file, ProgramSettings);
            file.Close();
        }
        public static Settings ReadProgramSettins()
        {
            Settings ProgramSettings = new Settings();
            string Path = Application.StartupPath + "\\ProgramSettings.data";
            BinaryFormatter bin = new BinaryFormatter();
            Stream file = new FileStream(Path, FileMode.OpenOrCreate);
            ProgramSettings = (Settings)bin.Deserialize(file);
            file.Close();
            return ProgramSettings;
        }
        public static List<Section> GetCleanSections()
        {
            List<Section> AllSections = new List<Section>();
            Section section = new Section() { Junks = new List<Junk>() };

            //Section 1 : Windows Explorer
            section.Name = "Windows Explorer";
            section.ID = 1;

            //Recenet Document
            Junk junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Recent Documents";
            junk.Enabled = true;
            Path path = new Path() { Operation = "del", FullPath = "%ApplicationData%\\Microsoft\\Windows\\Recent" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Run (in Start Menu)
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Run (in Start Menu)";
            junk.Enabled = true;
            path = new Path() { Operation = "delReg", FullPath = @"CurrentUser\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Windows Explorer Typed Paths
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Windows Explorer Typed Paths";
            junk.Enabled = true;
            path = new Path() { Operation = "delReg", FullPath = @"CurrentUser\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Thumbnail Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Thumbnail Cache";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = @"%LocalApplicationData%\Microsoft\Windows\Explorer", FilesName = new List<string>() { "*.db" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 2 : System
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "System";
            section.ID = 2;


            //Temporary Files
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Temporary Files";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%LocalApplicationData%\\Temp" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Clipboard
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Clipboard";
            junk.Enabled = true;
            path = new Path() { Operation = "delClipboard", FullPath = @"Clipboard" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Memory Dumps
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Memory Dumps";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = @"%LocalApplicationData%\CrashDumps", FilesName = new List<string>() { "*.dmp" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = @"%LocalApplicationData%\MiniDump", FilesName = new List<string>() { "*.dmp" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Windows Log Files
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Windows Log Files";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = @"%Windows%\Logs", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = @"%Windows%\security\logs", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = @"%Windows%\SoftwareDistribution", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = @"%Windows%\ModemLogs", FilesName = new List<string>() { "*.txt" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Windows Error Reporting
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Windows Error Reporting";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%ProgramData%\\Microsoft\\Windows\\WER\\ReportArchive", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ProgramData%\\Microsoft\\Windows\\WER\\ReportQueue", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalApplicationData%\\Microsoft\\Windows\\WER\\ReportArchive", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalApplicationData%\\Microsoft\\Windows\\WER\\ReportQueue", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Desktop Shortcut
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Desktop Shortcut";
            junk.Enabled = true;
            path = new Path() { Operation = "delShortcut", FullPath = @"%Desktop%", FilesName = new List<string>() { "*.lnk" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Start Menu Shortcut
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Start Menu Shortcut";
            junk.Enabled = true;
            path = new Path() { Operation = "delShortcut", FullPath = "%ApplicationData%\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\StartMenu", FilesName = new List<string>() { "*.lnk" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Font Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Font Cache";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%Windows%\\ServiceProfiles\\LocalService\\AppData\\Local", FilesName = new List<string>() { "*.dat" }, SearchOption = "TopDirectoryOnly" };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%System32%", FilesName = new List<string>() { "fntcach.dat" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalApplicationData%", FilesName = new List<string>() { "GDIPFONTCACHEV1.DAT" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Recycle Bin
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Recycle Bin";
            junk.Enabled = true;
            path = new Path() { Operation = "delRecycleBin", FullPath = @"%RecycleBin%" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 3 : Advanced Junks
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "Advanced Junks";
            section.ID = 3;


            //Winows Event Logs
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Winows Event Logs";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%Windows%\\Sysnative\\winevt\\logs", FilesName = new List<string>() { "*.evtx" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Tray Notifications Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Tray Notifications Cache";
            junk.Enabled = true;
            path = new Path() { Operation = "delReg", FullPath = @"CurrentUser\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify", FilesName = new List<string>() { "IconStreams" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "delReg", FullPath = @"CurrentUser\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify", FilesName = new List<string>() { "PastIconsStream" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //IIS Log Files
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "IIS Log Files";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%System32%\\LogFiles\\HTTPERR", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Windows Size/Location Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Windows Size/Location Cache";
            junk.Enabled = true;
            path = new Path() { Operation = "delReg", FullPath = @"CurrentUser\Software\Microsoft\Windows\CurrentVersion\Explorer\StreamMRU" };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Prefetched Data
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Prefetched Data";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%Windows%\\Prefetch", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 4 : Mozilla Firefox
            bool IsFireFoxInstalled = false;
            if (Directory.Exists(Environment.GetEnvironmentVariable("systemdrive") + "\\Program Files\\Mozilla Firefox") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Mozilla Firefox") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Mozilla Firefox"))
                IsFireFoxInstalled = true;
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "Mozilla Firefox";
            section.ID = 4;


            //Internet Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Internet Cache";
            junk.Enabled = IsFireFoxInstalled;
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%\\Cache", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%\\Cache2", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%\\Cache", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%\\Cache2", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Internet History
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Internet History";
            junk.Enabled = IsFireFoxInstalled;
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "places.sqlite" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "places.sqlite" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Download History
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Download History";
            junk.Enabled = IsFireFoxInstalled;
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "formhistory.sqlite" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "formhistory.sqlite" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Sessions
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Sessions";
            junk.Enabled = IsFireFoxInstalled;
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%\\storage\\default", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%\\sessionstore-backups", FilesName = new List<string>() { "*.js" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "sessionstore.js" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "sessionCheckpoints.json" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "sessionstore.jsonlz4" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%\\storage\\default", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%\\sessionstore-backups", FilesName = new List<string>() { "*.js" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "sessionstore.js" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "sessionCheckpoints.json" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "sessionstore.jsonlz4" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Cookies
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Cookies";
            junk.Enabled = IsFireFoxInstalled;
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "cookies.sqlite" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "cookies.sqlite" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Saved Passwords
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Saved Passwords";
            junk.Enabled = IsFireFoxInstalled;
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "logins.json" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "logins.json" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%FirefoxProfile%", FilesName = new List<string>() { "key4.db" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%LocalFirefoxProfile%", FilesName = new List<string>() { "key4.db" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 5 : Google Chrome
            bool IsChromeInstalled = false;
            if (Directory.Exists(Environment.GetEnvironmentVariable("systemdrive") + "\\Program Files\\Google\\Chrome\\Application") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Google\\Chrome\\Application") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Google\\Chrome\\Application") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\Application"))
                IsChromeInstalled = true;
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "Google Chrome";
            section.ID = 5;


            //Internet Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Internet Cache";
            junk.Enabled = IsChromeInstalled;
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Cache" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Local Storage" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "ShaderCache" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Internet History
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Internet History";
            junk.Enabled = IsChromeInstalled;
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Current Tabs" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Last Tabs" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Top Sites" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Visited Links" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Network Action Predictor" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "History Provider Cache" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "History" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);


            //Cookies
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Cookies";
            junk.Enabled = IsChromeInstalled;
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Cookies" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "IndexedDB" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Sessions
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Sessions";
            junk.Enabled = IsChromeInstalled;
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Current Session" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Last Session" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%\\Session Storage", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%\\Extension State", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Saved Passwords
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Saved Passwords";
            junk.Enabled = IsChromeInstalled;
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Login Data" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Downloads
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Download History";
            junk.Enabled = IsChromeInstalled;
            path = new Path() { Operation = "del", FullPath = "%ChromeProfile%", FilesName = new List<string>() { "Downloads" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 6 : Opera Browser
            bool IsOperaInstalled = false;
            if (Directory.Exists(Environment.GetEnvironmentVariable("systemdrive") + "\\Program Files\\Opera") || Directory.Exists(Environment.GetEnvironmentVariable("systemdrive") + "\\Program Files\\Opera developer") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Opera developer") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Opera developer") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Opera") || Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\Opera"))
                IsOperaInstalled = true;
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "Opera Browser";
            section.ID = 6;


            //Internet Cache
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Internet Cache";
            junk.Enabled = IsOperaInstalled;
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%\\Cache", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%\\GPUCache", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%\\Local Storage", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Internet History
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Internet History";
            junk.Enabled = IsOperaInstalled;
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Current Tabs" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Last Tabs" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Top Sites" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Visited Links" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Network Action Predictor" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "History Provider Cache" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "History" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);


            //Cookies
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Cookies";
            junk.Enabled = IsOperaInstalled;
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Cookies" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Sessions
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Sessions";
            junk.Enabled = IsOperaInstalled;
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Current Session" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Last Session" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%\\Extension State", FilesName = new List<string>() { "*.*" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Saved Passwords
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Saved Passwords";
            junk.Enabled = IsOperaInstalled;
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Login Data" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Downloads
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Download History";
            junk.Enabled = IsOperaInstalled;
            path = new Path() { Operation = "del", FullPath = "%OperaProfile%", FilesName = new List<string>() { "Downloads" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 7 : Utilities
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "Utilities";
            section.ID = 7;



            //Windows Defender
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Windows Defender";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%ProgramData%\\Microsoft\\Windows Defender\\Support", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //Avast
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "Avast";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%ProgramData%\\AVAST Software\\Avast\\log", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            //IDM
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "IDM";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%ApplicationData%\\IDM", FilesName = new List<string>() { "foldresHistory.txt" } };
            junk.Paths.Add(path);
            path = new Path() { Operation = "del", FullPath = "%ApplicationData%\\IDM", FilesName = new List<string>() { "UrlHistory.txt" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);
            //Section 8 : Windows Applications
            section = new Section() { Junks = new List<Junk>() };
            section.Name = "Windows Applications";
            section.ID = 8;



            //Windows Defender
            junk = new Junk() { Paths = new List<Path>() };
            junk.Name = "MS Search";
            junk.Enabled = true;
            path = new Path() { Operation = "del", FullPath = "%ProgramData%\\Microsoft\\Search\\Data\\Applications\\Windows", FilesName = new List<string>() { "*.log" } };
            junk.Paths.Add(path);
            section.Junks.Add(junk);

            AllSections.Add(section);

            return AllSections;
        }
        public static long GetFileSize(string FilePath)
        {
            if (!System.IO.File.Exists(FilePath))
                return 0;
            return new FileInfo(FilePath).Length;
        }
        public static void CentreControlInGroupBox(GroupBox theGroupBox, Control theControl)
        {
            // Find the centre point of the Group Box
            int groupBoxCentreWidth = theGroupBox.Width / 2;
            int groupBoxCentreHeight = theGroupBox.Height / 2;

            // Find the centre point of the Control to be positioned/added
            int controlCentreWidth = theControl.Width / 2;
            int controlCentreHeight = theControl.Height / 2;

            // Set the Control to be at the centre of the Group Box by
            // off-setting the Controls Left/Top from the Group Box centre
            theControl.Left = groupBoxCentreWidth - controlCentreWidth;
            theControl.Top = groupBoxCentreHeight - controlCentreHeight;

            // Set the Anchor to be None to make sure the control remains
            // centred after re-sizing of the form
            theControl.Anchor = AnchorStyles.None;


        }
        public static void CentreControlInPanel(Panel thePanel, Control theControl, int? ToReduceWidth)
        {
            // Find the centre point of the Group Box
            int groupBoxCentreWidth = thePanel.Width / 2;
            int groupBoxCentreHeight = thePanel.Height / 2;

            // Find the centre point of the Control to be positioned/added
            int controlCentreWidth = theControl.Width / 2;
            int controlCentreHeight = theControl.Height / 2;

            // Set the Control to be at the centre of the Group Box by
            // off-setting the Controls Left/Top from the Group Box centre
            theControl.Left = groupBoxCentreWidth - controlCentreWidth + ToReduceWidth.Value;
            //theControl.Top = groupBoxCentreHeight - controlCentreHeight;

            // Set the Anchor to be None to make sure the control remains
            // centred after re-sizing of the form
            theControl.Anchor = AnchorStyles.None;


        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct SHQUERYRBINFO
        {
            public Int32 cbSize;
            public UInt64 i64Size;
            public UInt64 i64NumItems;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHQueryRecycleBin(
                [MarshalAs(UnmanagedType.LPTStr)]
                String pszRootPath,
                ref SHQUERYRBINFO pSHQueryRBInfo
            );
        enum RecycleFlags : int
        {
            SHERB_NOCONFIRMATION = 0x00000001,          //No confirmation dialog will open while emptying recycle bin.
            SHERB_NOPROGRESSUI = 0x00000001,            //No progress tracking window appears while emptying recycle bin.
            SHERB_NOSOUND = 0x00000004                  //No sound whent while emptying recycle bin.
        }

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hwnd, string psrRootPath, RecycleFlags dwFlags);

        public static void emptyRecycleBin()
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION);
        }
        public static ulong GetRecycleSize()
        {
            SHQUERYRBINFO bb_Query = new SHQUERYRBINFO();
            bb_Query.cbSize = Marshal.SizeOf(bb_Query.GetType());
            SHQueryRecycleBin(null, ref bb_Query);
            return bb_Query.i64Size;
        }
        public static ulong GetRecycleNumberItems()
        {
            SHQUERYRBINFO bb_Query = new SHQUERYRBINFO();
            bb_Query.cbSize = Marshal.SizeOf(bb_Query.GetType());
            SHQueryRecycleBin(null, ref bb_Query);
            return bb_Query.i64NumItems;
        }

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);
            WshShell shell = new WshShell(); //Create a new WshShell Interface
            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutFilename);
            return link.TargetPath;
            //Shell shell = new Shell();
            //Folder folder = shell.NameSpace(pathOnly);
            //FolderItem folderItem = folder.ParseName(filenameOnly);
            //if (folderItem != null)
            //{
            //    Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
            //    return link.Path;
            //}


        }
        public static IEnumerable<string> GetDirectoryFiles(string rootPath, string patternMatch, SearchOption searchOption)
        {
            var foundFiles = Enumerable.Empty<string>();

            if (searchOption == SearchOption.AllDirectories)
            {
                try
                {
                    IEnumerable<string> subDirs = Directory.EnumerateDirectories(rootPath);
                    foreach (string dir in subDirs)
                    {
                        foundFiles = foundFiles.Concat(GetDirectoryFiles(dir, patternMatch, searchOption)); // Add files in subdirectories recursively to the list
                    }
                }
                catch (DirectoryNotFoundException) { }
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
            }

            try
            {
                foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch)); // Add files from the current directory
            }
            catch (UnauthorizedAccessException) { }

            return foundFiles;
        }
    }
    [Serializable]
    public class Path
    {
        public string FullPath { get; set; }
        //Delete or ...
        public string Operation { get; set; }
        public List<string> FilesName { get; set; }
        public string SearchOption { get; set; }
    
    }
    [Serializable]
    public class Junk
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<Path> Paths { get; set; }
    }
    [Serializable]
    public class Section
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<Junk> Junks { get; set; }
    }
    [Serializable]
    public class Settings
    {
        public bool LaunchInStartup { get; set; }
        public bool ShutdownAfterClean { get; set; }
        public bool CloseAfterClean { get; set; }
        public bool AutoClean { get; set; }
        public ulong TotalSaved { get; set; }
        public DateTime? LastScan { get; set; }
        public bool MonitorStartup { get; set; }
    }

    [Serializable]
    public class Registry
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
