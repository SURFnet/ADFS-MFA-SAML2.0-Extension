using System.Linq;

using SURFnet.Authentication.Adfs.Plugin.Setup.Question;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class AnyControllerUtils
    {
        /// <summary>
        /// When the proposed or current choice/value is on the display, ask WhatAboutIt.
        /// </summary>
        /// <param name="acceptCurrent">true if likes it.</param>
        /// <param name="question">question</param>
        /// <returns>false on error/abort, else true</returns>
        public static bool WhatAboutCurrent(out bool acceptCurrent, string question, string[] helpLines)
        {
            return WhatAboutCurrent(out acceptCurrent, question, helpLines, helpLines.Any());
        }

        /// <summary>
        /// When the proposed or current choice/value is on the display, ask WhatAboutIt.
        /// </summary>
        /// <param name="acceptCurrent">true if likes it.</param>
        /// <param name="question">question</param>
        /// <returns>false on error/abort, else true</returns>
        public static bool WhatAboutCurrent(out bool acceptCurrent, string question, bool showHelpChar)
        {
            return WhatAboutCurrent(out acceptCurrent, question, null, showHelpChar);
        }

        /// <summary>
        /// When the proposed or current choice/value is on the display, ask WhatAboutIt.
        /// </summary>
        /// <param name="acceptCurrent">true if likes it.</param>
        /// <param name="question">question</param>
        /// <returns>false on error/abort, else true</returns>
        private static bool WhatAboutCurrent(out bool acceptCurrent, string question, string[] helpLines, bool showHelpChar)
        {
            bool ok = false;

            ShowAndGetYesNo yesorno = new ShowAndGetYesNo(question, 'y', showHelpChar);
            acceptCurrent = true;

            bool more = true;
            while (more)
            {
                if (yesorno.Ask())
                {
                    // valid Y/N answer
                    if (yesorno.Value == 'n')
                        acceptCurrent = false;
                    else
                        acceptCurrent = true;
                    more = false;
                    ok = true;
                }
                else
                {
                    // Not a Y/N answer
                    if (yesorno.IsAbort)
                    {
                        more = false;
                    }
                    else if (yesorno.WantsDescription)
                    {
                        var help = helpLines ?? new string[]{ "Type 'y'(Yes) to accept current, 'n'(No) to edit, '?' for this help, x(eXit) to abort." };
                        QuestionIO.WriteDescription(help);
                    }
                }
            } // end ask loop

            return ok;
        }
    }
}
