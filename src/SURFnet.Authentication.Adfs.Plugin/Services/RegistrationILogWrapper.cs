using System;
using System.Globalization;

using log4net;
using log4net.Core;

namespace SURFnet.Authentication.Adfs.Plugin.Services
{
    public class RegistrationILogWrapper : ILog
    {
        bool ILog.IsDebugEnabled => true;

        bool ILog.IsInfoEnabled => true;

        bool ILog.IsWarnEnabled => true;

        bool ILog.IsErrorEnabled => true;

        bool ILog.IsFatalEnabled => true;

        ILogger ILoggerWrapper.Logger => throw new NotImplementedException();

        private void RegistrationLogWriteLine(string verb, object message)
        {
            RegistrationLog.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", verb, message));
        }

        private void RegistrationLogWriteLine(string verb, object message, Exception exception)
        {
            RegistrationLog.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1} Exception: {2}", verb, message, exception));
        }

        private void RegistrationLogWriteLine(string verb, string format, params object[] args)
        {
            RegistrationLogWriteLine(CultureInfo.InvariantCulture, verb, format, args);
        }

        private void RegistrationLogWriteLine(IFormatProvider provider, string verb, string format, params object[] args)
        {
            var message = string.Format(provider, format, args);
            RegistrationLog.WriteLine(string.Format(provider, "{0}: {1}", verb, message));
        }

        void ILog.Debug(object message)
        {
            RegistrationLogWriteLine("Debug", message);
        }

        void ILog.Debug(object message, Exception exception)
        {
            RegistrationLogWriteLine("Debug", message, exception);
        }

        void ILog.DebugFormat(string format, params object[] args)
        {
            RegistrationLogWriteLine("Debug", format, args);
        }

        void ILog.DebugFormat(string format, object arg0)
        {
            RegistrationLogWriteLine("Debug", format, arg0);
        }

        void ILog.DebugFormat(string format, object arg0, object arg1)
        {
            RegistrationLogWriteLine("Debug", format, arg0, arg1);
        }

        void ILog.DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            RegistrationLogWriteLine("Debug", format, arg0, arg1, arg2);
        }

        void ILog.DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            RegistrationLogWriteLine(provider, "Debug", format, args);
        }

        void ILog.Error(object message)
        {
            RegistrationLogWriteLine("Error", message);
        }

        void ILog.Error(object message, Exception exception)
        {
            RegistrationLogWriteLine("Error", message, exception);
        }

        void ILog.ErrorFormat(string format, params object[] args)
        {
            RegistrationLogWriteLine("Error", format, args);
        }

        void ILog.ErrorFormat(string format, object arg0)
        {
            RegistrationLogWriteLine("Error", format, arg0);
        }

        void ILog.ErrorFormat(string format, object arg0, object arg1)
        {
            RegistrationLogWriteLine("Error", format, arg0, arg1);
        }

        void ILog.ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            RegistrationLogWriteLine("Error", format, arg0, arg1, arg2);
        }

        void ILog.ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            RegistrationLogWriteLine(provider, "Error", format, args);
        }

        void ILog.Fatal(object message)
        {
            RegistrationLogWriteLine("Fatal", message);
        }

        void ILog.Fatal(object message, Exception exception)
        {
            RegistrationLogWriteLine("Fatal", message, exception);
        }

        void ILog.FatalFormat(string format, params object[] args)
        {
            RegistrationLogWriteLine("Fatal", format, args);
        }

        void ILog.FatalFormat(string format, object arg0)
        {
            RegistrationLogWriteLine("Fatal", format, arg0);
        }

        void ILog.FatalFormat(string format, object arg0, object arg1)
        {
            RegistrationLogWriteLine("Fatal", format, arg0, arg1);
        }

        void ILog.FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            RegistrationLogWriteLine("Fatal", format, arg0, arg1, arg2);
        }

        void ILog.FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            RegistrationLogWriteLine(provider, "Fatal", format, args);
        }

        void ILog.Info(object message)
        {
            RegistrationLogWriteLine("Info", message);
        }

        void ILog.Info(object message, Exception exception)
        {
            RegistrationLogWriteLine("Info", message, exception);
        }

        void ILog.InfoFormat(string format, params object[] args)
        {
            RegistrationLogWriteLine("Info", format, args);
        }

        void ILog.InfoFormat(string format, object arg0)
        {
            RegistrationLogWriteLine("Info", format, arg0);
        }

        void ILog.InfoFormat(string format, object arg0, object arg1)
        {
            RegistrationLogWriteLine("Info", format, arg0, arg1);
        }

        void ILog.InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            RegistrationLogWriteLine("Info", format, arg0, arg1, arg2);
        }

        void ILog.InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            RegistrationLogWriteLine(provider, "Info", format, args);
        }

        void ILog.Warn(object message)
        {
            RegistrationLogWriteLine("Warn", message);
        }

        void ILog.Warn(object message, Exception exception)
        {
            RegistrationLogWriteLine("Warn", message, exception);
        }

        void ILog.WarnFormat(string format, params object[] args)
        {
            RegistrationLogWriteLine("Warn", format, args);
        }

        void ILog.WarnFormat(string format, object arg0)
        {
            RegistrationLogWriteLine("Warn", format, arg0);
        }

        void ILog.WarnFormat(string format, object arg0, object arg1)
        {
            RegistrationLogWriteLine("Warn", format, arg0, arg1);
        }

        void ILog.WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            RegistrationLogWriteLine("Warn", format, arg0, arg1, arg2);
        }

        void ILog.WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            RegistrationLogWriteLine(provider, "Warn", format, args);
        }
    }
}
