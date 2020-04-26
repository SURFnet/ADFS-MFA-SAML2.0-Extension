using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class EnsureEventLog
    {
        const string Source = "ADFS Plugin";
        const string LogName = "AD FS Plugin";

        public static int Create()
        {
            int rc = -1;

            try
            {
                if (!EventLog.SourceExists(Source))
                {
                    LogService.Log.Info("Creating EventLog source: "+Source);
                    EventLog.CreateEventSource(Source, LogName);
                    int cnt = 3;
                    while ( (cnt-- > 0) && (!Exists) )
                    {
                        // MS samples say you need it.
                        // I think it was already fixed. I never needed it.
                        LogService.Log.Warn($"CreateEventSource() Sleep(200): {3-cnt}");
                        Thread.Sleep(300);
                    }
                }
                else
                {
                    LogService.Log.Info($"EventLog: '{Source}' was already present.");
                }

                rc = 0;
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("EventLog creation failed.", ex);
            }

            return rc;
        }

        public static bool Exists => EventLog.Exists(LogName);
    }
}
