using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NLog;
using System.Reflection;

namespace JwtOAuth2Server
{
    public class NLogTraceListener : TraceListener
    {
        private Logger _logger;
        private bool _enableConsoleOutput = false;

        public NLogTraceListener(bool enableConsoleOutput)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _enableConsoleOutput = enableConsoleOutput;
        }

        public override void Write(string message)
        {
            string writeMsg = $"{GetCaller()}:\r\n{message}";
            _logger.Trace(writeMsg);
            if (_enableConsoleOutput)
                System.Console.WriteLine($"\r\n{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")} {writeMsg}");
        }

        public override void WriteLine(string message)
        {
            string writeMsg = $"{GetCaller()}:\r\n{message}";
            _logger.Trace(writeMsg);
            if (_enableConsoleOutput)
                System.Console.WriteLine($"\r\n{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")} {writeMsg}");
        }

        public override void Write(string message, string category)
        {
            string writeMsg = $"{GetCaller()}:\r\n{message}";
            if (_enableConsoleOutput)
                System.Console.WriteLine($"\r\n{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")} {writeMsg}");
            WriteToNLog(writeMsg, category);
        }

        public override void WriteLine(string message, string category)
        {
            string writeMsg = $"{GetCaller()}:\r\n{message}";
            if (_enableConsoleOutput)
                System.Console.WriteLine($"\r\n{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {writeMsg}");
            WriteToNLog(writeMsg, category);
        }

        private string GetCaller()
        {
            StackTrace stackTrace;
            StackFrame target = null;
            MethodBase mi = null;
            try
            {
                stackTrace = new StackTrace(4, true);
                target = stackTrace.GetFrame(0);
                if (target == null)
                    return string.Empty;
                mi = target.GetMethod();
            }
            catch { return null; }

            return $"{mi.ReflectedType.Name}.{mi.Name}[{target.GetFileLineNumber()}]";
        }

        private void WriteToNLog(string message, string category)
        {
            switch (category)
            {
                case "Fatal":
                    _logger.Fatal(message);
                    break;
                case "Error":
                    _logger.Error(message);
                    break;
                case "Warn":
                    _logger.Warn(message);
                    break;
                case "Info":
                    _logger.Info(message);
                    break;
                case "Debug":
                    _logger.Debug(message);
                    break;
                case "Trace":
                default:
                    _logger.Trace(message);
                    break;
            }
        }
    }
}
