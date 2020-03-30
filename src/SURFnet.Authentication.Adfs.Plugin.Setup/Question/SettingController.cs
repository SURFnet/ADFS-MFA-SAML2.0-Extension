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

        public virtual void DisplayCurrent()
        {
            QuestionIO.WriteValue($"Current value for {Setting.DisplayName}: {Setting.FoundCfgValue}");
        }

        public virtual void DisplayValue()
        {
            if (!string.IsNullOrWhiteSpace(Setting.Value))
            {
                QuestionIO.WriteValue($"Value for {Setting.DisplayName}: {Setting.Value}");
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
                    // valid answer
                    ok = true;
                    if (q.Value == 'n')
                        wantToChange = false;
                    else
                        wantToChange = true;
                    more = false;
                }
                else
                {
                    if (q.IsAbort)
                    {
                        QuestionIO.WriteError("OK, stopping. Or do we want pose the RevertTo original value question?");
                        more = false;
                    }
                    else if (q.WantsDescription)
                    {
                        QuestionIO.WriteDescription("Type 'y' (Yes) to change, 'n' (No) to use existing, '?' for this help, x (eXit) to return unchanged.");
                    }
                }
            } // more q1

            return ok;
        }


        public virtual bool EditSetting()
        {
            bool ok = false;
            var q3 = new ShowAndGetString($"Provide value for {Setting.DisplayName}. Value is now: \"{Setting.Value}\"");
            bool more = true;
            while (more)
            {
                if (q3.Ask())
                {
                    // valid answer
                    Setting.NewValue = q3.Value;
                    more = false;
                    ok = true;
                }
                else
                {
                    if (q3.IsAbort)
                    {
                        QuestionIO.WriteError("OK, stopping.");
                        more = false;
                    }
                    else if (q3.WantsDescription)
                    {
                        QuestionIO.WriteDescription(Setting.Description.ToString());
                    }
                }

            }

            return ok;
        }

        public virtual bool Ask()
        {
            bool rc = false;

            if (!string.IsNullOrWhiteSpace(Setting.FoundCfgValue))
            {
                DisplayCurrent();
            }

            bool keepasking = true;
            while ( keepasking )
            {
                if (WantToChange(out bool wantToChange))
                {
                    // valid Yes/No
                    if (wantToChange == false)
                    {
                        keepasking = false;
                        rc = true;
                    }
                    else
                    {
                        // allow Edit
                        if (EditSetting())
                        {
                            // new value. Display it.
                            DisplayValue();
                        }
                        else
                        {
                            // aborting
                            keepasking = false;
                        }
                    }
                }
                else
                {
                    // Can only be eXit; rc remains false
                    keepasking = false;
                }
            }

            return rc;
        }
    }
}
