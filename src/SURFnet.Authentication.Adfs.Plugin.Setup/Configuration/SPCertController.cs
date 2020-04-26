using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    ///
    /// MAYBE:
    /// We will make this a derived class of the Setting.
    /// Setting would then have some virtual method for the handler....
    /// 
    /// It is messy! IdP stuff is similar, need to harmonize!


    public class SPCertController
    {
        public SPCertController(Setting setting)
        {
            Setting = setting;
        }

        private readonly Setting Setting;

        // instance members for storage
        private string TempValue;   // base candidate!
        private X509Certificate2 tempCert; // if not null then corresponds with TempValue.

        void FirstDisplay()
        {
            // Could be in a base class!
            string intro;
            if (Setting.Introduction.Contains("{0}"))
                intro = string.Format(Setting.Introduction, Setting.DisplayName);
            else
                intro = Setting.Introduction;
            QuestionIO.WriteIntro(intro);

            /// Disply current thumbprint if there
            /// 
            string textWithValue;
            if (false == string.IsNullOrWhiteSpace(Setting.DefaultValue))
            {
                TempValue = Setting.DefaultValue;
                textWithValue = $"Default value for '{Setting.DisplayName}': {TempValue}";
            }
            else if (false == string.IsNullOrWhiteSpace(Setting.FoundCfgValue))
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
            // End base functionality

            if ( TempValue != null )
            {
                if ( false == SetupCertService.SPCertChecker(TempValue, out tempCert))
                {
                    QuestionIO.WriteError("The supplied thumbprint is not of a valid certificate in the Store");
                    TempValue = null;
                }
            }
        }

        void DisplayAgain() // very much base!
        {
            QuestionIO.WriteValue($"Current value for '{Setting.DisplayName}': {TempValue}");
            DisplayCert();
            TempValue = null;   // this would hurt in a base :-(

            return;
        }

        private bool AskForCert()
        {
            // TODONOW: there are actually several return conditions? one:threw or failed miserably, two: abort, three: wasn't there
            bool ok = false;
            var methodSelector = new CertMethodController();

            if ( false == methodSelector.Ask() )
            {
                // abort
                TempValue = null;
            }
            else
            {
                int index = methodSelector.ChoosenIndex;
                switch (index)
                {
                    case 0:
                        // From store
                        if ( 0 == CertFromStore.Doit(out tempCert) )
                        {
                            TempValue = tempCert.Thumbprint;
                            ok = true;
                        }
                        else
                        {
                            ClearValue();
                        }
                        break;

                    case 1:
                        // Import
                        var pfxSelector = new CertImportPfx();
                        if ( 0== pfxSelector.Doit() )
                        {
                            tempCert = pfxSelector.ResultCertificate;
                            TempValue = tempCert.Thumbprint;
                            ok = true;
                        }
                        else
                        {
                            ClearValue();
                        }
                        break;

                    case 2:
                        // Create
                        tempCert = CertCreate.Create(AdfsPSService.GetAdfsHostname);
                        if ( tempCert != null )
                        {
                            TempValue = tempCert.Thumbprint;
                            ok = true;

                            CertExport.DoYouWantToExport(tempCert);
                        }
                        else
                        {
                            ClearValue();
                        }
                        break;

                    default:
                        // impossible! MUST be a BUG!!!
                        break;
                }
                //QuestionIO.WriteLine("***    cert method choice index: "+ methodSelector.ChoosenIndex.ToString());
            }
            return ok;
        }


        public bool Ask()
        {
            bool ok = false;

            FirstDisplay();

            bool needVerifiedCert = true;
            while ( needVerifiedCert )
            {
                if ( TempValue == null )
                {
                    // Ask for a cert
                    if ( AskForCert() == false )
                    {
                        // abort
                        break;
                    }
                }

                if ( TempValue != null )
                {
                    if ( SetupCertService.IsValidSPCert(tempCert) )
                    {
                        DisplayCert();
                        if (!AnyControllerUtils.WhatAboutCurrent(out bool acceptCurrent, "             Continue with this certificate"))
                        {
                            // abort
                            // but first cleanup.
                            ClearValue();
                            break;
                        }
                        else
                        {
                            if (acceptCurrent)
                            {
                                Setting.NewValue = TempValue;
                                ok = true;
                                needVerifiedCert = false;

                                // update ACL!
                                string ObjectName = AdfsServer.AdfsAccount;
                                SetupCertService.AddAllowAcl(tempCert, ObjectName);

                                // Add cert to registration data.
                                RegistrationData.SetCert(tempCert);

                                // finally release cert
                                ClearValue();
                            }
                            else
                            {
                                // cert was fine. However, decided not to use it.
                                // Force ask for new.
                                ClearValue();
                            }
                        }
                    }
                    else
                    {
                        // not a valid cert, force asking.
                        // checker already wrote the error message
                        ClearValue();
                    }
                }
            }

            return ok;
        }

        void ClearValue()
        {
            TempValue = null;
            if (tempCert != null)
            {
                tempCert = SetupCertService.CertDispose(tempCert);
            }
        }

        public static int DisplayAndAskOK(X509Certificate2 cert, out bool ok)
        {
            int rc = -1;
            ok = false;

            DisplayCert(cert);
            if (false==AnyControllerUtils.WhatAboutCurrent(out bool acceptCurrent, "             Continue with this certificate"))
            {
                // abort
            }
            else
            {
                ok = acceptCurrent;
                rc = 0;
            }

            return rc;
        }

        void DisplayCert()
        {
            if ( TempValue != null )
            {
                if ( tempCert!=null)
                {
                    DisplayCert(tempCert);
                }
            }
        }

        static string PropertyToString(string property, string value)
        {
            return $"   {property.PadLeft(25)}: {value}";
        }

        static void DisplayCert(X509Certificate2 cert)
        {
            QuestionIO.WriteLine(PropertyToString("Subject", cert.Subject));
            QuestionIO.WriteLine(PropertyToString("Issuer", cert.Issuer));
            QuestionIO.WriteLine(PropertyToString("Valid until", cert.NotAfter.ToString("yyyy-MM-dd")));
            QuestionIO.WriteLine(PropertyToString("Thumbprint", cert.Thumbprint));
            QuestionIO.WriteLine();
        }
    }
}
