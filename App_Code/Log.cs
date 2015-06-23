using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Web;

namespace AGI.Logger
{
    public class Log
    {
        static readonly object _syncObject = new object();

        public static void WriteToLog(string strMessage, string strStackTrace)
        {
            string userName = Environment.UserName;
            string loggerPath = ConfigurationManager.AppSettings["logPath"];
            lock (_syncObject)
            {
                if (!Directory.Exists(loggerPath))
                    Directory.CreateDirectory(loggerPath);

                DateTime date = DateTime.Today;
                string strDate = date.ToString("ddMMMyyyy");
                string filePath = "";
                if (loggerPath.EndsWith(@"\"))
                    filePath = loggerPath + "Logger_" + strDate + ".txt";
                else
                    filePath = loggerPath + @"\Logger_" + strDate + ".txt";
                StreamWriter _file = new StreamWriter(filePath, true);
                _file.Write(DateTime.Now.ToString());
                _file.Write("\t\t");
                _file.Write(userName);
                _file.Write("\t\t");
                _file.Write(strMessage);
                _file.Write("\t\t");
                _file.Write(strStackTrace);
                _file.Write("\r\n");
                _file.Close();
            }
        }
    }
}
