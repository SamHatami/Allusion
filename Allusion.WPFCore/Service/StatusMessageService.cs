using Caliburn.Micro;
using System.IO;

namespace Allusion.WPFCore.Service;

public static class StaticLogger
{

    public delegate void LogEventHandler(string message, LogLevel logLevel);

    public static event LogEventHandler LogEvent = delegate { };
   
    public static void Info(string message, bool includeTimeStamp, bool writeToLog)
    {
        if (includeTimeStamp)
            message = $"{TimeStamp(DateTime.Now)} - {message}";

        LogEvent(message, LogLevel.Info);

        if(writeToLog)
            WriteToLog(message,LogLevel.Info);
    }       
    
    public static void Warning(string message, bool writeToLog)
    {
        LogEvent(message, LogLevel.Warning);
    }

    public static void Error(string message, bool writeToLog, string exceptionMessage)
    {
        LogEvent(message, LogLevel.Error);
        if (writeToLog)
            WriteToLog(exceptionMessage, LogLevel.Info);
    }

    public static void WriteToLog(string message, LogLevel level)
    {
        var logMessage = $"{TimeStamp(DateTime.Now)} - {level} : {message}";

        string dateStamp = DateTime.Now.ToString("yyyy-MM-dd");
        string logFilePath = Path.Combine("Log", $"{dateStamp}.log"); // Define the log file path with date stamp


        try
        {
            Directory.CreateDirectory("Log");

            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (IOException ex)
        {
            LogEvent($"Failed to write log: {ex.Message}", LogLevel.Error);
            //LOL...
        }
    }


    public static string TimeStamp(DateTime time)
    {
        return time.ToShortTimeString();
    }

    public enum LogLevel
    {
        Info,
        Error,
        Warning,
        Debug,
        Trace

    }

    
}
