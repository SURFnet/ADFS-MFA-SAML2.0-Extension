using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowListGetDigit : ShowAndGetDigit
    {
        protected readonly OptionList OptionList;

        public ShowListGetDigit(OptionList list, int defaultChoice) : base(list.Question, 1, list.Options.Length, defaultChoice)
        {
            OptionList = list;
        }

        public int ChoosenIndex
        {
            get { return Digit2Index(Value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if OK</returns>
        public override bool Ask()
        {
            bool rc;

            QuestionIO.WriteIntro(OptionList.Introduction);
            QuestionIO.WriteOptions(OptionList.Options);

            rc = base.Ask();

            return rc;
        }

        ///
        /// Integer index and list ralated things.
        /// The program works with 0 based indices. The UI works with 1 based digits.
        ///

        /// <summary>
        /// Converts a '1' based choice to a 0 based index.
        /// </summary>
        /// <param name="digit"></param>
        /// <returns></returns>
        public static int Digit2Index(char digit)
        {
            int i = digit - '0' - 1;
            return i;
        }

        /// <summary>
        /// Converts a 0 based index to a '1' based choice digit.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static char Index2Digit(int index)
        {
            char digit = (char)(index + 1 + '0');
            return digit;
        }

    }
}
