using System;
using System.Configuration;
using System.Diagnostics;

namespace Jenkins2SkypeMsg.utils
{
    class TraceInit
    {
        TextWriterTraceListener consoleOutput;
        TextWriterTraceListener fileOutput;
        String logWritingDate;

        public TraceInit()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            checkLogFolder();

            initConsoleOutput();
            
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["writeLogFile"]))
            {
                initFileOutput();
            }
        }

        public void checkDate()
        {
            if (logWritingDate != TimeUtils.getCurrentDate())
            {
                initFileOutput();
            }
        }

        private void checkLogFolder()
        {
            bool isExists = System.IO.Directory.Exists("log");
            if (!isExists)
                System.IO.Directory.CreateDirectory("log");
        }

        private void initConsoleOutput()
        {
            consoleOutput = new TextWriterTraceListener(System.Console.Out);
            Trace.Listeners.Add(consoleOutput);
        }

        private void initFileOutput()
        {
            if (Trace.Listeners.Contains(fileOutput))
            {
                fileOutput.Close();
                Trace.Listeners.Remove("FileOutput");
            }
            logWritingDate = TimeUtils.getCurrentDate();
            fileOutput = new TextWriterTraceListener(System.IO.File.CreateText("log/" + logWritingDate + ".log"), "FileOutput");
            fileOutput.Filter = new EventTypeFilter(SourceLevels.Error);
            Trace.Listeners.Add(fileOutput);
            Trace.AutoFlush = true;
        }
    }
}
