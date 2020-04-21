using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    /// <summary>
    /// Still kludgy....
    /// All the defaults stuff is tricky.
    /// No solution for clearing a value. Not that we ever want that isn't it?
    /// </summary>
    public class SettingController
    {
        public Setting Setting { get; private set; }

        private string TempValue { get; set; }

        private bool triedCfg = false;      // TODO: kludgy flag
        private bool triedDefault = false;  // TODO: kludgy flag

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

            if ( false == string.IsNullOrWhiteSpace(TempValue) )
            {
                QuestionIO.WriteLine();
                QuestionIO.WriteValue($"Value for '{Setting.DisplayName}' now '{TempValue}'");
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
                    TempValue = Setting.FoundCfgValue;
                    QuestionIO.WriteValue($"Configured value for '{Setting.DisplayName}': {TempValue}");
                }
                else if (false==triedDefault  &&  false==string.IsNullOrWhiteSpace(Setting.DefaultValue))
                {
                    // Yes a default!
                    triedDefault = true;    // one shot
                    TempValue = Setting.DefaultValue;
                    QuestionIO.WriteValue($"The default value for '{Setting.DisplayName}' is: '{TempValue}'");
                }
                else
                {
                    // Really nothing
                    QuestionIO.WriteValue($"There is no value for '{Setting.DisplayName}'");
                    TempValue = null;
                }
            }
            else
            {
                // There was a newValue from previous rounds.
                TempValue = Setting.NewValue;
                QuestionIO.WriteValue($"Value for '{Setting.DisplayName}' now '{TempValue}'");
                hasValue = true;
            }

            return hasValue;
        }

        public bool WhatAboutCurrent(out bool acceptCurrent, string question)
        {
            bool ok = false;

            ShowAndGetYesNo yesorno = new ShowAndGetYesNo(question, 'y');
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
                        string[] help = { "Type 'y'(Yes) to accept current, 'n'(No) to edit, '?' for this help, x(eXit) to abort." };
                        QuestionIO.WriteDescription(help);
                    }
                }
            } // end ask loop

            return ok;
        }


        bool IsThisCorrect(out bool acceptCurrent)
        {
            return WhatAboutCurrent(out acceptCurrent, "     Is this correct?");
        }


        bool WantToContinueWith(out bool acceptCurrent)
        {
            return WhatAboutCurrent(out acceptCurrent, $"Do you want to continue with {TempValue}?");
        }


        public virtual bool EditSetting()
        {
            bool ok = false;
            TempValue = null;

            var editQuestion = new ShowAndGetString($"Provide a value for '{Setting.DisplayName}'");
            bool more = true;
            while (more)
            {
                if (editQuestion.Ask())
                {
                    // valid answer
                    TempValue = editQuestion.Value;
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
                        QuestionIO.WriteDescription(Setting.HelpLines);
                    }
                }

            }

            return ok;
        }


        void FirstDisplay()
        {
            string textWithValue;

            QuestionIO.WriteIntro(Setting.Introduction);

            if (false == string.IsNullOrWhiteSpace(Setting.DefaultValue))
            {
                TempValue = Setting.DefaultValue;
                textWithValue = $"Default value for '{Setting.DisplayName}': {TempValue}";
            }
            else if (false == string.IsNullOrWhiteSpace(Setting.FoundCfgValue) )
            {
                TempValue = Setting.FoundCfgValue;
                textWithValue = $"Found a value for '{Setting.DisplayName}': {TempValue}";
            }
            else if (false == string.IsNullOrWhiteSpace(Setting.NewValue))
            {
                TempValue = Setting.NewValue;
                textWithValue = $"Current value for '{Setting.DisplayName}': {TempValue}";
            }
            else
            {
                TempValue = null;
                textWithValue = $"'{Setting.DisplayName}' has no value";
            }

            QuestionIO.WriteValue(textWithValue);
            return;
        }

        void DisplayAgain()
        {
            QuestionIO.WriteValue($"Current value for '{Setting.DisplayName}': {TempValue}");
            TempValue = null;

            return;
        }

        bool AskNewValue()
        {
            bool ok = false;

            var editQuestion = new ShowAndGetString($"Provide a value for '{Setting.DisplayName}'");
            bool more = true;
            while (more)
            {
                if (editQuestion.Ask())
                {
                    // valid answer
                    TempValue = editQuestion.Value;
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
                        QuestionIO.WriteDescription(Setting.HelpLines);
                    }
                }
            }

            return ok;
        }

        public virtual bool Ask()
        {
            bool ok = false;
            bool asking = true;

            FirstDisplay();

            while ( asking )
            {
                if ( TempValue == null )
                {
                    if ( false == AskNewValue() )
                    {
                        // abort!
                        asking = false;
                    }
                }
                else
                {
                    // There is a value
                    if ( false == WantToContinueWith(out bool currentIsOK) )
                    {
                        // abort on Want to continue
                        asking = false;
                    }
                    else
                    {
                        // Value seems OK; Are you sure?
                        if ( currentIsOK )
                        {
                            if ( false == IsThisCorrect(out currentIsOK) )
                            {
                                // abort on Is This Correct.
                                asking = false;
                            }
                            else
                            {
                                if ( currentIsOK )
                                {
                                    // Done
                                    Setting.NewValue = TempValue;
                                    ok = true;
                                    asking = false;
                                }
                                else
                                {
                                    // Apparent is not correct!
                                    DisplayAgain();
                                }
                            }
                        }
                        else
                        {
                            // not continuing with
                            DisplayAgain();
                        }
                    } // end there is a value
                }
            } // end while asking

            return ok;
        }

        public virtual bool OldAsk()
        {
            bool ok = false;

            if (string.IsNullOrWhiteSpace(Setting.Introduction))
                QuestionIO.WriteIntro("TODO: Here should be an introduction to the question........");
            else
            {
                string intro;
                if (Setting.Introduction.Contains("{0}"))
                {
                    intro = string.Format(Setting.Introduction, Setting.DisplayName);
                }
                else
                    intro = Setting.Introduction;
                QuestionIO.WriteIntro(intro);
            }

            bool keepasking = true;
            while ( keepasking )
            {
                DisplayValue();

                if ( string.IsNullOrWhiteSpace(TempValue) )
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
                            Setting.NewValue = TempValue; // return approved value
                        }
                        else
                        {
                            //wants to change!
                            TempValue = null;
                        }
                    }
                    else
                    {
                        // an abort
                        keepasking = false;
                    }
                }
            }

            QuestionIO.WriteEndSeparator();
            return ok;
        }
    }
}
