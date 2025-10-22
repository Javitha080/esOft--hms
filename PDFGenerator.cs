using System;
using System.IO; // Used for file operations
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Geom; // Contains PageSize
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Data;
using iText.Layout.Borders; // Added for Border class
using iText.Kernel.Pdf.Canvas.Draw; // Contains SolidLine class for line separators

namespace SimpleHMS
{
    public class PDFGenerator
    {
        // Hospital information
        private static readonly string HospitalName = "Esoft Hospital Management System";
        private static readonly string HospitalAddress = "123 Healthcare Avenue, Medical District";
        private static readonly string HospitalPhone = "+94 123-456071";
        private static readonly string HospitalEmail = "info@esofthms.com";
        
        // Standard booking fee
        private static readonly decimal HospitalBookingFee = 50.00m;

        // Generate appointment receipt PDF
        public static string GenerateAppointmentReceipt(int appointmentId, int patientId, int doctorId, DateTime appointmentDate, DateTime appointmentTime)
        {
            try
            {
                // Validate input parameters
                if (patientId <= 0 || doctorId <= 0)
                {
                    throw new ArgumentException("Patient ID and Doctor ID must be valid positive integers");
                }
                
                // Create a unique filename for the PDF with timestamp to ensure uniqueness
                string fileName = $"Appointment_Receipt_{appointmentId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                // Use System.IO.Path explicitly to avoid ambiguity with iText.Kernel.Geom.Path
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                // Create PDF writer and document
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        // Create document with A4 page size
                        using (Document document = new Document(pdf, PageSize.A4))
                        {
                            // Set margins
                            document.SetMargins(36, 36, 36, 36);

                            // Add hospital header
                            AddHeader(document);

                            // Add appointment details
                            AddAppointmentDetails(document, appointmentId, patientId, doctorId, appointmentDate, appointmentTime);

                            // Add payment details
                            AddPaymentDetails(document, doctorId);

                            // Add footer
                            AddFooter(document, appointmentId);
                        }
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }
        }

        // Add hospital header to the document
        private static void AddHeader(Document document)
        {
            // Create header paragraph with hospital name
            Paragraph header = new Paragraph(HospitalName)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(header);

            // Add hospital address
            Paragraph address = new Paragraph(HospitalAddress)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(address);

            // Add hospital contact information
            Paragraph contact = new Paragraph($"Tel: {HospitalPhone} | Email: {HospitalEmail}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(contact);

            // Add separator line with 1px solid line style
            // SolidLine creates a continuous line with specified thickness for visual separation
            LineSeparator line = new LineSeparator(new SolidLine(1f)) // Using imported SolidLine class from iText.Kernel.Pdf.Canvas.Draw
                .SetMarginTop(10)
                .SetMarginBottom(10);
            document.Add(line);

            // Add receipt title
            Paragraph title = new Paragraph("APPOINTMENT RECEIPT")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(title);
        }

        // Add appointment details to the document
        private static void AddAppointmentDetails(Document document, int appointmentId, int patientId, int doctorId, DateTime appointmentDate, DateTime appointmentTime)
        {
            // Get patient information from database
            DataTable patientData = DB.GetData($"SELECT Name, Phone, Email, Address FROM Patients WHERE PatientID = {patientId}");
            if (patientData.Rows.Count == 0)
            {
                throw new Exception($"Patient with ID {patientId} not found");
            }

            // Get doctor information from database
            DataTable doctorData = DB.GetData($"SELECT Name, Specialization, ConsultationFee FROM Doctors WHERE DoctorID = {doctorId}");
            if (doctorData.Rows.Count == 0)
            {
                throw new Exception($"Doctor with ID {doctorId} not found");
            }

            // Extract patient and doctor information
            string patientName = patientData.Rows[0]["Name"].ToString() ?? "";
            string patientPhone = patientData.Rows[0]["Phone"].ToString() ?? "";
            string patientEmail = patientData.Rows[0]["Email"].ToString() ?? "";
            string patientAddress = patientData.Rows[0]["Address"].ToString() ?? "";

            string doctorName = doctorData.Rows[0]["Name"].ToString() ?? "";
            string doctorSpecialization = doctorData.Rows[0]["Specialization"].ToString() ?? "";
            decimal doctorFee = Convert.ToDecimal(doctorData.Rows[0]["ConsultationFee"]);

            // Create a table for appointment details with 2 columns
            Table table = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Add appointment ID row
            table.AddCell(CreateCell("Receipt No:", true));
            table.AddCell(CreateCell(appointmentId.ToString(), false));

            // Add date and time row
            table.AddCell(CreateCell("Date & Time:", true));
            table.AddCell(CreateCell($"{appointmentDate.ToString("yyyy-MM-dd")} at {appointmentTime.ToString("hh:mm tt")}", false));

            // Add patient information section header
            Cell patientHeaderCell = new Cell(1, 2)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(1))
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .Add(new Paragraph("PATIENT INFORMATION"));
            table.AddCell(patientHeaderCell);

            // Add patient details
            table.AddCell(CreateCell("Patient Name:", true));
            table.AddCell(CreateCell(patientName, false));

            table.AddCell(CreateCell("Contact:", true));
            table.AddCell(CreateCell(patientPhone, false));

            table.AddCell(CreateCell("Email:", true));
            table.AddCell(CreateCell(patientEmail, false));

            table.AddCell(CreateCell("Address:", true));
            table.AddCell(CreateCell(patientAddress, false));

            // Add doctor information section header
            Cell doctorHeaderCell = new Cell(1, 2)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER)
                .SetBorderBottom(new SolidBorder(1))
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .Add(new Paragraph("DOCTOR INFORMATION"));
            table.AddCell(doctorHeaderCell);

            // Add doctor details
            table.AddCell(CreateCell("Doctor Name:", true));
            table.AddCell(CreateCell(doctorName, false));

            table.AddCell(CreateCell("Specialization:", true));
            table.AddCell(CreateCell(doctorSpecialization, false));

            // Add the table to the document
            document.Add(table);
        }

        // Add payment details to the document
        private static void AddPaymentDetails(Document document, int doctorId)
        {
            // Get doctor fee from database
            DataTable doctorData = DB.GetData($"SELECT ConsultationFee FROM Doctors WHERE DoctorID = {doctorId}");
            if (doctorData.Rows.Count == 0)
            {
                throw new Exception($"Doctor with ID {doctorId} not found");
            }

            decimal doctorFee = Convert.ToDecimal(doctorData.Rows[0]["ConsultationFee"]);
            decimal totalFee = doctorFee + HospitalBookingFee;

            // Add payment section title
            Paragraph paymentTitle = new Paragraph("PAYMENT DETAILS")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginBottom(10);
            document.Add(paymentTitle);

            // Create a table for payment details with 2 columns
            Table paymentTable = new Table(2)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            // Add payment details
            paymentTable.AddCell(CreateCell("Doctor's Consultation Fee:", true));
            paymentTable.AddCell(CreateCell($"${doctorFee:F2}", false).SetTextAlignment(TextAlignment.RIGHT));

            paymentTable.AddCell(CreateCell("Hospital Booking Fee:", true));
            paymentTable.AddCell(CreateCell($"${HospitalBookingFee:F2}", false).SetTextAlignment(TextAlignment.RIGHT));

            // Add total with a thicker border
            Cell totalLabelCell = CreateCell("TOTAL AMOUNT:", true)
                .SetBorderTop(new SolidBorder(1))
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD));
            
            Cell totalValueCell = CreateCell($"${totalFee:F2}", false)
                .SetBorderTop(new SolidBorder(1))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD));

            paymentTable.AddCell(totalLabelCell);
            paymentTable.AddCell(totalValueCell);

            // Add the payment table to the document
            document.Add(paymentTable);

            // Add payment note
            Paragraph paymentNote = new Paragraph("Payment is due at the time of appointment. We accept cash, credit cards, and insurance.")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE))
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(paymentNote);
        }

        // Add footer to the document
        private static void AddFooter(Document document, int appointmentId)
        {
            // Add separator line
            LineSeparator line = new LineSeparator(new SolidLine(1f))
                .SetMarginTop(10)
                .SetMarginBottom(10);
            document.Add(line);

            // Add thank you message
            Paragraph thankYou = new Paragraph("Thank you for choosing Esoft Hospital Management System")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(thankYou);

            // Add appointment verification note
            Paragraph verificationNote = new Paragraph($"Please bring this receipt to verify your appointment (ID: {appointmentId})")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(verificationNote);

            // Add generated timestamp
            Paragraph timestamp = new Paragraph($"Generated on: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(20);
            document.Add(timestamp);
        }

        // Helper method to create a cell with consistent styling
        private static Cell CreateCell(string text, bool isHeader)
        {
            // Handle null text parameter
            text = text ?? "";
            
            Cell cell = new Cell()
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph(text));

            if (isHeader)
            {
                cell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetBackgroundColor(new DeviceRgb(240, 240, 240));
            }
            else
            {
                cell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
            }

            return cell;
        }
    }
}