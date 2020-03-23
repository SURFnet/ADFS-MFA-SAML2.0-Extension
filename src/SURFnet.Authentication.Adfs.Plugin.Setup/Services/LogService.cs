using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    /// <summary>
    /// Static logging class wrapper for log4net.
    /// Static property dependency insertion to replace the 'dummy' logger with a
    /// real log4net instance;
    /// </summary>
    public static class LogService
    {
        /// <summary>
        /// Do nothing ILog implementation.
        /// To avoid null pointer problems if accidentally used without a log4net instance.
        /// Yes, the dreadful silent failures for those who do not read the doc.
        /// </summary>
        private class DummyLogger : ILog
        {
            public bool IsDebugEnabled { get; } = false;
            public bool IsInfoEnabled { get; } = false;
            public bool IsWarnEnabled { get; } = false;
            public bool IsErrorEnabled { get; } = false;
            public bool IsFatalEnabled { get; } = false;

            public ILogger Logger => throw new NotImplementedException();

            public void Debug(object message) { return; }
            public void Debug(object message, Exception exception) { return; }
            public void DebugFormat(string format, params object[] args) { return; }
            public void DebugFormat(string format, object arg0) { return; }
            public void DebugFormat(string format, object arg0, object arg1) { return; }
            public void DebugFormat(string format, object arg0, object arg1, object arg2) { return; }
            public void DebugFormat(IFormatProvider provider, string format, params object[] args) { return; }
            public void Error(object message) { return; }
            public void Error(object message, Exception exception) { return; }
            public void ErrorFormat(string format, params object[] args) { return; }
            public void ErrorFormat(string format, object arg0) { return; }
            public void ErrorFormat(string format, object arg0, object arg1) { return; }
            public void ErrorFormat(string format, object arg0, object arg1, object arg2) { return; }
            public void ErrorFormat(IFormatProvider provider, string format, params object[] args) { return; }
            public void Fatal(object message) { return; }
            public void Fatal(object message, Exception exception) { return; }
            public void FatalFormat(string format, params object[] args) { return; }
            public void FatalFormat(string format, object arg0) { return; }
            public void FatalFormat(string format, object arg0, object arg1) { return; }
            public void FatalFormat(string format, object arg0, object arg1, object arg2) { return; }
            public void FatalFormat(IFormatProvider provider, string format, params object[] args) { return; }
            public void Info(object message) { return; }
            public void Info(object message, Exception exception) { return; }
            public void InfoFormat(string format, params object[] args) { return; }
            public void InfoFormat(string format, object arg0) { return; }
            public void InfoFormat(string format, object arg0, object arg1) { return; }
            public void InfoFormat(string format, object arg0, object arg1, object arg2) { return; }
            public void InfoFormat(IFormatProvider provider, string format, params object[] args) { return; }
            public void Warn(object message) { return; }
            public void Warn(object message, Exception exception) { return; }
            public void WarnFormat(string format, params object[] args) { return; }
            public void WarnFormat(string format, object arg0) { return; }
            public void WarnFormat(string format, object arg0, object arg1) { return; }
            public void WarnFormat(string format, object arg0, object arg1, object arg2) { return; }
            public void WarnFormat(IFormatProvider provider, string format, params object[] args) { return; }
        }

        public static ILog Log { get; private set; } = new DummyLogger();

        public static void InsertLoggerDependency(ILog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            Log = log;
        }

        public static void SetLevel(Level level)
        {
            //int tstlevel = Level.Debug.Value;
            //if ( level.Value >= Level.Debug.Value)
            var test = Log as log4net.Repository.Hierarchy.Logger;
            if ( test != null )
            {
                // ((log4net.Repository.Hierarchy.Logger)Log).Level = level;
                // See stackoverflow: 650694
                test.Level = level;
            }
            else
            {
                throw new ApplicationException($"Setting the log level on this class failed: {Log.GetType().FullName}");
            }
        }
    }
}
