using System;
using ewal.Data;
using System.Collections;
using System.Data.Common;
using System.Windows.Forms;
using SuvirinimaiApp.Properties;

namespace SuvirinimaiApp
{
    static class Program
    {
        // shared things
        static internal int pubInt = -1;
        static internal string pubString = string.Empty;
        static internal DateTime firstDate;
        static internal DateTime lastDate;
        static internal ArrayList sharedThings = new ArrayList();
        // end of shared things

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DbHelper.ConstructHelper(Settings.Default.DbProviderName, Settings.Default.DbConnectionString);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new fAktai());
        }
    }
}
