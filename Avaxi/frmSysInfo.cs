using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Avaxi
{
    public partial class frmSysInfo : Form
    {
        private bool IsMoving = false;
        private Point LastPos;
        [DllImport("process-killer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KillProcess(IntPtr handle, string proc_name);

        public frmSysInfo()
        {
            //_log.AppendFormat("Free Antivirus internal log:\n\n");
            InitializeComponent();
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
            l.BackColor = ColorTranslator.FromHtml("#F4F4F4");
        }

        private void label4_MouseMove(object sender, MouseEventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = ColorTranslator.FromHtml("#F4F4F4");
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            Label l = (Label)sender;
            l.BackColor = Color.Transparent;
        }

        private async void frmSysInfo_Load(object sender, EventArgs e)
        {
            Computer c = new Computer();
            DriveInfo driveinf = new DriveInfo(Environment.GetEnvironmentVariable("systemdrive"));
            List<ListViewItem> AllInfos = new List<ListViewItem>();
            await Task.Run(() => {
                ListViewItem sysinf = new ListViewItem();
                sysinf.Text = "Machine Name";
                sysinf.SubItems.Add(Environment.MachineName);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "OS Full Name";
                sysinf.SubItems.Add(c.Info.OSFullName);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "OS Version";
                sysinf.SubItems.Add(c.Info.OSVersion);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "OS Platform";
                sysinf.SubItems.Add(c.Info.OSPlatform);
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "RAM";
                ulong ram = c.Info.TotalPhysicalMemory;
                float ramcount = float.Parse(ram.ToString()) / (1024 * 1024 * 1024);
                sysinf.SubItems.Add(ramcount.ToString("0.00") + " GB");
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "Screen Driver";
                sysinf.SubItems.Add(c.Screen.DeviceName);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "Screen Bits Per Pixel";
                sysinf.SubItems.Add(c.Screen.BitsPerPixel.ToString());
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "Screen Bounds";
                sysinf.SubItems.Add(c.Screen.Bounds.ToString());
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "User Domaine Name";
                sysinf.SubItems.Add(Environment.UserDomainName);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "UserName";
                sysinf.SubItems.Add(Environment.UserName);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "System Directory";
                sysinf.SubItems.Add(Environment.SystemDirectory);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "System Drive";
                sysinf.SubItems.Add(Environment.GetEnvironmentVariable("systemdrive"));
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "System Drive Name";
                sysinf.SubItems.Add(driveinf.Name);
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "System Drive Size";
                float drivesize = float.Parse(driveinf.TotalSize.ToString()) / (1024 * 1024 * 1024);
                sysinf.SubItems.Add(drivesize.ToString("0.00") + " GB");
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "System Drive Format";
                sysinf.SubItems.Add(driveinf.DriveFormat);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "System Drive Type";
                sysinf.SubItems.Add(driveinf.DriveType.ToString());
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "System Drive RootDir";
                sysinf.SubItems.Add(driveinf.RootDirectory.FullName);
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "System Drive Free Size";
                drivesize = float.Parse(driveinf.TotalFreeSpace.ToString()) / (1024 * 1024 * 1024);
                sysinf.SubItems.Add(drivesize.ToString("0.00") + " GB");
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "Desktop Directory";
                sysinf.SubItems.Add(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "Windows Directory";
                sysinf.SubItems.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "User Profile Directory";
                sysinf.SubItems.Add(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                AllInfos.Add(sysinf);

                sysinf = new ListViewItem();
                sysinf.Text = "ProgramFiles Directory";
                sysinf.SubItems.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "ProgramFiles(X86) Directory";
                sysinf.SubItems.Add(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
                AllInfos.Add(sysinf);


                sysinf = new ListViewItem();
                sysinf.Text = "Times from starting PC to Now";
                sysinf.SubItems.Add(((Environment.TickCount / 1000) / 1000).ToString() + " Minute(s)");
                AllInfos.Add(sysinf);
            });
            foreach (var ele in AllInfos)
            {
                listView7.Items.Add(ele);
            }
            progressIndicator1.Visible = false;
            label2.Enabled = true;
            label4.Enabled = true;
        }

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

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Opacity < 1)
                Opacity += .1;
            else
                timer1.Enabled = false;
        }
    
    }
}
