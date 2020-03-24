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
    using System.ServiceProcess;
    using System.Threading;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class AdFsServer.
    /// </summary>
    public static class AdfsServer
    {
        private const int DefaultRetries = 29;
        private const int DefaultSleepMs = 1000;
        private const string DefaultName = "adfssrv";

        /// <summary>
        /// Defines the maximum retries to start and stop the service
        /// before we stop waiting for the action result.
        /// </summary>
        private static int MaxRetries = DefaultRetries;
        
        /// <summary>
        /// The sleep time in ms between retries.
        /// </summary>
        private static int SleepMs = DefaultSleepMs;

        private static string ServiceName = DefaultName;

        public static ServiceController SvcController { get; private set; } = null;

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
        /// Stops the ADFS service.
        /// </summary>
        public static int StopAdFsService()
        {
            if (SvcController == null)
                return -1;

            Console.Write("Stopping ADFS service");
            int rc = StopAdFsServiceInternal();
            Console.WriteLine();
            Console.WriteLine("Stopped ADFS service");

            return rc;
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
        /// <param name="failsave">The failsave.</param>
        private static int StartAdFsServiceInternal()
        {
            int rc = -1;
            bool more = true;
            int retriesLeft = MaxRetries;

            try
            {
                while (more)
                {
                    SvcController.Refresh();

                    switch (SvcController.Status)
                    {
                        case ServiceControllerStatus.Paused:
                        case ServiceControllerStatus.Stopped:
                            SvcController.Start();
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

                    if (more && (--retriesLeft > 0))
                    {
                        if (0 == (retriesLeft % 3))
                        {
                            Console.Write(".");
                        }
                    }
                } // timeout loop
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to stop the ADFS service. Details: {e}");
                rc = -2;
            }

            if (rc == -1 && retriesLeft <= 0)
            {
                rc = 1; // timeout
            }

            return rc;
        }

        /// <summary>
        /// Stops the ADFS service.
        /// </summary>
        /// <param name="failsave">The failsave.</param>
        private static int StopAdFsServiceInternal()
        {
            int rc = -1;
            bool more = true;
            int retriesLeft = MaxRetries;

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

                    if ( more && (--retriesLeft>0) )
                    {
                        if ( 0==(retriesLeft%3) )
                        {
                            Console.Write(".");
                        }
                    }
                } // timeout loop
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to stop the ADFS service. Details: {e}");
                rc = -2;
            }

            if ( rc==-1 && retriesLeft<=0 )
            {
                rc = 1; // timeout
            }

            return rc;
        }

        /// <summary>
        /// Gets the ad fs service.
        /// </summary>
        /// <returns><see cref="ServiceController"/>.</returns>
        private static ServiceController CheckAdFsService()
        {
            SvcController = null;

            try
            {
                SvcController = new ServiceController("adfssrv");
            }
            catch (ArgumentException)
            {
                LogService.Log.Fatal("No ADFS service on this machine");
            }
            catch (Exception ex)
            {
                LogService.Log.Fatal(ex.ToString());
            }

            return SvcController;
        }
    }
}
