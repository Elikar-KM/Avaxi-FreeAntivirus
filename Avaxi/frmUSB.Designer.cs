
namespace Avaxi
{
    partial class frmUSB
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmUSB));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnRemoveVaccinationFromAllUSB = new ReaLTaiizor.Controls.FoxButton();
            this.btnVaccineAllUSBDrivers = new ReaLTaiizor.Controls.FoxButton();
            this.btnVaccineMyComputerNow = new ReaLTaiizor.Controls.FoxButton();
            this.btnRemoveVaccination = new ReaLTaiizor.Controls.FoxButton();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(978, 43);
            this.panel1.TabIndex = 0;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Cursor = System.Windows.Forms.Cursors.Default;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(8, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 36);
            this.label3.TabIndex = 5;
            this.label3.Text = "PC && USB";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(711, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(267, 43);
            this.panel2.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label4.Font = new System.Drawing.Font("Trebuchet MS", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(180, 7);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 26);
            this.label4.TabIndex = 3;
            this.label4.Text = "-";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label4.Click += new System.EventHandler(this.label4_Click);
            this.label4.MouseLeave += new System.EventHandler(this.label4_MouseLeave);
            this.label4.MouseHover += new System.EventHandler(this.label4_MouseHover);
            this.label4.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label4_MouseMove);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label2.Font = new System.Drawing.Font("Trebuchet MS", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.LightGray;
            this.label2.Location = new System.Drawing.Point(220, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "X";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label2.Click += new System.EventHandler(this.label2_Click);
            this.label2.MouseLeave += new System.EventHandler(this.label4_MouseLeave);
            this.label2.MouseHover += new System.EventHandler(this.label4_MouseHover);
            this.label2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label4_MouseMove);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 20;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 10000;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(18, 60);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(945, 36);
            this.label1.TabIndex = 6;
            this.label1.Text = "Computer Vaccination";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Cursor = System.Windows.Forms.Cursors.Default;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label5.Location = new System.Drawing.Point(22, 111);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(945, 63);
            this.label5.TabIndex = 7;
            this.label5.Text = "Vaccinating your computer can prevent some malware from auto-executing on your co" +
    "mputer from USB drivers...";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Cursor = System.Windows.Forms.Cursors.Default;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Yellow;
            this.label6.Location = new System.Drawing.Point(18, 174);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(945, 36);
            this.label6.TabIndex = 8;
            this.label6.Text = "Your computer is not Vaccinated";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Cursor = System.Windows.Forms.Cursors.Default;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(18, 319);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(945, 36);
            this.label7.TabIndex = 21;
            this.label7.Text = "USB Vaccination";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Cursor = System.Windows.Forms.Cursors.Default;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label8.Location = new System.Drawing.Point(22, 367);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(945, 63);
            this.label8.TabIndex = 22;
            this.label8.Text = "Vaccinating your computer can prevent you from inadvertently transmitting malware" +
    " to/from other computers where you insert your USB drivers";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRemoveVaccinationFromAllUSB
            // 
            this.btnRemoveVaccinationFromAllUSB.BackColor = System.Drawing.Color.White;
            this.btnRemoveVaccinationFromAllUSB.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnRemoveVaccinationFromAllUSB.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            this.btnRemoveVaccinationFromAllUSB.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveVaccinationFromAllUSB.DisabledBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.btnRemoveVaccinationFromAllUSB.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(209)))));
            this.btnRemoveVaccinationFromAllUSB.DisabledTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(178)))), ((int)(((byte)(190)))));
            this.btnRemoveVaccinationFromAllUSB.DownColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.btnRemoveVaccinationFromAllUSB.EnabledCalc = true;
            this.btnRemoveVaccinationFromAllUSB.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveVaccinationFromAllUSB.ForeColor = System.Drawing.Color.White;
            this.btnRemoveVaccinationFromAllUSB.Location = new System.Drawing.Point(489, 462);
            this.btnRemoveVaccinationFromAllUSB.Name = "btnRemoveVaccinationFromAllUSB";
            this.btnRemoveVaccinationFromAllUSB.OverColor = System.Drawing.Color.Gray;
            this.btnRemoveVaccinationFromAllUSB.Size = new System.Drawing.Size(322, 36);
            this.btnRemoveVaccinationFromAllUSB.TabIndex = 24;
            this.btnRemoveVaccinationFromAllUSB.Text = "Remove Vaccination From All USB";
            this.btnRemoveVaccinationFromAllUSB.Click += new ReaLTaiizor.Util.FoxBase.ButtonFoxBase.ClickEventHandler(this.btnRemoveVaccinationFromAllUSB_Click);
            // 
            // btnVaccineAllUSBDrivers
            // 
            this.btnVaccineAllUSBDrivers.BackColor = System.Drawing.Color.White;
            this.btnVaccineAllUSBDrivers.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnVaccineAllUSBDrivers.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            this.btnVaccineAllUSBDrivers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVaccineAllUSBDrivers.DisabledBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.btnVaccineAllUSBDrivers.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(209)))));
            this.btnVaccineAllUSBDrivers.DisabledTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(178)))), ((int)(((byte)(190)))));
            this.btnVaccineAllUSBDrivers.DownColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.btnVaccineAllUSBDrivers.EnabledCalc = true;
            this.btnVaccineAllUSBDrivers.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVaccineAllUSBDrivers.ForeColor = System.Drawing.Color.White;
            this.btnVaccineAllUSBDrivers.Location = new System.Drawing.Point(161, 462);
            this.btnVaccineAllUSBDrivers.Name = "btnVaccineAllUSBDrivers";
            this.btnVaccineAllUSBDrivers.OverColor = System.Drawing.Color.Gray;
            this.btnVaccineAllUSBDrivers.Size = new System.Drawing.Size(322, 36);
            this.btnVaccineAllUSBDrivers.TabIndex = 25;
            this.btnVaccineAllUSBDrivers.Text = "Vaccine All USB Drivers";
            this.btnVaccineAllUSBDrivers.Click += new ReaLTaiizor.Util.FoxBase.ButtonFoxBase.ClickEventHandler(this.btnVaccineAllUSBDrivers_Click);
            // 
            // btnVaccineMyComputerNow
            // 
            this.btnVaccineMyComputerNow.BackColor = System.Drawing.Color.White;
            this.btnVaccineMyComputerNow.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnVaccineMyComputerNow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            this.btnVaccineMyComputerNow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVaccineMyComputerNow.DisabledBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.btnVaccineMyComputerNow.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(209)))));
            this.btnVaccineMyComputerNow.DisabledTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(178)))), ((int)(((byte)(190)))));
            this.btnVaccineMyComputerNow.DownColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.btnVaccineMyComputerNow.EnabledCalc = true;
            this.btnVaccineMyComputerNow.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVaccineMyComputerNow.ForeColor = System.Drawing.Color.White;
            this.btnVaccineMyComputerNow.Location = new System.Drawing.Point(161, 236);
            this.btnVaccineMyComputerNow.Name = "btnVaccineMyComputerNow";
            this.btnVaccineMyComputerNow.OverColor = System.Drawing.Color.Gray;
            this.btnVaccineMyComputerNow.Size = new System.Drawing.Size(322, 36);
            this.btnVaccineMyComputerNow.TabIndex = 27;
            this.btnVaccineMyComputerNow.Text = "Vaccine My Computer Now";
            this.btnVaccineMyComputerNow.Click += new ReaLTaiizor.Util.FoxBase.ButtonFoxBase.ClickEventHandler(this.btnVaccineMyComputerNow_Click);
            // 
            // btnRemoveVaccination
            // 
            this.btnRemoveVaccination.BackColor = System.Drawing.Color.White;
            this.btnRemoveVaccination.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnRemoveVaccination.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(193)))), ((int)(((byte)(193)))), ((int)(((byte)(193)))));
            this.btnRemoveVaccination.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveVaccination.DisabledBaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.btnRemoveVaccination.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(209)))));
            this.btnRemoveVaccination.DisabledTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(178)))), ((int)(((byte)(190)))));
            this.btnRemoveVaccination.DownColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(232)))), ((int)(((byte)(232)))));
            this.btnRemoveVaccination.EnabledCalc = true;
            this.btnRemoveVaccination.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRemoveVaccination.ForeColor = System.Drawing.Color.White;
            this.btnRemoveVaccination.Location = new System.Drawing.Point(489, 236);
            this.btnRemoveVaccination.Name = "btnRemoveVaccination";
            this.btnRemoveVaccination.OverColor = System.Drawing.Color.Gray;
            this.btnRemoveVaccination.Size = new System.Drawing.Size(322, 36);
            this.btnRemoveVaccination.TabIndex = 26;
            this.btnRemoveVaccination.Text = "Remove Vaccination";
            this.btnRemoveVaccination.Click += new ReaLTaiizor.Util.FoxBase.ButtonFoxBase.ClickEventHandler(this.btnRemoveVaccination_Click);
            // 
            // frmUSB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(978, 582);
            this.ControlBox = false;
            this.Controls.Add(this.btnVaccineMyComputerNow);
            this.Controls.Add(this.btnRemoveVaccination);
            this.Controls.Add(this.btnVaccineAllUSBDrivers);
            this.Controls.Add(this.btnRemoveVaccinationFromAllUSB);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmUSB";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.frmUSB_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;  // menu panel
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;  // minimize button panel
        //private System.Windows.Forms.Panel pnlToolBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private ReaLTaiizor.Controls.FoxButton btnRemoveVaccinationFromAllUSB;
        private ReaLTaiizor.Controls.FoxButton btnVaccineAllUSBDrivers;
        private ReaLTaiizor.Controls.FoxButton btnVaccineMyComputerNow;
        private ReaLTaiizor.Controls.FoxButton btnRemoveVaccination;
    }
}

