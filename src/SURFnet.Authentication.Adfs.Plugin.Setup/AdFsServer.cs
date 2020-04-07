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
        public static ServiceController CheckAdFsService()
        {
            SvcController = null;

            try
            {
                ServiceController tmp = new ServiceController("adfssrv");

                var status = tmp.Status; // trigger exception if not on machine.
                if ( status != ServiceControllerStatus.Running )
                {
                    LogService.WriteFatal("ADFS service not running, cannot analyze/setup. Start the service.");
                }
                else
                {
                    SvcController = tmp;
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
                LogService.WriteFatalException("Trying to get a ServiceController threw an unexpectedexception!", ex);
            }

            return SvcController;
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
                LogService.WriteFatalException("Failed to stop the ADFS service.", ex);
                rc = -2;
            }

            if (rc == -1 && retriesLeft <= 0)
            {
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
                rc = 1; // timeout
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
