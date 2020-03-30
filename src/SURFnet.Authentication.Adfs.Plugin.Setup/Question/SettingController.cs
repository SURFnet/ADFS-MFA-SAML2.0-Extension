using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class SettingController
    {
        public Setting Setting { get; private set; }

        public SettingController(Setting setting)
        {
            Setting = setting;
        }

        public virtual void DisplayValue()
        {
            if (!string.IsNullOrWhiteSpace(Setting.Value))
            {
                QuestionIO.WriteValue($"Value for {Setting.DisplayName} now '{Setting.Value}'");
            }
        }

        public bool WantToChange(out bool wantToChange)
        {
            bool ok = false;

            wantToChange = false;
            var q = new ShowAndGetYesNo("Do you want to change it?", 'n');

            bool more = true;
            while (more)
            {
                if (q.Ask())
                {
                    // valid Y/N answer
                    if (q.Value == 'n')
                        wantToChange = false;
                    else
                        wantToChange = true;
                    more = false;
                    ok = true;
                }
                else
                {
                    // Not a Y/N answer
                    if (q.IsAbort)
                    {
                        more = false;
                    }
                    else if (q.WantsDescription)
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
            var editQuestion = new ShowAndGetString($"Provide a value for '{Setting.DisplayName}'");
            bool more = true;
            while (more)
            {
                if (editQuestion.Ask())
                {
                    // valid answer
                    Setting.NewValue = editQuestion.Value;
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

            // TODO: display Intro
            if (string.IsNullOrWhiteSpace(Setting.Introduction))
                QuestionIO.WriteIntro("TODO: Here comes the introduction........");
            else
                QuestionIO.WriteIntro(Setting.Introduction);

            bool keepasking = true;
            while ( keepasking )
            {
                DisplayValue();

                if (WantToChange(out bool wantToChange))
                {
                    // valid Yes/No
                    if (wantToChange == false)
                    {
                        // Does not want to change.
                        keepasking = false; // break from while
                        ok = true;
                    }
                    else
                    {
                        // allow Edit
                        if ( EditSetting())
                        {
                            keepasking = false; // break from while
                            ok = true;
                        }
                        else
                        {
                            // aborting on Edit. NewValue was not set.
                            keepasking = false;
                        }
                    }
                }
                else
                {
                    // Abort/Exit on Y/N. Can only be eXit; ok remains false
                    keepasking = false;
                }
            }

            return ok;
        }
    }
}
