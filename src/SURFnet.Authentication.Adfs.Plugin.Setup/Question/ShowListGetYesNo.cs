namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowListGetYesNo : ShowAndGetYesNo
    {
        private readonly OptionList OptionList;

        public ShowListGetYesNo(OptionList list, bool showHelpChar) : base(list.Question, 'y', showHelpChar)
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

            return rc;
        }
    }
}
