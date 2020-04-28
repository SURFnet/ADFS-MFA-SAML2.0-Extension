/*
* Copyright 2017 SURFnet bv, The Netherlands
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    using System;
    using System.Management;
    using System.ServiceProcess;
    using System.Threading;
    using Microsoft.Win32;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Versions;

    /// <summary>
    /// Controls the ADFS server background service: Start/Stop etc.
    /// </summary>
    public static class AdfsServer
    {
        private const int DefaultRetries = 29;
        private const int DefaultSleepMs = 1000;
        private const string DefaultAdfsSvcName = "adfssrv";



        /// <summary>
        /// Gets the ADFS service controller.
        /// </summary>
        /// <returns><see cref="ServiceController"/>null on error</returns>
        public static ServiceController CheckAdFsService(out Version adfsProductVersion)
        {
            SvcController = null;

            adfsProductVersion = GetAdfsProductVersion();
            if ( adfsProductVersion.Major == 0 )
            {
                // No need to check the rest...
                // Avoiding the silly exceptions in 99 out 0f 100?
                LogService.Log.Info("No AdfsAssembly in ADFS directory");
                return (SvcController);
            }

            try
            {
                ServiceController tmpController = new ServiceController("adfssrv");

                var status = tmpController.Status; // trigger exception if not on machine.
                if ( status != ServiceControllerStatus.Running )
                {
                    LogService.WriteFatal("ADFS service not running, cannot analyze/setup. Start the service.");
                }
                else
                {
                    SvcController = tmpController;
                    if ( AdfsAccount == null )
                    {
                        LogService.WriteFatal("Something failed when looking for the service account name of the ADFS server.");
                        return null;
                    }
                }
            }
            catch (InvalidOperationException ex1)
            {
                LogService.WriteFatalException("No ADFS on this machine.", ex1);
            }
            catch (ArgumentException ex2)
            {
                // Why is this here? According to DOC, this never happens!
                LogService.WriteFatalException("Invalid name for ADFS service on this machine.", ex2);
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Trying to get a ServiceController threw an unexpected exception!", ex);
            }

            return SvcController;
        }

        private static Version GetAdfsProductVersion()
        {
            // This is some dreadful mess. The Windows file explorer displays a different FileVersion
            // then my AssemblySpec!! Looking at the exact values it is some ADFS madness??
            // So we take the Product version which looks like less nonsense. Or is it my bug? (PL)

            Version rc = V0Assemblies.AssemblyNullVersion;

            string adfsPath = FileService.OurDirCombine(FileDirectory.AdfsDir, SetupConstants.AdfsFilename);
            AssemblySpec adfsAssembly = AssemblySpec.GetAssemblySpec(adfsPath);
            if ( adfsAssembly!=null )
            {
                LogService.Log.Info($"ADFS FileVersion: {adfsAssembly.FileVersion.ToString()}");
                LogService.Log.Info($"ADFS ProductVersion: {adfsAssembly.ProductVersion.ToString()}");
                LogService.Log.Info($"ADFS AssemblyVersion: {adfsAssembly.AssemblyVersion.ToString()}");

                rc = adfsAssembly.ProductVersion;
            }

            return rc;
        }

        public static int StopAdfsIfRunning()
        {
            int rc = 0;

            if (IsAdfsRunning())
            {
                if (0 != StopAdFsService())
                {
                    LogService.WriteFatal("Failed to stop ADFS service.");
                    rc = 1;
                }
            }

            return rc;
        }

        ///
        /// The polling is theoretically not 100% correct.
        /// The official algorithm is more complex. If the machine
        /// is not under severe stress, then this will work.
        ///

        /// <summary>
        /// Defines the maximum retries to start and stop the service
        /// before we stop waiting for the action result.
        /// </summary>
        private static int MaxRetries = DefaultRetries;
        
        /// <summary>
        /// The sleep time in ms between retries.
        /// </summary>
        private static int SleepMs = DefaultSleepMs;

        private static string ServiceName = DefaultAdfsSvcName;

        /// <summary>
        /// Get Available for the service account, which must have rights for the private key.
        /// </summary>
        public static ServiceController SvcController { get; private set; } = null;

        /// <summary>
        /// This is really ridiculous, getting it from the Registry seems far simpler.....
        /// Queries, iterators within iterators just to say: "Hi mom".
        /// </summary>
        public static string AdfsAccount
        {
            get
            {
                // I hate "one liners". :-), LF solves it?
                var obj = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                    ?.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\adfssrv")
                                    ?.GetValue("ObjectName");
                if (obj is string accountName)
                    LogService.Log.Info("Registry says --- ADFS svc account: " + accountName as string);

                string result = null;
                string query = $"select * from Win32_Service where name like '{DefaultAdfsSvcName}'";

                ManagementObjectSearcher windowsServicesSearcher = new ManagementObjectSearcher("root\\cimv2", query);
                ManagementObjectCollection objectCollection = windowsServicesSearcher.Get();

                if ( (objectCollection!=null)  && (objectCollection.Count>0) )
                {
                    // yes, must iterate.....
                    foreach ( var svcobj in objectCollection )
                    {
                        var props = svcobj.Properties;
                        foreach ( var prop in props )
                        {
                            if (prop.Name.Equals("StartName", StringComparison.OrdinalIgnoreCase))
                            {
                                result = prop.Value.ToString();
                            }
                        }
                    }
                }

                if (result != null)
                    LogService.Log.Info("WMI says --- ADFS svc account: " + result);

                return result;

            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdfsServer"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        static void Configure(string name, int retries = DefaultRetries, int sleepMs = DefaultSleepMs)
        {
            ServiceName = name;
            MaxRetries = retries;
            SleepMs = sleepMs;
            if ( SvcController != null )
            {
                // reconfiguration
                try
                {
                    SvcController.Dispose();
                }
                finally
                {
                    SvcController = null;
                }
            }
        }

        /// <summary>
        /// Starts the ADFS service.
        /// </summary>
        public static int StartAdFsService()
        {
            Console.Write("Starting ADFS service");
            int rc = StartAdFsServiceInternal();
            Console.WriteLine();
            Console.WriteLine("Started ADFS service");
            return rc;
        }

        /// <summary>
        /// Starts the ADFS service.
        /// </summary>
        private static int StartAdFsServiceInternal()
        {
            int rc = -1;
            bool more = true;
            int retriesLeft = MaxRetries;
            LogService.Log.Info("Starting ADFS service.");

            try
            {
                while (more)
                {
                    SvcController.Refresh();

                    switch (SvcController.Status)
                    {
                        case ServiceControllerStatus.Paused:
                        case ServiceControllerStatus.Stopped:
                            SvcController.Start();  // must wait? do not thinks so...
                            Console.Write(".");
                            break;

                        case ServiceControllerStatus.Running:
                            rc = 0;
                            more = false;
                            break;

                        default:
                            // All pending states
                            Thread.Sleep(SleepMs);
                            retriesLeft--;
                            break;
                    }

                    // print once it 3 polls
                    if (more && (--retriesLeft > 0))
                    {
                        if (0 == (retriesLeft % 3))
                        {
                            Console.Write(".");
                        }
                    }
                } // timeout loop
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Failed to start the ADFS service.", ex);
                rc = -2;
            }

            if (rc == -1 && retriesLeft <= 0)
            {
                LogService.Log.Error("Start ADFS Service timeout.");
                rc = 1; // timeout
            }

            return rc;
        }

        /// <summary>
        /// Tries to Stop the ADFS server and writes activity to the Console
        /// </summary>
        /// <returns>0 if OK</returns>
        public static int StopAdFsService()
        {
            if (SvcController == null)
                return -1;

            Console.Write("Stopping ADFS service");
            int rc = StopAdFsServiceInternal();
            Console.WriteLine();
            if ( rc == 0 )
            {
                // TODO, create better test if ADFS service really released its files.
                // Start looking with: https://stackoverflow.com/questions/3790770/how-can-i-free-up-a-dll-that-is-referred-to-by-an-exe-that-isnt-running
                //
                Console.Write("Sleeping....");
                Thread.Sleep(3000);
                Console.WriteLine("\r                 \r");

                Console.WriteLine("Stopped ADFS service");
            }
            else
            {
                LogService.WriteFatal("Stopping the ADFS service failed.");
            }

            return rc;
        }



        /// <summary>
        /// Tries to get the service into the Stopped state.
        /// </summary>
        /// <returns>0 if OK</returns>
        private static int StopAdFsServiceInternal()
        {
            int rc = -1;
            bool more = true;
            int retriesLeft = MaxRetries;
            LogService.Log.Info("Stopping ADFS service.");

            try
            {
                while ( more )
                {
                    SvcController.Refresh();

                    switch (SvcController.Status)
                    {
                        case ServiceControllerStatus.Stopped:
                            rc = 0;
                            more = false;
                            break;

                        case ServiceControllerStatus.Paused:
                        case ServiceControllerStatus.Running:
                            SvcController.Stop();
                            Console.Write(".");
                            break;

                        default:
                            // All pending states
                            Thread.Sleep(SleepMs);
                            retriesLeft--;
                            break;
                    }

                    // Write dots once in the three polls
                    if ( more && (--retriesLeft>0) )
                    {
                        if ( 0==(retriesLeft%3) )
                        {
                            Console.Write(".");
                        }
                    }
                } // timeout loop
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Failed to stop the ADFS service. Details: ", ex);
                rc = -2;
            }

            if ( rc==-1 && retriesLeft<=0 )
            {
                LogService.Log.Error("Stop ADFS Service timeout.");
                rc = 1; // timeout
            }

            return rc;
        }

        public static int RestartAdFsService()
        {
            LogService.Log.Info("Restarting ADFS");
            int rc = StopAdFsService();
            if ( rc == 0 )
            {
                rc = StartAdFsService();
            }
            return rc;
        }

        public static bool IsAdfsRunning()
        {
            bool rc = false;

            try
            {
                if (SvcController != null)
                {
                    SvcController.Refresh();
                    if (SvcController.Status == ServiceControllerStatus.Running)
                        rc = true;
                }
            }
            catch (Exception )
            { } //silently discard...

            return rc;
        }
    }
}
