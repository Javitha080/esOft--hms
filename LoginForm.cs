using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SimpleHMS
{
    public partial class LoginForm : Form
    {
        public MacOSButton closeButton = null!;
        public MacOSButton minimizeButton = null!;
        public MacOSButton maximizeButton = null!;
       

        public LoginForm()
        {
            InitializeComponent();
            AddMacOSButton(); // Add macOS buttons
            
            
            // Set password char to hide password by default
            txtpassword.PasswordChar = '•';
 
            
        }

        // Password visibility toggle and Remember Me functionality removed

        private void AddMacOSButton()
        {
            // *** CUSTOMIZE BUTTON SIZE HERE ***
            int buttonSize = 14;    // Change this value (12, 14, 16, 18, etc.)

            // macOS exact positioning
            int yPosition = 23;      // Distance from top
            int startX = 25;         // Distance from left
            int spacing = 8;        // Space between buttons

            // Create Close button (Red)
            closeButton = new MacOSButton(MacOSButton.ButtonType.Close)
            {
                Location = new Point(startX, yPosition),
                Size = new Size(buttonSize, buttonSize)  // Custom size
            };
            closeButton.Click += (s, e) => this.Close();

            // Create Minimize button (Yellow)
            minimizeButton = new MacOSButton(MacOSButton.ButtonType.Minimize)
            {
                Location = new Point(startX + buttonSize + spacing, yPosition),
                Size = new Size(buttonSize, buttonSize)  // Custom size
            };
            minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Create Maximize button (Green)
            maximizeButton = new MacOSButton(MacOSButton.ButtonType.Maximize)
            {
                Location = new Point(startX + (buttonSize + spacing) * 2, yPosition),
                Size = new Size(buttonSize, buttonSize)  // Custom size
            };
            maximizeButton.Click += MaximizeButton_Click;

            // Add buttons to form
            this.Controls.Add(closeButton);
            this.Controls.Add(minimizeButton);
            this.Controls.Add(maximizeButton);

            // Bring to front to ensure visibility
            closeButton.BringToFront();
            minimizeButton.BringToFront();
            maximizeButton.BringToFront();
        }

        private void MaximizeButton_Click(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }
        private void button1_Click(object? sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtemail.Text) || string.IsNullOrWhiteSpace(txtpassword.Text))
            {
                MessageBox.Show("Please enter both user name and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Perform login validation
            bool loginSuccessful = ValidateLogin(txtemail.Text.Trim(), txtpassword.Text);

            if (loginSuccessful)
            {
                // Proceed to main form
                MessageBox.Show("Welcome, Admin!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                MainForm mainForm = new();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Invalid user name or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtpassword.Clear();
                txtpassword.Focus();
            }
        }

        // Clean up empty event handlers by removing them or adding functionality as needed
        private void label1_Click(object? sender, EventArgs e) { }
        private void textBox1_TextChanged(object? sender, EventArgs e) { }
        private void textBox2_TextChanged(object? sender, EventArgs e) { }
        private void button2_Click(object? sender, EventArgs e) { }
        private void label2_Click(object? sender, EventArgs e) { }
        private void label3_Click(object? sender, EventArgs e) { }
        private void label4_Click(object? sender, EventArgs e) { }
        private void button3_Click(object? sender, EventArgs e) { }
        private void pictureBox1_Click(object? sender, EventArgs e) { }
        private void pictureBox2_Click(object? sender, EventArgs e) { }
        private void label5_Click(object? sender, EventArgs e) { }
        private void pictureBox1_Click_1(object? sender, EventArgs e) { }
        private void panel2_Paint(object? sender, PaintEventArgs e) { }
        private void pictureBox1_Click_2(object? sender, EventArgs e) { }
        private void panel2_Paint_1(object? sender, PaintEventArgs e) { }
        private void pictureBox2_Click_1(object? sender, EventArgs e) { }
        private void pictureBox3_Click(object? sender, EventArgs e) { }
        private void pictureBox4_Click(object? sender, EventArgs e) { }
        private void pictureBox5_Click(object? sender, EventArgs e) { }
        private void pictureBox5_Click_1(object? sender, EventArgs e) { }
        
        private bool ValidateLogin(string user_name, string password)
        {
            // This is a simple example - in a real application, you would check against a database
            // or call an authentication service
            return user_name.ToLower() == "admin" && password == "admin";
        }
    }
}