using System;
using System.Data;
using System.Windows.Forms;

namespace SimpleHMS
{
    // DoctorForm - This form is used to manage doctors
    // You can Add, Update, and Delete doctor records
    public partial class DoctorForm : Form
    {
        // Declare all the controls we'll use
        TextBox txtName, txtSpecialization, txtPhone, txtFee;
        DataGridView dgv;
        Button btnAdd, btnUpdate, btnDelete, btnClear;
        ToolTip tooltip; // Tooltip shows hints when you hover over controls
        int selectedDoctorID = 0; // Stores the ID of selected doctor (0 means no selection)

        // Constructor - runs when form is created
        public DoctorForm()
        {
            InitializeComponent(); // Create all the controls
            LoadDoctors(); // Load existing doctors into the grid
        }

        // This method creates all the UI controls (textboxes, buttons, etc.)
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Icon = (Icon)resources.GetObject("$this.Icon");
            // Set form properties
            this.Text = "Doctor Registration"; // Form title
            this.Size = new System.Drawing.Size(700, 500); // Form size (width, height)
            this.StartPosition = FormStartPosition.CenterScreen; // Open in center of screen

            // Initialize ToolTip - shows helpful hints when hovering
            tooltip = new ToolTip();
            tooltip.AutoPopDelay = 5000; // How long tooltip stays visible (5 seconds)
            tooltip.InitialDelay = 500; // Delay before showing tooltip (0.5 seconds)

            // Name Label and TextBox
            Label lblName = new Label() { Text = "Name:", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(100, 20) };
            txtName = new TextBox() { Location = new System.Drawing.Point(130, 20), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblName, "Enter doctor's full name"); // Tooltip for label
            tooltip.SetToolTip(txtName, "Type the doctor's full name here"); // Tooltip for textbox
            this.Controls.Add(lblName); // Add label to form
            this.Controls.Add(txtName); // Add textbox to form

            // Specialization Label and TextBox
            Label lblSpec = new Label() { Text = "Specialization:", Location = new System.Drawing.Point(20, 50), Size = new System.Drawing.Size(100, 20) };
            txtSpecialization = new TextBox() { Location = new System.Drawing.Point(130, 50), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblSpec, "Enter doctor's specialization");
            tooltip.SetToolTip(txtSpecialization, "Type the medical specialization (e.g., Cardiology, Pediatrics)");
            this.Controls.Add(lblSpec);
            this.Controls.Add(txtSpecialization);

            // Phone Label and TextBox
            Label lblPhone = new Label() { Text = "Phone:", Location = new System.Drawing.Point(20, 80), Size = new System.Drawing.Size(100, 20) };
            txtPhone = new TextBox() { Location = new System.Drawing.Point(130, 80), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblPhone, "Enter contact phone number");
            tooltip.SetToolTip(txtPhone, "Type the doctor's phone number");
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);

            // Fee Label and TextBox
            Label lblFee = new Label() { Text = "Fee:", Location = new System.Drawing.Point(20, 110), Size = new System.Drawing.Size(100, 20) };
            txtFee = new TextBox() { Location = new System.Drawing.Point(130, 110), Size = new System.Drawing.Size(200, 20) };
            tooltip.SetToolTip(lblFee, "Enter consultation fee");
            tooltip.SetToolTip(txtFee, "Type the consultation fee amount (numbers only)");
            this.Controls.Add(lblFee);
            this.Controls.Add(txtFee);

            // Add Button - Adds a new doctor
            btnAdd = new Button() { Text = "Add Doctor", Location = new System.Drawing.Point(130, 150), Size = new System.Drawing.Size(100, 30) };
            btnAdd.Click += BtnAdd_Click; // When clicked, run BtnAdd_Click method
            tooltip.SetToolTip(btnAdd, "Click to add a new doctor to the database");
            this.Controls.Add(btnAdd);

            // Update Button - Updates selected doctor
            btnUpdate = new Button() { Text = "Update", Location = new System.Drawing.Point(240, 150), Size = new System.Drawing.Size(90, 30) };
            btnUpdate.Click += BtnUpdate_Click; // When clicked, run BtnUpdate_Click method
            tooltip.SetToolTip(btnUpdate, "Click to update the selected doctor's information");
            this.Controls.Add(btnUpdate);

            // Delete Button - Deletes selected doctor
            btnDelete = new Button() { Text = "Delete", Location = new System.Drawing.Point(340, 150), Size = new System.Drawing.Size(90, 30) };
            btnDelete.Click += BtnDelete_Click; // When clicked, run BtnDelete_Click method
            tooltip.SetToolTip(btnDelete, "Click to delete the selected doctor from the database");
            this.Controls.Add(btnDelete);

            // Clear Button - Clears all fields
            btnClear = new Button() { Text = "Clear", Location = new System.Drawing.Point(440, 150), Size = new System.Drawing.Size(90, 30) };
            btnClear.Click += (s, e) => ClearFields(); // When clicked, clear all fields
            tooltip.SetToolTip(btnClear, "Click to clear all input fields");
            this.Controls.Add(btnClear);

            // DataGridView - Shows all doctors in a table
            dgv = new DataGridView() { Location = new System.Drawing.Point(20, 200), Size = new System.Drawing.Size(650, 250) };
            dgv.ReadOnly = true; // User cannot edit cells directly
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Select entire row when clicked
            dgv.CellClick += Dgv_CellClick; // When a cell is clicked, run Dgv_CellClick method
            tooltip.SetToolTip(dgv, "Click on any row to select a doctor for update or delete");
            this.Controls.Add(dgv);
        }

        // Add Button Click - Adds a new doctor to database
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtSpecialization.Text) || 
                string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrWhiteSpace(txtFee.Text))
            {
                MessageBox.Show("Please fill all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop here if validation fails
            }

            // Create SQL INSERT query to add new doctor
            string query = $"INSERT INTO Doctors (Name, Specialization, Phone, Fee) VALUES " +
                          $"('{txtName.Text}', '{txtSpecialization.Text}', '{txtPhone.Text}', {txtFee.Text})";

            // Execute the query
            int result = DB.SetData(query);
            
            // Check if data was added successfully
            if (result > 0)
            {
                MessageBox.Show("Doctor added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear all input fields
                LoadDoctors(); // Refresh the doctor list
            }
        }

        // Update Button Click - Updates selected doctor's information
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Check if a doctor is selected
            if (selectedDoctorID == 0)
            {
                MessageBox.Show("Please select a doctor from the list to update!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop here if no doctor is selected
            }

            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtSpecialization.Text) || 
                string.IsNullOrWhiteSpace(txtPhone.Text) || string.IsNullOrWhiteSpace(txtFee.Text))
            {
                MessageBox.Show("Please fill all fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop here if validation fails
            }

            // Create SQL UPDATE query to modify doctor's data
            string query = $"UPDATE Doctors SET Name='{txtName.Text}', Specialization='{txtSpecialization.Text}', " +
                          $"Phone='{txtPhone.Text}', Fee={txtFee.Text} WHERE DoctorID={selectedDoctorID}";

            // Execute the query
            int result = DB.SetData(query);
            
            // Check if data was updated successfully
            if (result > 0)
            {
                MessageBox.Show("Doctor updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear all input fields
                LoadDoctors(); // Refresh the doctor list
            }
        }

        // Delete Button Click - Deletes selected doctor from database
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Check if a doctor is selected
            if (selectedDoctorID == 0)
            {
                MessageBox.Show("Please select a doctor from the list to delete!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop here if no doctor is selected
            }

            // Ask for confirmation before deleting
            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this doctor?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If user clicked Yes
            if (confirm == DialogResult.Yes)
            {
                // Create SQL DELETE query
                string query = $"DELETE FROM Doctors WHERE DoctorID={selectedDoctorID}";
                
                // Execute the query
                int result = DB.SetData(query);
                
                // Check if data was deleted successfully
                if (result > 0)
                {
                    MessageBox.Show("Doctor deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields(); // Clear all input fields
                    LoadDoctors(); // Refresh the doctor list
                }
            }
        }
         
        // DataGridView Cell Click - When user clicks on a row, load that doctor's data
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if a valid row was clicked (not header)
            if (e.RowIndex >= 0)
            {
                // Get the clicked row
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                
                // Get doctor's data from the row and fill the textboxes
                selectedDoctorID = Convert.ToInt32(row.Cells["DoctorID"].Value); // Store the ID
                txtName.Text = row.Cells["Name"].Value.ToString(); // Fill name
                txtSpecialization.Text = row.Cells["Specialization"].Value.ToString(); // Fill specialization
                txtPhone.Text = row.Cells["Phone"].Value.ToString(); // Fill phone
                txtFee.Text = row.Cells["Fee"].Value.ToString(); // Fill fee
            }
        }

        // Load all doctors from database and show in grid
        private void LoadDoctors()
        {
            // SQL query to get all doctors
            string query = "SELECT DoctorID, Name, Specialization, Phone, Fee FROM Doctors";
            
            // Execute query and bind results to DataGridView
            dgv.DataSource = DB.GetData(query);
            
            // Hide the DoctorID column (we don't need to show it to user)
            if (dgv.Columns.Contains("DoctorID"))
            {
                dgv.Columns["DoctorID"].Visible = false;
            }
        }

        // Clear all input fields
        private void ClearFields()
        {
            selectedDoctorID = 0; // Reset selected ID
            txtName.Clear(); // Clear name textbox
            txtSpecialization.Clear(); // Clear specialization textbox
            txtPhone.Clear(); // Clear phone textbox
            txtFee.Clear(); // Clear fee textbox
        }
    }
}

