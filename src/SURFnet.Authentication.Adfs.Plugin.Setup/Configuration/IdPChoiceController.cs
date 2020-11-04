using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class IdPChoiceController : ShowListGetDigit
    {
        /// <summary>
        /// Shows the list of IdPs and asks to choose and confirm.
        /// </summary>
        /// <param name="idpEnvironments">IDP environments Dictionary</param>
        /// <param name="defaultIndex">index for default choice</param>
        public IdPChoiceController(List<Dictionary<string, string>> idpEnvironments, int defaultIndex, bool showHelpChar) :
                    base(CreateOptionList(idpEnvironments), defaultIndex+1, showHelpChar)  // ShowListGetDigit() is 1 based, index 0 based
        {
        }

        public override bool Ask()
        {
            bool ok = false;

            bool keepasking = true;
            while ( keepasking )
            {
                if ( false == base.Ask() )
                {
                    // abort
                    break;
                }
                else
                {
                    // some valid choice
                    QuestionIO.WriteLine();
                    QuestionIO.WriteValue(OptionList.Options[ChoosenIndex]);
                    if ( ! AnyControllerUtils.WhatAboutCurrent(out bool acceptCurrent, "                  Is this correct?", true) )
                    {
                        // abort
                        break;
                    }
                    else
                    {
                        if ( acceptCurrent )
                        {
                            ok = true;
                            keepasking = false;
                        }
                    }
                }
            }

            QuestionIO.WriteEndSeparator();
            return ok;
        }

        /*
         * Static helpers for the constructor
         */
        private static OptionList CreateOptionList(List<Dictionary<string, string>> idpEnvironments)
        {
            string[] options = new string[idpEnvironments.Count];

            for (int index = 0; index < options.Length; index++)
            {
                var env = idpEnvironments[index];
                options[index] = IdPIndexToEnvString(env, index);
            }

            var ol = new OptionList()
            {
                Introduction = "There are different SecondFactorOnly servers, their names suggest their usage.",
                Options = options,
                Question = "Select a SecondFactorOnly gateway environment"
            };

            return ol;
        }

        private static string IdPIndexToEnvString(Dictionary<string, string> dict, int index)
        {
            return $"  {Index2Digit(index)}. {dict[SetupConstants.IdPEnvironmentType]}  ({dict[ConfigSettings.IdPEntityId]})";
        }
    }
}
