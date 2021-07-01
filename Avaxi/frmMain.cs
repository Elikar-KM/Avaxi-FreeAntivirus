﻿using Microsoft.VisualBasic.Devices;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        public frmMain()
        {
            //_log.AppendFormat("Free Antivirus internal log:\n\n");
            InitializeComponent();
            toolStrip1.Renderer = new MyRenderer();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void label3_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Sign up");
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.Hide();
            //Application.Exit();
        }

        private void label4_MouseHover(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#333333");
        }

        private void label4_MouseMove(object sender, MouseEventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#333333");
        }

        private void label4_MouseLeave(object sender, EventArgs e)
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
                                        //FormFreeAntivirus.PushLog(ex.Message);
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
                        System.Windows.Forms.MessageBox.Show(this, "Invalid file to scan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ret = 3;
                    return ret;
                }
            }
            catch (Exception ex)
            {

            }
            return ret;
        }

        //Real time scanner 
        private void RealTime(object sender, FileSystemEventArgs e)
        {
            if (exclusion.isExclusion(e.FullPath))
                return;

            if (switchRealTimeProtection.Checked)
            {
                //FormFreeAntivirus.PushLog("Real-time scan : " + e.FullPath);
                ScanFile(e.FullPath, true);
            }
        }

        private void DoStateChanged(UsbStateChangedEventArgs e)
        {
            if (e.State == UsbStateChange.Added)
            {
                if (switchAutoScanUSB.Checked)
                {
                    if(scanType == -1)
                    {
                        scanType = 4;
                        ShowPanel(pnlFormScan);
                        toolStrip1.Visible = false;
                        loc_to_search = e.Disk.Name;
                        this.Show();
                        search = new Thread(new ThreadStart(ScanFolder));
                        search.Start();
                    }
                }
            }
        }

        List<Section> AllSections = new List<Section>();
        List<Registry> AllSectionsRegistry = new List<Registry>();
        List<StartupManager> MonitorStartup = new List<StartupManager>();
        Settings ProgramSettings = new Settings() { MonitorStartup=false,TotalSaved = 0, AutoClean = false, CloseAfterClean = false, ShutdownAfterClean = false, LaunchInStartup = false, LastScan = null };
        private async void frmMain_Load(object sender, EventArgs e)
        {
            this.switchAppearanceInPerformance.Checked = Program.tuneAppearanceInPerformance;
            this.switchAutomaticUpdates.Checked = Program.tuneAutomaticUpdates;
            this.switchDesktopCleanUpWizard.Checked = Program.tuneDesktopCleanUpWizard;
            this.switchFeatureUpdates.Checked = Program.tuneFeatureUpdates;
            this.switchSpeedUpMenuShowDelay.Checked = Program.tuneMenuShowDelay;
            this.switchQuickAccessHistory.Checked = Program.tuneQuickAccessHistory;
            this.switchSensorService.Checked = Program.tuneSensorService;
            this.switchStartMenuAds.Checked = Program.tuneStartMenuAds;
            this.switchRealTimeProtection.Checked = Program.RealTimeProtection;
            this.switchAutoScanUSB.Checked = Program.AutoUSBScanner;

            this.flagPhishing = Program.flagPhishing;
            this.flagCryptojacking = Program.flagCryptojacking;
            this.flagRansomware = Program.flagRansomware;
            this.flagAffiliateOffers = Program.flagAffiliateOffers;

            // Protection Status 
            if (flagPhishing)
            {
                btnPhishing.ForeColor = Color.LimeGreen;
                btnPhishing.Text = "Enabled";
                label10.Visible = true;
                label14.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                btnPhishing.ForeColor = Color.Gray;
                btnPhishing.Text = "Disabled";
                label10.Visible = false;
                label14.ForeColor = Color.Gray;
            }

            if (flagCryptojacking)
            {
                btnCryptojacking.ForeColor = Color.LimeGreen;
                btnCryptojacking.Text = "Enabled";
                label12.Visible = true;
                label11.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                btnCryptojacking.ForeColor = Color.Gray;
                btnCryptojacking.Text = "Disabled";
                label12.Visible = false;
                label11.ForeColor = Color.Gray;
            }
            if (flagRansomware)
            {
                btnRansomware.ForeColor = Color.LimeGreen;
                btnRansomware.Text = "Enabled";
                label16.Visible = true;
                label15.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                btnRansomware.ForeColor = Color.Gray;
                btnRansomware.Text = "Disabled";
                label16.Visible = false;
                label15.ForeColor = Color.Gray;
            }

            if (flagAffiliateOffers)
            {
                btnAffiliateOffers.ForeColor = Color.LimeGreen;
                btnAffiliateOffers.Text = "Enabled";
            }
            else
            {
                btnAffiliateOffers.ForeColor = Color.Gray;
                btnAffiliateOffers.Text = "Disabled";
            }

            // RealTime status view
            if (switchRealTimeProtection.Checked)
            {
                label6.Image = global::Avaxi.Properties.Resources.check;
                label13.Text = "The live protection is enabled";
                label13.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                label6.Image = global::Avaxi.Properties.Resources.cross;
                label13.Text = "The live protection is disabled";
                label13.ForeColor = Color.Gray;
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
                this.switchFireWall.Checked = true;
                //Avaxi.PushLog("Firewall is on.");
            }
            else
            {
                this.switchFireWall.Checked = false;
                //Avaxi.PushLog("Firewall is off.");
            }

            ShowPanel(pnlShield);
            StripButtonColor(toolStripButton1);

            Task.Run(delegate ()
            {
                Thread.Sleep(1000);
                opt = new Optimize(this.lstPrograms, this.circularProgressBar1);
            });

        }

        void GetStartupItems(List<StartupManager> Results)
        {
            Computer str = new Computer();
            foreach (string name in str.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run").GetValueNames())
            {
                StartupManager item = new StartupManager()
                {
                    Key = "HCU:Run",
                    Name = name,
                    File = str.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run").GetValue(name).ToString()
                };
                Results.Add(item);
            }

            foreach (string name in str.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run").GetValueNames())
            {
                StartupManager item = new StartupManager()
                {
                    Key = "HLM:WOW6432Node:Run",
                    Name = name,
                    File = str.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run").GetValue(name).ToString()
                };
                Results.Add(item);

            }

            foreach (string name in RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValueNames())
            {
                StartupManager item = new StartupManager()
                {
                    Key = "HLM:Run",
                    Name = name,
                    File = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValue(name).ToString()
                };
                Results.Add(item);
            }
        }
        private void frmMain_Paint(object sender, PaintEventArgs e)
        {
            //Gradient.FillGradient(this.Width, this.Height, e);
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

            pnlShield.Left = (this.Width / 2) - (pnlShield.Width / 2) + (toolStrip1.Width / 2);
        }

        private void btnScan_Click(object sender, EventArgs e)  // QuickScan
        {
            scanType = 1;
            loc_to_search = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
            ShowPanel(pnlFormScan);
            toolStrip1.Visible = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        void ShowPanel(Panel panel)
        {
            pnlShield.Visible = false;
            pnlScan.Visible = false;
            pnlCrypto.Visible = false;
            pnlOptimize.Visible = false;
            pnlTuneUp.Visible = false;
            pnlSetting.Visible = false;
            pnlFormScan.Visible = false;
            pnlDelete.Visible = false;

            panel.Visible = true;
        }

        void FormatButtonColor()
        {
            toolStripButton1.BackColor = Color.Transparent;
            toolStripButton3.BackColor = Color.Transparent;
            toolStripButton4.BackColor = Color.Transparent;
            toolStripButton5.BackColor = Color.Transparent;
            toolStripButton6.BackColor = Color.Transparent;

            toolStripButton1.ForeColor = Color.White;
            toolStripButton3.ForeColor = Color.White;
            toolStripButton4.ForeColor = Color.White;
            toolStripButton5.ForeColor = Color.White;
            toolStripButton6.ForeColor = Color.White;
        }

        void  StripButtonColor(ToolStripButton tb)
        {
            FormatButtonColor();
            tb.BackColor = Color.LightGray;
            tb.ForeColor = Color.Black;
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlShield);
            ToolStripButton tb = (ToolStripButton)sender;
            StripButtonColor(tb);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlCrypto);
            ToolStripButton tb = (ToolStripButton)sender;
            StripButtonColor(tb);
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

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlScan);
            ToolStripButton tb = (ToolStripButton)sender;
            StripButtonColor(tb);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlOptimize);
            ToolStripButton tb = (ToolStripButton)sender;
            StripButtonColor(tb);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Opacity < 1)
                Opacity += .1;
            else
                timer1.Enabled = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
        }

        private void button12_Click(object sender, EventArgs e)
        {
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

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlTuneUp);
            ToolStripButton tb = (ToolStripButton)sender;
            StripButtonColor(tb);
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
                toolStrip1.Visible = false;
                search = new Thread(new ThreadStart(ScanFolder));
                search.Start();
            }
        }

        private void btnFullScan_Click(object sender, EventArgs e)
        {
            scanType = 3;
            ShowPanel(pnlFormScan);
            toolStrip1.Visible = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }
        private void launcherIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }

        private void switchFireWall_CheckedChanged(object sender, EventArgs e)
        {
            //reflect firewall settings
            if (this.switchFireWall.Checked)
            {
                launcherIcon.ShowBalloonTip(5, "Avaxi", "Firewall is turned on successfully", ToolTipIcon.Info);
                Task.Run(delegate ()
                {
                    firewall.FirewallStart(true);
                    PushLog("Firewall is turned on sucessfully.");
                });
            }
            else
            {
                launcherIcon.ShowBalloonTip(5, "Avaxi", "Firewall is turned off successfully", ToolTipIcon.Info);
                Task.Run(delegate ()
                {
                    shell.RunExternalExe("netsh.exe", "Firewall set opmode disable");
                    PushLog("Firewall is turned off sucessfully.");
                });
            }
        }

        private void openItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void closeItem_Click(object sender, EventArgs e)
        {
            Program.tuneAppearanceInPerformance = this.switchAppearanceInPerformance.Checked;
            Program.tuneAutomaticUpdates = this.switchAutomaticUpdates.Checked;
            Program.tuneDesktopCleanUpWizard = this.switchDesktopCleanUpWizard.Checked;
            Program.tuneFeatureUpdates = this.switchFeatureUpdates.Checked;
            Program.tuneMenuShowDelay = this.switchSpeedUpMenuShowDelay.Checked;
            Program.tuneQuickAccessHistory = this.switchQuickAccessHistory.Checked;
            Program.tuneSensorService = this.switchSensorService.Checked;
            Program.tuneStartMenuAds = this.switchStartMenuAds.Checked;
            Program.optClearCache = this.checkClearCache.Checked;
            Program.optClearMemory = this.checkClearMemory.Checked;
            Program.RealTimeProtection = this.switchRealTimeProtection.Checked;
            Program.AutoUSBScanner = this.switchAutoScanUSB.Checked;
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
            DateTime localDate = DateTime.Now;
            string time = String.Format("{0}", localDate.ToString());
            //_log.AppendFormat("{0} : {1}\n", time, message);
        }

        private async void btnClearMemory_Click(object sender, EventArgs e)
        {
            this.btnClearMemory.Enabled = false;
            await Optimize.ClearMemory(1);
            this.btnClearMemory.Enabled = true;
        }

        private async void btnOptimize_Click(object sender, EventArgs e)
        {
            this.btnOptimize.Enabled = false;
            if (checkClearMemory.Checked)
            {
                await Optimize.ClearMemory(1);
            }
            if(checkClearCache.Checked)
            {
                await Optimize.ClearMemory(2);
            }
            this.btnOptimize.Enabled = true;
        }

        private void btnRegistry_Click(object sender, EventArgs e)
        {
            this.btnRegistry.Enabled = false;
            Utilities.EnableCommandPrompt();
            Utilities.EnableControlPanel();
            Utilities.EnableFolderOptions();
            Utilities.EnableRunDialog();
            Utilities.EnableContextMenu();
            Utilities.EnableTaskManager();
            Utilities.EnableRegistryEditor();
            this.btnRegistry.Enabled = true;
        }

        private void btnCleaner_Click(object sender, EventArgs e)
        {
            this.btnCleaner.Enabled = false;
            try
            {
                CleanHelper.CleanTemporaries();
                CleanHelper.CleanMiniDumps();
                CleanHelper.CleanMediaPlayersCache();
                CleanHelper.CleanLogs();
                CleanHelper.CleanErrorReports();
                CleanHelper.EmptyRecycleBin();
            }
            catch (Exception ex)
            {
                //FormFreeAntivirus.PushLog("MainForm.CleanPC" + ex.Message + ex.StackTrace);
            }
            this.btnCleaner.Enabled = true;
        }

        private void btnTemporary_Click(object sender, EventArgs e)
        {
            this.btnTemporary.Enabled = false;
            try
            {
                CleanHelper.CleanTemporaries();
            }
            catch (Exception ex)
            {
                //FormFreeAntivirus.PushLog("MainForm.CleanPC" + ex.Message + ex.StackTrace);
            }
            this.btnTemporary.Enabled = true;
        }

        private void btnTuneUp_Click(object sender, EventArgs e)
        {
            Computer regedit = new Computer();

            this.progressIndicatorTuneUp.Visible = true;

            if (switchAutomaticUpdates.Checked == true)
            {
                OptimizeSpeedUp.DisableAutomaticUpdates();
            }
            else
            {
                OptimizeSpeedUp.EnableAutomaticUpdates();
            }

            if (switchDesktopCleanUpWizard.Checked == true)
            {
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\CleanupWiz");
                regedit.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\CleanupWiz", true).SetValue("NoRun", 1, RegistryValueKind.DWord);
            }
            else
            {
            }

            if (switchSpeedUpMenuShowDelay.Checked == true)
            {
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Control Panel\Desktop");
                MainMethods.FixedInvalidRegistryKey(regedit.Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics");
                regedit.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true).SetValue("MenuShowDelay", "50");
                regedit.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\WindowMetrics", true).SetValue("MinAnimate", "50");
            }
            else
            {
            }

            if (switchAppearanceInPerformance.Checked == true)
            {
                OptimizeSpeedUp.EnableDarkTheme();
            }
            else
            {
                OptimizeSpeedUp.EnableLightTheme();
            }

            if (switchQuickAccessHistory.Checked == true)
            {
                OptimizeSpeedUp.DisableQuickAccessHistory();
            }
            else
            {
                OptimizeSpeedUp.EnableQuickAccessHistory();
            }

            if (switchStartMenuAds.Checked == true)
            {
                OptimizeSpeedUp.DisableStartMenuAds();
            }
            else
            {
                OptimizeSpeedUp.EnableStartMenuAds();
            }

            if (switchSensorService.Checked == true)
            {
                OptimizeSpeedUp.DisableSensorServices();
            }
            else
            {
                OptimizeSpeedUp.EnableSensorServices();
            }

            if (switchFeatureUpdates.Checked == true)
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
                            var clam = new ClamClient("localhost", 3310);
                            var scanResult = clam.ScanFileOnServer(proc);
                            Invoke(new Action(delegate
                            {
                                try
                                {
                                    curr_File.Text = proc;
                                }
                                catch { }
                            }));

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
                            //FormFreeAntivirus.PushLog(ex.Message);
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
                bool detect = false;

                if (System.IO.File.Exists(file))
                {
                    try
                    {
                        var clam = new ClamClient("localhost", 3310);
                        var scanResult = clam.ScanFileOnServer(file);
                        Invoke(new Action(delegate
                        {
                            try
                            {
                                curr_File.Text = file;
                            }
                            catch { }
                        }));

                        /* clamAV engine */
                        switch (scanResult.Result)
                        {
                            case ClamScanResults.VirusDetected:
                                infected++;
                                detect = true;
                                AddToResult(file, scanResult.InfectedFiles.First().VirusName);
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        detect = false;
                        //FormFreeAntivirus.PushLog(ex.Message);
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
                    this.label41.Visible = false;
                    if(infected == 0)
                    {
                        curr_File.Text = "Scan finished!\nYour computer is now safer";
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
            label41.Visible = true;
            label42.Visible = true;
            label43.Visible = false;
            label44.Visible = false;
        }

        private void btnStopScan_Click(object sender, EventArgs e)
        {
            switch(flagScanStatus)
            {
                case 0: //stop
                    search.Abort();
                    hopeProgressBar1.Visible = false;
                    label41.Visible = false;
                    label42.Visible = false;
                    label43.Visible = true;
                    label44.Visible = true;
                    curr_File.Text = "You stopped the scan before we could find any hidden threats.";
                    btnStopScan.Text = "              OK              ";
                    flagScanStatus = 3;
                    break;
                case 1: //done
                    FormatScan();
                    btnStopScan.Text = "STOP SCAN";
                    flagScanStatus = 0;
                    toolStrip1.Visible = true;
                    ShowPanel(pnlShield);
                    StripButtonColor(toolStripButton1);
                    scanType = -1;
                    break;
                case 2: //next
                    FormatScan();
                    btnStopScan.Text = "STOP SCAN";
                    flagScanStatus = 0;
                    toolStrip1.Visible = false;
                    ShowPanel(pnlDelete);
                    break;
                case 3: //ok
                    FormatScan();
                    btnStopScan.Text = "STOP SCAN";
                    flagScanStatus = 0;
                    toolStrip1.Visible = true;
                    ShowPanel(pnlShield);
                    StripButtonColor(toolStripButton1);
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
                    label47.Visible = true;
                    label48.Visible = true;
                    objectListView1.Visible = false;
                    flagDeleteStatus = 1;
                }
                catch { }
            }
            else
            {
                flagDeleteStatus = 0;
                toolStrip1.Visible = true;
                ShowPanel(pnlShield);
                StripButtonColor(toolStripButton1);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            toolStrip1.Visible = true;
            ShowPanel(pnlShield);
            StripButtonColor(toolStripButton1);
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
            toolStrip1.Visible = false;
            search = new Thread(new ThreadStart(ScanFolder));
            search.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void foxButton1_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void foxButton2_Click(object sender, EventArgs e)
        {
            RunSmartScan();
        }

        private void btnPhishing_Click(object sender, EventArgs e)
        {
            if (flagPhishing)
            {
                // Phishing disable code

                // Status page view
                btnPhishing.ForeColor = Color.Gray;
                btnPhishing.Text = "Disabled";
                label10.Visible = false;
                label14.ForeColor = Color.Gray;
                flagPhishing = false;
            }
            else
            {
                // Phishing enable code

                // Status page view
                btnPhishing.ForeColor = Color.LimeGreen;
                btnPhishing.Text = "Enabled";
                label10.Visible = true;
                label14.ForeColor = Color.DeepSkyBlue;
                flagPhishing = true;
            }
        }

        private void btnCryptojacking_Click(object sender, EventArgs e)
        {
            if (flagCryptojacking)
            {
                // Crytojacking disable code

                // Status page view
                btnCryptojacking.ForeColor = Color.Gray;
                btnCryptojacking.Text = "Disabled";
                label12.Visible = false;
                label11.ForeColor = Color.Gray;
                flagCryptojacking = false;
            }
            else
            {
                // Crytojacking enable code

                // Status page view
                btnCryptojacking.ForeColor = Color.LimeGreen;
                btnCryptojacking.Text = "Enabled";
                label12.Visible = true;
                label11.ForeColor = Color.DeepSkyBlue;
                flagCryptojacking = true;
            }
        }

        private void btnRansomware_Click(object sender, EventArgs e)
        {
            if (flagRansomware)
            {
                // Ransomware disable code

                // Status page view
                btnRansomware.ForeColor = Color.Gray;
                btnRansomware.Text = "Disabled";
                label16.Visible = false;
                label15.ForeColor = Color.Gray;
                flagRansomware = false;
            }
            else
            {
                // Ransomware enable code

                // Status page view
                btnRansomware.ForeColor = Color.LimeGreen;
                btnRansomware.Text = "Enabled";
                label16.Visible = true;
                label15.ForeColor = Color.DeepSkyBlue;
                flagRansomware = true;
            }
        }

        private void btnAffiliateOffers_Click(object sender, EventArgs e)
        {
            if (flagAffiliateOffers)
            {
                // Affiliate Offers disable code

                // status view
                btnAffiliateOffers.ForeColor = Color.Gray;
                btnAffiliateOffers.Text = "Disabled";
                flagAffiliateOffers = false;
            }
            else
            {
                // Affiliate Offers enable code

                // status view
                btnAffiliateOffers.ForeColor = Color.LimeGreen;
                btnAffiliateOffers.Text = "Enabled";
                flagAffiliateOffers = true;
            }
        }

        private void switchRealTimeProtection_CheckedChanged(object sender, EventArgs e)
        {
            if(switchRealTimeProtection.Checked)
            {
                label6.Image = global::Avaxi.Properties.Resources.check;
                label13.Text = "The live protection is enabled";
                label13.ForeColor = Color.DeepSkyBlue;
            }
            else
            {
                label6.Image = global::Avaxi.Properties.Resources.cross;
                label13.Text = "The live protection is disabled";
                label13.ForeColor = Color.Gray;
            }
        }
    }
}
