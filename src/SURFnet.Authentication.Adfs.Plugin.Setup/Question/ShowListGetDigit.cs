using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowListGetDigit : ShowAndGetDigit
    {
        private readonly OptionList OptionList;

        public ShowListGetDigit(OptionList list) : base(list.Question, 1, list.Options.Length)
        {
            OptionList = list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if OK</returns>
        public override bool Ask()
        {
            QuestionIO.WriteIntro(OptionList.Introduction);
            QuestionIO.WriteOptions(OptionList.Options);

            bool rc = base.Ask();

            QuestionIO.WriteEndSeparator();
            return rc;
        }
    }
}
