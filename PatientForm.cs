using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SimpleHMS
{
    // PatientForm - This form is used to manage patients
    // You can Add, Update, and Delete patient records
    public partial class PatientForm : Form
    {
        // Declare all the controls we'll use
        private TextBox txtName;
        private TextBox txtAge;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private TextBox txtEmail;
        private ComboBox cmbGender;
        private DataGridView dgv;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private ToolTip tooltip;
        private Label lblName;
        private Label lblAge;
        private Label lblGender;
        private Label lblPhone;
        private Label lblAddress;
        private Label lblEmail;
        private int selectedPatientID = 0; // Stores the ID of selected patient (0 means no selection)

        // Constructor - runs when form is created
        public PatientForm()
        {
            InitializeComponent(); // Create all the controls
            LoadPatients(); // Load existing patients into the grid
        }

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
            components = new System.ComponentModel.Container();
            tooltip = new ToolTip(components);
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblName = new Label();
            txtName = new TextBox();
            lblAge = new Label();
            txtAge = new TextBox();
            lblGender = new Label();
            cmbGender = new ComboBox();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblAddress = new Label();
            txtAddress = new TextBox();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnClear = new Button();
            dgv = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // tooltip
            // 
            tooltip.AutoPopDelay = 5000;
            tooltip.InitialDelay = 500;
            tooltip.ReshowDelay = 100;
            // 
            // lblEmail
            // 
            lblEmail.Location = new Point(23, 196);
            lblEmail.Margin = new Padding(4, 0, 4, 0);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(93, 23);
            lblEmail.TabIndex = 10;
            lblEmail.Text = "Email:";
            tooltip.SetToolTip(lblEmail, "Enter patient's email address");
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(117, 196);
            txtEmail.Margin = new Padding(4, 3, 4, 3);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(233, 23);
            txtEmail.TabIndex = 11;
            tooltip.SetToolTip(txtEmail, "Type the patient's email address for billing");
            txtEmail.TextChanged += txtEmail_TextChanged;
            // 
            // lblName
            // 
            lblName.Location = new Point(23, 23);
            lblName.Margin = new Padding(4, 0, 4, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(93, 23);
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            tooltip.SetToolTip(lblName, "Enter patient's full name");
            // 
            // txtName
            // 
            txtName.Location = new Point(117, 23);
            txtName.Margin = new Padding(4, 3, 4, 3);
            txtName.Name = "txtName";
            txtName.Size = new Size(233, 23);
            txtName.TabIndex = 1;
            tooltip.SetToolTip(txtName, "Type the patient's full name here");
            // 
            // lblAge
            // 
            lblAge.Location = new Point(23, 58);
            lblAge.Margin = new Padding(4, 0, 4, 0);
            lblAge.Name = "lblAge";
            lblAge.Size = new Size(93, 23);
            lblAge.TabIndex = 2;
            lblAge.Text = "Age:";
            tooltip.SetToolTip(lblAge, "Enter patient's age");
            // 
            // txtAge
            // 
            txtAge.Location = new Point(117, 58);
            txtAge.Margin = new Padding(4, 3, 4, 3);
            txtAge.Name = "txtAge";
            txtAge.Size = new Size(233, 23);
            txtAge.TabIndex = 3;
            tooltip.SetToolTip(txtAge, "Type the patient's age in years");
            // 
            // lblGender
            // 
            lblGender.Location = new Point(23, 92);
            lblGender.Margin = new Padding(4, 0, 4, 0);
            lblGender.Name = "lblGender";
            lblGender.Size = new Size(93, 23);
            lblGender.TabIndex = 4;
            lblGender.Text = "Gender:";
            tooltip.SetToolTip(lblGender, "Select patient's gender");
            // 
            // cmbGender
            // 
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.Items.AddRange(new object[] { "Male", "Female" });
            cmbGender.Location = new Point(117, 92);
            cmbGender.Margin = new Padding(4, 3, 4, 3);
            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(233, 23);
            cmbGender.TabIndex = 5;
            tooltip.SetToolTip(cmbGender, "Choose Male or Female from the dropdown");
            // 
            // lblPhone
            // 
            lblPhone.Location = new Point(23, 127);
            lblPhone.Margin = new Padding(4, 0, 4, 0);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(93, 23);
            lblPhone.TabIndex = 6;
            lblPhone.Text = "Phone:";
            tooltip.SetToolTip(lblPhone, "Enter contact phone number");
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(117, 127);
            txtPhone.Margin = new Padding(4, 3, 4, 3);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(233, 23);
            txtPhone.TabIndex = 7;
            tooltip.SetToolTip(txtPhone, "Type the patient's phone number");
            // 
            // lblAddress
            // 
            lblAddress.Location = new Point(23, 162);
            lblAddress.Margin = new Padding(4, 0, 4, 0);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(93, 23);
            lblAddress.TabIndex = 8;
            lblAddress.Text = "Address:";
            tooltip.SetToolTip(lblAddress, "Enter patient's address");
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(117, 162);
            txtAddress.Margin = new Padding(4, 3, 4, 3);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(233, 23);
            txtAddress.TabIndex = 9;
            tooltip.SetToolTip(txtAddress, "Type the patient's residential address");
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(117, 242);
            btnAdd.Margin = new Padding(4, 3, 4, 3);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(117, 35);
            btnAdd.TabIndex = 10;
            btnAdd.Text = "Add Patient";
            tooltip.SetToolTip(btnAdd, "Click to add a new patient to the database");
            btnAdd.Click += BtnAdd_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(245, 242);
            btnUpdate.Margin = new Padding(4, 3, 4, 3);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(117, 35);
            btnUpdate.TabIndex = 11;
            btnUpdate.Text = "Update";
            tooltip.SetToolTip(btnUpdate, "Click to update the selected patient's information");
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += BtnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(373, 242);
            btnDelete.Margin = new Padding(4, 3, 4, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(117, 35);
            btnDelete.TabIndex = 12;
            btnDelete.Text = "Delete";
            tooltip.SetToolTip(btnDelete, "Click to delete the selected patient from the database");
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(502, 242);
            btnClear.Margin = new Padding(4, 3, 4, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(105, 35);
            btnClear.TabIndex = 13;
            btnClear.Text = "Clear";
            tooltip.SetToolTip(btnClear, "Click to clear all input fields");
            btnClear.Click += BtnClear_Click;
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.Location = new Point(23, 300);
            dgv.Margin = new Padding(4, 3, 4, 3);
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new Size(875, 346);
            dgv.TabIndex = 14;
            tooltip.SetToolTip(dgv, "Click on any row to select a patient for update or delete");
            dgv.CellClick += Dgv_CellClick;
            // 
            // PatientForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(912, 663);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblAge);
            Controls.Add(txtAge);
            Controls.Add(lblGender);
            Controls.Add(cmbGender);
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
            Controls.Add(lblAddress);
            Controls.Add(txtAddress);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            Controls.Add(btnClear);
            Controls.Add(dgv);
            Margin = new Padding(4, 3, 4, 3);
            Name = "PatientForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Patient Registration";
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // Add Button Click - Adds a new patient to database
        private void BtnAdd_Click(object sender, EventArgs e)
        {
           
            // Validate all inputs
            if (!ValidateInputs())
            {
                return; // Stop here if validation fails
            }

            // Create SQL INSERT query to add new patient
            string query = $"INSERT INTO Patients (Name, Age, Gender, Phone, Address, Email) VALUES " +
                          $"('{txtName.Text}', {txtAge.Text}, '{cmbGender.Text}', '{txtPhone.Text}', '{txtAddress.Text}', '{txtEmail.Text}')";

            // Execute the query
            int result = DB.SetData(query);

            // Check if data was added successfully
            if (result > 0)
            {
                MessageBox.Show("Patient added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear all input fields
                LoadPatients(); // Refresh the patient list
            }
        }

        // Update Button Click - Updates selected patient's information
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Check if a patient is selected
            if (selectedPatientID == 0)
            {
                MessageBox.Show("Please select a patient from the list to update!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop here if no patient is selected
            }

            // Validate all inputs
            if (!ValidateInputs())
            {
                return; // Stop here if validation fails
            }

            // Create SQL UPDATE query to modify patient's data (NOW INCLUDES EMAIL)
            string query = $"UPDATE Patients SET Name='{txtName.Text}', Age={txtAge.Text}, Gender='{cmbGender.Text}', " +
                          $"Phone='{txtPhone.Text}', Address='{txtAddress.Text}', Email='{txtEmail.Text}' WHERE PatientID={selectedPatientID}";

            // Execute the query
            int result = DB.SetData(query);

            // Check if data was updated successfully
            if (result > 0)
            {
                MessageBox.Show("Patient updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear all input fields
                LoadPatients(); // Refresh the patient list
            }
        }

        // Delete Button Click - Deletes selected patient from database
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Check if a patient is selected
            if (selectedPatientID == 0)
            {
                MessageBox.Show("Please select a patient from the list to delete!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop here if no patient is selected
            }

            // Ask for confirmation before deleting
            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this patient?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If user clicked Yes
            if (confirm == DialogResult.Yes)
            {
                // Create SQL DELETE query
                string query = $"DELETE FROM Patients WHERE PatientID={selectedPatientID}";

                // Execute the query
                int result = DB.SetData(query);

                // Check if data was deleted successfully
                if (result > 0)
                {
                    MessageBox.Show("Patient deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields(); // Clear all input fields
                    LoadPatients(); // Refresh the patient list
                }
            }
        }

        // DataGridView Cell Click - When user clicks on a row, load that patient's data
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if a valid row was clicked (not header)
            if (e.RowIndex >= 0)
            {
                // Get the clicked row
                DataGridViewRow row = dgv.Rows[e.RowIndex];

                // Get patient's data from the row and fill the textboxes
                selectedPatientID = Convert.ToInt32(row.Cells["PatientID"].Value); // Store the ID
                txtName.Text = row.Cells["FirstName"].Value.ToString(); // Fill name
                txtAge.Text = row.Cells["Age"].Value.ToString(); // Fill age
                cmbGender.Text = row.Cells["Gender"].Value.ToString(); // Fill gender
                txtPhone.Text = row.Cells["Phone"].Value.ToString(); // Fill phone
                txtAddress.Text = row.Cells["Address"].Value.ToString(); // Fill address
                txtEmail.Text = row.Cells["Email"].Value.ToString(); // Fill email (FIXED)
            }
        }

        // Load all patients from database and show in grid
        private void LoadPatients()
        {
            try
            {
                // SQL query to get all patients (NOW INCLUDES EMAIL)
                string query = "SELECT PatientID, Name AS FirstName, Age, Gender, Phone, Address, Email FROM Patients";

                // Execute query and get the data
                DataTable dt = DB.GetData(query);

                // Bind results to DataGridView
                dgv.DataSource = dt;

                // Make sure DataGridView is visible and properly configured
                dgv.Visible = true;
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Hide the PatientID column (we don't need to show it to user)
                if (dgv.Columns.Contains("PatientID"))
                {
                    dgv.Columns["PatientID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patient data: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear all input fields
        private void ClearFields()
        {
            selectedPatientID = 0; // Reset selected ID
            txtName.Clear(); // Clear name textbox
            txtAge.Clear(); // Clear age textbox
            cmbGender.SelectedIndex = -1; // Deselect gender
            txtPhone.Clear(); // Clear phone textbox
            txtAddress.Clear(); // Clear address textbox
            txtEmail.Clear(); // Clear email textbox (FIXED)
        }

        // Clear Button Click - Clears all input fields
        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields(); // Clear all input fields
        }

        // Validate all input fields
        private bool ValidateInputs()
        {
            // Validate Name (only letters, spaces, and minimum length)
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Length < 3 || !Regex.IsMatch(txtName.Text, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Please enter a valid name (minimum 3 characters, letters only).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            // Validate Age (must be a number between 1-100)
            if (!int.TryParse(txtAge.Text, out int age) || age < 1 || age > 100)
            {
                MessageBox.Show("Please enter a valid age (1-100).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAge.Focus();
                return false;
            }

            // Validate Gender selection
            if (cmbGender.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a gender.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbGender.Focus();
                return false;
            }

            // Validate Phone (must be 10-15 digits, can include +, -, spaces)
            if (string.IsNullOrWhiteSpace(txtPhone.Text) || !Regex.IsMatch(txtPhone.Text, @"^[\d\s\-\+]{10,15}$"))
            {
                MessageBox.Show("Please enter a valid phone number (10-15 digits).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            // Validate Email (FIXED - NOW INCLUDES EMAIL VALIDATION)
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email address (e.g., example@email.com).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            // All validations passed
            return true;
        }

        // Email TextChanged event - Optional: Add real-time validation feedback
        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            // Optional: You can add real-time email validation here
            // For example, change the textbox color based on valid/invalid email

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.BackColor = SystemColors.Window; // Default color
            }
            else if (Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                txtEmail.BackColor = Color.LightGreen; // Valid email
            }
            else
            {
                txtEmail.BackColor = Color.LightPink; // Invalid email
            }
        }
    }
}