using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetInt : ShowAndGetChar
    {
        private ShowAndGetInt(string question, string validChars) : base(question, validChars)
        {
        }
        private ShowAndGetInt(string question, string validChars, char defaultChar) : base(question, validChars, defaultChar)
        {
        }

        public static ShowAndGetInt Create(string question, int min, int max, int defaulChoice = -1)
        {
            ShowAndGetInt rc = null;
            StringBuilder sb = new StringBuilder();

            for ( int i = min; i<=max; i++ )
            {
                sb.Append(i.ToString());
            }

            if (defaulChoice >= 0)
                // with default
                rc = new ShowAndGetInt(question, sb.ToString(), (char)('0' + defaulChoice));
            else
                rc = new ShowAndGetInt(question, sb.ToString());

            return rc;
        }
    }
}
