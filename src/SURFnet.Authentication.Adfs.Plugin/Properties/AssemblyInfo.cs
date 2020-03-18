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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SURFnet.Authentication.Adfs.Plugin.Setup.Common;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SURFnet.Authentication.Adfs.Plugin")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SURFnet bv")]
[assembly: AssemblyProduct("SURFnet.Authentication.Adfs.Plugin")]
[assembly: AssemblyCopyright("Copyright ©  2017-2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("neutral")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7f165692-9e1e-4231-b3bf-3e3aed44b11a")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.1.0")]
// Shows as "Product version" in the File Explorer
[assembly: AssemblyInformationalVersion(Values.ProductVersion)]
// Shows as "File version" in the File Explorer
[assembly: AssemblyFileVersion(Values.FileVersion)]

// 
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "SURFnet.Authentication.ADFS.MFA.Plugin.log4net", Watch = true)]