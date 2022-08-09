using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public static class AskYesNo
    {
        /// <summary>
        /// Straight blocking Yes/No call.
        /// </summary>
        /// <returns>'y', 'n' or 'x'</returns>
        public static char Ask(string line, bool showHelpChar, char defaultChoice = '\0')
        {
            var c = '\0';

            var dialogue = defaultChoice != '\0'
                ? new ShowAndGetYesNo(line, defaultChoice, showHelpChar)
                : new ShowAndGetYesNo(line, showHelpChar);

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
                    if (dialogue.WantsDescription)
                    {
                        continue;
                    }

                    // abort
                    c = 'x';
                    loop = false;

                    // TODO: Bit of a kludge to get rid of the '?'
                    // else: loop on '?'
                }
            }

            return c;
        }
    }
}