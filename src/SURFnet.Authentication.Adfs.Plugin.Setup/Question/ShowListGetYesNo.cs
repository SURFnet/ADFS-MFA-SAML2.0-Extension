namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowListGetYesNo : ShowAndGetYesNo
    {
        private readonly OptionList OptionList;

        public ShowListGetYesNo(OptionList list) : base(list.Question, 'y')
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
