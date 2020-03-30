using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetDigit : ShowAndGetChar
    {
        public ShowAndGetDigit(string question, int min, int max) : base(question, GetValidChars(min, max))
        {
        }
        public ShowAndGetDigit(string question, int min, int max, int defaulChoice) : base(question, GetValidChars(min, max), (char)(defaulChoice+'0'))
        {
        }

        public static string GetValidChars(int min, int max)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = min; i <= max; i++)
            {
                sb.Append((char)(i+'0'));
            }

            return sb.ToString();
        }
    }
}
