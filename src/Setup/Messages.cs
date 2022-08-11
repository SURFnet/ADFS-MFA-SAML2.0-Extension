using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public static class Messages
    {
        public static bool ReallyXyz()
        {
            LogService.WriteWarning("   There is a registration in the ADFS configuration of an SFO MFA extension.");
            LogService.WriteWarning("   In general, it is not a good idea to remove this registration.");
            LogService.WriteWarning("   This ADFS server will then produce loading errors in the ADFS EventLog.");
            LogService.WriteWarning("   ");
            LogService.WriteWarning("   However, if you want to immediately (re)install a version of the SFO MFA extension.");
            LogService.WriteWarning("   However, IFF you want to immediately (re)install a version of the SFO MFA extension.");
            LogService.WriteWarning("   Then it could be OK (as cleanup first).");
            Console.WriteLine();
            return false;
        }

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
            if ('y' == AskYesNo.Ask(question,true))
            {
                doit = true;
            }
            else
            {
                if ( noMessage== null ) noMessage = "OK. Will not do it.";
                LogService.WriteWarning(noMessage);
                doit = false;
            }
            Console.WriteLine();

            return doit;
        }
    }
}
