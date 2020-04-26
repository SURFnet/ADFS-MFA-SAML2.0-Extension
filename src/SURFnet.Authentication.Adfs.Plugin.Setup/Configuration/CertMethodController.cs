using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class CertMethodController : ShowListGetDigit
    {

        static readonly OptionList CertOptions = new OptionList()
        {
            Introduction = "The SFO MFA extention needs a certificate to sign its SAML2 messages",
            Options = new string[]
            {
                "  1. Select an existing certificate in the store",
                "  2. Import a certificate from a '.PFX' file",
                "  3. Create a Self Signed certificate"
            },
            Question = "How do you want to select a certificate"
        };

        public CertMethodController() : base(CertOptions, 1)
        {
        }
    }
}
