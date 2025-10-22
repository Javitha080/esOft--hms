namespace SimpleHMS
{
    // Program - Entry point of the application (.NET 9 version)
    // This is where the program starts running
    internal static class Program
    {
        // Main() - The first method that runs when program starts
        // [STAThread] is required for Windows Forms applications
        [STAThread]
        static void Main()
        {
            // .NET 9 uses ApplicationConfiguration instead of Application.EnableVisualStyles()
            // This configures the application for high DPI displays and modern visual styles
            ApplicationConfiguration.Initialize();
            
            // Run the application and show the MainForm window
            // This line opens the main menu and keeps the program running
            Application.Run(new LoginForm());
            
            // When MainForm is closed, the program exits
        }
    }
}

