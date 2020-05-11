using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static char Ask(string line, char defaultChoice = '\0')
        {
            char c = '\0';
            ShowAndGetYesNo dialogue;

            if ( defaultChoice != '\0')
                dialogue = new ShowAndGetYesNo(line, defaultChoice);
            else
                dialogue = new ShowAndGetYesNo(line);

            bool loop = true;
            while ( loop )
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
                    if ( ! dialogue.WantsDescription )
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
