using System.Diagnostics;

namespace MauiLab.Services.Logging
{
    internal class DebugLogger : ILog
    {
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Error(string message)
        {
            Trace.WriteLine($"ERROR: {message}");
        }

        public void Error(Exception ex)
        {
            Error(ex.Message);
            System.Diagnostics.Debug.WriteLine(ex);
        }

        public void Info(string message)
        {
            Trace.WriteLine(message);
        }

        public void Warn(string message)
        {
            Trace.WriteLine($"WARNING: {message}");
        }
    }
}
