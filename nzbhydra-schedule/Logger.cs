using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nzbhydra_schedule
{

    public static class Logger
    {
        public enum LogLevel
        {
            err,
            warn,
            info,
            debug,
        }

        public static LogLevel logLevel = LogLevel.info;
        public static LogLevel MinLogLevel { get; set; } = LogLevel.info;

        public static void WriteLog(string message, LogLevel logLevel = LogLevel.info)
        {
            if (MinLogLevel < logLevel)
            {
                return;
            }

            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {logLevel.ToString()}: {message}");
        }

        public static void Throw(string message, Exception exception)
        {
            WriteLog(message, LogLevel.err);
            Throw(exception);
        }

        public static void Throw(Exception exception)
        {
            WriteLog(exception.Message, LogLevel.err);
            throw (exception);
        }

    }
}
