using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace SimpleHMS
{
    // DoctorForm - This form is used to manage doctors
    // You can Add, Update, and Delete doctor records
    public partial class DoctorForm : Form
    {
        // Designer-generated private fields for controls
        private TextBox txtName;
        private TextBox txtSpecialization;
        private TextBox txtPhone;
        private TextBox txtFee;
        private DataGridView dgv;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private ToolTip tooltip; // Tooltip shows hints when you hover over controls
        private IContainer components = null; // Required for designer

        // Stores the ID of selected doctor (0 means no selection)
        private int selectedDoctorID = 0;

        // Constructor - runs when form is created
        public DoctorForm()
        {
            InitializeComponent(); // Create all the controls
            LoadDoctors(); // Load existing doctors into the grid
        }

        // Required for Windows Form Designer support
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // This method creates all the UI controls (textboxes, buttons, etc.)
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
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
            btnClear.Click += new System.EventHandler(this.BtnClear_Click);
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

        #endregion

        // Add Button Click - Adds a new doctor to database
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Validate all inputs
            if (!ValidateInputs())
            {
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

            // Validate all inputs
            if (!ValidateInputs())
            {
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

            try
            {
                // First, check if this doctor has any appointments
                string appointmentQuery = $"SELECT COUNT(*) FROM Appointments WHERE DoctorID = {selectedDoctorID}";
                DataTable appointmentResult = DB.GetData(appointmentQuery);
                int appointmentCount = 0;

                if (appointmentResult != null && appointmentResult.Rows.Count > 0)
                {
                    appointmentCount = Convert.ToInt32(appointmentResult.Rows[0][0]);
                }

                // Check if this doctor has any bills
                string billQuery = $"SELECT COUNT(*) FROM Bills WHERE DoctorID = {selectedDoctorID}";
                DataTable billResult = DB.GetData(billQuery);
                int billCount = 0;

                if (billResult != null && billResult.Rows.Count > 0)
                {
                    billCount = Convert.ToInt32(billResult.Rows[0][0]);
                }

                // If doctor has associated records, show appropriate message
                if (appointmentCount > 0 || billCount > 0)
                {
                    string message = "Cannot delete this doctor because:\n\n";

                    if (appointmentCount > 0)
                    {
                        message += $"• {appointmentCount} appointment(s) are scheduled with this doctor\n";
                    }

                    if (billCount > 0)
                    {
                        message += $"• {billCount} bill(s) are associated with this doctor\n";
                    }

                    message += "\nPlease remove these records first before deleting the doctor.";

                    MessageBox.Show(message, "Cannot Delete Doctor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
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
            catch (Exception ex)
            {
                // This should rarely happen now since we check beforehand
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("foreign key"))
                {
                    MessageBox.Show("Cannot delete this doctor because they have associated appointments or bills. " +
                        "Please delete those records first.",
                        "Foreign Key Constraint Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    // Show the original error message for other types of errors
                    MessageBox.Show($"Error deleting doctor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            try
            {
                // SQL query to get all doctors
                string query = "SELECT DoctorID, Name, Specialization, Phone, Fee FROM Doctors";

                // Execute query and get the data
                DataTable dt = DB.GetData(query);

                // Bind results to DataGridView
                dgv.DataSource = dt;

                // Make sure DataGridView is visible and properly configured
                dgv.Visible = true;
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Hide the DoctorID column (we don't need to show it to user)
                if (dgv.Columns.Contains("DoctorID"))
                {
                    dgv.Columns["DoctorID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctor data: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // Clear Button Click - Clears all input fields
        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearFields(); // Clear all input fields
        }

        // Validate all input fields
        private bool ValidateInputs()
        {
            // Validate Name (only letters, spaces, and minimum length)
            if (string.IsNullOrWhiteSpace(txtName.Text) || txtName.Text.Length < 3 || !Regex.IsMatch(txtName.Text, @"^[a-zA-Z\s\.]+$"))
            {
                MessageBox.Show("Please enter a valid name (minimum 3 characters, letters only).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            // Validate Specialization (only letters, spaces, and minimum length)
            if (string.IsNullOrWhiteSpace(txtSpecialization.Text) || txtSpecialization.Text.Length < 3 ||
                !Regex.IsMatch(txtSpecialization.Text, @"^[a-zA-Z\s\-]+$"))
            {
                MessageBox.Show("Please enter a valid specialization (minimum 3 characters, letters only).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSpecialization.Focus();
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

            // Validate Fee (must be a positive number)
            if (!decimal.TryParse(txtFee.Text, out decimal fee) || fee <= 0)
            {
                MessageBox.Show("Please enter a valid fee amount (positive number).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFee.Focus();
                return false;
            }

            // All validations passed
            return true;
        }
    }
}