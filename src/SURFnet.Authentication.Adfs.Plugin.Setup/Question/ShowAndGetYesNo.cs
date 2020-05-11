using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetYesNo : ShowAndGetChar
    {
        public ShowAndGetYesNo(string question) : base(question, "yn")
        {
        }
        public ShowAndGetYesNo(string question, char defaultChoice) : base(question, "yn", defaultChoice)
        {
        }

    }
}
