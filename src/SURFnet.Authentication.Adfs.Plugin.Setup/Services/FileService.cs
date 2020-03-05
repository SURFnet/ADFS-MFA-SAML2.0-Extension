using System;
using System.Collections.Generic;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Services
{
    using System.IO;
    using System.Xml.Linq;

    public class FileService
    {
        /// <summary>
        /// The adfs directory.
        /// </summary>
        private string adfsDir;

        public FileService()
        {
            //todo: remove after testing
#if DEBUG
            this.adfsDir = Path.GetDirectoryName(@"c:\temp\data\");
#else
            this.adfsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "ADFS");
#endif
        }

        public void CopyAssembliesToOutput()
        {
            
        }

        public void CopyOutputToAdFsDirectory()
        {
            //set new assemblies in adfs dir. First config, than assembly

        }

        /// <summary>
        /// Loads the ad fs configuration file.
        /// </summary>
        /// <returns>The ADFS configuration file.</returns>
        public XDocument LoadAdFsConfigurationFile()
        {
            var document = XDocument.Load($"{this.adfsDir}/Microsoft.IdentityServer.Servicehost.exe.config");
            return document;
        }
    }
}
