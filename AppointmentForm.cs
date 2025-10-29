using System;
using System.Data;
using System.Windows.Forms;

namespace SimpleHMS
{
    // AppointmentForm - This form is used to manage appointments
    // You can Book, Update, and Delete appointments
    public partial class AppointmentForm : Form
    {
        // Declare all the controls we'll use
        ComboBox cmbPatient, cmbDoctor;
        DateTimePicker dtpDate, dtpTime;
        DataGridView dgv;
        Button btnBook, btnUpdate, btnDelete, btnClear;
        ToolTip tooltip; // Tooltip shows hints when you hover over controls
        int selectedAppointmentID = 0; // Stores the ID of selected appointment (0 means no selection)

        // Constructor - runs when form is created
        public AppointmentForm()
        {
            InitializeComponent(); // Create all the controls
            LoadPatients(); // Load patients into dropdown
            LoadDoctors(); // Load doctors into dropdown
            LoadAppointments(); // Load existing appointments into grid
        }

        // This method creates all the UI controls (dropdowns, buttons, etc.)
        private void InitializeComponent()
        {   
            // icon 
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Icon = (Icon)resources.GetObject("$this.Icon");
            // Set form properties
            this.Text = "Appointments"; // Form title
            this.Size = new System.Drawing.Size(700, 550); // Form size (width, height)
            this.StartPosition = FormStartPosition.CenterScreen; // Open in center of screen

            // Initialize ToolTip - shows helpful hints when hovering
            tooltip = new ToolTip();
            tooltip.AutoPopDelay = 5000; // How long tooltip stays visible (5 seconds)
            tooltip.InitialDelay = 500; // Delay before showing tooltip (0.5 seconds)

            // Patient Label and ComboBox (Dropdown)
            Label lblPatient = new Label() { Text = "Patient:", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(100, 20) };
            cmbPatient = new ComboBox() { Location = new System.Drawing.Point(130, 20), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            tooltip.SetToolTip(lblPatient, "Select a patient for the appointment");
            tooltip.SetToolTip(cmbPatient, "Choose a patient from the dropdown list");
            this.Controls.Add(lblPatient);
            this.Controls.Add(cmbPatient);

            // Doctor Label and ComboBox (Dropdown)
            Label lblDoctor = new Label() { Text = "Doctor:", Location = new System.Drawing.Point(20, 50), Size = new System.Drawing.Size(100, 20) };
            cmbDoctor = new ComboBox() { Location = new System.Drawing.Point(130, 50), Size = new System.Drawing.Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            tooltip.SetToolTip(lblDoctor, "Select a doctor for the appointment");
            tooltip.SetToolTip(cmbDoctor, "Choose a doctor from the dropdown list");
            this.Controls.Add(lblDoctor);
            this.Controls.Add(cmbDoctor);

            // Date Label and DateTimePicker
            Label lblDate = new Label() { Text = "Date:", Location = new System.Drawing.Point(20, 80), Size = new System.Drawing.Size(100, 20) };
            dtpDate = new DateTimePicker() { Location = new System.Drawing.Point(130, 80), Size = new System.Drawing.Size(200, 20), Format = DateTimePickerFormat.Short };
            tooltip.SetToolTip(lblDate, "Select appointment date");
            tooltip.SetToolTip(dtpDate, "Click to open calendar and choose a date");
            this.Controls.Add(lblDate);
            this.Controls.Add(dtpDate);

            // Time Label and DateTimePicker
            Label lblTime = new Label() { Text = "Time:", Location = new System.Drawing.Point(20, 110), Size = new System.Drawing.Size(100, 20) };
            dtpTime = new DateTimePicker() { Location = new System.Drawing.Point(130, 110), Size = new System.Drawing.Size(200, 20), Format = DateTimePickerFormat.Time, ShowUpDown = true };
            tooltip.SetToolTip(lblTime, "Select appointment time");
            tooltip.SetToolTip(dtpTime, "Use up/down arrows to set the time");
            this.Controls.Add(lblTime);
            this.Controls.Add(dtpTime);

            // Book Button - Books a new appointment
            btnBook = new Button() { Text = "Book Appointment", Location = new System.Drawing.Point(130, 150), Size = new System.Drawing.Size(120, 30) };
            btnBook.Click += BtnBook_Click; // When clicked, run BtnBook_Click method
            tooltip.SetToolTip(btnBook, "Click to book a new appointment");
            this.Controls.Add(btnBook);

            // Update Button - Updates selected appointment
            btnUpdate = new Button() { Text = "Update", Location = new System.Drawing.Point(260, 150), Size = new System.Drawing.Size(90, 30) };
            btnUpdate.Click += BtnUpdate_Click; // When clicked, run BtnUpdate_Click method
            tooltip.SetToolTip(btnUpdate, "Click to update the selected appointment");
            this.Controls.Add(btnUpdate);

            // Delete Button - Deletes selected appointment
            btnDelete = new Button() { Text = "Delete", Location = new System.Drawing.Point(360, 150), Size = new System.Drawing.Size(90, 30) };
            btnDelete.Click += BtnDelete_Click; // When clicked, run BtnDelete_Click method
            tooltip.SetToolTip(btnDelete, "Click to cancel/delete the selected appointment");
            this.Controls.Add(btnDelete);

            // Clear Button - Clears all fields
            btnClear = new Button() { Text = "Clear", Location = new System.Drawing.Point(460, 150), Size = new System.Drawing.Size(70, 30) };
            btnClear.Click += (s, e) => ClearFields(); // When clicked, clear all fields
            tooltip.SetToolTip(btnClear, "Click to clear all selections");
            this.Controls.Add(btnClear);

            // DataGridView - Shows all appointments in a table
            dgv = new DataGridView() { Location = new System.Drawing.Point(20, 200), Size = new System.Drawing.Size(650, 300) };
            dgv.ReadOnly = true; // User cannot edit cells directly
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Select entire row when clicked
            dgv.CellClick += Dgv_CellClick; // When a cell is clicked, run Dgv_CellClick method
            tooltip.SetToolTip(dgv, "Click on any row to select an appointment for update or delete");
            this.Controls.Add(dgv);
        }

        // Book Button Click - Books a new appointment
        private void BtnBook_Click(object sender, EventArgs e)
        {
            // Check if patient and doctor are selected
            if (cmbPatient.SelectedValue == null || cmbDoctor.SelectedValue == null)
            {
                MessageBox.Show("Please select patient and doctor!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop here if validation fails
            }

            // Format date and time for SQL
            string date = dtpDate.Value.ToString("yyyy-MM-dd"); // Format: 2025-10-18
            string time = dtpTime.Value.ToString("HH:mm:ss"); // Format: 14:30:00

            // Create SQL INSERT query to book new appointment
            string query = $"INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, AppointmentTime) VALUES " +
                          $"({cmbPatient.SelectedValue}, {cmbDoctor.SelectedValue}, '{date}', '{time}')";

            // Execute the query
            int result = DB.SetData(query);
            
            // Check if appointment was booked successfully
            if (result > 0)
            {
                MessageBox.Show("Appointment booked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear all selections
                LoadAppointments(); // Refresh the appointment list
            }
        }

        // Update Button Click - Updates selected appointment
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Check if an appointment is selected
            if (selectedAppointmentID == 0)
            {
                MessageBox.Show("Please select an appointment from the list to update!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop here if no appointment is selected
            }

            // Check if patient and doctor are selected
            if (cmbPatient.SelectedValue == null || cmbDoctor.SelectedValue == null)
            {
                MessageBox.Show("Please select patient and doctor!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop here if validation fails
            }

            // Format date and time for SQL
            string date = dtpDate.Value.ToString("yyyy-MM-dd"); // Format: 2025-10-18
            string time = dtpTime.Value.ToString("HH:mm:ss"); // Format: 14:30:00

            // Create SQL UPDATE query to modify appointment
            string query = $"UPDATE Appointments SET PatientID={cmbPatient.SelectedValue}, DoctorID={cmbDoctor.SelectedValue}, " +
                          $"AppointmentDate='{date}', AppointmentTime='{time}' WHERE AppointmentID={selectedAppointmentID}";

            // Execute the query
            int result = DB.SetData(query);
            
            // Check if appointment was updated successfully
            if (result > 0)
            {
                MessageBox.Show("Appointment updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear all selections
                LoadAppointments(); // Refresh the appointment list
            }
        }

        // Delete Button Click - Deletes selected appointment
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Check if an appointment is selected
            if (selectedAppointmentID == 0)
            {
                MessageBox.Show("Please select an appointment from the list to delete!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop here if no appointment is selected
            }

            // Ask for confirmation before deleting
            DialogResult confirm = MessageBox.Show("Are you sure you want to cancel this appointment?", 
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // If user clicked Yes
            if (confirm == DialogResult.Yes)
            {
                // Create SQL DELETE query
                string query = $"DELETE FROM Appointments WHERE AppointmentID={selectedAppointmentID}";
                
                // Execute the query
                int result = DB.SetData(query);
                
                // Check if appointment was deleted successfully
                if (result > 0)
                {
                    MessageBox.Show("Appointment cancelled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields(); // Clear all selections
                    LoadAppointments(); // Refresh the appointment list
                }
            }
        }

        // DataGridView Cell Click - When user clicks on a row, load that appointment's data
        private void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if a valid row was clicked (not header)
            if (e.RowIndex >= 0)
            {
                // Get the clicked row
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                
                // Get appointment's data from the row
                selectedAppointmentID = Convert.ToInt32(row.Cells["AppointmentID"].Value); // Store the ID
                
                // Set patient dropdown to the appointment's patient
                int patientID = Convert.ToInt32(row.Cells["PatientID"].Value);
                cmbPatient.SelectedValue = patientID;
                
                // Set doctor dropdown to the appointment's doctor
                int doctorID = Convert.ToInt32(row.Cells["DoctorID"].Value);
                cmbDoctor.SelectedValue = doctorID;
                
                // Set date picker to the appointment's date
                dtpDate.Value = Convert.ToDateTime(row.Cells["Date"].Value);
                
                // Set time picker to the appointment's time
                dtpTime.Value = DateTime.Parse(row.Cells["Time"].Value.ToString());
            }
        }

        // Load all patients from database into dropdown
        private void LoadPatients()
        {
            // SQL query to get all patients
            DataTable dt = DB.GetData("SELECT PatientID, Name FROM Patients");
            
            // Bind data to patient dropdown
            cmbPatient.DataSource = dt;
            cmbPatient.DisplayMember = "Name"; // Show patient name
            cmbPatient.ValueMember = "PatientID"; // Store patient ID (hidden)
            cmbPatient.SelectedIndex = -1; // Don't select anything by default
        }

        // Load all doctors from database into dropdown
        private void LoadDoctors()
        {
            // SQL query to get all doctors
            DataTable dt = DB.GetData("SELECT DoctorID, Name FROM Doctors");
            
            // Bind data to doctor dropdown
            cmbDoctor.DataSource = dt;
            cmbDoctor.DisplayMember = "Name"; // Show doctor name
            cmbDoctor.ValueMember = "DoctorID"; // Store doctor ID (hidden)
            cmbDoctor.SelectedIndex = -1; // Don't select anything by default
        }

        // Load all appointments from database and show in grid
        private void LoadAppointments()
        {
            // SQL query to get all appointments with patient and doctor names
            // Uses INNER JOIN to combine data from multiple tables
            string query = @"SELECT a.AppointmentID, a.PatientID, a.DoctorID,
                            p.Name AS Patient, d.Name AS Doctor, 
                            a.AppointmentDate AS Date, a.AppointmentTime AS Time
                            FROM Appointments a
                            INNER JOIN Patients p ON a.PatientID = p.PatientID
                            INNER JOIN Doctors d ON a.DoctorID = d.DoctorID
                            ORDER BY a.AppointmentDate DESC, a.AppointmentTime DESC";
            
            // Execute query and bind results to DataGridView
            dgv.DataSource = DB.GetData(query);
            
            // Hide ID columns (we don't need to show them to user)
            if (dgv.Columns.Contains("AppointmentID"))
                dgv.Columns["AppointmentID"].Visible = false;
            if (dgv.Columns.Contains("PatientID"))
                dgv.Columns["PatientID"].Visible = false;
            if (dgv.Columns.Contains("DoctorID"))
                dgv.Columns["DoctorID"].Visible = false;
        }

        // Clear all selections
        private void ClearFields()
        {
            selectedAppointmentID = 0; // Reset selected ID
            cmbPatient.SelectedIndex = -1; // Deselect patient
            cmbDoctor.SelectedIndex = -1; // Deselect doctor
            dtpDate.Value = DateTime.Now; // Reset date to today
            dtpTime.Value = DateTime.Now; // Reset time to current time
        }
    }
}

