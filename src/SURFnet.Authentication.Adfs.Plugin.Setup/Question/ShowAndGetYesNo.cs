using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetYesNo : ShowAndGetChar
    {
        public ShowAndGetYesNo(string question, bool showHelpChar) : base(question, "yn", showHelpChar)
        {
        }
        public ShowAndGetYesNo(string question, char defaultChoice, bool showHelpChar) : base(question, "yn", defaultChoice, showHelpChar)
        {
        }
    }
}
