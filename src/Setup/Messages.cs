using System;

using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class Messages
    {
        public static ReturnOptions EndWarning(string msg)
        {
            LogService.Log.Warn("EndWarning: " + msg);
            Console.WriteLine();
            Console.WriteLine(msg);

            return ReturnOptions.Failure;
        }

        public static void SayAllSeemsOK()
        {
            LogService.Log.Info("SayAllSeemsOK()");

            Console.WriteLine();
            Console.WriteLine("Everything was OK.");
            Console.WriteLine("Please check the ADFS EventLog and also the");
            Console.WriteLine("MFA extension EventLog 'AD FS plugin' for warnings");
            Console.WriteLine("and errors.");
        }

        public static bool DoYouWantTO(string question, string noMessage = null)
        {
            bool doit;

            LogService.Log.Warn(question);
            if ('y' == AskYesNo.Ask(question, true))
            {
                doit = true;
            }
            else
            {
                if (noMessage == null)
                {
                    noMessage = "OK. Will not do it.";
                }

                LogService.WriteWarning(noMessage);
                doit = false;
            }

            Console.WriteLine();

            return doit;
        }
    }
}