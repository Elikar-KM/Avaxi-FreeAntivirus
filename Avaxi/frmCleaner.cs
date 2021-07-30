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
    public partial class frmCleaner : Form
    {
        private bool IsMoving = false;
        private Point LastPos;
        [DllImport("process-killer.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int KillProcess(IntPtr handle, string proc_name);

        public frmCleaner()
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

        private async void frmCleaner_Load(object sender, EventArgs e)
        {
            Computer c = new Computer();
            DriveInfo driveinf = new DriveInfo(Environment.GetEnvironmentVariable("systemdrive"));

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
