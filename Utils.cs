using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExchangeUtil
{
    class Utils
    {
        public enum logLevel { BEGIN, END, FATAL, WARN, CONFIRM, INFO, SUCCESS, FAIL }

        public static void writeLog(logLevel level, string message, Exception ex)
        {
            String theLogString = DateTime.Now + "  [" + level + "] " + message;

            StreamWriter theLogWriter = File.AppendText("ExpressoExchangeServiceLog.log");
            theLogWriter.WriteLine(theLogString);

            Console.WriteLine(theLogString);

            if (level.Equals(logLevel.FATAL))
            {
                if (ex != null)
                {
                    theLogWriter.WriteLine(ex.StackTrace);

                    Console.WriteLine(ex.StackTrace);
                }
            }

            theLogWriter.Close();
        }
    }
}
