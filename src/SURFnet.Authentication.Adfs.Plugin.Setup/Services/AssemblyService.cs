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

using System;
using System.Collections.Generic;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    public class AssemblyService
    {

        public void RemoveAssembliesFromGac()
        {
            //remove old assemblies from gac <- depend on installed plugin version (1.0)
            /*
                a.	If remove adapter 1.0.1 fails, stop everything
                b.	If remove Kentor fails, stop everything. Who has reused/changed our signed Kentor??? Newer versions use Sustain.sys!!
                c.	If remove log4net fails: leave it as is, warn and go on.

             */

        }
    }
}
