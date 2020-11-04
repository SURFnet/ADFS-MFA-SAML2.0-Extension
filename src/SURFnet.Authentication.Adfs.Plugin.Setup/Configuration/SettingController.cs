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

        public SettingController(Setting setting)
        {
            Setting = setting;
        }

        /// <summary>
        /// Local value for editing and confirmation.
        /// Only copied to Setting.NewValue on "Correct" confirmation.
        /// </summary>
        private string TempValue { get; set; }


        bool IsThisCorrect(out bool acceptCurrent)
        {
            QuestionIO.WriteLine();
            string value = $"{Setting.DisplayName}: {TempValue}".PadLeft(35);
            QuestionIO.WriteValue(value);
            return AnyControllerUtils.WhatAboutCurrent(out acceptCurrent, "                  Is this correct?", true);
        }

        bool WantToContinueWith(out bool acceptCurrent)
        {
            return AnyControllerUtils.WhatAboutCurrent(out acceptCurrent, $"Do you want to continue with '{TempValue}'?", true);
        }

        /// <summary>
        /// On first entry, pick TempValue from: NewValue, FoundCfgValue or DefaultValue. And display that.
        /// </summary>
        void FirstDisplay()
        {
            // Could be in a base class!
            string intro;
            if (Setting.Introduction.Contains("{0}"))
                intro = string.Format(Setting.Introduction, Setting.DisplayName);
            else
                intro = Setting.Introduction;
            QuestionIO.WriteIntro(intro);

            string textWithValue;
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

        /// <summary>
        /// There is already a value: displays it, then clears it.
        /// </summary>
        void DisplayAgain()
        {
            QuestionIO.WriteValue($"Current value for '{Setting.DisplayName}': {TempValue}");
            TempValue = null;

            return;
        }

        /// <summary>
        /// Assumes the current (or missing) Value is printed. Prompts for new value into TempValue
        /// </summary>
        /// <returns></returns>
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
            bool keepasking = true;
            bool currentIsOK;

            FirstDisplay();

            if (TempValue != null)
            {
                // There is already a value, initiate possible quick fall through
                if (false == WantToContinueWith(out currentIsOK))
                {
                    // abort on Want to continue
                    keepasking = false;
                }
                else
                {
                    if ( false == currentIsOK )
                    {
                        TempValue = null; // no quick fall through
                    }
                }

            }

            while ( keepasking )
            {
                if (TempValue == null)
                {
                    // there is nothing, must ask
                    if (false == AskNewValue())
                    {
                        // abort!
                        break; // break from keepasking.
                    }
                }

                if (false == IsThisCorrect(out currentIsOK))
                {
                    // abort on IsThisCorrect.
                    break;
                }
                else
                {
                    if (currentIsOK) // after IsThisCorrect.
                    {
                        // Done, confirmed
                        Setting.NewValue = TempValue;
                        ok = true;
                        keepasking = false;
                    }
                    else
                    {
                        // Apparently is not yet correct!
                        DisplayAgain();
                    }
                }
            } // end: while (keepasking)

            QuestionIO.WriteEndSeparator();

            return ok;
        } // end: ask()
    }
}
