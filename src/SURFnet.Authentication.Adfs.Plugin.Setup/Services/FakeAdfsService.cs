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

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System;

    using SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces;

    /// <summary>
    /// Class FakeAdfsService.
    /// Implements the <see cref="SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces.IAdFsService" />
    /// </summary>
    /// <seealso cref="SURFnet.Authentication.Adfs.Plugin.Setup.Services.Interfaces.IAdFsService" />
    public class FakeAdfsService : IAdFsService
    {
        /// <summary>
        /// Registers the ADFS MFA extension.
        /// </summary>
        public void RegisterAdapter()
        {
            Console.WriteLine("Registered ADFS MFA Extension");
        }

        /// <summary>
        /// Unregisters the ADFS MFA extension adapter.
        /// </summary>
        public void UnregisterAdapter()
        {
            Console.WriteLine("Unregistered ADFS MFA Extension");
        }
    }
}
