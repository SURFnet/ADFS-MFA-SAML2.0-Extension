using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public static class AskYesNo
    {
        /// <summary>
        /// Straight blocking Yes/No call.
        /// TODO: Bit of a kludge to get rid of the '?'
        /// </summary>
        /// <param name="line"></param>
        /// <param name="defaultChoice"></param>
        /// <returns>'y', 'n' or 'x'</returns>
        public static char Ask(string line, bool showHelpChar, char defaultChoice = '\0')
        {
            var c = '\0';
            ShowAndGetYesNo dialogue;

            if (defaultChoice != '\0')
            {
                dialogue = new ShowAndGetYesNo(line, defaultChoice, showHelpChar);
            }
            else
            {
                dialogue = new ShowAndGetYesNo(line, showHelpChar);
            }

            var loop = true;
            while (loop)
            {
                if (dialogue.Ask())
                {
                    // Yes or No
                    switch (dialogue.Value)
                    {
                        case 'y':
                        case 'n':
                            c = dialogue.Value;
                            loop = false;
                            break;

                        default:
                            LogService.Log.Error("Bug Check! In AskYesNo.Ask(). Non yn.");
                            break;
                    }
                }
                else
                {
                    if (!dialogue.WantsDescription)
                    {
                        // abort
                        c = 'x';
                        loop = false;
                    }

                    // else: loop on '?'
                }
            }

            return c;
        }
    }
}