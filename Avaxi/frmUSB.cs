using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Avaxi
{
    public partial class frmUSB : Form
    {
        private bool IsMoving = false;
        private Point LastPos;
        [DllImport("process-killer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KillProcess(IntPtr handle, string proc_name);

        public frmUSB()
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

        List<string> usbdrivers = new List<string>();
        private void frmUSB_Load(object sender, EventArgs e)
        {
            usbdrivers.Clear();
            foreach (DriveInfo drv in DriveInfo.GetDrives())
            {
                if (drv.DriveType == DriveType.Removable && drv.IsReady)
                {
                    usbdrivers.Add(drv.Name);
                }
            }
            MainMethods.FixedInvalidRegistryKey(MyComputer.Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            if ((MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoDriveTypeAutoRun") == null || MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoDriveTypeAutoRun").ToString() != "255")
                && (MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoAutoRun") == null || MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoAutoRun").ToString() != "1")
                && (MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoDriveTypeAutoRun") == null || MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoDriveTypeAutoRun").ToString() != "255")
                && (MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoAutoRun") == null || MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer").GetValue("NoAutoRun").ToString() != "1"))
            {
                label6.Text = "Your Computer is not Vaccinated";
                label6.ForeColor = Color.Yellow;
            }
            else
            {
                label6.Text = "Your Computer is Vaccinated Now";

                label6.ForeColor = Color.Lime;
            }


            if (error >= 4)
            {
                label6.Text = "You Computer is not eligible for Vaccination";
                label6.ForeColor = Color.Orange;

            }
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

        Computer MyComputer = new Computer();
        int error = 0;
        private void btnVaccineMyComputerNow_Click(object sender, EventArgs e)
        {
            try
            {
                MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).SetValue("NoDriveTypeAutoRun", 255, RegistryValueKind.DWord);
            }
            catch { error++; }
            try
            {
                MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).SetValue("NoAutoRun", 1, RegistryValueKind.DWord);
            }
            catch { error++; }
            try
            {
                MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).SetValue("NoDriveTypeAutoRun", 255, RegistryValueKind.DWord);
            }
            catch { error++; }
            try
            {
                MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).SetValue("NoAutoRun", 1, RegistryValueKind.DWord);
            }
            catch { error++; }
            MessageBox.Show("Vaccination Completed", "Finish", MessageBoxButtons.OK, MessageBoxIcon.Information);
            frmUSB_Load(null, null);
        }

        private void btnRemoveVaccination_Click(object sender, EventArgs e)
        {
            try
            {
                MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).DeleteValue("NoDriveTypeAutoRun");
            }
            catch { }
            try
            {
                MyComputer.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).DeleteValue("NoAutoRun");
            }
            catch { }
            try
            {
                MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).DeleteValue("NoDriveTypeAutoRun");
            }
            catch { }
            try
            {
                MyComputer.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true).DeleteValue("NoAutoRun");
            }
            catch { }
            MessageBox.Show("Vaccination Removed", "Finish", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            frmUSB_Load(null, null);
        }

        private void btnVaccineAllUSBDrivers_Click(object sender, EventArgs e)
        {
            try
            {
                if (usbdrivers.Count > 0)
                {
                    foreach (string namedrive in usbdrivers)
                    {
                        Directory.CreateDirectory(namedrive + "autorun.inf");
                        ProcessStartInfo autorun = new ProcessStartInfo();
                        autorun.WindowStyle = ProcessWindowStyle.Hidden;
                        autorun.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";
                        autorun.Arguments = "/c attrib +h +r +s +a " + namedrive + "autorun.inf";
                        Process.Start(autorun).WaitForExit();
                        autorun.Arguments = "/c mkdir " + namedrive + @"autorun.inf\.\con\";
                        Process.Start(autorun).WaitForExit();
                    }
                    MessageBox.Show("Vaccination of all USB is Completed", "finish", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No USB Drive Founded", "error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch
            {
                MessageBox.Show("Something Wrong !!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemoveVaccinationFromAllUSB_Click(object sender, EventArgs e)
        {
            try
            {
                if (usbdrivers.Count > 0)
                {
                    foreach (string namedrive in usbdrivers)
                    {
                        ProcessStartInfo autorun = new ProcessStartInfo();
                        autorun.WindowStyle = ProcessWindowStyle.Hidden;
                        autorun.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";
                        autorun.Arguments = "/c attrib -h -r -s -a " + namedrive + "autorun.inf";
                        Process.Start(autorun).WaitForExit();
                        autorun.Arguments = "/c rmdir " + namedrive + @"autorun.inf\.\con\";
                        Process.Start(autorun).WaitForExit();
                        autorun.Arguments = "/c rmdir " + namedrive + @"autorun.inf";
                        Process.Start(autorun).WaitForExit();
                    }
                    MessageBox.Show("Vaccination of all USB is Removed", "Finish", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    MessageBox.Show("No USB Drive Founded", "error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch
            {
                MessageBox.Show("Something Wrong !!.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
