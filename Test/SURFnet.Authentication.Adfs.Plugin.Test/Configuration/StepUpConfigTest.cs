using System.Collections.Generic;
using System.IO;
using System.Reflection;

using log4net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using SURFnet.Authentication.Adfs.Plugin.Configuration;
using SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration;

namespace SURFnet.Authentication.Adfs.Plugin.Test.Configuration
{
    [TestClass]
    public class StepUpConfigTest
    {
        [TestMethod]
        public void BaseTest()
        {
            this.CopyFile(
                @"ConfigurationFiles\SURFnet.Authentication.ADFS.Plugin.config_base.xml",
                "SURFnet.Authentication.ADFS.Plugin.config.xml");

            var iLog = new Mock<ILog>();
            StepUpConfig.ReadXmlConfig(iLog.Object);

            Assert.IsNotNull(StepUpConfig.Current);
            Assert.AreEqual(
                "http://test.surfconext.nl/assurance/sfo-level2",
                StepUpConfig.Current.GetMinimalLoa()
                            .AbsoluteUri);
            Assert.IsTrue(StepUpConfig.Current.GetNameID.GetType() == typeof(UserIDFromADAttr));
        }

        public static IEnumerable<object[]> DynamicLoaTestCases()
        {
            // case 1
            yield return new object[]
            {
                new List<string>()
                {
                    "group1.5"
                },
                "http://test.surfconext.nl/assurance/sfo-level1.5a"
            };

            // case 2
            yield return new object[]
            {
                new List<string>()
                {
                    "group2"
                },
                "http://test.surfconext.nl/assurance/sfo-level2a"
            };

            // case 3
            yield return new object[]
            {
                new List<string>()
                {
                    "group3"
                },
                "http://test.surfconext.nl/assurance/sfo-level3a"
            };

            // case 4
            yield return new object[]
            {
                new List<string>()
                {
                    "group3",
                    "group2",
                    "group1.5"
                },
                "http://test.surfconext.nl/assurance/sfo-level3a"
            };

            // case 5
            yield return new object[]
            {
                new List<string>()
                {
                    "group2",
                    "group3",
                    "group1.5"
                },
                "http://test.surfconext.nl/assurance/sfo-level2a"
            };

            // case 5
            yield return new object[]
            {
                new List<string>()
                {
                    "group4",
                    "group5",
                    "group6"
                },
                "http://test.surfconext.nl/assurance/sfo-level2"
            };
        }

        [DataTestMethod]
        [DynamicData(nameof(DynamicLoaTestCases), DynamicDataSourceType.Method)]
        public void DynamicLoaTest(List<string> userGroups, string expectedLoa)
        {
            this.CopyFile(
                @"ConfigurationFiles\SURFnet.Authentication.ADFS.Plugin.config_dynamicLoa.xml",
                "SURFnet.Authentication.ADFS.Plugin.config.xml");
            this.CopyFile(
                @"ConfigurationFiles\SURFnet.Authentication.ADFS.Plugin.config.dynamicLoa.json",
                "SURFnet.Authentication.ADFS.Plugin.config.dynamicLoa.json");

            var iLog = new Mock<ILog>();
            StepUpConfig.ReadXmlConfig(iLog.Object);

            Assert.IsNotNull(StepUpConfig.Current);
            Assert.AreEqual(
                "http://test.surfconext.nl/assurance/sfo-level2",
                StepUpConfig.Current.GetMinimalLoa()
                            .AbsoluteUri);
            Assert.IsTrue(StepUpConfig.Current.GetNameID.GetType() == typeof(UserIDFromADAttr));

            Assert.AreEqual(
                expectedLoa,
                StepUpConfig.Current.GetMinimalLoa(
                                "user",
                                userGroups,
                                iLog.Object)
                            .AbsoluteUri);
        }

        private void CopyFile(string fromFile, string toFile)
        {
            var baseDir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly()
                        .Location);
            File.Copy(Path.Combine(baseDir, fromFile), Path.Combine(baseDir, toFile), true);
        }
    }
}