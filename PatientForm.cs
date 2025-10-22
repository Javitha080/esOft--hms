using System;
using System.Data;
using System.Windows.Forms;

namespace SimpleHMS
{
    // PatientForm - This form is used to manage patients
    // You can Add, Update, and Delete patient records
    public partial class PatientForm : Form
    {
        // Declare all the controls we'll use
        TextBox txtName, txtAge, txtPhone, txtAddress;
        ComboBox cmbGender;
        DataGridView dgv;
        Button btnAdd, btnUpdate, btnDelete, btnClear;
        ToolTip tooltip; // Tooltip shows hints when you hover over controls
        int selectedPatientID = 0; // Stores the ID of selected patient (0 means no selection)

        // Constructor - runs when form is created
        public PatientForm()
        {
            InitializeComponent(); // Create all the controls
            LoadPatients(); // Load existing patients into the grid
        }

        // This method creates all the UI controls (textboxes, buttons, etc.)
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Icon = (Icon)resources.GetObject("$this.Icon");
            // Set form properties
            this.Text = "Patient Registration"; // Form title
            this.Size = new System.Drawing.Size(700, 500); // Form size (width, height)
            this.StartPosition = FormStartPosition.CenterScreen; // Open in center of screen

            // Initialize ToolTip - shows helpful hints when hovering
            tooltip = new ToolTip();
            tooltip.AutoPopDelay = 5000; // How long tooltip stays visible (5 seconds)
            tooltip.InitialDelay = 500; // Delay before showing tooltip (0.5 seconds)

            // Name Label and TextBox
            Label lblName = new Label() { Text = "Name:", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(80, 20) };
            txtName = new TextBox() { Location = new System.Drawing.Point(100, 20), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblName, "Enter patient's full name"); // Tooltip for label
            tooltip.SetToolTip(txtName, "Type the patient's full name here"); // Tooltip for textbox
            this.Controls.Add(lblName); // Add label to form
            this.Controls.Add(txtName); // Add textbox to form

            // Age Label and TextBox
            Label lblAge = new Label() { Text = "Age:", Location = new System.Drawing.Point(20, 50), Size = new System.Drawing.Size(80, 20) };
            txtAge = new TextBox() { Location = new System.Drawing.Point(100, 50), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblAge, "Enter patient's age");
            tooltip.SetToolTip(txtAge, "Type the patient's age in years");
            this.Controls.Add(lblAge);
            this.Controls.Add(txtAge);

            // Gender Label and ComboBox (Dropdown)
            Label lblGender = new Label() { Text = "Gender:", Location = new System.Drawing.Point(20, 80), Size = new System.Drawing.Size(80, 20) };
            cmbGender = new ComboBox() { Location = new System.Drawing.Point(100, 80), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new string[] { "Male", "Female" }); // Add Male and Female options
            tooltip.SetToolTip(lblGender, "Select patient's gender");
            tooltip.SetToolTip(cmbGender, "Choose Male or Female from the dropdown");
            this.Controls.Add(lblGender);
            this.Controls.Add(cmbGender);

            // Phone Label and TextBox
            Label lblPhone = new Label() { Text = "Phone:", Location = new System.Drawing.Point(20, 110), Size = new System.Drawing.Size(80, 20) };
            txtPhone = new TextBox() { Location = new System.Drawing.Point(100, 110), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblPhone, "Enter contact phone number");
            tooltip.SetToolTip(txtPhone, "Type the patient's phone number");
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);

            // Address Label and TextBox
            Label lblAddress = new Label() { Text = "Address:", Location = new System.Drawing.Point(20, 140), Size = new System.Drawing.Size(80, 20) };
            txtAddress = new TextBox() { Location = new System.Drawing.Point(100, 140), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblAddress, "Enter patient's address");
            tooltip.SetToolTip(txtAddress, "Type the patient's residential address");
            this.Controls.Add(lblAddress);
            this.Controls.Add(txtAddress);

            // Add Button - Adds a new patient
            btnAdd = new Button() { Text = "Add Patient", Location = new System.Drawing.Point(100, 180), Size = new System.Drawing.Size(100, 30) };
            btnAdd.Click += BtnAdd_Click; // When clicked, run BtnAdd_Click method
            tooltip.SetToolTip(btnAdd, "Click to add a new patient to the database");
            this.Controls.Add(btnAdd);

            // Update Button - Updates selected patient
            btnUpdate = new Button() { Text = "Update", Location = new System.Drawing.Point(210, 180), Size = new System.Drawing.Size(90, 30) };
            btnUpdate.Click += BtnUpdate_Click; // When clicked, run BtnUpdate_Click method
            tooltip.SetToolTip(btnUpdate, "Click to update the selected patient's information");
            this.Controls.Add(btnUpdate);

            // Delete Button - Deletes selected patient
            btnDelete = new Button() { Text = "Delete", Location = new System.Drawing.Point(310, 180), Size = new System.Drawing.Size(90, 30) };
            btnDelete.Click += BtnDelete_Click; // When clicked, run BtnDelete_Click method
            tooltip.SetToolTip(btnDelete, "Click to delete the selected patient from the database");
            this.Controls.Add(btnDelete);

            // Clear Button - Clears all fields
            btnClear = new Button() { Text = "Clear", Location = new System.Drawing.Point(410, 180), Size = new System.Drawing.Size(90, 30) };
            btnClear.Click += (s, e) => ClearFields(); // When clicked, clear all fields
            tooltip.SetToolTip(btnClear, "Click to clear all input fields");
            this.Controls.Add(btnClear);

            // DataGridView - Shows all patients in a table
            dgv = new DataGridView() { Location = new System.Drawing.Point(20, 230), Size = new System.Drawing.Size(650, 220) };
            dgv.ReadOnly = true; // User cannot edit cells directly
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Select entire row when clicked
            dgv.CellClick += Dgv_CellClick; // When a cell is clicked, run Dgv_CellClick method
            tooltip.SetToolTip(dgv, "Click on any row to select a patient for update or delete");
            this.Controls.Add(dgv);
        }

        // Add Button Click - Adds a new patient to database
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtAge.Text) || 
                cmbGender.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please fill all required fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop here if validation fails
            }

            // Create SQL INSERT query to add new patient
            string query = $"INSERT INTO Patients (Name, Age, Gender, Phone, Address) VALUES " +
                          $"('{txtName.Text}', {txtAge.Text}, '{cmbGender.Text}', '{txtPhone.Text}', '{txtAddress.Text}')";

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

            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtAge.Text) || 
                cmbGender.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Please fill all required fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop here if validation fails
            }

            // Create SQL UPDATE query to modify patient's data
            string query = $"UPDATE Patients SET Name='{txtName.Text}', Age={txtAge.Text}, Gender='{cmbGender.Text}', " +
                          $"Phone='{txtPhone.Text}', Address='{txtAddress.Text}' WHERE PatientID={selectedPatientID}";

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
                txtName.Text = row.Cells["Name"].Value.ToString(); // Fill name
                txtAge.Text = row.Cells["Age"].Value.ToString(); // Fill age
                cmbGender.Text = row.Cells["Gender"].Value.ToString(); // Fill gender
                txtPhone.Text = row.Cells["Phone"].Value.ToString(); // Fill phone
                txtAddress.Text = row.Cells["Address"].Value.ToString(); // Fill address
            }
        }

        // Load all patients from database and show in grid
        private void LoadPatients()
        {
            // SQL query to get all patients
            string query = "SELECT PatientID, Name, Age, Gender, Phone, Address FROM Patients";
            
            // Execute query and bind results to DataGridView
            dgv.DataSource = DB.GetData(query);
            
            // Hide the PatientID column (we don't need to show it to user)
            if (dgv.Columns.Contains("PatientID"))
            {
                dgv.Columns["PatientID"].Visible = false;
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
        }
    }
}

