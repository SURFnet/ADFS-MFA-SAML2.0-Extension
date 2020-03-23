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
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class AdFsServer.
    /// </summary>
    public class AdfsServer
    {
        /// <summary>
        /// Defines the maximum retries to start and stop the service before we cancel the setup
        /// </summary>
        private const int MaxRetries = 15;
        
        /// <summary>
        /// The sleep time in ms.
        /// </summary>
        private const int SleepMs = 1000;

        /// <summary>
        /// The ADFS service.
        /// </summary>
        private readonly IAdfsPSService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdfsServer"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public AdfsServer(IAdfsPSService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Stops the ADFS service.
        /// </summary>
        public void StopAdFsService()
        {
            Console.Write("Stopping ADFS service");
            this.StopAdFsServiceInternal();
            Console.WriteLine();
            Console.WriteLine("Stopped ADFS service");
        }

        /// <summary>
        /// Starts the ADFS service.
        /// </summary>
        public void StartAdFsService()
        {
            Console.Write("Starting ADFS service");
            this.StartAdFsServiceInternal();
            Console.WriteLine();
            Console.WriteLine("Started ADFS service");
        }

        /// <summary>
        /// Unregisters the old plugin.
        /// </summary>
        public void UnregisterAdapter()
        {
            this.service.UnregisterAdapter();
        }


        /// <summary>
        /// Registers the new plugin and refreshes the plugin metadata.
        /// </summary>
        public void RegisterAdapter()
        {
            this.service.RegisterAdapter();
        }

        /// <summary>
        /// Starts the ADFS service.
        /// </summary>
        /// <param name="failsave">The failsave.</param>
        private void StartAdFsServiceInternal(int failsave = 0)
        {
            if (failsave > MaxRetries)
            {
                throw new Exception("Reached max retries to start the ADFS service");
            }

            // TODO: use Refresh()!!
            var service = this.GetAdFsService();
            if (service.Status.Equals(ServiceControllerStatus.StopPending))
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(SleepMs);
                this.StartAdFsServiceInternal(failsave + 1);
                return;
            }

            try
            {
                if (service.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    service.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to start the ADFS service. Details: {e}");
            }
        }

        /// <summary>
        /// Stops the ADFS service.
        /// </summary>
        /// <param name="failsave">The failsave.</param>
        private void StopAdFsServiceInternal(int failsave = 0)
        {
            if (failsave > MaxRetries)
            {
                throw new Exception("Reached max retries to stop the ADFS service");
            }

            // TODO: use Refresh()!!
            var service = this.GetAdFsService();
            if (service.Status.Equals(ServiceControllerStatus.StartPending))
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(SleepMs);
                this.StopAdFsServiceInternal(failsave + 1);
                return;
            }

            try
            {
                if (service.Status.Equals(ServiceControllerStatus.Running))
                {
                    service.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to stop the ADFS service. Details: {e}");
            }
        }

        /// <summary>
        /// Gets the ad fs service.
        /// </summary>
        /// <returns><see cref="ServiceController"/>.</returns>
        private ServiceController GetAdFsService()
        {
            ServiceController rc = null;

            try
            {
                rc = new ServiceController("adfssrv");
            }
            catch (ArgumentException)
            {
                LogService.Log.Fatal("No ADFS service on this machine");
            }
            catch (Exception ex)
            {
                LogService.Log.Fatal(ex.ToString());
            }
            return rc;
        }
    }
}
