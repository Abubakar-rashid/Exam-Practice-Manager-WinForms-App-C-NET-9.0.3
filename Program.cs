namespace ExamPracticeSystemFull
{
    using System;
    using System.Windows.Forms;
    using ExamPracticeSystemFull.Forms; // Ensure this namespace is correct

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] // STAThread attribute is required for Windows Forms applications
        static void Main()
        {
            Application.EnableVisualStyles(); // Enables visual styles for the application
            Application.SetCompatibleTextRenderingDefault(false); // Sets the default text rendering for compatibility

            // Run the LoginForm first. If login is successful, LoginForm will open MainForm.
            // If LoginForm is closed without successful login, the application will exit.
            Application.Run(new LoginForm());
        }
    }
}
