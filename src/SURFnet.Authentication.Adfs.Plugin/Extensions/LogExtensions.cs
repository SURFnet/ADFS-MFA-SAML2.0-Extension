using System.Collections.Generic;
using log4net;

namespace SURFnet.Authentication.Adfs.Plugin.Extensions
{
    public static class LogExtensions
    {
        public static void DebugLogDictionary<T1, T2>(this ILog log, IDictionary<T1, T2> dictionary, string variableName)
        {
            foreach (var d in dictionary)
            {
                log.DebugFormat("{0}: '{1}'='{2}'", variableName, d.Key, d.Value);
            }
        }
    }
}
