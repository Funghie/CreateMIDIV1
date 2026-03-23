using System;
using System.Windows.Forms;

namespace CreateMIDI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Check if -startup flag is present
            bool isStartup = false;
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (string.Equals(args[i], "-startup", StringComparison.OrdinalIgnoreCase))
                    {
                        isStartup = true;
                        break;
                    }
                }
            }

            if (isStartup)
            {
                // Recreate ports from tracking file and exit silently
                Form1.RecreatePortsFromCsv();
                return;
            }

            // Normal UI startup
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
