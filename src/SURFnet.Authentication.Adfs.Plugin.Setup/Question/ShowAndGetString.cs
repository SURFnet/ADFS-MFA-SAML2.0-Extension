namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetString : ShowAndGet<string>
    {
        public ShowAndGetString(string question) : base(question)
        {

        }

        public override bool Ask()
        {
            bool rc = false;
            bool ask = true;
            Value = string.Empty; // just a little bit safer, less null pointers

            while (ask)
            {
                Show();

                string answer = QuestionIO.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(answer) )
                {
                    if ( HasDefault )
                    {
                        IsDefault = true;
                        Value = DefaultValue;
                        ask = false;
                        rc = true;
                    }
                }
                else
                {
                    if ( answer.Length == 1 )
                    {
                        switch ( answer[0] )
                        {
                            case '?':
                                WantsDescription = true;
                                ask = false;
                                // rc remains false
                                continue; // :-) goto return

                            case 'x':
                                IsAbort = true;
                                ask = false;
                                // rc remains false
                                continue; // :-) goto return
                        }
                    } // End single character

                    // OK, some string, not a single 'x' nor '?'
                    ask = false;
                    rc = true;
                    Value = answer;
                }

                // no WriteLine after ReadLine() 
            }

            return rc;
        }
    }
}
