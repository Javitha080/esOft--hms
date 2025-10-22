using System;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace SimpleHMS
{
    // MainForm - This is the main menu of the application
    // It has 3 buttons to open different forms
    public partial class MainForm : Form
    {
        ToolTip tooltip; // Tooltip shows hints when you hover over controls
        private System.Windows.Forms.Timer greetingTimer; // updates greeting periodically

        // Constructor - runs when form is created
        public MainForm()
        {
            InitializeComponent(); // Create all the controls
        }

        // This method creates all the UI controls (buttons, labels, etc.)
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tooltip = new ToolTip(components);
            btnPatient = new Button();
            btnDoctor = new Button();
            btnAppointment = new Button();
            greetingTimer = new System.Windows.Forms.Timer(components);
            lblTitle = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // tooltip
            // 
            tooltip.AutoPopDelay = 5000;
            tooltip.InitialDelay = 500;
            tooltip.ReshowDelay = 100;
            // 
            // btnPatient
            // 
            btnPatient.BackColor = Color.LightSteelBlue;
            btnPatient.FlatStyle = FlatStyle.Flat;
            btnPatient.Location = new Point(463, 181);
            btnPatient.Name = "btnPatient";
            btnPatient.Size = new Size(216, 47);
            btnPatient.TabIndex = 1;
            btnPatient.Text = "Patient Registration";
            tooltip.SetToolTip(btnPatient, "Click to manage patient records (Add, Update, Delete)");
            btnPatient.UseVisualStyleBackColor = false;
            btnPatient.Click += btnPatient_Click;
            // 
            // btnDoctor
            // 
            btnDoctor.FlatStyle = FlatStyle.Flat;
            btnDoctor.Location = new Point(463, 246);
            btnDoctor.Name = "btnDoctor";
            btnDoctor.Size = new Size(216, 47);
            btnDoctor.TabIndex = 2;
            btnDoctor.Text = "Doctor Registration";
            tooltip.SetToolTip(btnDoctor, "Click to manage doctor records (Add, Update, Delete)");
            btnDoctor.Click += btnDoctor_Click;
            // 
            // btnAppointment
            // 
            btnAppointment.FlatStyle = FlatStyle.Flat;
            btnAppointment.Location = new Point(463, 314);
            btnAppointment.Name = "btnAppointment";
            btnAppointment.Size = new Size(216, 47);
            btnAppointment.TabIndex = 3;
            btnAppointment.Text = "Appointments";
            tooltip.SetToolTip(btnAppointment, "Click to manage appointments (Book, Update, Cancel)");
            btnAppointment.Click += btnAppointment_Click;
            // 
            // greetingTimer
            // 
            greetingTimer.Interval = 60000;
            greetingTimer.Tick += greetingTimer_Tick;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Corbel", 33.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Black;
            lblTitle.Location = new Point(-2, 34);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(300, 58);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Welcome To";
            lblTitle.Click += lblTitle_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Arial", 9F, FontStyle.Bold);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(412, 455);
            label1.Name = "label1";
            label1.Size = new Size(331, 18);
            label1.TabIndex = 5;
            label1.Text = "© 2025 Project X - ESOFT™ H.M.S. All Rights Reserved.";
            label1.TextAlign = ContentAlignment.BottomCenter;
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Bebas Neue", 40F);
            label2.ForeColor = Color.DarkRed;
            label2.Location = new Point(144, 81);
            label2.Name = "label2";
            label2.Size = new Size(100, 58);
            label2.TabIndex = 5;
            label2.Text = "HMS";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Bebas Neue", 40F);
            label3.ForeColor = Color.Navy;
            label3.Location = new Point(1, 81);
            label3.Name = "label3";
            label3.Size = new Size(134, 58);
            label3.TabIndex = 6;
            label3.Text = "ESOFT";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Font = new Font("Segoe UI Semilight", 39.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(412, 44);
            label4.Name = "label4";
            label4.Size = new Size(156, 71);
            label4.TabIndex = 7;
            label4.Text = "Good";
            label4.Click += label4_Click;
            // 
            // MainForm
            // 
            BackColor = Color.LightSteelBlue;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(758, 493);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblTitle);
            Controls.Add(btnPatient);
            Controls.Add(btnDoctor);
            Controls.Add(btnAppointment);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hospital Management System";
            TransparencyKey = Color.DimGray;
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private void btnDoctor_Click(object sender, EventArgs e)
        {
            DoctorForm();
        }

        private void DoctorForm()
        {
            try
            {
                using var patientForm = new PatientForm();
                // open new window
                patientForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Patient Form: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPatient_Click(object sender, EventArgs e)
        {
            OpenPatientForm();
        }
        private void OpenPatientForm()
        {
            try
            {
                using var patientForm = new PatientForm();
                // open new window
                patientForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Patient Form: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnAppointment_Click(object sender, EventArgs e)
        {
            OpenAppointmentForm();
        }

        private void OpenAppointmentForm()
        {
            try
            {
                using var patientForm = new PatientForm();
                // open new window
                patientForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Patient Form: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private System.ComponentModel.IContainer components;
        private Button btnPatient;
        private Button btnDoctor;
        private Button btnAppointment;
        private Label lblTitle;
        private Label label1;
        private Label label3;
        private Label label4;
        private Label label2;

        private void MainForm_Load(object sender, EventArgs e)
        {
            // quick debug to confirm Load runs (remove when done)
            System.Diagnostics.Debug.WriteLine("MainForm_Load called");

            // Set initial greeting and start timer to keep it current
            UpdateGreeting();
            // ensure label is above others after layout
            label4.BringToFront();
            greetingTimer.Start();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            // allow manual refresh by clicking the label
            UpdateGreeting();
        }

        // timer tick handler - keep greeting up to date
        private void greetingTimer_Tick(object sender, EventArgs e)
        {
            UpdateGreeting();
            label4.BringToFront(); // keep on top in case something else moves above it
        }

        // determine part of day and set label text
        private void UpdateGreeting()
        {
            var hour = DateTime.Now.Hour;
            string greeting;

            if (hour >= 5 && hour < 12)
                greeting = "Good Morning";
            else if (hour >= 12 && hour < 17)
                greeting = "Good Afternoon";
            else if (hour >= 17 && hour < 21)
                greeting = "Good Evening";
            else
                greeting = "Good Night";

            label4.Text = greeting;
            label4.ForeColor = Color.Black; // quick test: change if text is blending with the background
            label4.BringToFront();
        }
    }
}

