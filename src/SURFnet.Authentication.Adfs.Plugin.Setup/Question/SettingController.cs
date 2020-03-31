using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    /// <summary>
    /// Still kludgy....
    /// All the defaults stuff is tricky.
    /// No solution for clearing a value. Not that we ever want that isn't it?
    /// </summary>
    public class SettingController
    {
        public Setting Setting { get; private set; }

        private string TempResult { get; set; }

        private bool triedCfg = false;      // kludgy flag
        private bool triedDefault = false;  // kludgy flag

        public SettingController(Setting setting)
        {
            Setting = setting;
        }

        /// <summary>
        /// Writes the "NewValue", "FoundCfgValue", "DefaultValue" or "no value yet".
        /// </summary>
        /// <returns>true iff NewValue was already filled.</returns>
        public virtual bool DisplayValue()
        {
            bool hasValue = false;

            if ( false == string.IsNullOrWhiteSpace(TempResult) )
            {
                QuestionIO.WriteValue($"Value for {Setting.DisplayName} now '{TempResult}'");
                hasValue = true;
                return true;
            }

            if (string.IsNullOrWhiteSpace(Setting.NewValue))
            {
                // no value decision from past; go check configuration and default.

                if (false==triedCfg  &&  false==string.IsNullOrWhiteSpace(Setting.FoundCfgValue))
                {
                    // there was something in configuration
                    triedCfg = true;  // one shot
                    TempResult = Setting.FoundCfgValue;
                    QuestionIO.WriteValue($"Configuration has a value for {Setting.DisplayName}: {TempResult}");
                }
                else if (false==triedDefault  &&  false==string.IsNullOrWhiteSpace(Setting.DefaultValue))
                {
                    // Yes a default!
                    triedDefault = true;    // one shot
                    TempResult = Setting.DefaultValue;
                    QuestionIO.WriteValue($"The default value for {Setting.DisplayName} is: '{TempResult}'");
                }
                else
                {
                    // Really nothing
                    QuestionIO.WriteValue($"There is no value for {Setting.DisplayName}");
                    TempResult = null;
                }
            }
            else
            {
                // There was a newValue from previous rounds.
                TempResult = Setting.NewValue;
                QuestionIO.WriteValue($"Value for {Setting.DisplayName} now '{TempResult}'");
                hasValue = true;
            }

            return hasValue;
        }

        public bool WantToContinueWith(out bool wantToContinue)
        {
            bool ok = false;

            ShowAndGetYesNo question = new ShowAndGetYesNo($"Do you want to continue with {TempResult}?", 'y');
            wantToContinue = false;

            bool more = true;
            while (more)
            {
                if (question.Ask())
                {
                    // valid Y/N answer
                    if (question.Value == 'n')
                        wantToContinue = false;
                    else
                        wantToContinue = true;
                    more = false;
                    ok = true;
                }
                else
                {
                    // Not a Y/N answer
                    if (question.IsAbort)
                    {
                        more = false;
                    }
                    else if (question.WantsDescription)
                    {
                        string[] help = { "Type 'y'(Yes) to change, 'n'(No) to use existing, '?' for this help, x(eXit) to return unchanged." };
                        QuestionIO.WriteDescription(help);
                    }
                }
            } // end ask loop

            return ok;
        }


        public virtual bool EditSetting()
        {
            bool ok = false;
            TempResult = null;

            var editQuestion = new ShowAndGetString($"Provide a value for '{Setting.DisplayName}'");
            bool more = true;
            while (more)
            {
                if (editQuestion.Ask())
                {
                    // valid answer
                    TempResult = editQuestion.Value;
                    more = false;
                    ok = true;
                }
                else
                {
                    if (editQuestion.IsAbort)
                    {
                        QuestionIO.WriteError("OK, stopping.");
                        more = false;
                    }
                    else if (editQuestion.WantsDescription)
                    {
                        QuestionIO.WriteDescription(Setting.Description);
                    }
                }

            }

            return ok;
        }

        public virtual bool Ask()
        {
            bool ok = false;

            if (string.IsNullOrWhiteSpace(Setting.Introduction))
                QuestionIO.WriteIntro("TODO: Here should be an introduction to the question........");
            else
                QuestionIO.WriteIntro(Setting.Introduction);

            bool keepasking = true;
            while ( keepasking )
            {
                //bool hasValue = DisplayValue();
                DisplayValue();

                if ( string.IsNullOrWhiteSpace(TempResult) )
                {
                    // nothing yet => ask for for value: Edit
                    if ( false == EditSetting() )
                    {
                        // abort!!
                        keepasking = false;  // break out of loop.
                    }
                }
                else
                {
                    // something there, ask Y/N
                    if (WantToContinueWith(out bool valueOK))
                    {
                        if (valueOK)
                        {
                            ok = true;
                            keepasking = false;
                            Setting.NewValue = TempResult; // return approved value
                        }
                        else
                        {
                            //wants to change!
                            TempResult = null;
                        }
                    }
                    else
                    {
                        // an abort
                        keepasking = false;
                    }
                }
            }

            return ok;
        }
    }
}
