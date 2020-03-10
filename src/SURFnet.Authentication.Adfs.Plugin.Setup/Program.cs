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

    using SURFnet.Authentication.Adfs.Plugin.Setup.Upgrades;

    /// <summary>
    /// Class Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting SurfNet MFA Plugin setup. Detected installed version '{VersionDetector.InstalledVersion}'");
            Console.WriteLine($"upgrading to version '{VersionDetector.NewVersion}'. Is upgrade to version 2: '{VersionDetector.IsUpgradeToVersion2()}'");

            // Currently we only support v1.0.1 to v2.x
            if (VersionDetector.IsUpgradeToVersion2())
            {
                var upgrade = new UpgradeToV2();
                upgrade.Execute();
            }

            Console.WriteLine($"Finished upgrade from version '{VersionDetector.InstalledVersion}' to '{VersionDetector.NewVersion}'");

            ConsoleWriter.WriteHeader("End of installation");
            Console.WriteLine("Type 'exit' to exit");
            while (Console.ReadLine() != "exit")
            {
                Console.WriteLine("Type 'exit' to exit");
            }

        }
    }
}
