using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.PS
{
    /// <summary>
    /// The idea is to work with our own defined classes and use PowerShell in string form.
    /// Never real references to ADFS assemblies. Then it should work for all versions of ADFS.
    /// Our method should (iff necessary) check OS version and limit property fetching.
    /// <para>
    /// Our own types may have only a subset of properties of the real ADFS type.
    /// Implement more properties as needed.
    /// </para> 
    /// </summary>
    public static class PSUtil
    {
        // All in: 
        // Microsoft.IdentityServer.Management.Commands |  Ref to S.Man.Automation
        //        Assembly Version     FileVersion      |  Assembly Version
        // S2012R2:   6.3.0.0           6.3.m.n         |     3.0.0.0
        // S2016:    10.0.0.0          10.0.y.n         |     3.0.0.0
        // S2019:    10.0.0.0          10.0.z.n         |     3.0.0.0
        //
        // Athough the AssemblyVersions are equal, it requires separate compilation because the
        // interfaces and enums are not the same. Officially the Assembly Versions should be different then too.

        // Get-AdfsProperties
        // S2012R2: GetServicePropertiesCommand : Cmdlet
        // S2016:  GetServicePropertiesCommand : PSCmdletBase
        // S2019:  GetServicePropertiesCommand : PSCmdletBase
        // This is just the ServiceProperties.
        // ServiceSettingsData is another set of configuration data

        // Get-AdfsSyncProperties
        // S2012R2: GetSyncPropertiesCommand : Cmdlet
        // S2016:  GetSyncPropertiesCommand : PSCmdletBase
        // S2019:  GetSyncPropertiesCommand : PSCmdletBase

        // Get-AdfsAuthenticationProvider
        // S2012R2: GetAuthProviderPropertiesCommand : Cmdlet
        // S2016:  GetAuthProviderPropertiesCommand : Cmdlet
        // S2019:  GetAuthProviderPropertiesCommand : Cmdlet

        // Register-AdfsAuthenticationProvider
        // S2012R2: AddExternalAuthProviderCommand : PSCmdlet
        // S2016:  AddExternalAuthProviderCommand : PSCmdletBase
        // S2019:  AddExternalAuthProviderCommand : PSCmdletBase

        // UnRegister-AdfsAuthenticationProvider
        // S2012R2: RemoveExternalAuthProviderCommand : Cmdlet
        // S2016:  RemoveExternalAuthProviderCommand : PSCmdletBase
        // S2019:  RemoveExternalAuthProviderCommand : PSCmdletBase

        // Get-AdfsGlobalAuthenticationPolicy
        // S2012R2: GetGlobalAuthenticationPolicyCommand : Cmdlet
        // S2016:  GetGlobalAuthenticationPolicyCommand : PSCmdletBase
        // S2019:  GetGlobalAuthenticationPolicyCommand : PSCmdletBase

        // Set-AdfsGlobalAuthenticationPolicy
        // S2012R2: SetGlobalAuthenticationPolicyCommand : Cmdlet
        // S2016:  SetGlobalAuthenticationPolicyCommand : PSCmdletBase
        // S2019:  SetGlobalAuthenticationPolicyCommand : PSCmdletBase

        // Import-AdfsAuthenticationProviderConfigurationData
        // S2012R2: ImportAuthProviderConfigurationData : PSCmdletBase
        // S2016:  ImportAuthProviderConfigurationData : PSCmdletBase
        // S2019:  ImportAuthProviderConfigurationData : PSCmdletBase

        // Export-AdfsAuthenticationProviderConfigurationData
        // S2012R2: ExportAuthProviderConfigurationData : PSCmdletBase
        // S2016:  ExportAuthProviderConfigurationData : PSCmdletBase
        // S2019:  ExportAuthProviderConfigurationData : PSCmdletBase

        /// <summary>
        /// PSObject extensions.
        /// Not sure what the PSMemberInfoCollection does with the string indexer
        /// on missing property. Therefor always try-catch. Should only happen in tests.
        /// </summary>

        public static bool TryGetPropertyValue<T>(this PSObject psobj, string name, out T value) where T : class
        {
            if (psobj == null)
            {
                throw new ArgumentNullException(nameof(psobj));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), "IsNullOrWhiteSpace");
            }

            var rc = false;
            value = null;

            try
            {
                var tmp = psobj.Properties[name];
                value = (T)tmp.Value;
                rc = true;
            }
            catch (Exception)
            {
                value = null;
            }

            return rc;
        }

        static public bool TryGetPropertyString(this PSObject psobj, string name, out string value)
        {
            if (psobj == null) throw new ArgumentNullException(nameof(psobj));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException(nameof(name), "IsNullOrWhiteSpace");

            bool rc = false;
            value = string.Empty;

            try
            {
                var tmp = psobj.Properties[name];
                value = (string)tmp.Value;
                rc = true;
            }
            catch (Exception)
            {
                value = string.Empty;
            }

            return rc;
        }

        static public bool TryGetPropertyInt(this PSObject psobj, string name, out int value)
        {
            if (psobj == null) throw new ArgumentNullException(nameof(psobj));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException(nameof(name), "IsNullOrWhiteSpace");

            bool rc = false;
            value = 0;

            try
            {
                var tmp = psobj.Properties[name];
                value = (int)tmp.Value;
                rc = true;
            }
            catch (Exception)
            {
                value = 0;
            }

            return rc;
        }
    }
}
