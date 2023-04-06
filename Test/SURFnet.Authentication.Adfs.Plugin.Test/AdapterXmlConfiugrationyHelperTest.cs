
using System.Security.Claims;

using log4net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SURFnet.Authentication.Adfs.Plugin.Helpers;
using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;

namespace SURFnet.Authentication.Adfs.Plugin.Test
{
    [TestClass]
    public class AdapterXmlConfiugrationyHelperTest
    {
        [TestMethod]
        public void Test_Classic()
        {
            ILog Log = null;
            var adapterConfigurationPath = @"ConfigurationFiles\ConfigClassic.xml";
            var getNameId = AdapterXmlConfigurationyHelper.CreateGetNameIdFromFile(adapterConfigurationPath, Log);            

            Assert.IsNotNull(getNameId);
        }

        [TestMethod]
        public void Test_BothFromAnAttribute()
        {
            ILog Log = null;
            var adapterConfigurationPath = @"ConfigurationFiles\BothFromAnAttribute.xml";
            var getNameId = AdapterXmlConfigurationyHelper.CreateGetNameIdFromFile(adapterConfigurationPath, Log);

            Assert.IsNotNull(getNameId);
        }

        [TestMethod]
        public void Test_AmcPilotSample()
        {
            ILog Log = null;
            var adapterConfigurationPath = @"ConfigurationFiles\AmcPilotSample.xml";
            var getNameId = AdapterXmlConfigurationyHelper.CreateGetNameIdFromFile(adapterConfigurationPath, Log);

            Assert.IsNotNull(getNameId);
        }
    }
}
