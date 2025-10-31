using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IronPdf;


namespace SimpleHMS
{
    /// <summary>
    /// BillingForm - This form is used to manage billing operations
    /// Allows users to generate, update, and delete bills, as well as manage bill items
    /// </summary>
    public partial class BillingForm : Form
    {
        // Email Bill button already declared in Form Variables region
        /// <summary>
        /// Generates a new bill number in the format BILL-YYYY-NNNN
        /// </summary>
        /// <returns>A new bill number string</returns>
        private string GenerateNewBillNumber()
        {
            try
            {
                // Format: BILL-YYYY-NNNN where YYYY is current year and NNNN is sequential number
                int currentYear = DateTime.Now.Year;
                int nextNumber = 1;

                // Get the highest bill number for the current year from the database
                string connectionString = DB.GetConnectionString();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT MAX(CAST(SUBSTRING(BillNumber, 11, 4) AS INT)) 
                                    FROM Bills 
                                    WHERE BillNumber LIKE @pattern";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@pattern", $"BILL-{currentYear}-%");
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        nextNumber = Convert.ToInt32(result) + 1;
                    }
                }

                // Format the bill number
                return $"BILL-{currentYear}-{nextNumber:D4}";
            }
            catch (Exception ex)
            {
                // If there's an error, use timestamp as fallback
                MessageBox.Show("Error generating bill number: " + ex.Message + "\nUsing timestamp instead.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return $"BILL-{DateTime.Now:yyyyMMdd-HHmmss}";
            }
        }

        /// <summary>
        /// Loads payment methods into the payment method combo box
        /// </summary>
        private void LoadPaymentMethods()
        {
            try
            {
                // Clear existing items
                cmbPaymentMethod.Items.Clear();

                // Add standard payment methods
                cmbPaymentMethod.Items.Add("Cash");
                cmbPaymentMethod.Items.Add("Credit Card");
                cmbPaymentMethod.Items.Add("Debit Card");
                cmbPaymentMethod.Items.Add("Insurance");
                cmbPaymentMethod.Items.Add("Bank Transfer");
                cmbPaymentMethod.Items.Add("Mobile Payment");
                cmbPaymentMethod.Items.Add("Check");
                cmbPaymentMethod.Items.Add("Online Payment");
                cmbPaymentMethod.Items.Add("Gift Card");
                cmbPaymentMethod.Items.Add("Other");

                // Get any additional payment methods from the database
                string connectionString = DB.GetConnectionString();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT DISTINCT PaymentMethod FROM Bills 
                                    WHERE PaymentMethod IS NOT NULL 
                                    AND PaymentMethod NOT IN ('Cash', 'Credit Card', 'Debit Card', 'Insurance', 'Bank Transfer', 
                                    'Mobile Payment', 'Check', 'Online Payment', 'Gift Card', 'Other')";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string paymentMethod = reader["PaymentMethod"].ToString();
                        if (!string.IsNullOrEmpty(paymentMethod) && !cmbPaymentMethod.Items.Contains(paymentMethod))
                        {
                            cmbPaymentMethod.Items.Add(paymentMethod);
                        }
                    }
                }

                // Set default payment method
                if (cmbPaymentMethod.Items.Count > 0)
                {
                    cmbPaymentMethod.SelectedIndex = 0; // Select "Cash" by default
                }

                // Add event handler for payment method change
                cmbPaymentMethod.SelectedIndexChanged += (sender, e) =>
                {
                    // Additional logic can be added here if needed
                    // For example, showing different fields based on payment method
                    if (cmbPaymentMethod.SelectedItem != null &&
                        (cmbPaymentMethod.SelectedItem.ToString() == "Credit Card" ||
                         cmbPaymentMethod.SelectedItem.ToString() == "Debit Card"))
                    {
                        // Could show card fields here if needed
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payment methods: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates the fee summary panel with calculated totals based on bill items
        /// </summary>
        private void UpdateFeeSummary()
        {
            try
            {
                // Initialize fee categories
                decimal medicineFee = 0;
                decimal testFee = 0;
                decimal consultationFee = 0;
                decimal otherFee = 0;
                decimal totalBeforeDiscount = 0;
                decimal discount = 0;
                decimal totalAfterDiscount = 0;

                // Calculate fees by category from bill items
                if (billItemsTable != null && billItemsTable.Rows.Count > 0)
                {
                    foreach (DataRow row in billItemsTable.Rows)
                    {
                        decimal itemTotal = Convert.ToDecimal(row["TotalPrice"]);
                        string itemType = row["ItemType"].ToString().ToLower();

                        // Categorize by item type
                        switch (itemType)
                        {
                            case "medicine":
                                medicineFee += itemTotal;
                                break;
                            case "test":
                                testFee += itemTotal;
                                break;
                            case "consultation":
                                consultationFee += itemTotal;
                                break;
                            default:
                                otherFee += itemTotal;
                                break;
                        }

                        totalBeforeDiscount += itemTotal;
                    }
                }

                // Get doctor's consultation fee if not already added
                if (consultationFee == 0 && cmbDoctor.SelectedIndex > -1)
                {
                    try
                    {
                        int doctorID = Convert.ToInt32(cmbDoctor.SelectedValue);
                        string query = $"SELECT Fee FROM Doctors WHERE DoctorID = {doctorID}";
                        object result = null;
                        using (SqlConnection conn = new SqlConnection(DB.GetConnectionString()))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                result = cmd.ExecuteScalar();
                            }
                        }
                        if (result != null && result != DBNull.Value)
                        {
                            consultationFee = Convert.ToDecimal(result);
                            totalBeforeDiscount += consultationFee;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Just log the error, don't show message box to avoid interrupting the UI flow
                        Console.WriteLine("Error getting doctor fee: " + ex.Message);
                    }
                }

                // Calculate discount
                if (!string.IsNullOrWhiteSpace(txtDiscount.Text) && decimal.TryParse(txtDiscount.Text, out discount))
                {
                    // Check if discount is percentage or fixed amount
                    if (this.Controls.Find("chkDiscountPercent", true).FirstOrDefault() is CheckBox chk && chk.Checked)
                    {
                        // Percentage discount (limit to 100%)
                        discount = Math.Min(discount, 100);
                        discount = totalBeforeDiscount * (discount / 100);
                    }
                    // else discount is already the fixed amount
                }

                // Calculate final total
                totalAfterDiscount = totalBeforeDiscount - discount;

                // Update UI with calculated values
                txtMedicineFee.Text = medicineFee.ToString("0.00");
                txtTestFee.Text = testFee.ToString("0.00");
                txtConsultationFee.Text = consultationFee.ToString("0.00");
                txtOtherFee.Text = otherFee.ToString("0.00");
                txtTotalAmount.Text = totalAfterDiscount.ToString("0.00");

                // Update payment status
                if (cmbPaymentStatus.SelectedItem != null && cmbPaymentStatus.SelectedItem.ToString() == "Paid")
                {
                    cmbPaymentStatus.SelectedItem = "Paid";
                    dtpPaymentDate.Value = DateTime.Now;
                    dtpPaymentDate.Enabled = true;
                    cmbPaymentMethod.Enabled = true;
                }
                else
                {
                    cmbPaymentStatus.SelectedItem = "Pending";
                    dtpPaymentDate.Enabled = false;
                    cmbPaymentMethod.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating fee summary: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Event handler for discount text changed
        /// </summary>
        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            UpdateFeeSummary();
        }

        /// <summary>
        /// Event handler for discount percentage checkbox changed
        /// </summary>
        private void chkDiscountPercent_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFeeSummary();
        }

        /// <summary>
        /// Event handler for paid checkbox changed
        /// </summary>
        private void chkPaid_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFeeSummary();
        }
        #region Form Variables

        /// <summary>
        /// Required designer variable
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Variables to store selected IDs
        /// </summary>
        private int selectedBillID = 0; // Stores the ID of selected bill (0 means no selection)
        
        /// <summary>
        /// Email Bill button
        /// </summary>
        private Button btnEmailBill;
        
        /// <summary>
        /// Handles the Email Bill button click event
        /// </summary>
        private async void BtnEmailBill_Click(object sender, EventArgs e)
        {
            if (selectedBillID == 0)
            {
                MessageBox.Show("Please select a bill to email.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Get patient email from database
                string patientEmail = GetPatientEmail();
                
                if (string.IsNullOrEmpty(patientEmail))
                {
                    MessageBox.Show("Patient does not have an email address on file.\n\nPlease update patient information to add email.", 
                        "Email Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm email address
                var confirmResult = MessageBox.Show(
                    $"Send bill to: {patientEmail}?\n\nPatient: {cmbPatient.Text}\nBill: {txtBillNumber.Text}", 
                    "Confirm Email", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirmResult != DialogResult.Yes)
                    return;

                // Show progress
                this.Cursor = Cursors.WaitCursor;
                btnEmailBill.Enabled = false;
                btnEmailBill.Text = "Sending...";

                // Ask if user wants to attach PDF
                var attachPdf = MessageBox.Show(
                    "Do you want to attach the bill as PDF?", 
                    "Attach PDF", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);

                // Generate HTML content
                string htmlContent = GenerateBillHtmlForEmail();
                
                // Generate PDF if requested
                byte[] pdfBytes = null;
                string pdfFileName = null;
                
                if (attachPdf == DialogResult.Yes)
                {
                    pdfBytes = GenerateBillPdfBytes();
                    pdfFileName = $"Bill_{txtBillNumber.Text}.pdf";
                }

                // Create email service with your credentials
                var emailService = new EmailService(
                    "smtp.gmail.com", 
                    587,
                    "your-hospital-email@gmail.com", 
                    "your-app-password-here",
                    "your-hospital-email@gmail.com", 
                    "Hospital Management System"
                );

                // Send email
                if (pdfBytes != null)
                {
                    // For now, just send regular email since attachment method isn't implemented
                    await emailService.SendEmailAsync(
                        patientEmail,
                        $"Bill {txtBillNumber.Text} - Hospital Management System",
                        htmlContent
                    );
                }
                else
                {
                    await emailService.SendEmailAsync(
                        patientEmail,
                        $"Bill {txtBillNumber.Text} - Hospital Management System",
                        htmlContent
                    );
                }
                
                bool success = true; // Assume success for now

                // Show result
                if (success)
                {
                    MessageBox.Show($"Bill successfully sent to {patientEmail}", 
                        "Email Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to send email. Please check your email settings.", 
                        "Email Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending email: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset UI
                this.Cursor = Cursors.Default;
                btnEmailBill.Enabled = true;
                btnEmailBill.Text = "Email Bill";
            }
        }
        
        /// <summary>
        /// Gets the patient email from the database
        /// </summary>
        private string GetPatientEmail()
        {
            string email = string.Empty;
            
            try
            {
                // Get the patient ID from the selected patient in the combo box
                if (cmbPatient.SelectedValue == null)
                    return string.Empty;
                
                int patientId = Convert.ToInt32(cmbPatient.SelectedValue);
                
                using (SqlConnection conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT Email FROM Patients WHERE PatientID = @PatientID";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientId);
                        
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            email = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving patient email: {ex.Message}", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return email;
        }
        
        /// <summary>
        /// Generates HTML content for the bill email
        /// </summary>
        private string GenerateBillHtmlForEmail()
        {
            // Simple HTML template for bill
            string htmlTemplate = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }
                    .header { background-color: #4a6da7; color: white; padding: 20px; text-align: center; }
                    .bill-info { margin: 20px 0; }
                    .bill-info table { width: 100%; border-collapse: collapse; }
                    .bill-info td { padding: 8px; }
                    .items-table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                    .items-table th { background-color: #4a6da7; color: white; padding: 10px; text-align: left; }
                    .items-table td { padding: 8px; border-bottom: 1px solid #ddd; }
                    .total-row { font-weight: bold; background-color: #f2f2f2; }
                    .footer { margin-top: 30px; text-align: center; font-size: 12px; color: #777; }
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Hospital Management System</h1>
                    <h2>Bill Invoice</h2>
                </div>
                
                <div class='bill-info'>
                    <table>
                        <tr>
                            <td><strong>Bill Number:</strong></td>
                            <td>{BillNumber}</td>
                            <td><strong>Bill Date:</strong></td>
                            <td>{BillDate}</td>
                        </tr>
                        <tr>
                            <td><strong>Patient:</strong></td>
                            <td>{PatientName}</td>
                            <td><strong>Doctor:</strong></td>
                            <td>{DoctorName}</td>
                        </tr>
                    </table>
                </div>
                
                <h3>Bill Items</h3>
                <table class='items-table'>
                    <tr>
                        <th>Item</th>
                        <th>Amount</th>
                    </tr>
                    {BillItems}
                    <tr class='total-row'>
                        <td>Total Amount:</td>
                        <td>{TotalAmount}</td>
                    </tr>
                </table>
                
                <div class='footer'>
                    <p>This is an automatically generated email. Please do not reply.</p>
                    <p>For any questions regarding this bill, please contact our billing department.</p>
                </div>
            </body>
            </html>";

            // Replace placeholders with actual values
            htmlTemplate = htmlTemplate
                .Replace("{BillNumber}", txtBillNumber.Text)
                .Replace("{BillDate}", dtpBillDate.Value.ToString("MMM dd, yyyy"))
                .Replace("{PatientName}", cmbPatient.Text)
                .Replace("{DoctorName}", cmbDoctor.Text)
                .Replace("{TotalAmount}", txtTotalAmount.Text);

            // Generate bill items rows
            StringBuilder billItemsHtml = new StringBuilder();
            
            // Add consultation fee if present
            if (!string.IsNullOrEmpty(txtConsultationFee.Text) && decimal.Parse(txtConsultationFee.Text) > 0)
            {
                billItemsHtml.Append("<tr><td>Consultation Fee</td><td>").Append(txtConsultationFee.Text).Append("</td></tr>");
            }
            
            // Add medicine fee if present
            if (!string.IsNullOrEmpty(txtMedicineFee.Text) && decimal.Parse(txtMedicineFee.Text) > 0)
            {
                billItemsHtml.Append("<tr><td>Medicine Fee</td><td>").Append(txtMedicineFee.Text).Append("</td></tr>");
            }
            
            // Add test fee if present
            if (!string.IsNullOrEmpty(txtTestFee.Text) && decimal.Parse(txtTestFee.Text) > 0)
            {
                billItemsHtml.Append("<tr><td>Test Fee</td><td>").Append(txtTestFee.Text).Append("</td></tr>");
            }
            
            // Add other fee if present
            if (!string.IsNullOrEmpty(txtOtherFee.Text) && decimal.Parse(txtOtherFee.Text) > 0)
            {
                billItemsHtml.Append("<tr><td>Other Fee</td><td>").Append(txtOtherFee.Text).Append("</td></tr>");
            }
            
            // Add discount if present
            if (!string.IsNullOrEmpty(txtDiscount.Text) && decimal.Parse(txtDiscount.Text) > 0)
            {
                billItemsHtml.Append("<tr><td>Discount</td><td>-").Append(txtDiscount.Text).Append("</td></tr>");
            }
            
            // Replace bill items placeholder
            htmlTemplate = htmlTemplate.Replace("{BillItems}", billItemsHtml.ToString());
            
            return htmlTemplate;
        }
        
        /// <summary>
        /// Generates PDF bytes for the bill
        /// </summary>
        private byte[] GenerateBillPdfBytes()
        {
            // This is a placeholder method
            // In a real implementation, you would use a PDF library to generate the PDF
            // For now, we'll return an empty byte array
            return new byte[0];
        }

        /// <summary>
        /// DataTable to store bill items temporarily
        /// </summary>
        private DataTable billItemsTable;

        #endregion

        #region Form Controls

        // Labels for form fields
        private Label lblBillNumber;
        
        // Email Bill button
        // btnEmailBill is already declared at class level
        private Label lblBillDate;
        private Label lblPatient;
        private Label lblDoctor;
        private Label lblAppointment;
        private Label lblConsultationFee;
        private Label lblMedicineFee;
        private Label lblTestFee;
        private Label lblOtherFee;
        private Label lblDiscount;
        private Label lblTotalAmount;
        private Label lblPaymentStatus;
        private Label lblPaymentMethod;
        private Label lblPaymentDate;
        private Label lblNotes;
        private Label lblSearch;
        private Label lblItemName;
        private Label lblItemType;
        private Label lblQuantity;
        private Label lblUnitPrice;
        private Label lblTotalPrice;

        // ComboBoxes for selections
        private ComboBox cmbPatient;
        private ComboBox cmbDoctor;
        private ComboBox cmbAppointment;
        private ComboBox cmbPaymentStatus;
        private ComboBox cmbPaymentMethod;
        private ComboBox cmbItemType;

        // TextBoxes for bill information
        private TextBox txtBillNumber;
        private TextBox txtConsultationFee;
        private TextBox txtMedicineFee;
        private TextBox txtTestFee;
        private TextBox txtOtherFee;
        private TextBox txtDiscount;
        private TextBox txtTotalAmount;
        private TextBox txtNotes;
        private TextBox txtItemName;
        private TextBox txtQuantity;
        private TextBox txtUnitPrice;
        private TextBox txtTotalPrice;

        // DateTimePickers for dates
        private DateTimePicker dtpBillDate;
        private DateTimePicker dtpPaymentDate;

        // DataGridViews for bills and bill items
        private DataGridView dgvBills;
        private DataGridView dgvBillItems;

        // Buttons for actions
        private Button btnDelete;
        private Button btnAddItem;
        private Button btnRemoveItem;
        private Button btnNew;
        private Button btnSave;
        private Button btnPrintPDF;
        private Button btnUpdateItem;
        private Button btnClear;

        // GroupBoxes for organizing controls
        private GroupBox grpBillInfo;
        private GroupBox grpFeeBreakdown;
        private GroupBox grpBillItems;
        private GroupBox grpPaymentInfo;
        private Button button1;

        // ToolTip shows hints when you hover over controls
        private ToolTip tooltip;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Constructor - runs when form is created
        /// </summary>
        public BillingForm()
        {
            InitializeComponent(); // Create all the controls
            InitializeBillItemsTable(); // Initialize temporary table for bill items
            LoadPatients(); // Load patients into dropdown
            LoadDoctors(); // Load doctors into dropdown
            LoadBills(); // Load existing bills into grid
            LoadPaymentMethods(); // Load payment methods into dropdown

            // Initialize payment status options
            cmbPaymentStatus.Items.Clear();
            cmbPaymentStatus.Items.AddRange(new string[] { "Pending", "Partial", "Paid", "Cancelled", "Refunded" });
            cmbPaymentStatus.SelectedIndex = 0; // Default to "Pending"

            // Add event handler for payment status change
            cmbPaymentStatus.SelectedIndexChanged += (sender, e) =>
            {
                // Enable/disable payment date and method based on status
                bool isPaid = cmbPaymentStatus.SelectedItem.ToString() == "Paid" ||
                              cmbPaymentStatus.SelectedItem.ToString() == "Partial";
                dtpPaymentDate.Enabled = isPaid;
                cmbPaymentMethod.Enabled = isPaid;

                // Set payment date to today if paid
                if (isPaid && dtpPaymentDate.Value.Date != DateTime.Now.Date)
                {
                    dtpPaymentDate.Value = DateTime.Now;
                }
            };

            // Initialize item types
            cmbItemType.Items.Clear();
            cmbItemType.Items.AddRange(new string[] { "Consultation", "Medicine", "Test", "Procedure", "Equipment", "Room", "Service", "Other" });
            cmbItemType.SelectedIndex = 0; // Default to "Consultation"

            // Add event handler for item type change
            cmbItemType.SelectedIndexChanged += (sender, e) =>
            {
                // Update unit price based on selected type if needed
                if (cmbItemType.SelectedItem != null)
                {
                    string selectedType = cmbItemType.SelectedItem.ToString();
                    if (selectedType == "Consultation" && cmbDoctor.SelectedIndex > -1)
                    {
                        // Try to get doctor's consultation fee
                        try
                        {
                            int doctorID = Convert.ToInt32(cmbDoctor.SelectedValue);
                            string query = $"SELECT Fee FROM Doctors WHERE DoctorID = {doctorID}";
                            using (SqlConnection conn = new SqlConnection(DB.GetConnectionString()))
                            {
                                conn.Open();
                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                {
                                    var result = cmd.ExecuteScalar();
                                    if (result != null && result != DBNull.Value)
                                    {
                                        txtUnitPrice.Text = Convert.ToDecimal(result).ToString("0.00");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error getting doctor fee: " + ex.Message);
                        }
                    }
                }
            };

            // Set default values
            dtpBillDate.Value = DateTime.Now;
            dtpPaymentDate.Value = DateTime.Now;

            // Initialize Clear button
            btnClear = new Button();
            btnClear.Text = "Clear";
            btnClear.Location = new Point(btnNew.Location.X + btnNew.Width + 10, btnNew.Location.Y);
            btnClear.Size = btnNew.Size;
            btnClear.Click += button1_Click_1;
            this.Controls.Add(btnClear);
        }

        /// <summary>
        /// Initialize temporary table for bill items
        /// </summary>
        private void InitializeBillItemsTable()
        {
            billItemsTable = new DataTable();
            billItemsTable.Columns.Add("ItemID", typeof(int));
            billItemsTable.Columns.Add("ItemName", typeof(string));
            billItemsTable.Columns.Add("ItemType", typeof(string));
            billItemsTable.Columns.Add("Quantity", typeof(int));
            billItemsTable.Columns.Add("UnitPrice", typeof(decimal));
            billItemsTable.Columns.Add("TotalPrice", typeof(decimal));

            // Set default value for ItemID column (will be used for new items only)
            billItemsTable.Columns["ItemID"].DefaultValue = 0;
        }

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            tooltip = new ToolTip(components);
            lblBillNumber = new Label();
            lblBillDate = new Label();
            lblPatient = new Label();
            lblDoctor = new Label();
            lblAppointment = new Label();
            lblConsultationFee = new Label();
            lblMedicineFee = new Label();
            lblTestFee = new Label();
            lblOtherFee = new Label();
            lblDiscount = new Label();
            lblTotalAmount = new Label();
            lblPaymentStatus = new Label();
            lblPaymentMethod = new Label();
            lblPaymentDate = new Label();
            lblNotes = new Label();
            lblSearch = new Label();
            lblItemName = new Label();
            lblItemType = new Label();
            lblQuantity = new Label();
            lblUnitPrice = new Label();
            lblTotalPrice = new Label();
            txtBillNumber = new TextBox();
            txtConsultationFee = new TextBox();
            txtMedicineFee = new TextBox();
            txtTestFee = new TextBox();
            txtOtherFee = new TextBox();
            txtDiscount = new TextBox();
            txtTotalAmount = new TextBox();
            txtNotes = new TextBox();
            txtItemName = new TextBox();
            txtQuantity = new TextBox();
            txtUnitPrice = new TextBox();
            txtTotalPrice = new TextBox();
            cmbPatient = new ComboBox();
            cmbDoctor = new ComboBox();
            cmbAppointment = new ComboBox();
            cmbPaymentStatus = new ComboBox();
            cmbPaymentMethod = new ComboBox();
            cmbItemType = new ComboBox();
            dtpBillDate = new DateTimePicker();
            dtpPaymentDate = new DateTimePicker();
            dgvBills = new DataGridView();
            dgvBillItems = new DataGridView();
            btnNew = new Button();
            btnSave = new Button();
            btnDelete = new Button();
            btnPrintPDF = new Button();
            btnEmailBill = new Button();
            
            // Configure Email Bill button
            btnEmailBill.Location = new Point(450, 729);
            btnEmailBill.Name = "btnEmailBill";
            btnEmailBill.Size = new Size(90, 23);
            btnEmailBill.TabIndex = 12;
            btnEmailBill.Text = "Email Bill";
            btnEmailBill.UseVisualStyleBackColor = true;
            btnEmailBill.Click += BtnEmailBill_Click;
            btnAddItem = new Button();
            btnUpdateItem = new Button();
            btnRemoveItem = new Button();
            btnClear = new Button();
            grpBillInfo = new GroupBox();
            grpFeeBreakdown = new GroupBox();
            grpPaymentInfo = new GroupBox();
            grpBillItems = new GroupBox();
            button1 = new Button();
            ((ISupportInitialize)dgvBills).BeginInit();
            ((ISupportInitialize)dgvBillItems).BeginInit();
            grpBillInfo.SuspendLayout();
            grpFeeBreakdown.SuspendLayout();
            grpPaymentInfo.SuspendLayout();
            grpBillItems.SuspendLayout();
            SuspendLayout();
            // 
            // tooltip
            // 
            tooltip.AutoPopDelay = 5000;
            tooltip.InitialDelay = 500;
            tooltip.ReshowDelay = 100;
            // 
            // lblBillNumber
            // 
            lblBillNumber.Location = new Point(19, 30);
            lblBillNumber.Name = "lblBillNumber";
            lblBillNumber.Size = new Size(100, 23);
            lblBillNumber.TabIndex = 0;
            lblBillNumber.Text = "Bill #";
            lblBillNumber.Click += lblBillNumber_Click;
            // 
            // lblBillDate
            // 
            lblBillDate.Location = new Point(20, 60);
            lblBillDate.Name = "lblBillDate";
            lblBillDate.Size = new Size(100, 23);
            lblBillDate.TabIndex = 2;
            lblBillDate.Text = "Bill Date";
            lblBillDate.Click += lblBillDate_Click;
            // 
            // lblPatient
            // 
            lblPatient.Location = new Point(20, 90);
            lblPatient.Name = "lblPatient";
            lblPatient.Size = new Size(100, 23);
            lblPatient.TabIndex = 4;
            lblPatient.Text = "Patient";
            // 
            // lblDoctor
            // 
            lblDoctor.Location = new Point(20, 120);
            lblDoctor.Name = "lblDoctor";
            lblDoctor.Size = new Size(100, 23);
            lblDoctor.TabIndex = 6;
            lblDoctor.Text = "Doctor";
            // 
            // lblAppointment
            // 
            lblAppointment.Location = new Point(20, 150);
            lblAppointment.Name = "lblAppointment";
            lblAppointment.Size = new Size(100, 23);
            lblAppointment.TabIndex = 8;
            lblAppointment.Text = "Appointment";
            // 
            // lblConsultationFee
            // 
            lblConsultationFee.Location = new Point(20, 30);
            lblConsultationFee.Name = "lblConsultationFee";
            lblConsultationFee.Size = new Size(100, 23);
            lblConsultationFee.TabIndex = 0;
            lblConsultationFee.Text = "Consultation Fee";
            // 
            // lblMedicineFee
            // 
            lblMedicineFee.Location = new Point(20, 60);
            lblMedicineFee.Name = "lblMedicineFee";
            lblMedicineFee.Size = new Size(100, 23);
            lblMedicineFee.TabIndex = 2;
            lblMedicineFee.Text = "Medicine Fee";
            // 
            // lblTestFee
            // 
            lblTestFee.Location = new Point(20, 90);
            lblTestFee.Name = "lblTestFee";
            lblTestFee.Size = new Size(100, 23);
            lblTestFee.TabIndex = 4;
            lblTestFee.Text = "Test Fee";
            // 
            // lblOtherFee
            // 
            lblOtherFee.Location = new Point(20, 120);
            lblOtherFee.Name = "lblOtherFee";
            lblOtherFee.Size = new Size(100, 23);
            lblOtherFee.TabIndex = 6;
            lblOtherFee.Text = "Other Fee";
            // 
            // lblDiscount
            // 
            lblDiscount.Location = new Point(20, 150);
            lblDiscount.Name = "lblDiscount";
            lblDiscount.Size = new Size(100, 23);
            lblDiscount.TabIndex = 8;
            lblDiscount.Text = "Discount";
            // 
            // lblTotalAmount
            // 
            lblTotalAmount.Location = new Point(260, 151);
            lblTotalAmount.Name = "lblTotalAmount";
            lblTotalAmount.Size = new Size(100, 23);
            lblTotalAmount.TabIndex = 10;
            lblTotalAmount.Text = "Total Amount";
            // 
            // lblPaymentStatus
            // 
            lblPaymentStatus.Location = new Point(20, 180);
            lblPaymentStatus.Name = "lblPaymentStatus";
            lblPaymentStatus.Size = new Size(100, 23);
            lblPaymentStatus.TabIndex = 10;
            lblPaymentStatus.Text = "Payment Status";
            // 
            // lblPaymentMethod
            // 
            lblPaymentMethod.Location = new Point(14, 32);
            lblPaymentMethod.Name = "lblPaymentMethod";
            lblPaymentMethod.Size = new Size(100, 23);
            lblPaymentMethod.TabIndex = 0;
            lblPaymentMethod.Text = "Payment Method";
            // 
            // lblPaymentDate
            // 
            lblPaymentDate.Location = new Point(264, 33);
            lblPaymentDate.Name = "lblPaymentDate";
            lblPaymentDate.Size = new Size(100, 23);
            lblPaymentDate.TabIndex = 2;
            lblPaymentDate.Text = "Payment Date";
            // 
            // lblNotes
            // 
            lblNotes.Location = new Point(563, 38);
            lblNotes.Name = "lblNotes";
            lblNotes.Size = new Size(100, 23);
            lblNotes.TabIndex = 4;
            lblNotes.Text = "Notes";
            // 
            // lblSearch
            // 
            lblSearch.Location = new Point(12, 90);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(100, 23);
            lblSearch.TabIndex = 4;
            // 
            // lblItemName
            // 
            lblItemName.Location = new Point(28, 33);
            lblItemName.Name = "lblItemName";
            lblItemName.Size = new Size(100, 23);
            lblItemName.TabIndex = 0;
            lblItemName.Text = "Item Name";
            lblItemName.Click += lblItemName_Click;
            // 
            // lblItemType
            // 
            lblItemType.ForeColor = SystemColors.ActiveCaptionText;
            lblItemType.Location = new Point(278, 30);
            lblItemType.Name = "lblItemType";
            lblItemType.Size = new Size(100, 23);
            lblItemType.TabIndex = 2;
            lblItemType.Text = "Type";
            // 
            // lblQuantity
            // 
            lblQuantity.Location = new Point(26, 61);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Size = new Size(100, 23);
            lblQuantity.TabIndex = 4;
            lblQuantity.Text = "Quantity";
            // 
            // lblUnitPrice
            // 
            lblUnitPrice.Location = new Point(484, 24);
            lblUnitPrice.Name = "lblUnitPrice";
            lblUnitPrice.Size = new Size(100, 23);
            lblUnitPrice.TabIndex = 6;
            lblUnitPrice.Text = "Unit Price";
            // 
            // lblTotalPrice
            // 
            lblTotalPrice.Location = new Point(481, 63);
            lblTotalPrice.Name = "lblTotalPrice";
            lblTotalPrice.Size = new Size(100, 23);
            lblTotalPrice.TabIndex = 8;
            lblTotalPrice.Text = "Total Price";
            // 
            // txtBillNumber
            // 
            txtBillNumber.Location = new Point(120, 30);
            txtBillNumber.Name = "txtBillNumber";
            txtBillNumber.Size = new Size(100, 23);
            txtBillNumber.TabIndex = 1;
            // 
            // txtConsultationFee
            // 
            txtConsultationFee.Location = new Point(150, 30);
            txtConsultationFee.Name = "txtConsultationFee";
            txtConsultationFee.Size = new Size(100, 23);
            txtConsultationFee.TabIndex = 1;
            txtConsultationFee.TextChanged += Fee_TextChanged;
            // 
            // txtMedicineFee
            // 
            txtMedicineFee.Location = new Point(150, 60);
            txtMedicineFee.Name = "txtMedicineFee";
            txtMedicineFee.Size = new Size(100, 23);
            txtMedicineFee.TabIndex = 3;
            txtMedicineFee.TextChanged += Fee_TextChanged;
            // 
            // txtTestFee
            // 
            txtTestFee.Location = new Point(150, 90);
            txtTestFee.Name = "txtTestFee";
            txtTestFee.Size = new Size(100, 23);
            txtTestFee.TabIndex = 5;
            txtTestFee.TextChanged += Fee_TextChanged;
            // 
            // txtOtherFee
            // 
            txtOtherFee.Location = new Point(150, 120);
            txtOtherFee.Name = "txtOtherFee";
            txtOtherFee.Size = new Size(100, 23);
            txtOtherFee.TabIndex = 7;
            txtOtherFee.TextChanged += Fee_TextChanged;
            // 
            // txtDiscount
            // 
            txtDiscount.Location = new Point(150, 150);
            txtDiscount.Name = "txtDiscount";
            txtDiscount.Size = new Size(100, 23);
            txtDiscount.TabIndex = 9;
            txtDiscount.TextChanged += Fee_TextChanged;
            // 
            // txtTotalAmount
            // 
            txtTotalAmount.Location = new Point(350, 150);
            txtTotalAmount.Name = "txtTotalAmount";
            txtTotalAmount.ReadOnly = true;
            txtTotalAmount.Size = new Size(100, 23);
            txtTotalAmount.TabIndex = 11;
            // 
            // txtNotes
            // 
            txtNotes.Location = new Point(616, 17);
            txtNotes.Multiline = true;
            txtNotes.Name = "txtNotes";
            txtNotes.Size = new Size(234, 67);
            txtNotes.TabIndex = 5;
            // 
            // txtItemName
            // 
            txtItemName.Location = new Point(133, 30);
            txtItemName.Name = "txtItemName";
            txtItemName.Size = new Size(100, 23);
            txtItemName.TabIndex = 1;
            // 
            // txtQuantity
            // 
            txtQuantity.Location = new Point(133, 60);
            txtQuantity.Name = "txtQuantity";
            txtQuantity.Size = new Size(100, 23);
            txtQuantity.TabIndex = 5;
            txtQuantity.TextChanged += TxtQuantity_TextChanged;
            // 
            // txtUnitPrice
            // 
            txtUnitPrice.Location = new Point(568, 22);
            txtUnitPrice.Name = "txtUnitPrice";
            txtUnitPrice.Size = new Size(100, 23);
            txtUnitPrice.TabIndex = 7;
            txtUnitPrice.TextChanged += TxtUnitPrice_TextChanged;
            // 
            // txtTotalPrice
            // 
            txtTotalPrice.Location = new Point(568, 60);
            txtTotalPrice.Name = "txtTotalPrice";
            txtTotalPrice.ReadOnly = true;
            txtTotalPrice.Size = new Size(100, 23);
            txtTotalPrice.TabIndex = 9;
            // 
            // cmbPatient
            // 
            cmbPatient.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPatient.Location = new Point(120, 90);
            cmbPatient.Name = "cmbPatient";
            cmbPatient.Size = new Size(121, 23);
            cmbPatient.TabIndex = 5;
            cmbPatient.SelectedIndexChanged += CmbPatient_SelectedIndexChanged;
            // 
            // cmbDoctor
            // 
            cmbDoctor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDoctor.Location = new Point(120, 120);
            cmbDoctor.Name = "cmbDoctor";
            cmbDoctor.Size = new Size(121, 23);
            cmbDoctor.TabIndex = 7;
            cmbDoctor.SelectedIndexChanged += CmbDoctor_SelectedIndexChanged;
            // 
            // cmbAppointment
            // 
            cmbAppointment.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAppointment.Location = new Point(120, 150);
            cmbAppointment.Name = "cmbAppointment";
            cmbAppointment.Size = new Size(121, 23);
            cmbAppointment.TabIndex = 9;
            cmbAppointment.SelectedIndexChanged += CmbAppointment_SelectedIndexChanged;
            // 
            // cmbPaymentStatus
            // 
            cmbPaymentStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentStatus.Location = new Point(120, 180);
            cmbPaymentStatus.Name = "cmbPaymentStatus";
            cmbPaymentStatus.Size = new Size(121, 23);
            cmbPaymentStatus.TabIndex = 11;
            cmbPaymentStatus.SelectedIndexChanged += CmbPaymentStatus_SelectedIndexChanged;
            // 
            // cmbPaymentMethod
            // 
            cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentMethod.Location = new Point(120, 30);
            cmbPaymentMethod.Name = "cmbPaymentMethod";
            cmbPaymentMethod.Size = new Size(121, 23);
            cmbPaymentMethod.TabIndex = 1;
            // 
            // cmbItemType
            // 
            cmbItemType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbItemType.Location = new Point(321, 27);
            cmbItemType.Name = "cmbItemType";
            cmbItemType.Size = new Size(121, 23);
            cmbItemType.TabIndex = 3;
            // 
            // dtpBillDate
            // 
            dtpBillDate.Format = DateTimePickerFormat.Short;
            dtpBillDate.Location = new Point(120, 60);
            dtpBillDate.Name = "dtpBillDate";
            dtpBillDate.Size = new Size(200, 23);
            dtpBillDate.TabIndex = 3;
            // 
            // dtpPaymentDate
            // 
            dtpPaymentDate.Format = DateTimePickerFormat.Short;
            dtpPaymentDate.Location = new Point(350, 30);
            dtpPaymentDate.Name = "dtpPaymentDate";
            dtpPaymentDate.Size = new Size(200, 23);
            dtpPaymentDate.TabIndex = 3;
            // 
            // dgvBills
            // 
            dgvBills.AllowUserToAddRows = false;
            dgvBills.AllowUserToDeleteRows = false;
            dgvBills.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBills.Location = new Point(10, 339);
            dgvBills.Name = "dgvBills";
            dgvBills.ReadOnly = true;
            dgvBills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBills.Size = new Size(870, 150);
            dgvBills.TabIndex = 7;
            dgvBills.CellClick += DgvBills_CellClick;
            // 
            // dgvBillItems
            // 
            dgvBillItems.AllowUserToAddRows = false;
            dgvBillItems.AllowUserToDeleteRows = false;
            dgvBillItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBillItems.Location = new Point(20, 117);
            dgvBillItems.Name = "dgvBillItems";
            dgvBillItems.ReadOnly = true;
            dgvBillItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBillItems.Size = new Size(830, 100);
            dgvBillItems.TabIndex = 10;
            dgvBillItems.CellClick += DgvBillItems_CellClick;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(30, 729);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 23);
            btnNew.TabIndex = 8;
            btnNew.Text = "New";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += BtnNew_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(135, 729);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 9;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(342, 729);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 10;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += BtnDelete_Click;
            // 
            // btnPrintPDF
            // 
            btnPrintPDF.Location = new Point(239, 729);
            btnPrintPDF.Name = "btnPrintPDF";
            btnPrintPDF.Size = new Size(75, 23);
            btnPrintPDF.TabIndex = 11;
            btnPrintPDF.Text = "Print PDF";
            btnPrintPDF.UseVisualStyleBackColor = true;
            btnPrintPDF.Click += BtnPrintPDF_Click;
            // 
            // btnAddItem
            // 
            btnAddItem.Location = new Point(650, 30);
            btnAddItem.Name = "btnAddItem";
            btnAddItem.Size = new Size(75, 23);
            btnAddItem.TabIndex = 11;
            btnAddItem.Text = "Add Item";
            btnAddItem.UseVisualStyleBackColor = true;
            btnAddItem.Click += BtnAddItem_Click;
            // 
            // btnUpdateItem
            // 
            btnUpdateItem.Location = new Point(650, 60);
            btnUpdateItem.Name = "btnUpdateItem";
            btnUpdateItem.Size = new Size(75, 23);
            btnUpdateItem.TabIndex = 12;
            btnUpdateItem.Click += BtnUpdateItem_Click;
            // 
            // btnRemoveItem
            // 
            btnRemoveItem.Location = new Point(750, 60);
            btnRemoveItem.Name = "btnRemoveItem";
            btnRemoveItem.Size = new Size(75, 23);
            btnRemoveItem.TabIndex = 13;
            btnRemoveItem.Click += BtnRemoveItem_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(0, 0);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 0;
            // 
            // grpBillInfo
            // 
            grpBillInfo.Controls.Add(txtBillNumber);
            grpBillInfo.Controls.Add(lblBillDate);
            grpBillInfo.Controls.Add(dtpBillDate);
            grpBillInfo.Controls.Add(lblPatient);
            grpBillInfo.Controls.Add(cmbPatient);
            grpBillInfo.Controls.Add(lblDoctor);
            grpBillInfo.Controls.Add(cmbDoctor);
            grpBillInfo.Controls.Add(lblAppointment);
            grpBillInfo.Controls.Add(cmbAppointment);
            grpBillInfo.Controls.Add(lblPaymentStatus);
            grpBillInfo.Controls.Add(cmbPaymentStatus);
            grpBillInfo.Controls.Add(lblBillNumber);
            grpBillInfo.Location = new Point(18, 9);
            grpBillInfo.Name = "grpBillInfo";
            grpBillInfo.Size = new Size(342, 227);
            grpBillInfo.TabIndex = 0;
            grpBillInfo.TabStop = false;
            grpBillInfo.Text = "Bill Information";
            // 
            // grpFeeBreakdown
            // 
            grpFeeBreakdown.Controls.Add(lblConsultationFee);
            grpFeeBreakdown.Controls.Add(txtConsultationFee);
            grpFeeBreakdown.Controls.Add(lblMedicineFee);
            grpFeeBreakdown.Controls.Add(txtMedicineFee);
            grpFeeBreakdown.Controls.Add(lblTestFee);
            grpFeeBreakdown.Controls.Add(txtTestFee);
            grpFeeBreakdown.Controls.Add(lblOtherFee);
            grpFeeBreakdown.Controls.Add(txtOtherFee);
            grpFeeBreakdown.Controls.Add(lblDiscount);
            grpFeeBreakdown.Controls.Add(txtDiscount);
            grpFeeBreakdown.Controls.Add(txtTotalAmount);
            grpFeeBreakdown.Controls.Add(lblTotalAmount);
            grpFeeBreakdown.Location = new Point(370, 10);
            grpFeeBreakdown.Name = "grpFeeBreakdown";
            grpFeeBreakdown.Size = new Size(510, 226);
            grpFeeBreakdown.TabIndex = 1;
            grpFeeBreakdown.TabStop = false;
            grpFeeBreakdown.Text = "Fee Breakdown";
            grpFeeBreakdown.Enter += grpFeeBreakdown_Enter;
            // 
            // grpPaymentInfo
            // 
            grpPaymentInfo.Controls.Add(cmbPaymentMethod);
            grpPaymentInfo.Controls.Add(dtpPaymentDate);
            grpPaymentInfo.Controls.Add(txtNotes);
            grpPaymentInfo.Controls.Add(lblNotes);
            grpPaymentInfo.Controls.Add(lblPaymentDate);
            grpPaymentInfo.Controls.Add(lblPaymentMethod);
            grpPaymentInfo.Location = new Point(10, 242);
            grpPaymentInfo.Name = "grpPaymentInfo";
            grpPaymentInfo.Size = new Size(870, 90);
            grpPaymentInfo.TabIndex = 2;
            grpPaymentInfo.TabStop = false;
            grpPaymentInfo.Text = "Payment Information";
            // 
            // grpBillItems
            // 
            grpBillItems.Controls.Add(txtItemName);
            grpBillItems.Controls.Add(cmbItemType);
            grpBillItems.Controls.Add(txtQuantity);
            grpBillItems.Controls.Add(txtUnitPrice);
            grpBillItems.Controls.Add(txtTotalPrice);
            grpBillItems.Controls.Add(dgvBillItems);
            grpBillItems.Controls.Add(lblTotalPrice);
            grpBillItems.Controls.Add(lblUnitPrice);
            grpBillItems.Controls.Add(lblQuantity);
            grpBillItems.Controls.Add(lblItemName);
            grpBillItems.Controls.Add(lblItemType);
            grpBillItems.Location = new Point(10, 492);
            grpBillItems.Name = "grpBillItems";
            grpBillItems.Size = new Size(870, 231);
            grpBillItems.TabIndex = 3;
            grpBillItems.TabStop = false;
            grpBillItems.Text = "Bill Items";
            grpBillItems.Enter += grpBillItems_Enter;
            // 
            // button1
            // 
            button1.Location = new Point(438, 729);
            button1.Name = "button1";
            button1.Size = new Size(87, 23);
            button1.TabIndex = 14;
            button1.Text = "Clear";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // BillingForm
            // 
            ClientSize = new Size(897, 759);
            Controls.Add(button1);
            Controls.Add(grpBillInfo);
            Controls.Add(grpFeeBreakdown);
            Controls.Add(grpPaymentInfo);
            Controls.Add(lblSearch);
            Controls.Add(btnNew);
            Controls.Add(btnSave);
            Controls.Add(btnDelete);
            Controls.Add(btnAddItem);
            Controls.Add(btnUpdateItem);
            Controls.Add(btnPrintPDF);
            Controls.Add(btnRemoveItem);
            Controls.Add(grpBillItems);
            Controls.Add(dgvBills);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "BillingForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Billing Management";
            ((ISupportInitialize)dgvBills).EndInit();
            ((ISupportInitialize)dgvBillItems).EndInit();
            grpBillInfo.ResumeLayout(false);
            grpBillInfo.PerformLayout();
            grpFeeBreakdown.ResumeLayout(false);
            grpFeeBreakdown.PerformLayout();
            grpPaymentInfo.ResumeLayout(false);
            grpPaymentInfo.PerformLayout();
            grpBillItems.ResumeLayout(false);
            grpBillItems.PerformLayout();
            ResumeLayout(false);
        }

        // Event handlers
        private void CmbPatient_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When patient changes, reload appointments
            LoadAppointments();
        }

        private void CmbDoctor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When doctor changes, reload appointments
            LoadAppointments();
        }

        private void CmbAppointment_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When appointment changes, load appointment details if needed
            if (cmbAppointment.SelectedValue != null)
            {
                try
                {
                    int appointmentId = Convert.ToInt32(cmbAppointment.SelectedValue);
                    // You could load appointment details here if needed
                }
                catch (Exception ex)
                {
                    // Log the error but don't show message to avoid disrupting the user experience
                    Console.WriteLine("Error converting appointment ID: " + ex.Message);
                }
            }
        }

        private void CmbPaymentStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable/disable payment method and date based on status
            bool isPaid = cmbPaymentStatus.Text == "Paid" || cmbPaymentStatus.Text == "Partial";
            cmbPaymentMethod.Enabled = isPaid;
            dtpPaymentDate.Enabled = isPaid;
        }

        private void Fee_TextChanged(object sender, EventArgs e)
        {
            // Calculate total amount when any fee field changes
            CalculateTotalAmount();
        }

        private void CalculateTotalAmount()
        {
            try
            {
                // Parse fee values (default to 0 if empty or invalid)
                decimal consultationFee = decimal.TryParse(txtConsultationFee.Text, out decimal cf) ? cf : 0;
                decimal medicineFee = decimal.TryParse(txtMedicineFee.Text, out decimal mf) ? mf : 0;
                decimal testFee = decimal.TryParse(txtTestFee.Text, out decimal tf) ? tf : 0;
                decimal otherFee = decimal.TryParse(txtOtherFee.Text, out decimal of) ? of : 0;
                decimal discount = decimal.TryParse(txtDiscount.Text, out decimal d) ? d : 0;

                // Calculate total
                decimal total = consultationFee + medicineFee + testFee + otherFee - discount;

                // Update total amount field
                txtTotalAmount.Text = total.ToString("F2");
            }
            catch (Exception ex)
            {
                // Silently handle calculation errors
                txtTotalAmount.Text = "0.00";
            }
        }

        private void TxtQuantity_TextChanged(object sender, EventArgs e)
        {
            // Recalculate item total price
            CalculateItemTotalPrice();
        }

        private void TxtUnitPrice_TextChanged(object sender, EventArgs e)
        {
            // Recalculate item total price
            CalculateItemTotalPrice();
        }

        private void CalculateItemTotalPrice()
        {
            try
            {
                // Parse quantity and unit price
                int quantity = int.TryParse(txtQuantity.Text, out int q) ? q : 0;
                decimal unitPrice = decimal.TryParse(txtUnitPrice.Text, out decimal up) ? up : 0;

                // Calculate and display total price
                decimal totalPrice = quantity * unitPrice;
                txtTotalPrice.Text = totalPrice.ToString("F2");
            }
            catch (Exception ex)
            {
                txtTotalPrice.Text = "0.00";
            }
        }

        private void DgvBills_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore header clicks
            if (e.RowIndex < 0) return;

            try
            {
                // Get selected bill ID
                if (dgvBills.CurrentRow != null)
                {
                    // Get the BillID from the first column
                    int billId = Convert.ToInt32(dgvBills.CurrentRow.Cells[0].Value);
                    selectedBillID = billId;

                    // Clear existing bill items
                    billItemsTable.Clear();

                    // Load bill details
                    LoadBillDetails(billId);

                    // Load bill items
                    LoadBillItems(billId);

                    // Make sure all panels are visible
                    grpBillInfo.Visible = true;
                    grpFeeBreakdown.Visible = true;
                    grpPaymentInfo.Visible = true;
                    grpBillItems.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting bill: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvBillItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore header clicks
            if (e.RowIndex < 0) return;

            try
            {
                // Get selected bill item
                if (dgvBillItems.CurrentRow != null)
                {
                    // Populate item fields with selected item data
                    txtItemName.Text = dgvBillItems.CurrentRow.Cells["ItemName"].Value.ToString();
                    cmbItemType.Text = dgvBillItems.CurrentRow.Cells["ItemType"].Value.ToString();
                    txtQuantity.Text = dgvBillItems.CurrentRow.Cells["Quantity"].Value.ToString();
                    txtUnitPrice.Text = dgvBillItems.CurrentRow.Cells["UnitPrice"].Value.ToString();
                    txtTotalPrice.Text = dgvBillItems.CurrentRow.Cells["TotalPrice"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            // Clear form for new bill
            ClearForm();

            // Generate new bill number
            GenerateBillNumber();

            // Set focus to patient dropdown
            cmbPatient.Focus();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Validate form
            if (!ValidateBillForm()) return;

            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Determine if this is an insert or update
                            bool isNewBill = selectedBillID == 0;

                            // Prepare command
                            string query = isNewBill ?
                                "INSERT INTO Bills (BillNumber, BillDate, PatientID, DoctorID, AppointmentID, " +
                                "ConsultationFee, MedicineFee, TestFee, OtherFee, Discount, TotalAmount, " +
                                "PaymentStatus, PaymentMethod, PaymentDate, Notes) " +
                                "VALUES (@BillNumber, @BillDate, @PatientID, @DoctorID, @AppointmentID, " +
                                "@ConsultationFee, @MedicineFee, @TestFee, @OtherFee, @Discount, @TotalAmount, " +
                                "@PaymentStatus, @PaymentMethod, @PaymentDate, @Notes); SELECT SCOPE_IDENTITY()" :
                                "UPDATE Bills SET BillDate = @BillDate, PatientID = @PatientID, DoctorID = @DoctorID, " +
                                "AppointmentID = @AppointmentID, ConsultationFee = @ConsultationFee, MedicineFee = @MedicineFee, " +
                                "TestFee = @TestFee, OtherFee = @OtherFee, Discount = @Discount, TotalAmount = @TotalAmount, " +
                                "PaymentStatus = @PaymentStatus, PaymentMethod = @PaymentMethod, PaymentDate = @PaymentDate, " +
                                "Notes = @Notes WHERE BillID = @BillID";

                            using (var cmd = new SqlCommand(query, conn, transaction))
                            {
                                // Add parameters
                                cmd.Parameters.AddWithValue("@BillNumber", txtBillNumber.Text);
                                cmd.Parameters.AddWithValue("@BillDate", dtpBillDate.Value);
                                cmd.Parameters.AddWithValue("@PatientID", cmbPatient.SelectedValue);
                                cmd.Parameters.AddWithValue("@DoctorID", cmbDoctor.SelectedValue);

                                // Handle nullable appointment
                                if (cmbAppointment.SelectedValue != null)
                                    cmd.Parameters.AddWithValue("@AppointmentID", cmbAppointment.SelectedValue);
                                else
                                    cmd.Parameters.AddWithValue("@AppointmentID", DBNull.Value);

                                // Fee breakdown
                                cmd.Parameters.AddWithValue("@ConsultationFee", decimal.Parse(txtConsultationFee.Text));
                                cmd.Parameters.AddWithValue("@MedicineFee", decimal.Parse(txtMedicineFee.Text));
                                cmd.Parameters.AddWithValue("@TestFee", decimal.Parse(txtTestFee.Text));
                                cmd.Parameters.AddWithValue("@OtherFee", decimal.Parse(txtOtherFee.Text));
                                cmd.Parameters.AddWithValue("@Discount", decimal.Parse(txtDiscount.Text));
                                cmd.Parameters.AddWithValue("@TotalAmount", decimal.Parse(txtTotalAmount.Text));

                                // Payment info
                                cmd.Parameters.AddWithValue("@PaymentStatus", cmbPaymentStatus.Text);

                                // Handle nullable payment method
                                if (cmbPaymentMethod.SelectedIndex >= 0)
                                    cmd.Parameters.AddWithValue("@PaymentMethod", cmbPaymentMethod.Text);
                                else
                                    cmd.Parameters.AddWithValue("@PaymentMethod", DBNull.Value);

                                // Payment date only if status is Paid or Partial
                                if (cmbPaymentStatus.Text == "Paid" || cmbPaymentStatus.Text == "Partial")
                                    cmd.Parameters.AddWithValue("@PaymentDate", dtpPaymentDate.Value);
                                else
                                    cmd.Parameters.AddWithValue("@PaymentDate", DBNull.Value);

                                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text);

                                // For updates, add BillID parameter
                                if (!isNewBill)
                                    cmd.Parameters.AddWithValue("@BillID", selectedBillID);

                                // Execute command
                                if (isNewBill)
                                {
                                    // For new bills, get the new ID
                                    selectedBillID = Convert.ToInt32(cmd.ExecuteScalar());
                                }
                                else
                                {
                                    // For updates, just execute
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Save bill items
                            SaveBillItems(conn, transaction, selectedBillID);

                            // Commit transaction
                            transaction.Commit();

                            MessageBox.Show("Bill saved successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Reload bills
                            LoadBills();
                        }
                        catch (Exception)
                        {
                            // Rollback transaction on error
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving bill: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateBillForm()
        {
            // Check required fields
            if (string.IsNullOrEmpty(txtBillNumber.Text))
            {
                MessageBox.Show("Bill number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbPatient.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a patient.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbDoctor.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a doctor.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbPaymentStatus.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a payment status.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void SaveBillItems(SqlConnection conn, SqlTransaction transaction, int billId)
        {
            // First delete existing items for this bill
            string deleteQuery = "DELETE FROM BillItems WHERE BillID = @BillID";
            using (var cmd = new SqlCommand(deleteQuery, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@BillID", billId);
                cmd.ExecuteNonQuery();
            }

            // Then insert all current items
            foreach (DataRow row in billItemsTable.Rows)
            {
                string insertQuery = "INSERT INTO BillItems (BillID, ItemName, ItemType, Quantity, UnitPrice) " +
                                    "VALUES (@BillID, @ItemName, @ItemType, @Quantity, @UnitPrice)";

                using (var cmd = new SqlCommand(insertQuery, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@BillID", billId);
                    cmd.Parameters.AddWithValue("@ItemName", row["ItemName"]);
                    cmd.Parameters.AddWithValue("@ItemType", row["ItemType"]);
                    cmd.Parameters.AddWithValue("@Quantity", row["Quantity"]);
                    cmd.Parameters.AddWithValue("@UnitPrice", row["UnitPrice"]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Confirm deletion
            if (selectedBillID == 0)
            {
                MessageBox.Show("Please select a bill to delete.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this bill?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // First delete bill items
                            string deleteItemsQuery = "DELETE FROM BillItems WHERE BillID = @BillID";
                            using (var cmd = new SqlCommand(deleteItemsQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BillID", selectedBillID);
                                cmd.ExecuteNonQuery();
                            }

                            // Then delete the bill
                            string deleteBillQuery = "DELETE FROM Bills WHERE BillID = @BillID";
                            using (var cmd = new SqlCommand(deleteBillQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BillID", selectedBillID);
                                cmd.ExecuteNonQuery();
                            }

                            // Commit transaction
                            transaction.Commit();

                            MessageBox.Show("Bill deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Clear form and reload bills
                            ClearForm();
                            LoadBills();
                        }
                        catch (Exception)
                        {
                            // Rollback transaction on error
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting bill: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnPrintPDF_Click(object sender, EventArgs e)
        {
            if (selectedBillID == 0)
            {
                MessageBox.Show("Please select a bill to print.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Set IronPDF license key
                IronPdf.License.LicenseKey = "IRONSUITE.BDJKODK.GMAIL.COM.14389-970815E96F-DFP6ETPPGKB6JJ-KLZR3VKUDOUX-5SOZQNC7OVZU-GMEM5CUVRVMW-NUKGG5LDPZYX-QMSOA4AYZLZU-ZLY2HE-TQDK6K74E3GQEA-DEPLOYMENT.TRIAL-WAHROD.TRIAL.EXPIRES.26.NOV.2025";

                // Step 3: Test if your key has been installed correctly
                // Check if a given license key string is valid 
                bool result = IronPdf.License.IsValidLicense("IRONSUITE.BDJKODK.GMAIL.COM.14389-970815E96F-DFP6ETPPGKB6JJ-KLZR3VKUDOUX-5SOZQNC7OVZU-GMEM5CUVRVMW-NUKGG5LDPZYX-QMSOA4AYZLZU-ZLY2HE-TQDK6K74E3GQEA-DEPLOYMENT.TRIAL-WAHROD.TRIAL.EXPIRES.26.NOV.2025");
                // Check if IronPDF is licensed successfully 
                bool is_licensed = IronPdf.License.IsLicensed;

                // Verify license is valid
                if (!is_licensed)
                {
                    MessageBox.Show("IronPDF license is not valid. PDF generation may be limited.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Ask if user wants to email the bill to patient
                var emailBill = MessageBox.Show(
                    "Would you like to email this bill to the patient?", 
                    "Email Bill", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                bool sendEmail = (emailBill == DialogResult.Yes);
                string pdfFilePath = "";
                byte[] pdfBytes = null;

                // Show save file dialog
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDialog.FileName = $"Bill_{txtBillNumber.Text}.pdf";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // First generate HTML content
                    string tempHtmlPath = Path.Combine(Path.GetTempPath(), $"Bill_{txtBillNumber.Text}_temp.html");
                    GenerateBillPDF(tempHtmlPath);

                    // Convert HTML to PDF using IronPDF
                    try
                    {
                        // Create a renderer
                        var renderer = new ChromePdfRenderer();

                        // Set rendering options for better output
                        var renderOptions = new ChromePdfRenderOptions()
                        {
                            PaperSize = IronPdf.Rendering.PdfPaperSize.A4,
                            MarginTop = 20,
                            MarginBottom = 20,
                            MarginLeft = 20,
                            MarginRight = 20,
                            CreatePdfFormsFromHtml = true,
                            CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print
                        };

                        renderer.RenderingOptions = renderOptions;

                        // Render HTML file to PDF
                        var pdf = renderer.RenderHtmlFileAsPdf(tempHtmlPath);

                        // Save the PDF
                        pdf.SaveAs(saveDialog.FileName);
                        pdfFilePath = saveDialog.FileName;
                        
                        // Get PDF bytes for email attachment
                        if (sendEmail)
                        {
                            pdfBytes = pdf.BinaryData;
                        }

                        // Clean up temporary HTML file
                        if (File.Exists(tempHtmlPath))
                        {
                            File.Delete(tempHtmlPath);
                        }
                        
                        // Send email if requested
                        if (sendEmail)
                        {
                            try
                            {
                                this.Cursor = Cursors.WaitCursor;
                                
                                // Get patient email from database
                                string patientEmail = GetPatientEmail();
                                
                                if (string.IsNullOrEmpty(patientEmail))
                                {
                                    MessageBox.Show("Patient does not have an email address on file.\n\nPlease update patient information to add email.", 
                                        "Email Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                // Confirm email address
                                var confirmResult = MessageBox.Show(
                                    $"Send bill to: {patientEmail}?\n\nPatient: {cmbPatient.Text}\nBill: {txtBillNumber.Text}", 
                                    "Confirm Email", 
                                    MessageBoxButtons.YesNo, 
                                    MessageBoxIcon.Question);
                                
                                if (confirmResult != DialogResult.Yes)
                                    return;
                                    
                                // Generate HTML content for email
                                string htmlContent = GenerateBillHtmlForEmail();
                                
                                // Create email service with your credentials
                                var emailService = new EmailService(
                                    "smtp.gmail.com", 
                                    587,
                                    "your-hospital-email@gmail.com", 
                                    "your-app-password-here",
                                    "your-hospital-email@gmail.com", 
                                    "Hospital Management System"
                                );

                                // Send email with PDF attachment
                                await emailService.SendEmailWithAttachmentAsync(
                                    patientEmail,
                                    $"Bill {txtBillNumber.Text} - Hospital Management System",
                                    htmlContent,
                                    pdfBytes,
                                    $"Bill_{txtBillNumber.Text}.pdf"
                                );
                                
                                MessageBox.Show($"Bill successfully sent to {patientEmail}", 
                                    "Email Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error sending email: {ex.Message}", 
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            finally
                            {
                                this.Cursor = Cursors.Default;
                            }
                        }

                        // Ask if user wants to open the file
                        if (MessageBox.Show("PDF bill generated successfully! Do you want to open it?", "Success",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            // Open the PDF file
                            var psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            };
                            System.Diagnostics.Process.Start(psi);
                        }
                    }
                    catch (Exception pdfEx)
                    {
                        MessageBox.Show($"Error converting to PDF: {pdfEx.Message}\n\nPlease ensure IronPDF is properly installed.",
                            "PDF Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating bill: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateBillPDF(string filePath)
        {
            // Read the HTML template - always use the latest version
            string templatePath = @"c:\Users\user\source\repos\HMS00\HMS00\BillTemplate.html";
            string htmlTemplate = "";

            // Always use the template file if it exists
            if (File.Exists(templatePath))
            {
                htmlTemplate = File.ReadAllText(templatePath);
            }
            else
            {
                // Fallback to embedded template only if file doesn't exist
                htmlTemplate = GetEmbeddedHtmlTemplate();
                MessageBox.Show("Could not find BillTemplate.html. Using embedded template instead.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Replace placeholders with actual values
            htmlTemplate = htmlTemplate.Replace("{BillNumber}", txtBillNumber.Text)
                                      .Replace("{BillDate}", dtpBillDate.Value.ToShortDateString())
                                      .Replace("{DueDate}", dtpBillDate.Value.AddDays(30).ToShortDateString())
                                      .Replace("{PatientName}", cmbPatient.Text)
                                      .Replace("{DoctorName}", cmbDoctor.Text)
                                      .Replace("{ConsultationFee}", txtConsultationFee.Text)
                                      .Replace("{MedicineFee}", txtMedicineFee.Text)
                                      .Replace("{TestFee}", txtTestFee.Text)
                                      .Replace("{OtherFee}", txtOtherFee.Text)
                                      .Replace("{Discount}", txtDiscount.Text)
                                      .Replace("{TotalAmount}", txtTotalAmount.Text)
                                      .Replace("{PaymentStatus}", cmbPaymentStatus.Text)
                                      .Replace("{PaymentMethod}", cmbPaymentMethod.Text)
                                      .Replace("{PaymentDate}", dtpPaymentDate.Value.ToShortDateString());

            // Set payment status class
            string paymentStatusClass = cmbPaymentStatus.Text.ToLower();
            htmlTemplate = htmlTemplate.Replace("{PaymentStatusClass}", paymentStatusClass);

            // Generate bill items rows
            StringBuilder billItemsRows = new StringBuilder();
            foreach (DataRow row in billItemsTable.Rows)
            {
                string category = row["ItemType"].ToString();
                string categoryClass = "badge-other";

                // Set appropriate category badge class
                if (category.ToLower().Contains("consultation"))
                    categoryClass = "badge-consultation";
                else if (category.ToLower().Contains("medicine"))
                    categoryClass = "badge-medicine";
                else if (category.ToLower().Contains("test") || category.ToLower().Contains("lab"))
                    categoryClass = "badge-test";

                billItemsRows.AppendLine($"<tr>");
                billItemsRows.AppendLine($"    <td class=\"item-name\">{row["ItemName"]}</td>");
                billItemsRows.AppendLine($"    <td><span class=\"category-badge {categoryClass}\">{row["ItemType"]}</span></td>");
                billItemsRows.AppendLine($"    <td class=\"text-center\">{row["Quantity"]}</td>");
                billItemsRows.AppendLine($"    <td class=\"text-right amount\">${row["UnitPrice"]}</td>");
                billItemsRows.AppendLine($"    <td class=\"text-right amount\">${row["TotalPrice"]}</td>");
                billItemsRows.AppendLine($"</tr>");
            }
            htmlTemplate = htmlTemplate.Replace("<!-- {BillItemsRows} -->", billItemsRows.ToString());

            // Add notes section if available
            if (!string.IsNullOrEmpty(txtNotes.Text))
            {
                string notesSection = $@"
                <div class=""notes"">
                    <h3>NOTES</h3>
                    <p>{txtNotes.Text.Replace(Environment.NewLine, "<br/>")}</p>
                </div>";
                htmlTemplate = htmlTemplate.Replace("<!-- {NotesSection} -->", notesSection);
            }
            else
            {
                htmlTemplate = htmlTemplate.Replace("<!-- {NotesSection} -->", "");
            }

            // Write the HTML to file
            File.WriteAllText(filePath, htmlTemplate);
        }

        private string GetEmbeddedHtmlTemplate()
        {
            // This is a fallback template in case the external template file is not found
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Hospital Bill</title>
    <style>
        :root {
            --primary-color: #2c3e50;
            --secondary-color: #3498db;
            --accent-color: #e74c3c;
            --light-color: #ecf0f1;
            --dark-color: #34495e;
            --success-color: #2ecc71;
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        
        body {
            background-color: #f9f9f9;
            color: #333;
            line-height: 1.6;
        }
        
        .invoice-container {
            max-width: 800px;
            margin: 20px auto;
            background-color: white;
            box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
            overflow: hidden;
        }
        
        .invoice-header {
            background-color: var(--primary-color);
            color: white;
            padding: 20px;
            text-align: center;
        }
        
        .invoice-header h1 {
            font-size: 24px;
            margin-bottom: 5px;
        }
        
        .invoice-header h2 {
            font-size: 18px;
            font-weight: 400;
            margin-bottom: 10px;
        }
        
        .invoice-body {
            padding: 30px;
        }
        
        .bill-info {
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
            flex-wrap: wrap;
        }
        
        .bill-info-left, .bill-info-right {
            flex-basis: 48%;
        }
        
        .info-group {
            margin-bottom: 20px;
        }
        
        .info-group h3 {
            color: var(--primary-color);
            border-bottom: 2px solid var(--secondary-color);
            padding-bottom: 5px;
            margin-bottom: 10px;
            font-size: 16px;
        }
        
        .info-row {
            display: flex;
            margin-bottom: 5px;
        }
        
        .info-label {
            font-weight: 600;
            width: 140px;
            color: var(--dark-color);
        }
        
        .info-value {
            flex: 1;
        }
        
        .fee-breakdown {
            margin-bottom: 30px;
        }
        
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        
        th {
            background-color: var(--secondary-color);
            color: white;
            text-align: left;
            padding: 10px;
        }
        
        td {
            padding: 10px;
            border-bottom: 1px solid #ddd;
        }
        
        tr:nth-child(even) {
            background-color: #f2f2f2;
        }
        
        .text-right {
            text-align: right;
        }
        
        .total-row {
            font-weight: 700;
            background-color: var(--light-color) !important;
        }
        
        .discount {
            color: var(--accent-color);
        }
        
        .notes {
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 30px;
            border-left: 4px solid var(--secondary-color);
        }
        
        .payment-status {
            display: inline-block;
            padding: 5px 10px;
            border-radius: 4px;
            font-weight: 600;
            margin-left: 10px;
        }
        
        .status-paid {
            background-color: var(--success-color);
            color: white;
        }
        
        .status-pending {
            background-color: #f39c12;
            color: white;
        }
        
        .status-unpaid {
            background-color: var(--accent-color);
            color: white;
        }
        
        .invoice-footer {
            text-align: center;
            padding: 20px;
            background-color: var(--light-color);
            font-style: italic;
            color: var(--dark-color);
        }
        
        .hospital-logo {
            text-align: center;
            margin-bottom: 20px;
        }
        
        .hospital-logo img {
            max-width: 150px;
        }
        
        @media print {
            body {
                background-color: white;
            }
            
            .invoice-container {
                box-shadow: none;
                margin: 0;
                max-width: 100%;
            }
        }
    </style>
</head>
<body>
    <div class=""invoice-container"">
        <div class=""invoice-header"">
            <h1>SIMPLE HOSPITAL MANAGEMENT SYSTEM</h1>
            <h2>PATIENT INVOICE</h2>
        </div>
        
        <div class=""invoice-body"">
            <div class=""hospital-logo"">
                <!-- Hospital logo can be added here -->
                <!-- <img src=""logo.png"" alt=""Hospital Logo""> -->
            </div>
            
            <div class=""bill-info"">
                <div class=""bill-info-left"">
                    <div class=""info-group"">
                        <h3>BILL INFORMATION</h3>
                        <div class=""info-row"">
                            <div class=""info-label"">Bill Number:</div>
                            <div class=""info-value"">{BillNumber}</div>
                        </div>
                        <div class=""info-row"">
                            <div class=""info-label"">Date:</div>
                            <div class=""info-value"">{BillDate}</div>
                        </div>
                    </div>
                    
                    <div class=""info-group"">
                        <h3>PATIENT INFORMATION</h3>
                        <div class=""info-row"">
                            <div class=""info-label"">Patient:</div>
                            <div class=""info-value"">{PatientName}</div>
                        </div>
                        <div class=""info-row"">
                            <div class=""info-label"">Doctor:</div>
                            <div class=""info-value"">{DoctorName}</div>
                        </div>
                    </div>
                </div>
                
                <div class=""bill-info-right"">
                    <div class=""info-group"">
                        <h3>PAYMENT INFORMATION</h3>
                        <div class=""info-row"">
                            <div class=""info-label"">Payment Status:</div>
                            <div class=""info-value"">
                                <span class=""payment-status status-{PaymentStatusClass}"">{PaymentStatus}</span>
                            </div>
                        </div>
                        <div class=""info-row"">
                            <div class=""info-label"">Payment Method:</div>
                            <div class=""info-value"">{PaymentMethod}</div>
                        </div>
                        <div class=""info-row"">
                            <div class=""info-label"">Payment Date:</div>
                            <div class=""info-value"">{PaymentDate}</div>
                        </div>
                    </div>
                </div>
            </div>
            
            <div class=""fee-breakdown"">
                <h3>FEE BREAKDOWN</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Service</th>
                            <th class=""text-right"">Amount</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Consultation Fee</td>
                            <td class=""text-right"">${ConsultationFee}</td>
                        </tr>
                        <tr>
                            <td>Medicine Fee</td>
                            <td class=""text-right"">${MedicineFee}</td>
                        </tr>
                        <tr>
                            <td>Test Fee</td>
                            <td class=""text-right"">${TestFee}</td>
                        </tr>
                        <tr>
                            <td>Other Fee</td>
                            <td class=""text-right"">${OtherFee}</td>
                        </tr>
                        <tr>
                            <td>Discount</td>
                            <td class=""text-right discount"">-${Discount}</td>
                        </tr>
                        <tr class=""total-row"">
                            <td>TOTAL AMOUNT</td>
                            <td class=""text-right"">${TotalAmount}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
            
            <div class=""bill-items"">
                <h3>BILL ITEMS</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Item</th>
                            <th>Type</th>
                            <th class=""text-right"">Qty</th>
                            <th class=""text-right"">Unit Price</th>
                            <th class=""text-right"">Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- {BillItemsRows} -->
                    </tbody>
                </table>
            </div>
            
            <!-- {NotesSection} -->
            
        </div>
        
        <div class=""invoice-footer"">
            <p>Thank you for choosing our hospital. We wish you a speedy recovery!</p>
        </div>
    </div>
</body>
</html>";
        }



        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            // Validate item fields
            if (string.IsNullOrEmpty(txtItemName.Text))
            {
                MessageBox.Show("Please enter an item name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbItemType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an item type.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Parse quantity and unit price
                int quantity = int.Parse(txtQuantity.Text);
                decimal unitPrice = decimal.Parse(txtUnitPrice.Text);
                decimal totalPrice = quantity * unitPrice;

                // Add item to table
                DataRow newRow = billItemsTable.NewRow();
                newRow["ItemID"] = 0; // New item
                newRow["ItemName"] = txtItemName.Text;
                newRow["ItemType"] = cmbItemType.Text;
                newRow["Quantity"] = quantity;
                newRow["UnitPrice"] = unitPrice;
                newRow["TotalPrice"] = totalPrice;

                billItemsTable.Rows.Add(newRow);

                // Update grid
                dgvBillItems.DataSource = null;
                dgvBillItems.DataSource = billItemsTable;

                // Clear item fields
                ClearItemFields();

                // Update fee breakdown based on item type
                UpdateFeeBreakdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearItemFields()
        {
            txtItemName.Clear();
            cmbItemType.SelectedIndex = -1;
            txtQuantity.Text = "1";
            txtUnitPrice.Text = "0.00";
            txtTotalPrice.Text = "0.00";
        }

        private void UpdateFeeBreakdown()
        {
            // Calculate totals by item type
            decimal medicineTotal = 0;
            decimal testTotal = 0;
            decimal otherTotal = 0;

            foreach (DataRow row in billItemsTable.Rows)
            {
                string itemType = row["ItemType"].ToString();
                decimal totalPrice = Convert.ToDecimal(row["TotalPrice"]);

                switch (itemType)
                {
                    case "Medicine":
                        medicineTotal += totalPrice;
                        break;
                    case "Test":
                        testTotal += totalPrice;
                        break;
                    case "Procedure":
                    case "Other":
                        otherTotal += totalPrice;
                        break;
                }
            }

            // Update fee fields
            txtMedicineFee.Text = medicineTotal.ToString("F2");
            txtTestFee.Text = testTotal.ToString("F2");
            txtOtherFee.Text = otherTotal.ToString("F2");

            // Recalculate total
            CalculateTotalAmount();
        }

        private void BtnUpdateItem_Click(object sender, EventArgs e)
        {
            // Check if an item is selected
            if (dgvBillItems.CurrentRow == null)
            {
                MessageBox.Show("Please select an item to update.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Validate item fields
            if (string.IsNullOrEmpty(txtItemName.Text))
            {
                MessageBox.Show("Please enter an item name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbItemType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an item type.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Parse quantity and unit price
                int quantity = int.Parse(txtQuantity.Text);
                decimal unitPrice = decimal.Parse(txtUnitPrice.Text);
                decimal totalPrice = quantity * unitPrice;

                // Update selected row
                int rowIndex = dgvBillItems.CurrentRow.Index;
                DataRow row = billItemsTable.Rows[rowIndex];

                row["ItemName"] = txtItemName.Text;
                row["ItemType"] = cmbItemType.Text;
                row["Quantity"] = quantity;
                row["UnitPrice"] = unitPrice;
                row["TotalPrice"] = totalPrice;

                // Update grid
                dgvBillItems.DataSource = null;
                dgvBillItems.DataSource = billItemsTable;

                // Clear item fields
                ClearItemFields();

                // Update fee breakdown
                UpdateFeeBreakdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            // Check if an item is selected
            if (dgvBillItems.CurrentRow == null)
            {
                MessageBox.Show("Please select an item to remove.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Remove selected row
                int rowIndex = dgvBillItems.CurrentRow.Index;
                billItemsTable.Rows.RemoveAt(rowIndex);

                // Update grid
                dgvBillItems.DataSource = null;
                dgvBillItems.DataSource = billItemsTable;

                // Clear item fields
                ClearItemFields();

                // Update fee breakdown
                UpdateFeeBreakdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Data loading methods
        private void LoadPatients()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT PatientID, Name AS FullName FROM Patients ORDER BY Name";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable patientsTable = new DataTable();
                        adapter.Fill(patientsTable);

                        cmbPatient.DataSource = patientsTable;
                        cmbPatient.DisplayMember = "FullName";
                        cmbPatient.ValueMember = "PatientID";
                        cmbPatient.SelectedIndex = -1; // No selection by default
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading patients: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDoctors()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT DoctorID, Name AS FullName FROM Doctors ORDER BY Name";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable doctorsTable = new DataTable();
                        adapter.Fill(doctorsTable);

                        cmbDoctor.DataSource = doctorsTable;
                        cmbDoctor.DisplayMember = "FullName";
                        cmbDoctor.ValueMember = "DoctorID";
                        cmbDoctor.SelectedIndex = -1; // No selection by default
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading doctors: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAppointments()
        {
            try
            {
                // Only load appointments if both patient and doctor are selected
                if (cmbPatient.SelectedValue == null || cmbDoctor.SelectedValue == null)
                {
                    cmbAppointment.DataSource = null;
                    return;
                }

                // Fix for DataRowView casting error
                int patientId = Convert.ToInt32(cmbPatient.SelectedValue);
                int doctorId = Convert.ToInt32(cmbDoctor.SelectedValue);

                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT AppointmentID, 
                                    CONVERT(VARCHAR, AppointmentDate, 103) + ' ' + 
                                    CONVERT(VARCHAR, AppointmentTime, 108) AS AppointmentDateTime 
                                    FROM Appointments 
                                    WHERE PatientID = @PatientID AND DoctorID = @DoctorID 
                                    ORDER BY AppointmentDate DESC, AppointmentTime DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientId);
                        cmd.Parameters.AddWithValue("@DoctorID", doctorId);

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable appointmentsTable = new DataTable();
                            adapter.Fill(appointmentsTable);

                            cmbAppointment.DataSource = appointmentsTable;
                            cmbAppointment.DisplayMember = "AppointmentDateTime";
                            cmbAppointment.ValueMember = "AppointmentID";
                            cmbAppointment.SelectedIndex = -1; // No selection by default
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading appointments: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBills()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT b.BillID, b.BillNumber, b.BillDate, 
                                    p.Name AS PatientName,
                                    d.Name AS DoctorName,
                                    b.TotalAmount, b.PaymentStatus
                                    FROM Bills b
                                    JOIN Patients p ON b.PatientID = p.PatientID
                                    JOIN Doctors d ON b.DoctorID = d.DoctorID
                                    ORDER BY b.BillDate DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable billsTable = new DataTable();
                        adapter.Fill(billsTable);

                        dgvBills.DataSource = billsTable;

                        // Configure columns
                        if (dgvBills.Columns.Count > 0)
                        {
                            dgvBills.Columns["BillID"].Visible = false;
                            dgvBills.Columns["BillNumber"].HeaderText = "Bill #";
                            dgvBills.Columns["BillDate"].HeaderText = "Date";
                            dgvBills.Columns["PatientName"].HeaderText = "Patient";
                            dgvBills.Columns["DoctorName"].HeaderText = "Doctor";
                            dgvBills.Columns["TotalAmount"].HeaderText = "Total";
                            dgvBills.Columns["TotalAmount"].DefaultCellStyle.Format = "C2";
                            dgvBills.Columns["PaymentStatus"].HeaderText = "Status";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bills: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load bill items for selected bill
        private void LoadBillItems(int billId)
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT ItemID, ItemType, ItemName, Quantity, UnitPrice, 
                                    (Quantity * UnitPrice) AS TotalPrice
                                    FROM BillItems
                                    WHERE BillID = @BillID";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BillID", billId);

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            // Create a new DataTable for bill items if needed
                            if (billItemsTable == null)
                            {
                                InitializeBillItemsTable();
                            }

                            // Clear existing data
                            billItemsTable.Clear();
                            adapter.Fill(billItemsTable);

                            // Make sure the DataGridView is using the table
                            dgvBillItems.DataSource = billItemsTable;

                            // Configure columns
                            if (dgvBillItems.Columns.Count > 0)
                            {
                                if (dgvBillItems.Columns.Contains("BillItemID"))
                                    dgvBillItems.Columns["BillItemID"].Visible = false;

                                if (dgvBillItems.Columns.Contains("ItemType"))
                                    dgvBillItems.Columns["ItemType"].HeaderText = "Type";

                                if (dgvBillItems.Columns.Contains("ItemName"))
                                    dgvBillItems.Columns["ItemName"].HeaderText = "Item";

                                if (dgvBillItems.Columns.Contains("Quantity"))
                                    dgvBillItems.Columns["Quantity"].HeaderText = "Qty";

                                if (dgvBillItems.Columns.Contains("UnitPrice"))
                                {
                                    dgvBillItems.Columns["UnitPrice"].HeaderText = "Unit Price";
                                    dgvBillItems.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
                                }

                                if (dgvBillItems.Columns.Contains("TotalPrice"))
                                {
                                    dgvBillItems.Columns["TotalPrice"].HeaderText = "Total";
                                    dgvBillItems.Columns["TotalPrice"].DefaultCellStyle.Format = "C2";
                                }
                            }

                            // Make sure the bill items panel is visible
                            grpBillItems.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bill items: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear form fields and reset to default state
        private void ClearForm()
        {
            txtBillNumber.Text = "";
            dtpBillDate.Value = DateTime.Now;
            cmbPatient.SelectedIndex = -1;
            cmbDoctor.SelectedIndex = -1;
            cmbAppointment.SelectedIndex = -1;
            cmbPaymentStatus.SelectedIndex = -1;
            cmbPaymentMethod.SelectedIndex = -1;
            dtpPaymentDate.Value = DateTime.Now;

            txtConsultationFee.Text = "0";
            txtMedicineFee.Text = "0";
            txtTestFee.Text = "0";
            txtOtherFee.Text = "0";
            txtDiscount.Text = "0";
            txtTotalAmount.Text = "0";
            txtNotes.Text = "";

            // Clear bill items
            billItemsTable.Clear();
            dgvBillItems.DataSource = billItemsTable;

            // Reset item fields
            txtItemName.Text = "";
            cmbItemType.SelectedIndex = -1;
            txtQuantity.Text = "1";
            txtUnitPrice.Text = "0";
            txtTotalPrice.Text = "0";

            // Reset selected bill ID
            selectedBillID = 0;
        }

        // Generate a new bill number
        private void GenerateBillNumber()
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ISNULL(MAX(BillNumber), 0) + 1 FROM Bills";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        txtBillNumber.Text = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating bill number: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtBillNumber.Text = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        // Load bill details for selected bill
        private void LoadBillDetails(int billId)
        {
            try
            {
                using (var conn = DB.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT b.BillNumber, b.BillDate, b.PatientID, b.DoctorID, 
                                    b.AppointmentID, b.ConsultationFee, b.MedicineFee, b.TestFee, 
                                    b.OtherFee, b.Discount, b.TotalAmount, b.PaymentStatus, 
                                    b.PaymentMethod, b.PaymentDate, b.Notes
                                    FROM Bills b
                                    WHERE b.BillID = @BillID";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BillID", billId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Set bill information
                                txtBillNumber.Text = reader["BillNumber"].ToString();
                                dtpBillDate.Value = Convert.ToDateTime(reader["BillDate"]);

                                // Set patient, doctor, appointment
                                int patientId = Convert.ToInt32(reader["PatientID"]);
                                int doctorId = Convert.ToInt32(reader["DoctorID"]);

                                // Make sure the combo boxes have items and ValueMember is set before setting selected values
                                if (cmbPatient.Items.Count > 0 && !string.IsNullOrEmpty(cmbPatient.ValueMember))
                                    cmbPatient.SelectedValue = patientId;

                                if (cmbDoctor.Items.Count > 0 && !string.IsNullOrEmpty(cmbDoctor.ValueMember))
                                    cmbDoctor.SelectedValue = doctorId;

                                if (reader["AppointmentID"] != DBNull.Value && cmbAppointment.Items.Count > 0
                                    && !string.IsNullOrEmpty(cmbAppointment.ValueMember))
                                    cmbAppointment.SelectedValue = Convert.ToInt32(reader["AppointmentID"]);

                                // Set fees
                                txtConsultationFee.Text = reader["ConsultationFee"].ToString();
                                txtMedicineFee.Text = reader["MedicineFee"].ToString();
                                txtTestFee.Text = reader["TestFee"].ToString();
                                txtOtherFee.Text = reader["OtherFee"].ToString();
                                txtDiscount.Text = reader["Discount"].ToString();
                                txtTotalAmount.Text = reader["TotalAmount"].ToString();

                                // Set payment info
                                // Initialize payment status combo box if needed
                                if (cmbPaymentStatus.Items.Count == 0)
                                {
                                    cmbPaymentStatus.Items.AddRange(new string[] { "Pending", "Partial", "Paid", "Cancelled" });
                                }
                                cmbPaymentStatus.Text = reader["PaymentStatus"].ToString();

                                if (reader["PaymentMethod"] != DBNull.Value)
                                    cmbPaymentMethod.Text = reader["PaymentMethod"].ToString();

                                if (reader["PaymentDate"] != DBNull.Value)
                                    dtpPaymentDate.Value = Convert.ToDateTime(reader["PaymentDate"]);

                                txtNotes.Text = reader["Notes"].ToString();

                                // Calculate and display fee breakdown
                                decimal consultationFee = Convert.ToDecimal(reader["ConsultationFee"]);
                                decimal medicineFee = Convert.ToDecimal(reader["MedicineFee"]);
                                decimal testFee = Convert.ToDecimal(reader["TestFee"]);
                                decimal otherFee = Convert.ToDecimal(reader["OtherFee"]);
                                decimal discount = Convert.ToDecimal(reader["Discount"]);
                                decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);

                                // Update fee breakdown in text boxes instead of labels
                                txtConsultationFee.Text = consultationFee.ToString();
                                txtMedicineFee.Text = medicineFee.ToString();
                                txtTestFee.Text = testFee.ToString();
                                txtOtherFee.Text = otherFee.ToString();
                                txtDiscount.Text = discount.ToString();
                                txtTotalAmount.Text = totalAmount.ToString();

                                // Make sure all panels are visible
                                grpBillInfo.Visible = true;
                                grpFeeBreakdown.Visible = true;
                                grpPaymentInfo.Visible = true;
                                grpBillItems.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading bill details: " + ex.Message, "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblItemName_Click(object sender, EventArgs e)
        {

        }

        private void lblBillNumber_Click(object sender, EventArgs e)
        {

        }

        private void lblBillDate_Click(object sender, EventArgs e)
        {

        }

        private void grpBillItems_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Clear all form fields
            txtBillNumber.Text = GenerateNewBillNumber();
            dtpBillDate.Value = DateTime.Now;
            cmbPatient.SelectedIndex = -1;
            cmbDoctor.SelectedIndex = -1;
            cmbAppointment.SelectedIndex = -1;
            cmbPaymentStatus.SelectedIndex = 0; // Default to "Pending"
            cmbPaymentMethod.SelectedIndex = 0; // Default to first method
            dtpPaymentDate.Value = DateTime.Now;

            // Clear fee fields
            txtConsultationFee.Text = "0";
            txtMedicineFee.Text = "0";
            txtTestFee.Text = "0";
            txtOtherFee.Text = "0";
            txtDiscount.Text = "0";
            txtTotalAmount.Text = "0";
            txtNotes.Text = "";

            // Clear bill items
            billItemsTable.Clear();
            dgvBillItems.DataSource = billItemsTable;

            // Reset selected bill ID
            selectedBillID = 0;

            MessageBox.Show("Form cleared successfully!", "Clear", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }

        private void grpFeeBreakdown_Enter(object sender, EventArgs e)
        {

        }
    }
}

// Extension method for adding tooltips to controls
public static class ControlExtensions
{
    public static T ToolTip<T>(this T control, string text) where T : Control
    {
        ToolTip toolTip = new ToolTip();
        toolTip.SetToolTip(control, text);
        return control;
    }
}

        #endregion
