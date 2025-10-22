namespace SimpleHMS
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            button1 = new Button();
            label1 = new Label();
            label2 = new Label();
            txtemail = new TextBox();
            txtpassword = new TextBox();
            label3 = new Label();
            label5 = new Label();
            pictureBox2 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = SystemColors.MenuHighlight;
            button1.ForeColor = Color.WhiteSmoke;
            button1.Location = new Point(75, 408);
            button1.Name = "button1";
            button1.Size = new Size(252, 43);
            button1.TabIndex = 0;
            button1.Text = "Login";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 10F);
            label1.ForeColor = Color.MidnightBlue;
            label1.Location = new Point(75, 210);
            label1.Name = "label1";
            label1.Size = new Size(41, 19);
            label1.TabIndex = 2;
            label1.Text = "Email";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 10F);
            label2.ForeColor = Color.MidnightBlue;
            label2.Location = new Point(75, 288);
            label2.Name = "label2";
            label2.Size = new Size(67, 19);
            label2.TabIndex = 3;
            label2.Text = "Password";
            label2.Click += label2_Click;
            // 
            // txtemail
            // 
            txtemail.BackColor = SystemColors.GradientActiveCaption;
            txtemail.Location = new Point(75, 242);
            txtemail.Name = "txtemail";
            txtemail.Size = new Size(256, 23);
            txtemail.TabIndex = 4;
            txtemail.TextChanged += textBox1_TextChanged;
            // 
            // txtpassword
            // 
            txtpassword.BackColor = SystemColors.GradientActiveCaption;
            txtpassword.Location = new Point(75, 315);
            txtpassword.Name = "txtpassword";
            txtpassword.Size = new Size(256, 23);
            txtpassword.TabIndex = 5;
            txtpassword.TextChanged += textBox2_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = Color.Transparent;
            label3.Font = new Font("Segoe UI Black", 20F, FontStyle.Bold);
            label3.ForeColor = Color.Navy;
            label3.Location = new Point(97, 119);
            label3.Name = "label3";
            label3.Size = new Size(210, 37);
            label3.TabIndex = 7;
            label3.Text = "Welcome Back";
            label3.Click += label3_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Font = new Font("Segoe UI", 10F);
            label5.ForeColor = Color.MediumBlue;
            label5.Location = new Point(31, 165);
            label5.Name = "label5";
            label5.Size = new Size(342, 19);
            label5.TabIndex = 10;
            label5.Text = "Start a better experience by logging into your account.";
            label5.Click += label5_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.BackgroundImage = (Image)resources.GetObject("pictureBox2.BackgroundImage");
            pictureBox2.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox2.BorderStyle = BorderStyle.Fixed3D;
            pictureBox2.Cursor = Cursors.IBeam;
            pictureBox2.Location = new Point(162, 26);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(78, 76);
            pictureBox2.TabIndex = 11;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(399, 493);
            Controls.Add(pictureBox2);
            Controls.Add(label5);
            Controls.Add(label3);
            Controls.Add(txtpassword);
            Controls.Add(txtemail);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            DoubleBuffered = true;
            Name = "LoginForm";
            Text = "Login";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private Label label2;
        private TextBox txtemail;
        private TextBox txtpassword;
        private Label label3;
        private Label label5;
        private PictureBox pictureBox2;
    }
}