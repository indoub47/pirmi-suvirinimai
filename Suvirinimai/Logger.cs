using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuvirinimaiApp
{
    class Logger
    {
        string fileName;
        TextWriter sw;
        static int nr;

        public Logger(string logFileName)
        {
            fileName = logFileName;
            nr = 0;
        }

        public void Open()
        {
            sw = File.AppendText(fileName);
        }

        public void Log (string message)
        {
            sw.Write(string.Format("{0}. {1} {2}:   {3}{4}", ++nr, DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString(), message, Environment.NewLine));
            // Update the underlying file.
        }

        public void Close()
        {
            sw.Flush();
            sw.Close();
        }
    }
}
