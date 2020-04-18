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
            LogService.WriteWarning("   This ADFS server will then produce loading errors in the ADFS eventlog.");
            LogService.WriteWarning("   ");
            LogService.WriteWarning("   However, if you want to immediately (re)install a version of the SFO MFA extension.");
            LogService.WriteWarning("   However, IFF you want to immediately (re)install a version of the SFO MFA extension.");
            LogService.WriteWarning("   Then it could be OK (as cleanup first).");
            Console.WriteLine();
            return false;
        }

        public static int EndWarning(string msg)
        {
            Console.WriteLine();
            Console.WriteLine(msg);

            return 4;
        }

        public static void SayAllSeemsOK()
        {
            Console.WriteLine();
            Console.WriteLine("Everything was OK.");
            Console.WriteLine("Take a look at the ADFS EventLog and also the");
            Console.WriteLine("MFA extension EventLog 'AD FS plugin'. To verify.");
        }

        public static bool DoYouWantTO(string question, string noMessage = null)
        {
            bool doit;

            LogService.Log.Warn(question);
            if ('y' == AskYesNo.Ask(question))
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
