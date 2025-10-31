# Hospital Management System - Billing Module

## Overview
This Hospital Management System includes a comprehensive billing module that allows hospital staff to create bills, manage bill items, generate PDF receipts, and send these receipts to patients via email.

## Features
- **Patient and Doctor Selection**: Choose patients and doctors from dropdown menus
- **Bill Management**: Create, save, and delete bills
- **Item Management**: Add and remove items from bills with automatic total calculation
- **PDF Generation**: Generate professional PDF receipts for patients
- **Email Integration**: Send PDF receipts directly to patients via email
- **Data Persistence**: All billing data is stored in the database for future reference

## Technical Details
- **Platform**: Windows Forms (.NET)
- **Database**: SQL Server (SimpleHospitalDB)
- **PDF Generation**: Uses IronText library
- **Email Service**: System.Net.Mail

## Usage Instructions

### Creating a New Bill
1. Click the "New Bill" button
2. Select a patient from the dropdown
3. Select a doctor from the dropdown
4. Set the bill date
5. Add items to the bill using the "Add Item" section
6. Click "Save Bill" to store the bill in the database

### Managing Bill Items
1. Enter item name, price, and quantity
2. Click "Add Item" to add to the current bill
3. Select an item and click "Remove Item" to delete it
4. The total amount is calculated automatically

### Generating PDF Receipts
1. Select a bill from the bills list
2. Click "Generate PDF" to create a PDF receipt
3. The PDF will open automatically for preview

### Sending Email Receipts
1. Select a bill from the bills list
2. Click "Send Email" to send the PDF receipt to the patient's email
3. A confirmation message will appear when the email is sent successfully

## Error Handling
The system includes comprehensive error handling for:
- Database connection issues
- Invalid data entry
- PDF generation failures
- Email sending problems

## System Requirements
- Windows 7 or higher
- .NET Framework 4.7.2 or higher
- SQL Server 2012 or higher
- Internet connection (for email functionality)

## Security
- Patient information is handled securely
- Email functionality uses secure SMTP connections
- Database access is protected with proper authentication

## Support
For technical support or questions, please contact the system administrator.