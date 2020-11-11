
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

            //Test_TryGetNameIDValue(getNameId, "ToDo");
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
    
        private void Test_TryGetNameIDValue(IGetNameID getNameId, string expectedName)
        {
            var domain = System.Environment.UserDomainName;
            var userName = System.Environment.UserName;
            var domainUserName = string.Format(@"{0}\{1}", domain, userName);
            var claim = new Claim(ClaimTypes.WindowsAccountName, domainUserName);

            string nameId = null;
            if (getNameId.TryGetNameIDValue(claim, out nameId))
            {
                Assert.AreEqual(expectedName, nameId);
            }
            else
            {
                Assert.Fail("NameId could not be resolved");
            }
        }
    }
}
