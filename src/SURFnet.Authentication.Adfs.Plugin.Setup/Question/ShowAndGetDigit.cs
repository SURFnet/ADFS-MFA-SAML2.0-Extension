using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetDigit : ShowAndGetChar
    {
        /// <summary>
        /// Character (digit) based list Choice.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public ShowAndGetDigit(string question, int min, int max, bool showHelpChar) : base(question, GetValidChars(min, max), showHelpChar)
        {
        }

        /// <summary>
        /// Character (digit) based list Choice, with a default.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="defaulChoice"></param>
        public ShowAndGetDigit(string question, int min, int max, int defaulChoice, bool showHelpChar) : base(question, GetValidChars(min, max), (char)(defaulChoice+'0'), showHelpChar)
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
