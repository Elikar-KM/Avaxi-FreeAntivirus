using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using TejashPlayer;
using nClam;
using Avaxi.SpeedUp;

namespace Avaxi
{
    public partial class frmMain : Form
    {
        [DllImport("process-killer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KillProcess(IntPtr handle, string proc_name);

        private FW firewall = new FW();
        private CommandShell shell = new CommandShell();
        private Optimize opt;
        private Thread search = null;
        private string my_location = string.Empty;
        private int scanType = -1;   // -1: no scan | 0: smartscan | 1: quickscan | 2: targetedscan | 3: fullscan | 4: usbscan
        private int infected = 0;
        //private string smart_ext = "*.exe|*.cpl|*.reg|*.ini|*.bat|*.com|*.dll|*.pif|*.lnk|*.scr|*.vbs|*.ocx|*.drv|*.sys";
        private string smart_ext = "*.*";
        private string wildcard = "*.*";
        private string[] files = null;
        private string loc_to_search = string.Empty;
        private ConsoleSetups con = new ConsoleSetups();
        private int flagScanStatus = 0;  // 0: stop | 1: done | 2: next | 3: stop->ok
        private int flagDeleteStatus = 0;  // 0: delete | 1: done
        private ArrayList watchers = new ArrayList();
        private Exclusion exclusion = new Exclusion();
        private UsbManager usb = new UsbManager();
        
        private bool flagPhishing = true;
        private bool flagCryptojacking = true;
        private bool flagRansomware = true;
        private bool flagAffiliateOffers = true;

        private bool flagSilent = false;

        public static string[] strArray = null;

        public Predicate<Process> ProcessFilter;

        private Thread runThread;
        private HashSet<int> procIds, newProcIds;

        private uint handle;
        private uint pnProcInfoNeeded;
        private uint pnProcInfo;
        private uint lpdwRebootReasons = RestartManager.RmRebootReasonNone;
        private RmProcesInfo[] processInfo;
        private int xloc;
        private int yloc;

        enum WindowsState
        {
            Hide,
            Show,
            Minimize
        }

        enum PanelState
        {
            NONE,
            SHIELD,
            CRYPTO,
            SCAN,
            OPTIMIZE,
            TUNEUP
        }
        private WindowsState _curWindowState;
        private Panel _animcurpanel;
        private Panel _curpanel;

        // start up code
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string methodName);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [SecurityCritical]
        internal static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool IsWow64Process([In] IntPtr hSourceProcessHandle, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);

        [SecuritySafeCritical]
        public static bool get_Is64BitOperatingSystem()
        {
            bool flag;
            return (IntPtr.Size == 8) ||
                ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                IsWow64Process(GetCurrentProcess(), out flag)) && flag);
        }

        public static void AddToRegistry()
        {
            try
            {
                Microsoft.Win32.RegistryKey key;

                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Avaxi", true);
                if (key == null)
                {
                    Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Avaxi");
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Avaxi", true);
                }
                key.Close();
                if (get_Is64BitOperatingSystem())
                {
                    try
                    {
                        key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                        key.SetValue("Avaxi", "\"" + Application.ExecutablePath + "\"");
                        key.Close();
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                        key.SetValue("Avaxi", "\"" + Application.ExecutablePath + "\"");
                        key.Close();
                    }
                    catch { }
                }
            }
            catch { }
        }

        public frmMain()
        {
            InitializeComponent();
            if (Process.GetProcessesByName("Avaxi").Length > 1)
            {
                Environment.Exit(0);
            }
            AddToRegistry();
            //for animation
            _curWindowState = WindowsState.Show;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void label3_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Sign up");
        }

        private void func_labelCloseClick(object sender, EventArgs e)
        {
            this.hideAntiVirus();
        }

        private void func_labelMoveHover(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#F4F4F4");
        }
        private void func_labelMoveHover_Close(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#B72828");
        }
        private void func_labelMoveMove_Close(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#B72828");
        }

        private void func_labelMoveMove(object sender, MouseEventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#F4F4F4");
        }

        private void func_labelMoveLeave(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = Color.Transparent;
        }

        //setup the scanner engine
        private void SetupScannerEngine()
        {
            //setup the scanner engine path
            string temp = my_location + "\\engine\\";

            //write the correct engine configurations according to path
            StreamWriter writter = new StreamWriter(temp + "clamd.conf");
            writter.WriteLine("TCPSocket 3310");
            writter.WriteLine("MaxThreads 2");
            writter.WriteLine("LogFile " + temp + "clamd.log");
            writter.WriteLine("DatabaseDirectory " + temp + "db");
            writter.Close();

            //insert the correct registry settings according to path
            //I am not using try catch block because, I am already running as admin
            //so probably it will not give error, will fix this if gives error in future 
            RegistryKey key;
            key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\ClamAV\\");
            key.SetValue("ConfigDir", temp);
            key.Close();
            key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("Software\\ClamAV\\");
            key.SetValue("DataDir", temp + "db");
            key.Close();

            // Install the AV scanner service
            //con.RunExternalExe(temp + "clamd.exe", "--install");
            Process[] pname = Process.GetProcessesByName("clamd");
            if (pname.Length == 0)
            {
                Task.Run(delegate ()
                {
                    con.RunExternalExe(temp + "clamd.exe", "--config-file=\"" + temp + "clamd.conf\"");
                });
            }

            //System.Diagnostics.Process.Start("CMD.exe", "/C sc create ClamD binPath=\"" + temp + "clamd.exe --foreground\"");

            //install the AV updater service
            //con.RunExternalExe(temp + "freshclam.exe", "--install");
            pname = Process.GetProcessesByName("freshclam");
            if (pname.Length == 0)
            {
                Task.Run(delegate ()
                {
                    con.RunExternalExe(temp + "freshclam.exe", "--config-file=\"" + temp + "freshclam.conf\"");
                });
            }
        }

        //scan the folder or file
        private int ScanFile(string loc, bool silent)
        {
            int ret = 0;
            try
            {
                if (File.Exists(loc))
                {
                    var clam = new ClamClient("localhost", 3310);
                    var scanResult = clam.ScanFileOnServer(loc);

                    switch (scanResult.Result)
                    {
                        case ClamScanResults.Clean:
                            if (!silent)
                                System.Windows.Forms.MessageBox.Show(this, "The file is clean, it's not infected!", "Fine", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ret = 0;
                            break;
                        case ClamScanResults.VirusDetected:
                            {
                                // if (!silent)
                                // {
                                DialogResult dr = System.Windows.Forms.MessageBox.Show(this, "The file is infected\nVirus: " + scanResult.InfectedFiles[0].FileName + "\nDo you want to delete?", "Virus Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr == DialogResult.Yes)
                                    try
                                    {
                                        File.Delete(loc);
                                    }
                                    catch (Exception ex)
                                    {
                                        frmMain.PushLog(ex.Message);
                                    }
                                //  }
                                ret = 1;
                            }
                            break;
                    }
                    return ret;
                }
                else
                {
                    if (!silent)
                        MessageBox.Show(this, "Invalid file to scan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ret = 3;
                    return ret;
                }
            }
            catch (Exception ex)
            {
                frmMain.PushLog(ex.Message);
            }
            return ret;
        }

        //Real time scanner 
        private void RealTime(object sender, FileSystemEventArgs e)
        {
            if (exclusion.isExclusion(e.FullPath))
                return;

            if (this.flagRealTimeProtection)
            {
                frmMain.PushLog("Real-time scan : " + e.FullPath);
                ScanFile(e.FullPath, true);
            }
        }

        private void DoStateChanged(UsbStateChangedEventArgs e)
        {
            if (e.State == UsbStateChange.Added)
            {
                if (this.flagAutoScanUSB)
                {
                    if(scanType == -1)
                    {
                        scanType = 4;
                        ShowPanel(pnlFormScan);
                        //this.BackgroundImage = null;
                        this.BackColor = Color.WhiteSmoke;
                        panelLeftbar.Visible = false;
                        loc_to_search = e.Disk.Name;
                        this.showAntiVirus();
                        search = new Thread(new ThreadStart(ScanFolder));
                        search.Start();
                    }
                }
            }
        }

        private void SetSwitch(Label label, bool flag)
        {
            if (flag)
                label.Image = Avaxi.Properties.Resources.on;
            else
                label.Image = Avaxi.Properties.Resources.off;
        }

        private void SetYesNo(Label label, bool flag)
        {
            if (flag)
                label.Image = Avaxi.Properties.Resources.set_yes;
            else
                label.Image = Avaxi.Properties.Resources.set_no;
        }

        List<Section> AllSections = new List<Section>();
        List<Registry> AllSectionsRegistry = new List<Registry>();
        List<StartupManager> MonitorStartup = new List<StartupManager>();
        Settings ProgramSettings = new Settings() { MonitorStartup=false,TotalSaved = 0, AutoClean = false, CloseAfterClean = false, ShutdownAfterClean = false, LaunchInStartup = false, LastScan = null };
        private void frmMain_Load(object sender, EventArgs e)
        {
            this.flagRealTimeProtection = Program.RealTimeProtection;
            this.flagAutoScanUSB = Program.AutoUSBScanner;

            this.flagFeatureUpdates = true;
            this.flagAppearanceInPerformance = true;
            this.flagSensorService = true;
            this.flagSpeedUpMenuShowDelay = true;
            this.flagStartMenuAds = true;
            this.flagDesktopCleanUpWizard = true;
            this.flagQuickAccessHistory = true;
            this.flagAutomaticUpdates = true;
            this.materialCheckBoxClearCache.Checked = true;
            this.materialCheckBoxClearMemory.Checked = true;

            this.flagPhishing = Program.flagPhishing;
            this.flagCryptojacking = Program.flagCryptojacking;
            this.flagRansomware = false;
            this.flagAffiliateOffers = Program.flagAffiliateOffers;

            // Tune Up page : switch format
            this.materialCheckBoxClearCache.Checked = Program.optClearCache;
            this.materialCheckBoxClearMemory.Checked = Program.optClearMemory;
            switchAutomaticUpdates.Checked = Program.tuneAutomaticUpdates;
            switchDisableDesktop.Checked = Program.tuneDesktopCleanUpWizard;
            switchSpeedUpMenu.Checked = Program.tuneMenuShowDelay;
            switchDisableQuickAccess.Checked = Program.tuneQuickAccessHistory;
            switchDisalbeStartMenuAds.Checked = Program.tuneStartMenuAds;
            switchDisableSensorService.Checked = Program.tuneSensorService;
            switchDisableFeatureUpdates.Checked = Program.tuneFeatureUpdates;
            switchAppearanceInPerformance.Checked = Program.tuneAppearanceInPerformance;

            // Settings page : format
            SetYesNo(this.switchLabelRealTimeProtection, this.flagRealTimeProtection);
            SetYesNo(this.switchLabelAutoScanUSB, this.flagAutoScanUSB);

            if(this.flagRealTimeProtection)
                launcherIcon.ShowBalloonTip(5, "Avaxi", "The live protection is enabled.", ToolTipIcon.Info);
            else
                launcherIcon.ShowBalloonTip(5, "Avaxi", "The live protection is disabled.", ToolTipIcon.Info);

            // Protection Status view - anti-phishing, anti-cryptojacking, anti-ransomware, anti-affiliate offers
            if (flagPhishing)
            {
                btnPhishing.ForeColor = Color.White;
                btnPhishing.Text = "Enabled";
                btnPhishing.BaseColor = Color.FromArgb(15, 220, 200);
                btnPhishing.OverColor = Color.FromArgb(15, 220, 200);
                btnPhishing.DownColor = Color.FromArgb(15, 220, 200);
                labelWebsecGrey.Image = global::Avaxi.Properties.Resources.websec_status;
                materialButtonAntiPishing.Icon = global::Avaxi.Properties.Resources.AntiPhishing;
                labelCheckTick1.Visible = true;
                labelTextWebProtection.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                btnPhishing.ForeColor = Color.Gray;
                btnPhishing.Text = "Disabled";
                btnPhishing.BaseColor = Color.FromArgb(209, 209, 209);
                btnPhishing.OverColor = Color.FromArgb(209, 209, 209);
                btnPhishing.DownColor = Color.FromArgb(209, 209, 209);
                labelWebsecGrey.Image = global::Avaxi.Properties.Resources.websec_status_grey;
                materialButtonAntiPishing.Icon = global::Avaxi.Properties.Resources.AntiPhishing_grey;
                labelCheckTick1.Visible = false;
                labelTextWebProtection.ForeColor = Color.Gray;
            }

            if (flagCryptojacking)
            {
                btnCryptojacking.ForeColor = Color.White;
                btnCryptojacking.Text = "Enabled";
                btnCryptojacking.BaseColor = Color.FromArgb(15, 220, 200);
                btnCryptojacking.OverColor = Color.FromArgb(15, 220, 200);
                btnCryptojacking.DownColor = Color.FromArgb(15, 220, 200);
                labelCryptoGrey.Image = global::Avaxi.Properties.Resources.crypto;
                materialButtonAntiCrypto.Icon = global::Avaxi.Properties.Resources.AntiCryptojacking;
                labelCheckTick2.Visible = true;
                lableTextCryptoProtection.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                btnCryptojacking.ForeColor = Color.Gray;
                btnCryptojacking.Text = "Disabled";
                btnCryptojacking.BaseColor = Color.FromArgb(209, 209, 209);
                btnCryptojacking.OverColor = Color.FromArgb(209, 209, 209);
                btnCryptojacking.DownColor = Color.FromArgb(209, 209, 209);
                labelCryptoGrey.Image = global::Avaxi.Properties.Resources.crypto_grey;
                materialButtonAntiCrypto.Icon = global::Avaxi.Properties.Resources.AntiCryptojacking_grey;
                labelCheckTick2.Visible = false;
                lableTextCryptoProtection.ForeColor = Color.Gray;
            }
            if (flagRansomware)
            {
                btnRansomware.ForeColor = Color.White;
                btnRansomware.Text = "Enabled";
                btnRansomware.BaseColor = Color.FromArgb(15, 220, 200);
                btnRansomware.OverColor = Color.FromArgb(15, 220, 200);
                btnRansomware.DownColor = Color.FromArgb(15, 220, 200);
                labelShieldGrey.Image = global::Avaxi.Properties.Resources.shield_status;
                materialButtonAntiRansomware.Icon = global::Avaxi.Properties.Resources.AntiRansomware;
                labelCheckTick.Visible = true;
                labelTextLiveProtectionEnabled.ForeColor = Color.DeepSkyBlue;
                //RunRansomwareProtection();
            }
            else
            {
                btnRansomware.ForeColor = Color.Gray;
                btnRansomware.Text = "Disabled";
                btnRansomware.BaseColor = Color.FromArgb(209, 209, 209);
                btnRansomware.OverColor = Color.FromArgb(209, 209, 209);
                btnRansomware.DownColor = Color.FromArgb(209, 209, 209);
                labelShieldGrey.Image = global::Avaxi.Properties.Resources.shield_status_grey;
                materialButtonAntiRansomware.Icon = global::Avaxi.Properties.Resources.AntiRansomware_grey;
                labelCheckTick.Visible = false;
                labelTextLiveProtectionEnabled.ForeColor = Color.Gray;
            }

            if (flagAffiliateOffers)
            {
                btnAffiliateOffers.ForeColor = Color.White;
                btnAffiliateOffers.Text = "Enabled";
                btnAffiliateOffers.BaseColor = Color.FromArgb(15, 220, 200);
                btnAffiliateOffers.OverColor = Color.FromArgb(15, 220, 200);
                btnAffiliateOffers.DownColor = Color.FromArgb(15, 220, 200);
                materialButtonAntiAffilate.Icon = global::Avaxi.Properties.Resources.AntiAffiliate;
            }
            else
            {
                btnAffiliateOffers.ForeColor = Color.Gray;
                btnAffiliateOffers.Text = "Disabled";
                btnAffiliateOffers.BaseColor = Color.FromArgb(209, 209, 209);
                btnAffiliateOffers.OverColor = Color.FromArgb(209, 209, 209);
                btnAffiliateOffers.DownColor = Color.FromArgb(209, 209, 209);
                materialButtonAntiAffilate.Icon = global::Avaxi.Properties.Resources.AntiAffiliate_grey;
            }

            // RealTime status view
            if (this.flagRealTimeProtection)
            {
                labelCheck.Image = global::Avaxi.Properties.Resources.check;
                labelTextOtherScans.Text = "The live protection is enabled";
                labelTextOtherScans.ForeColor = Color.White;
            }
            else
            {
                labelCheck.Image = global::Avaxi.Properties.Resources.cross;
                labelTextOtherScans.Text = "The live protection is disabled";
                labelTextOtherScans.ForeColor = Color.Gray;
            }

            this.progressIndicatorTuneUp.Visible = false;
            
            my_location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            SetupScannerEngine(); // setup the scanner engine

            //start the usb monitor
            usb.StateChanged += new UsbStateChangedEventHandler(DoStateChanged);

            // file system watcher
            string[] drives = Directory.GetLogicalDrives();
            DriveInfo di = null;
            for (int i = 0; i < drives.Length; i++)
            {
                di = new DriveInfo(drives[i]);
                if (di.IsReady)
                {
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Filter = "*.*";
                    watcher.Path = drives[i];
                    watcher.IncludeSubdirectories = true;
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes;
                    watcher.Created += new FileSystemEventHandler(RealTime);
                    watcher.Renamed += new RenamedEventHandler(RealTime);
                    //watcher.Changed += new FileSystemEventHandler(RealTime);
                    watcher.InternalBufferSize = 4058;
                    watcher.EnableRaisingEvents = true;
                    watchers.Add(watcher);
                }
            }

            //firewall setting
            if (firewall.firewallOn)
            {
                //this.switchFireWall.Checked = true;
                this.flagFireWall = true;
                //Avaxi.PushLog("Firewall is on.");
            }
            else
            {
                //this.switchFireWall.Checked = false;
                this.flagFireWall = false;
                //Avaxi.PushLog("Firewall is off.");
            }

            xloc = panelLeftbar.Width;
            yloc = panelLeftbar.Height;
            _curpanel = pnlShield;
            ShowPanel(pnlShield);
            //StripButtonColor(panel3);

            Task.Run(delegate ()
            {
                Thread.Sleep(1000);
                opt = new Optimize(this.lstPrograms, this.circleProgressBar1);
            });

        }

        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            //Gradient.FillGradient(this.Width, this.Height, e);
        }

        private void materialBtn_MouseLeave(object sender, EventArgs e)
        {
            ReaLTaiizor.Controls.MaterialButton temp = (ReaLTaiizor.Controls.MaterialButton)sender;
            System.Drawing.Size tempsize = temp.Size;
            System.Drawing.Point temploc = temp.Location;
            temploc.X += 1;
            temploc.Y += 1;
            tempsize.Width -= 2;
            tempsize.Height -= 2;

            temp.Location = temploc;
            temp.Size = tempsize;
            temp.DrawShadows = false;
            Invalidate();

        }
        private void materialBtn_MouseEnter(object sender, EventArgs e)
        {
            ReaLTaiizor.Controls.MaterialButton temp = (ReaLTaiizor.Controls.MaterialButton)sender;
            System.Drawing.Size tempsize = temp.Size;
            System.Drawing.Point temploc = temp.Location;

            temploc.X -= 1;
            temploc.Y -= 1;

            tempsize.Width += 2;
            tempsize.Height += 2;

            temp.Location = temploc;
            temp.Size = tempsize;
            temp.DrawShadows = true;
            Invalidate();
        }

        private void materialBtn_MouseLeave1(object sender, EventArgs e)
        {
            ReaLTaiizor.Controls.MaterialButton temp = (ReaLTaiizor.Controls.MaterialButton)sender;
            temp.DrawShadows = false;
            Invalidate();

        }
        private void materialBtn_MouseEnter1(object sender, EventArgs e)
        {
            ReaLTaiizor.Controls.MaterialButton temp = (ReaLTaiizor.Controls.MaterialButton)sender;
            temp.DrawShadows = true;
            Invalidate();
        }
        private void toolStripButton1_MouseHover(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void toolStripButton1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void toolStripButton1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void frmMain_Activated(object sender, EventArgs e)
        {

        }

        private void frmMain_Resize(object sender, EventArgs e)
        {

            //pnlShield.Left = (this.Width / 2) - (pnlShield.Width / 2) + (toolStrip1.Width / 2);
        }

        private void btnScan_Click(object sender, EventArgs e)  // QuickScan
        {
            scanType = 1;
            loc_to_search = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            ShowPanel(pnlFormScan);
            this.BackgroundImage = null;
            this.BackColor = Color.WhiteSmoke;
            panelLeftbar.Visible = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        void ShowPanel(Panel panel)
        {
            if (panel == _animcurpanel) return;

            pnlShield.Visible = false;
            pnlScan.Visible = false;
            pnlCrypto.Visible = false;
            pnlOptimize.Visible = false;
            pnlTuneUp.Visible = false;
            pnlSetting.Visible = false;
            pnlFormScan.Visible = false;
            pnlDelete.Visible = false;
            this.BackgroundImage = null;
            //this.BackColor = Color.Gainsboro;
            hopeProgressBar1.ValueNumber = 0;

            //panel.Dock = System.Windows.Forms.DockStyle.None;
            System.Drawing.Point loc = panel.Location;
            System.Drawing.Size sz = panel.Size;
            loc.X += sz.Width;
            sz.Height = 80;
            panel.Location = loc;
            panel.Size = sz;
            //panel.BackColor = Color.FromArgb(0);

            _animcurpanel = panel;

            PanelTimer.Enabled = true;
            panel.Visible = true;
        }

        void FormatButtonColor()
        {

        }

        private void ckWindowsExplorer_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                foreach (var junk in AllSections.Where(u => u.ID == 1).FirstOrDefault().Junks)
                    junk.Enabled = true;

            }
            else
            {
                foreach (var junk in AllSections.Where(u => u.ID == 1).FirstOrDefault().Junks)
                    junk.Enabled = false;
            }
            MainMethods.SaveCleanSettins(AllSections);
        }

        private void ckSystem_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                foreach (var junk in AllSections.Where(u => u.ID == 2).FirstOrDefault().Junks)
                    junk.Enabled = true;

            }
            else
            {
                foreach (var junk in AllSections.Where(u => u.ID == 2).FirstOrDefault().Junks)
                    junk.Enabled = false;
            }
            MainMethods.SaveCleanSettins(AllSections);
        }

        private void ckWindowsApps_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                foreach (var junk in AllSections.Where(u => u.ID == 8).FirstOrDefault().Junks)
                    junk.Enabled = true;

            }
            else
            {
                foreach (var junk in AllSections.Where(u => u.ID == 8).FirstOrDefault().Junks)
                    junk.Enabled = false;
            }
            MainMethods.SaveCleanSettins(AllSections);
        }

        private void ckAdvanced_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                foreach (var junk in AllSections.Where(u => u.ID == 3).FirstOrDefault().Junks)
                    junk.Enabled = true;

            }
            else
            {
                foreach (var junk in AllSections.Where(u => u.ID == 3).FirstOrDefault().Junks)
                    junk.Enabled = false;
            }
            MainMethods.SaveCleanSettins(AllSections);
        }

        private void ckUtilities_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                foreach (var junk in AllSections.Where(u => u.ID == 7).FirstOrDefault().Junks)
                    junk.Enabled = true;

            }
            else
            {
                foreach (var junk in AllSections.Where(u => u.ID == 7).FirstOrDefault().Junks)
                    junk.Enabled = false;
            }
            MainMethods.SaveCleanSettins(AllSections);
        }

        private void chkFilExt_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                AllSectionsRegistry.Where(u => u.ID == 1).FirstOrDefault().Enabled = true;

            }
            else
            {
                AllSectionsRegistry.Where(u => u.ID == 1).FirstOrDefault().Enabled = false;
            }
            MainMethods.SaveRegistrySettins(AllSectionsRegistry);
        }

        private void chkrun_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                AllSectionsRegistry.Where(u => u.ID == 2).FirstOrDefault().Enabled = true;

            }
            else
            {
                AllSectionsRegistry.Where(u => u.ID == 2).FirstOrDefault().Enabled = false;
            }
            MainMethods.SaveRegistrySettins(AllSectionsRegistry);
        }

        private void chkAppPath_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                AllSectionsRegistry.Where(u => u.ID == 3).FirstOrDefault().Enabled = true;

            }
            else
            {
                AllSectionsRegistry.Where(u => u.ID == 3).FirstOrDefault().Enabled = false;
            }
            MainMethods.SaveRegistrySettins(AllSectionsRegistry);
        }

        private void chkMui_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                AllSectionsRegistry.Where(u => u.ID == 4).FirstOrDefault().Enabled = true;

            }
            else
            {
                AllSectionsRegistry.Where(u => u.ID == 4).FirstOrDefault().Enabled = false;
            }
            MainMethods.SaveRegistrySettins(AllSectionsRegistry);
        }

        private void chkUnistaller_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                AllSectionsRegistry.Where(u => u.ID == 5).FirstOrDefault().Enabled = true;

            }
            else
            {
                AllSectionsRegistry.Where(u => u.ID == 5).FirstOrDefault().Enabled = false;
            }
            MainMethods.SaveRegistrySettins(AllSectionsRegistry);
        }

        private void chkObsoApp_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                AllSectionsRegistry.Where(u => u.ID == 6).FirstOrDefault().Enabled = true;

            }
            else
            {
                AllSectionsRegistry.Where(u => u.ID == 6).FirstOrDefault().Enabled = false;
            }
            MainMethods.SaveRegistrySettins(AllSectionsRegistry);
        }

        private void timer_HideOrShowAV(object sender, EventArgs e)
        {
            if(_curWindowState == WindowsState.Show)
            {
                //show animation
                if (Opacity < 1 )
                {
                    if(Opacity < 1)
                        Opacity += .1;
                }
                else
                {
                    timerHideOrShowAV.Enabled = false;
                }

            }
            else if (_curWindowState == WindowsState.Hide)
            {
                //hide animation
                if (Opacity > 0)
                    Opacity -= .1;
                else
                {
                    timerHideOrShowAV.Enabled = false;
                    this.Hide();
                }
            }
            else
            {
                //minimize animation
                if (Opacity > 0)
                    Opacity -= .1;
                else
                {
                    timerHideOrShowAV.Enabled = false;
                    this.Hide();
                }
            }
        }
        private void timer_PanelAnimation(object sender, EventArgs e)
        {
            //panel Y position xloc height yloc
            //panel x location
            System.Drawing.Point loc = _animcurpanel.Location;
            System.Drawing.Size sz = _animcurpanel.Size;

            if (loc.X > xloc + 50 || sz.Height < yloc - 20 )
            {
                if (loc.X > xloc + 50)
                {
                    loc.X -= 50;
                    _animcurpanel.Location = loc;
                }
                else if(sz.Height < yloc - 20)
                {
                    loc.X = xloc;
                    _animcurpanel.Location = loc;
                    sz.Height += 20;
                    _animcurpanel.Size = sz;
                }
            }
            else
            {
                loc.X = xloc;
                sz.Height = yloc;
                _animcurpanel.Location = loc;
                _animcurpanel.Size = sz;
                //_curPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                PanelTimer.Enabled = false;
            }
        }

        
        bool IsMoving = false;
        Point LastPos = new Point();
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMoving)
            {
                this.Location = new Point(e.X + (this.Location.X - LastPos.X), e.Y + (this.Location.Y - LastPos.Y));
                Update();
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            IsMoving = true;
            LastPos = e.Location;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            IsMoving = false;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlSetting);
            FormatButtonColor();
        }

        private void btnTargetedScan_Click(object sender, EventArgs e)
        {
            scanType = 2;
            folderBrowserDialog1.Description = "Select your folder or drive";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                loc_to_search = folderBrowserDialog1.SelectedPath;
                ShowPanel(pnlFormScan);
                this.BackgroundImage = null;
                this.BackColor = Color.WhiteSmoke;
                panelLeftbar.Visible = false;
                search = new Thread(new ThreadStart(ScanFolder));
                search.Start();
            }
        }

        private void btnFullScan_Click(object sender, EventArgs e)
        {
            scanType = 3;
            ShowPanel(pnlFormScan);
            this.BackgroundImage = null;
            this.BackColor = Color.WhiteSmoke;
            panelLeftbar.Visible = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }
        private void launcherIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
                this.hideAntiVirus();
            }
            else
            {
                this.showAntiVirus();
            }
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            this.showAntiVirus();
        }

        private void hideAntiVirus()
        {
            this._curWindowState = WindowsState.Hide;
            this.Opacity = 1;
            timerHideOrShowAV.Enabled = true;
        }
        private void showAntiVirus()
        {
            this._curWindowState = WindowsState.Show;
            this.Opacity = 0D;
            timerHideOrShowAV.Enabled = true;

            this.ActiveControl = null;

            this.Show();
        }
        private void minimizeAntiVirus()
        {
            this._curWindowState = WindowsState.Minimize;
            this.Opacity = 1;
            timerHideOrShowAV.Enabled = true;
        }
        private void closeItem_Click(object sender, EventArgs e)
        {
            Program.tuneAppearanceInPerformance = this.flagAppearanceInPerformance;
            Program.tuneAutomaticUpdates = this.flagAutomaticUpdates;
            Program.tuneDesktopCleanUpWizard = this.flagDesktopCleanUpWizard;
            Program.tuneFeatureUpdates = this.flagFeatureUpdates;
            Program.tuneMenuShowDelay = this.flagSpeedUpMenuShowDelay;
            Program.tuneQuickAccessHistory = this.flagQuickAccessHistory;
            Program.tuneSensorService = this.flagSensorService;
            Program.tuneStartMenuAds = this.flagStartMenuAds;
            Program.optClearCache = this.materialCheckBoxClearCache.Checked;
            Program.optClearMemory = this.materialCheckBoxClearMemory.Checked;
            Program.RealTimeProtection = this.flagRealTimeProtection;
            Program.AutoUSBScanner = this.flagAutoScanUSB;
            Program.flagPhishing = this.flagPhishing;
            Program.flagCryptojacking = this.flagCryptojacking;
            Program.flagRansomware = this.flagRansomware;
            Program.flagAffiliateOffers = this.flagAffiliateOffers;

            Program.SaveConfigs();
            Application.Exit();
        }

        //log manipuation
        public static void PushLog(string message)
        {
            File.AppendAllText("log.txt", message);
        }

        private async void btnClearMemory_Click(object sender, EventArgs e)
        {
            this.btnClearMemory.Enabled = false;
            if (materialCheckBoxClearMemory.Checked)
            {
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "Clear Memory is running! Your PC will be better!", ToolTipIcon.Info);
                await Optimize.ClearMemory(0);
            }
            if (materialCheckBoxClearCache.Checked)
            {
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "Clear FileSystem Caches is running! Your PC will be better!", ToolTipIcon.Info);
                await Optimize.ClearMemory(2);
            }
            this.btnClearMemory.Enabled = true;
        }

        private async void btnRegistry_Click(object sender, EventArgs e)
        {
            this.btnRegistry.Enabled = false;
            await Task.Run(delegate () 
            {
                try
                {
                    Utilities.EnableCommandPrompt();
                    Utilities.EnableControlPanel();
                    Utilities.EnableFolderOptions();
                    Utilities.EnableRunDialog();
                    Utilities.EnableContextMenu();
                    Utilities.EnableTaskManager();
                    Utilities.EnableRegistryEditor();
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "Registry cleaned", ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    frmMain.PushLog("MainForm.CleanPC" + ex.Message + ex.StackTrace);
                }

            });
            this.btnRegistry.Enabled = true;
        }

        private async void btnCleaner_Click(object sender, EventArgs e)
        {
            frmCleaner cleaner = new frmCleaner();
            cleaner.ShowDialog();

            //this.btnCleaner.Enabled = false;
            //await CleanHelper.Cleaner();
            //MessageBox.Show("Cleaned");
            //this.btnCleaner.Enabled = true;
        }

        private async void btnTemporary_Click(object sender, EventArgs e)
        {
            this.btnTemporary.Enabled = false;
            await Task.Run(delegate  () 
            {
                try
                {
                    CleanHelper.CleanTemporaries();
                }
                catch (Exception ex)
                {
                    frmMain.PushLog("MainForm.CleanPC" + ex.Message + ex.StackTrace);
                }

                MessageBox.Show("Temporary cleaned");
            });
            this.btnTemporary.Enabled = true;
        }

        private List<string> GetAllRunningProcesses()
        {
            List<string> list = new List<string>();
            var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            };
                foreach (var item in query)
                {
                    list.Add(item.Path);
                }
            }
            return list;
        }

        // infected file details to list
        private void AddToResult(string file, string vir_name)
        {
            string[] row1 = { vir_name };
            Invoke(new Action(delegate
            {
                try
                {
                    ListViewItem item = objectListView1.FindItemWithText(file);
                    if (item == null)
                    {
                        objectListView1.AddObject(new InfectionObject(file, vir_name));
                    }
                }
                catch { }
            }));
        }

        // check if a path is drive or not
        public static bool IsLogicalDrive(string path)
        {
            bool IsRoot = false;
            DirectoryInfo d = new DirectoryInfo(path);
            if (d.Parent == null) { IsRoot = true; }
            return IsRoot;
        }

        // get the files from the given path
        public string[] getFiles(string SourceFolder, string Filter, System.IO.SearchOption searchOption)
        {
            ArrayList alFiles = new ArrayList();
            string[] MultipleFilters = Filter.Split('|');
            if (IsLogicalDrive(SourceFolder))
            {
                foreach (string d in Directory.GetDirectories(SourceFolder))
                {
                    foreach (string FileFilter in MultipleFilters)
                    {
                        try
                        {
                            foreach (string name in Directory.GetFiles(d, FileFilter, searchOption))
                            {
                                Invoke(new Action(delegate
                                {
                                    curr_File.Text = name;
                                }));
                            }

                            alFiles.AddRange(Directory.GetFiles(d, FileFilter, searchOption));
                        }
                        catch { continue; }
                    }
                }
            }
            else
            {
                foreach (string FileFilter in MultipleFilters)
                {
                    try
                    {
                        alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
                    }
                    catch { continue; }
                }
            }

            return (string[])alFiles.ToArray(typeof(string));
        }


        //scan a folder
        private void ScanFolder()
        {
            ArrayList fileList = new ArrayList();
            string temp = my_location + "\\engine\\custom\\";

            //if it's a quick scan, first check all the running processes
            if (scanType == 1)
            {
                foreach (string proc in GetAllRunningProcesses())
                {
                    if (System.IO.File.Exists(proc))
                    {
                        try
                        {
                            Invoke(new Action(delegate
                            {
                                try
                                {
                                    curr_File.Text = proc;
                                }
                                catch { }
                            }));
                            var clam = new ClamClient("localhost", 3310);
                            var scanResult = clam.ScanFileOnServer(proc);

                            switch (scanResult.Result)
                            {
                                case ClamScanResults.VirusDetected:
                                    infected++;
                                    AddToResult(proc, scanResult.InfectedFiles.First().VirusName);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            frmMain.PushLog(ex.Message);
                        }
                    }
                }
                Invoke(new Action(delegate
                {
                    try
                    {
                        curr_File.Text = "...";
                    }
                    catch { }
                }));
            }

            if (scanType == 0)      // smart scan
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        fileList.AddRange(getFiles(drive.Name, smart_ext, SearchOption.TopDirectoryOnly));
                    }
                    catch { }
                }
                files = (string[])fileList.ToArray(typeof(string));
            }
            if (scanType == 1)      // quick scan
            {
                files = getFiles(loc_to_search, wildcard, SearchOption.AllDirectories);
            }
            if (scanType == 2)      // targeted scan
            {
                files = getFiles(loc_to_search, wildcard, SearchOption.AllDirectories);
            }
            if (scanType == 3)      // full scan
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    fileList.AddRange(getFiles(drive.Name, smart_ext, SearchOption.AllDirectories));
                }
                files = (string[])fileList.ToArray(typeof(string));
            }
            if (scanType == 4)      // usb scan
            {
                files = getFiles(loc_to_search, wildcard, SearchOption.AllDirectories);
            }

            int total = files.Length;
            int fileNum = 0;
            foreach (string file in files)
            {
                fileNum++;

                if (System.IO.File.Exists(file))
                {
                    try
                    {
                        Invoke(new Action(delegate
                        {
                            try
                            {
                                curr_File.Text = file;
                            }
                            catch { }
                        }));
                        var clam = new ClamClient("localhost", 3310);
                        var scanResult = clam.ScanFileOnServer(file);

                        /* clamAV engine */
                        switch (scanResult.Result)
                        {
                            case ClamScanResults.VirusDetected:
                                infected++;
                                AddToResult(file, scanResult.InfectedFiles.First().VirusName);
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        frmMain.PushLog(ex.Message);
                    }

                }

                Invoke(new Action(delegate
                {
                    try
                    {
                        if (total != 0)
                        {
                            hopeProgressBar1.ValueNumber = (int)(fileNum * 100 / total);
                        }
                        else
                        {
                            hopeProgressBar1.ValueNumber = 0;
                        }
                    }
                    catch { }
                }));
            }

            files = null; // allow garbage collector to collect it
            Invoke(new Action(delegate
            {
                try
                {
                    this.labelTestScanning.Visible = false;
                    if(infected == 0)
                    {
                        //curr_File.Text = "Scan finished!\nYour computer is now safer";
                        curr_File.Visible = false;
                        labelTextScanFinished.Visible = true;
                        labelTextYourComputer.Visible = true;
                        btnStopScan.Text = "           DONE           ";
                        flagScanStatus = 1;
                    }
                    else
                    {
                        curr_File.Text = "Scan finished!\n Infected Files: " + infected.ToString();
                        btnStopScan.Text = "           NEXT           ";
                        flagScanStatus = 2;
                    }
                }
                catch { }
            }));

        }

        private void FormatScan()
        {
            hopeProgressBar1.Visible = true;
            labelTestScanning.Visible = true;
            labelCheckTick3.Visible = true;
            labelTextScanStopped.Visible = false;
            labelNotice.Visible = false;
            labelTextScanFinished.Visible = false;
            labelTextYourComputer.Visible = false;
            curr_File.Visible = true;
        }

        private void btnStopScan_Click(object sender, EventArgs e)
        {
            switch(flagScanStatus)
            {
                case 0: //stop
                    search.Abort();
                    hopeProgressBar1.Visible = false;
                    labelTestScanning.Visible = false;
                    labelCheckTick3.Visible = false;
                    labelTextScanStopped.Visible = true;
                    labelNotice.Visible = true;
                    curr_File.Text = "You stopped the scan before we could find any hidden threats.";
                    btnStopScan.Text = "              OK              ";
                    flagScanStatus = 3;
                    break;
                case 1: //done
                    FormatScan();
                    btnStopScan.Text = "STOP SCAN";
                    flagScanStatus = 0;
                    panelLeftbar.Visible = true;
                    materialButtonSetting.Enabled = true;
                    ShowPanel(_curpanel);
                    //StripButtonColor(panel3);
                    scanType = -1;
                    break;
                case 2: //next
                    FormatScan();
                    btnStopScan.Text = "STOP SCAN";
                    flagScanStatus = 0;
                    panelLeftbar.Visible = false;
                    ShowPanel(pnlDelete);
                    btnCancel.Enabled = true;
                    this.BackgroundImage = null;
                    this.BackColor = Color.WhiteSmoke;
                    infected = 0;
                    break;
                case 3: //ok
                    FormatScan();
                    btnStopScan.Text = "STOP SCAN";
                    flagScanStatus = 0;
                    panelLeftbar.Visible = true;
                    materialButtonSetting.Enabled = true;
                    ShowPanel(_curpanel);
                    //StripButtonColor(panel3);
                    scanType = -1;
                    break;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if(flagDeleteStatus == 0)
            {
                try
                {
                    string path = string.Empty;
                    for (int i = 0; i < objectListView1.Items.Count; i++)
                    {
                        path = objectListView1.Items[i].SubItems[0].Text;
                        try
                        {
                            KillProcess(this.Handle, path); // try to kill the process before deleting it
                            System.IO.File.Delete(path);
                        }
                        catch { System.IO.File.Delete(path); }
                    }
                    objectListView1.Items.Clear();
                    btnDelete.Text = "        DONE        ";
                    btnCancel.Enabled = false;
                    labelTextGoodForYou.Visible = true;
                    labelThreatsRevomed.Visible = true;
                    objectListView1.Visible = false;
                    flagDeleteStatus = 1;
                }
                catch { }
            }
            else
            {
                flagDeleteStatus = 0;
                panelLeftbar.Visible = true;
                btnDelete.Text = "      DELETE      ";
                labelTextGoodForYou.Visible = false;
                labelThreatsRevomed.Visible = false;
                objectListView1.Visible = true;
                ShowPanel(pnlShield);
                //StripButtonColor(panel3);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            panelLeftbar.Visible = true;
            ShowPanel(pnlShield);
            //StripButtonColor(panel3);
        }

        private void btnUSBVaccination_Click(object sender, EventArgs e)
        {
            frmUSB dlg = new frmUSB();
            dlg.Show();
        }

        private void btnSystemInfo_Click(object sender, EventArgs e)
        {
            frmSysInfo dlg = new frmSysInfo();
            dlg.Show();
        }

        private void RunSmartScan()
        {
            scanType = 0;
            ShowPanel(pnlFormScan);
            this.BackgroundImage = null;
            this.BackColor = Color.WhiteSmoke;
            panelLeftbar.Visible = false;
            materialButtonSetting.Enabled = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        // Ransomware protection
        private void RunRansomwareProtection()
        {
            if (strArray != null)
            {
                if (RestartManager.RmStartSession(out handle, 0, Guid.NewGuid().ToString()) != 0)
                {
                    throw new Exception("Could not start session.");
                }
                
                if (RestartManager.RmRegisterResources(handle, (uint)strArray.Length, strArray, 0, null, 0, null) != 0)
                {
                    throw new Exception("Could not register resources");
                }
            }

            procIds = new HashSet<int>();
            newProcIds = new HashSet<int>();
            runThread = new Thread(Loop);
            runThread.Start();
        }

        private void Loop()
        {
            while (true)
            {
                try
                {
                    Step();
                }
                catch (Exception ex)
                {
                    frmMain.PushLog("Error: " + ex.Message);
                }
                Thread.Sleep(0);
            }
        }

        private void Step()
        {
            int hResult = RestartManager.RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);
            if (hResult != RestartManager.ERROR_MORE_DATA)
            {
                if (hResult != 0)
                    throw new Exception(string.Format("Get List thrown {0}", hResult));
                return;
            }
            if (pnProcInfoNeeded == 0)
                return;
            frmMain.PushLog("Detected" + pnProcInfoNeeded.ToString());
            if (processInfo == null || processInfo.Length != pnProcInfoNeeded)
                processInfo = new RmProcesInfo[pnProcInfoNeeded];
            hResult = RestartManager.RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
            if (hResult != RestartManager.ERROR_MORE_DATA && hResult != 0)
                throw new Exception(string.Format("Get List thrown {0}", hResult));
            newProcIds.Clear();
            foreach (RmProcesInfo procInfo in processInfo)
            {
                try
                {
                    using (Process proc = Process.GetProcessById(procInfo.process.dwProcessId))
                    {
                        frmMain.PushLog("Detected process" + proc.ProcessName + " is playing with one of your file!");
                        bool filter = true;
                        if (!procIds.Contains(procInfo.process.dwProcessId))
                            if (ProcessFilter != null)
                                filter = ProcessFilter(proc);
                        if (filter)
                        {
                            newProcIds.Add(procInfo.process.dwProcessId);
                            proc.Kill();
                        }
                    }
                }
                catch (Exception ex)
                {
                    frmMain.PushLog("Error: " + ex.Message);
                }
            }
            HashSet<int> temp = procIds;
            procIds = newProcIds;
            newProcIds = temp;
        }

        private void quickScanItem_Click(object sender, EventArgs e)    // launcher menu quick scan
        {
            this.showAntiVirus();
            scanType = 1;
            loc_to_search = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            ShowPanel(pnlFormScan);
            this.BackgroundImage = null;
            this.BackColor = Color.WhiteSmoke;
            panelLeftbar.Visible = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        private void updateDatabaseItem_Click(object sender, EventArgs e)   // launcher menu update database
        {

        }

        private void silentModeItem_Click(object sender, EventArgs e)
        {
            if (!flagSilent)
            {
                labelCheck.Image = global::Avaxi.Properties.Resources.check;
                    labelTextOtherScans.Text = "Silent mode is enabled. Enjoy!";
                labelTextOtherScans.ForeColor = Color.White;
                flagSilent = true;
                silentModeItem.Text = "Disable Silent Mode";
            }
            else
            {
                if (this.flagRealTimeProtection)
                {
                    labelCheck.Image = global::Avaxi.Properties.Resources.check;
                    labelTextOtherScans.Text = "The live protection is enabled";
                    labelTextOtherScans.ForeColor = Color.White;
                }
                else
                {
                    labelCheck.Image = global::Avaxi.Properties.Resources.cross;
                    labelTextOtherScans.Text = "The live protection is disabled";
                    labelTextOtherScans.ForeColor = Color.Gray;
                }
                flagSilent = false;
                silentModeItem.Text = "Enable Silent Mode";
            }
        }

        private void btnRunSmartScan1_Click(object sender, EventArgs e)
        {
            //RunSmartScan();
        }    

        private void btnRunSmartScan2_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void btnRunSmartScan3_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void btnTune_Click(object sender, EventArgs e)
        {
            Computer regedit = new Computer();

            this.progressIndicatorTuneUp.Visible = true;

            if (this.flagAutomaticUpdates == true)
            {
                OptimizeSpeedUp.DisableAutomaticUpdates();
            }
            else
            {
                OptimizeSpeedUp.EnableAutomaticUpdates();
            }

            if (this.flagDesktopCleanUpWizard == true)
            {
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\CleanupWiz");
                regedit.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\CleanupWiz", true).SetValue("NoRun", 1, RegistryValueKind.DWord);
            }
            else
            {
            }

            if (this.flagSpeedUpMenuShowDelay == true)
            {
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Control Panel\Desktop");
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics");
                regedit.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true).SetValue("MenuShowDelay", "50");
                regedit.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\WindowMetrics", true).SetValue("MinAnimate", "50");
            }
            else
            {
            }

            if (this.flagAppearanceInPerformance == true)
            {
                OptimizeSpeedUp.EnableDarkTheme();
            }
            else
            {
                OptimizeSpeedUp.EnableLightTheme();
            }

            if (this.flagQuickAccessHistory == true)
            {
                OptimizeSpeedUp.DisableQuickAccessHistory();
            }
            else
            {
                OptimizeSpeedUp.EnableQuickAccessHistory();
            }

            if (this.flagStartMenuAds == true)
            {
                OptimizeSpeedUp.DisableStartMenuAds();
            }
            else
            {
                OptimizeSpeedUp.EnableStartMenuAds();
            }

            if (this.flagSensorService == true)
            {
                OptimizeSpeedUp.DisableSensorServices();
            }
            else
            {
                OptimizeSpeedUp.EnableSensorServices();
            }

            if (this.flagFeatureUpdates == true)
            {
                OptimizeSpeedUp.DisableForcedFeatureUpdates();
            }
            else
            {
                OptimizeSpeedUp.EnableForcedFeatureUpdates();
            }

            DialogResult result = System.Windows.Forms.MessageBox.Show("Restart you computer?", "SpeedUp", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Utilities.Reboot();
            }
            this.progressIndicatorTuneUp.Visible = false;
        }

        /*
         * Tune Up : switch setting
         * */
        private void switchLabelAutomaticUpdates_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelDesktopCleanUpWizard_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelSpeedUpMenuShowDelay_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelAppearanceInPerformance_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelQuickAccessHistory_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelStartMenuAds_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelSensorService_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelFeatureUpdates_Click(object sender, EventArgs e)
        {

        }

        private void switchLabelRealTimeProtection_Click(object sender, EventArgs e)
        {
            if (!this.flagRealTimeProtection)
            {
                this.flagRealTimeProtection = true;
                this.switchLabelRealTimeProtection.Image = Avaxi.Properties.Resources.set_yes;
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "The live protection is enabled", ToolTipIcon.Info);
                labelCheck.Image = global::Avaxi.Properties.Resources.check;
                labelTextOtherScans.Text = "The live protection is enabled";
                labelTextOtherScans.ForeColor = Color.White;
            }
            else
            {
                this.flagRealTimeProtection = false;
                this.switchLabelRealTimeProtection.Image = Avaxi.Properties.Resources.set_no;
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "The live protection is disabled", ToolTipIcon.Info);
                labelCheck.Image = global::Avaxi.Properties.Resources.cross;
                labelTextOtherScans.Text = "The live protection is disabled";
                labelTextOtherScans.ForeColor = Color.Gray;
            }
        }

        private void switchLabelFireWall_Click(object sender, EventArgs e)
        {
            //reflect firewall settings
            if (!this.flagFireWall)
            {
                this.flagFireWall = true;
                this.switchLabelFireWall.Image = Avaxi.Properties.Resources.set_yes;
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "Firewall is turned on successfully", ToolTipIcon.Info);
                Task.Run(delegate ()
                {
                    firewall.FirewallStart(true);
                    PushLog("Firewall is turned on sucessfully.");
                });
            }
            else
            {
                this.flagFireWall = false;
                this.switchLabelFireWall.Image = Avaxi.Properties.Resources.set_no;
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "Firewall is turned off successfully", ToolTipIcon.Info);
                Task.Run(delegate ()
                {
                    shell.RunExternalExe("netsh.exe", "Firewall set opmode disable");
                    PushLog("Firewall is turned off sucessfully.");
                });
            }
        }

        private void switchLabelAutoScanUSB_Click(object sender, EventArgs e)
        {
            if (this.flagAutoScanUSB)
            {
                this.flagAutoScanUSB = false;
                this.switchLabelAutoScanUSB.Image = Avaxi.Properties.Resources.set_no;
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "AutoScan USB is disabled", ToolTipIcon.Info);
            }
            else
            {
                this.flagAutoScanUSB = true;
                this.switchLabelAutoScanUSB.Image = Avaxi.Properties.Resources.set_yes;
                if(!flagSilent)
                    launcherIcon.ShowBalloonTip(5, "Avaxi", "AutoScan USB is enabled", ToolTipIcon.Info);
            }
        }

        private void func_labelMoveMove_Close(object sender, MouseEventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#B72828");
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void materialButtonClose_Click(object sender, EventArgs e)
        {
            //Task.Run(delegate ()
            //{
            //    Thread.Sleep(100);
                this.hideAntiVirus();
            //});
        }

        private void materialButtonMinimize_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Minimized;

            this.minimizeAntiVirus();
        }

        private void materialButtonSetting_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlSetting);
            FormatButtonColor();
        }

        private void materialButtonSignUp_Click(object sender, EventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("Sign up");
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void materialBtnRunSmartScan_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            Computer regedit = new Computer();

            this.progressIndicatorTuneUp.Visible = true;

            if (this.flagAutomaticUpdates == true)
            {
                OptimizeSpeedUp.DisableAutomaticUpdates();
            }
            else
            {
                OptimizeSpeedUp.EnableAutomaticUpdates();
            }

            if (this.flagDesktopCleanUpWizard == true)
            {
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\CleanupWiz");
                regedit.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\CleanupWiz", true).SetValue("NoRun", 1, RegistryValueKind.DWord);
            }
            else
            {
            }

            if (this.flagSpeedUpMenuShowDelay == true)
            {
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Control Panel\Desktop");
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics");
                regedit.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true).SetValue("MenuShowDelay", "50");
                regedit.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\WindowMetrics", true).SetValue("MinAnimate", "50");
            }
            else
            {
            }

            if (this.flagAppearanceInPerformance == true)
            {
                OptimizeSpeedUp.EnableDarkTheme();
            }
            else
            {
                OptimizeSpeedUp.EnableLightTheme();
            }

            if (this.flagQuickAccessHistory == true)
            {
                OptimizeSpeedUp.DisableQuickAccessHistory();
            }
            else
            {
                OptimizeSpeedUp.EnableQuickAccessHistory();
            }

            if (this.flagStartMenuAds == true)
            {
                OptimizeSpeedUp.DisableStartMenuAds();
            }
            else
            {
                OptimizeSpeedUp.EnableStartMenuAds();
            }

            if (this.flagSensorService == true)
            {
                OptimizeSpeedUp.DisableSensorServices();
            }
            else
            {
                OptimizeSpeedUp.EnableSensorServices();
            }

            if (this.flagFeatureUpdates == true)
            {
                OptimizeSpeedUp.DisableForcedFeatureUpdates();
            }
            else
            {
                OptimizeSpeedUp.EnableForcedFeatureUpdates();
            }

            DialogResult result = System.Windows.Forms.MessageBox.Show("Restart you computer?", "SpeedUp", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Utilities.Reboot();
            }
            this.progressIndicatorTuneUp.Visible = false;
        }

        // private void materialBtnRunSmartScan_MoseEnter(object sender, EventArgs e)
        // {
        // 
        // }
        // 
        // private void materialBtnRunSmartScan_MoseLeave(object sender, EventArgs e)
        // {
        // 
        // }

        async private void materialButtonOptimize_Click(object sender, EventArgs e)
        {
            this.materialButtonOptimize.Enabled = false;
            if (!flagSilent)
                launcherIcon.ShowBalloonTip(5, "Avaxi", "Clear Momery is running! Your PC will be better!", ToolTipIcon.Info);
            await Optimize.ClearMemory(0);
            if (!flagSilent)
                launcherIcon.ShowBalloonTip(5, "Avaxi", "Clear Working Sets is running! Your PC will be better!", ToolTipIcon.Info);
            await Optimize.ClearMemory(1);
            if (!flagSilent)
                launcherIcon.ShowBalloonTip(5, "Avaxi", "Clear FileSystem Caches is running! Your PC will be better!", ToolTipIcon.Info);
            await Optimize.ClearMemory(2);
            if (!flagSilent)
                launcherIcon.ShowBalloonTip(5, "Avaxi", "Fill Ram Data is running! Your PC will be better!", ToolTipIcon.Info);
            await Optimize.ClearMemory(3);
            this.materialButtonOptimize.Enabled = true;
        }

        // private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        // {
        // 
        // }

        private void materialButtonShield_Click(object sender, EventArgs e)
        {
            _curpanel = pnlShield;
            ShowPanel(pnlShield);
        }

        private void materialButtonCrypto_Click(object sender, EventArgs e)
        {
            _curpanel = pnlCrypto;
            ShowPanel(pnlCrypto);
        }

        private void materialButtonScan_Click(object sender, EventArgs e)
        {
            _curpanel = pnlScan;
            ShowPanel(pnlScan);
        }

        private void materialBtnOptimaize_Click(object sender, EventArgs e)
        {
            _curpanel = pnlOptimize;
            ShowPanel(pnlOptimize);
        }

        private void materialBtnTuneUp_Click(object sender, EventArgs e)
        {
            _curpanel = pnlTuneUp;
            ShowPanel(pnlTuneUp);
        }

        private void materialButtonAntiPishing_Click(object sender, EventArgs e)
        {

            if (flagPhishing)
            {
                // Phishing disable code

                // Status page view
                btnPhishing.ForeColor = Color.Gray;
                btnPhishing.Text = "Disabled";
                btnPhishing.BaseColor = Color.FromArgb(209, 209, 209);
                btnPhishing.OverColor = Color.FromArgb(209, 209, 209);
                btnPhishing.DownColor = Color.FromArgb(209, 209, 209);
                labelWebsecGrey.Image = global::Avaxi.Properties.Resources.websec_status_grey;
                materialButtonAntiPishing.Icon = global::Avaxi.Properties.Resources.AntiPhishing_grey;
                labelCheckTick1.Visible = false;
                labelTextWebProtection.ForeColor = Color.Gray;
                flagPhishing = false;
            }
            else
            {
                // Phishing enable code

                // Status page view
                btnPhishing.ForeColor = Color.White;
                btnPhishing.Text = "Enabled";
                btnPhishing.BaseColor = Color.FromArgb(15, 220, 200);
                btnPhishing.OverColor = Color.FromArgb(15, 220, 200);
                btnPhishing.DownColor = Color.FromArgb(15, 220, 200);
                labelWebsecGrey.Image = global::Avaxi.Properties.Resources.websec_status;
                materialButtonAntiPishing.Icon = global::Avaxi.Properties.Resources.AntiPhishing;
                labelCheckTick1.Visible = true;
                labelTextWebProtection.ForeColor = Color.DeepSkyBlue;
                flagPhishing = true;
            }
        }

        private void materialButton1_Click_1(object sender, EventArgs e)
        {

            if (flagCryptojacking)
            {
                // Crytojacking disable code

                // Status page view
                btnCryptojacking.ForeColor = Color.Gray;
                btnCryptojacking.Text = "Disabled";
                btnCryptojacking.BaseColor = Color.FromArgb(209, 209, 209);
                btnCryptojacking.OverColor = Color.FromArgb(209, 209, 209);
                btnCryptojacking.DownColor = Color.FromArgb(209, 209, 209);
                labelCryptoGrey.Image = global::Avaxi.Properties.Resources.crypto_grey;
                materialButtonAntiCrypto.Icon = global::Avaxi.Properties.Resources.AntiCryptojacking_grey;
                labelCheckTick2.Visible = false;
                lableTextCryptoProtection.ForeColor = Color.Gray;
                flagCryptojacking = false;
            }
            else
            {
                // Crytojacking enable code

                // Status page view
                btnCryptojacking.ForeColor = Color.White;
                btnCryptojacking.Text = "Enabled";
                btnCryptojacking.BaseColor = Color.FromArgb(15, 220, 200);
                btnCryptojacking.OverColor = Color.FromArgb(15, 220, 200);
                btnCryptojacking.DownColor = Color.FromArgb(15, 220, 200);
                labelCryptoGrey.Image = global::Avaxi.Properties.Resources.crypto;
                materialButtonAntiCrypto.Icon = global::Avaxi.Properties.Resources.AntiCryptojacking;
                labelCheckTick2.Visible = true;
                lableTextCryptoProtection.ForeColor = Color.DeepSkyBlue;
                flagCryptojacking = true;
            }
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            if (flagAffiliateOffers)
            {
                // Affiliate Offers disable code

                // status view
                btnAffiliateOffers.ForeColor = Color.Gray;
                btnAffiliateOffers.Text = "Disabled";
                btnAffiliateOffers.BaseColor = Color.FromArgb(209, 209, 209);
                btnAffiliateOffers.OverColor = Color.FromArgb(209, 209, 209);
                btnAffiliateOffers.DownColor = Color.FromArgb(209, 209, 209);
                materialButtonAntiAffilate.Icon = global::Avaxi.Properties.Resources.AntiAffiliate_grey;
                flagAffiliateOffers = false;
            }
            else
            {
                // Affiliate Offers enable code

                // status view
                btnAffiliateOffers.ForeColor = Color.White;
                btnAffiliateOffers.Text = "Enabled";
                btnAffiliateOffers.BaseColor = Color.FromArgb(15, 220, 200);
                btnAffiliateOffers.OverColor = Color.FromArgb(15, 220, 200);
                btnAffiliateOffers.DownColor = Color.FromArgb(15, 220, 200);
                materialButtonAntiAffilate.Icon = global::Avaxi.Properties.Resources.AntiAffiliate;
                flagAffiliateOffers = true;
            }
        }

        private void materialButtonAntiRansomware_Click(object sender, EventArgs e)
        {
            if (flagRansomware)
            {
                // Ransomware disable code
                runThread.Abort();

                // Status page view
                btnRansomware.ForeColor = Color.Gray;
                btnRansomware.Text = "Disabled";
                btnRansomware.BaseColor = Color.FromArgb(209, 209, 209);
                btnRansomware.OverColor = Color.FromArgb(209, 209, 209);
                btnRansomware.DownColor = Color.FromArgb(209, 209, 209);
                labelShieldGrey.Image = global::Avaxi.Properties.Resources.shield_status_grey;
                materialButtonAntiRansomware.Icon = global::Avaxi.Properties.Resources.AntiRansomware_grey;
                labelCheckTick.Visible = false;
                labelTextLiveProtectionEnabled.ForeColor = Color.Gray;
                flagRansomware = false;
            }
            else
            {
                frmRansomware dlg = new frmRansomware();
                dlg.ShowDialog();

                // Ransomware enable code
                if (strArray == null)
                {
                    MessageBox.Show("Please select files to protect.");
                    return;
                }

                RunRansomwareProtection();
                // Status page view
                btnRansomware.ForeColor = Color.White;
                btnRansomware.Text = "Enabled";
                btnRansomware.BaseColor = Color.FromArgb(15, 220, 200);
                btnRansomware.OverColor = Color.FromArgb(15, 220, 200);
                btnRansomware.DownColor = Color.FromArgb(15, 220, 200);
                labelShieldGrey.Image = global::Avaxi.Properties.Resources.shield_status;
                materialButtonAntiRansomware.Icon = global::Avaxi.Properties.Resources.AntiRansomware;
                labelCheckTick.Visible = true;
                labelTextLiveProtectionEnabled.ForeColor = Color.DeepSkyBlue;
                flagRansomware = true;
            }
        }

        private void materialButtonSmartScan1_Click(object sender, EventArgs e)
        {
            _curpanel = pnlScan;
            RunSmartScan();
        }

        private void materialButton1_Click_2(object sender, EventArgs e)
        {
            scanType = 1;
            loc_to_search = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            ShowPanel(pnlFormScan);
            this.BackgroundImage = null;
            this.BackColor = Color.WhiteSmoke;
            panelLeftbar.Visible = false;
            materialButtonSetting.Enabled = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            scanType = 2;
            folderBrowserDialog1.Description = "Select your folder or drive";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                loc_to_search = folderBrowserDialog1.SelectedPath;
                ShowPanel(pnlFormScan);
                this.BackgroundImage = null;
                this.BackColor = Color.WhiteSmoke;
                panelLeftbar.Visible = false;
                materialButtonSetting.Enabled = false;
                search = new Thread(new ThreadStart(ScanFolder));
                search.Start();
            }
        }

        private void materialButton3_Click_1(object sender, EventArgs e)
        {
            scanType = 3;
            ShowPanel(pnlFormScan);
            this.BackgroundImage = null;
            this.BackColor = Color.WhiteSmoke;
            panelLeftbar.Visible = false;
            materialButtonSetting.Enabled = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        private void switchAutomaticUpdates_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagAutomaticUpdates)
            {
                this.flagAutomaticUpdates = false;
            }
            else
            {
                this.flagAutomaticUpdates = true;
            }
        }

        private void switchDisableDesktop_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagDesktopCleanUpWizard)
            {
                this.flagDesktopCleanUpWizard = false;
            }
            else
            {
                this.flagDesktopCleanUpWizard = true;
            }
        }

        private void switchSpeedUpMenu_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagSpeedUpMenuShowDelay)
            {
                this.flagSpeedUpMenuShowDelay = false;
            }
            else
            {
                this.flagSpeedUpMenuShowDelay = true;
            }
        }

        private void switchAppearanceInPerformance_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagAppearanceInPerformance)
            {
                this.flagAppearanceInPerformance = false;
            }
            else
            {
                this.flagAppearanceInPerformance = true;
            }
        }

        private void switchDisableQuickAccess_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagQuickAccessHistory)
            {
                this.flagQuickAccessHistory = false;
            }
            else
            {
                this.flagQuickAccessHistory = true;
            }
        }

        private void switchDisalbeStartMenuAds_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagStartMenuAds)
            {
                this.flagStartMenuAds = false;
            }
            else
            {
                this.flagStartMenuAds = true;
            }
        }

        private void switchDisableSensorService_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagSensorService)
            {
                this.flagSensorService = false;
            }
            else
            {
                this.flagSensorService = true;
            }
        }

        private void switchDisableFeatureUpdates_CheckedChanged(object sender, EventArgs e)
        {
            if (this.flagFeatureUpdates)
            {
                this.flagFeatureUpdates = false;
            }
            else
            {
                this.flagFeatureUpdates = true;
            }
        }

        private void labelRansomware_Click(object sender, EventArgs e)
        {
            frmRansomware dlg = new frmRansomware();
            dlg.Show();
        }
    }
}
