using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using log4net;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public static class ResolveNameIDType
    {
        public static IGetNameID InstantitiateNameIDType(ILog log, Dictionary<string, string> paramaters)
        {
            IGetNameID getNameID = null;

            if (paramaters.TryGetValue(AdapterConfiguration.NameIdAlgorithmAttribute, out string algorihtm))
            {
                if (algorihtm.Equals(AdapterConfiguration.UserIdFromADAttr, StringComparison.Ordinal))
                {
                    getNameID = new UserIDFromADAttr(log);
                }

                else if (algorihtm.Equals(AdapterConfiguration.UserIdAndShoFromADAttr, StringComparison.Ordinal))
                {
                    getNameID = new UserIdAndShoFromADAttr(log);
                }

                else if (algorihtm.Equals(AdapterConfiguration.NameIDFromType, StringComparison.Ordinal))
                {
                    getNameID = CreateNameIDFromType(log, paramaters, getNameID);
                }

                else
                {
                    log.Fatal($"Unkown value in adapterconfiguration for {AdapterConfiguration.NameIdAlgorithmAttribute}: {algorihtm}");
                }

                if (getNameID != null)
                {
                    getNameID.Initialize(paramaters);
                }
            }
            else
            {
                log.Fatal($"Missing '{AdapterConfiguration.NameIdAlgorithmAttribute}' in adapter configuration");
            }

            return getNameID;
        }

        private static IGetNameID CreateNameIDFromType(ILog log, Dictionary<string, string> dict, IGetNameID getNameID)
        {
            // dynamic loading
            if (dict.TryGetValue(AdapterConfiguration.GetNameIDTypeNameAttribute, out string typename))
            {
                try
                {
                    var classType = Type.GetType(typename);   // can produce a ton of exceptions!
                    try
                    {
                        var instance = Activator.CreateInstance(
                                        classType, // the type
                                        BindingFlags.Public | BindingFlags.Instance,
                                        null,
                                        new object[] { log },
                                        CultureInfo.InvariantCulture);
                        if (instance == null)
                        {
                            log.Fatal($"CreateInstance for '{typename}' returned null");
                        }
                        else
                        {
                            getNameID = instance as IGetNameID;
                            if (getNameID == null)
                            {
                                log.Fatal($"Cast to 'IGetNameID' of '{typename}' failed");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Fatal($"for '{typename}' threw:\r\n" + ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal($"Failed to load '{typename}'. {ex.ToString()}");
                }
            }
            else
            {
                log.Fatal("Missing attribute in adapter configuration: " + AdapterConfiguration.GetNameIDTypeNameAttribute);
            }

            return getNameID;
        }
    }
}
