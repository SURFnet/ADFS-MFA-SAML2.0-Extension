using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public static class QuestionIO
    {

        // TODO:
        //    SetConsoleMode and SetConsoleCtrlHandler
        //       in: https://docs.microsoft.com/en-us/windows/console/ctrl-c-and-ctrl-break-signals
        //   ConsoleCancelEventArgs.Cancel=true   
        //       in: https://docs.microsoft.com/en-us/dotnet/api/system.console.cancelkeypress?view=netframework-4.8
        //
        // use handler to set flag and privide: Poll() AtomicPollClear()  ?

        public static void WriteHeader(string header)
        {
            Console.WriteLine(header);
        }
        public static void WriteValue(string currentValue)
        {
            Console.WriteLine(currentValue);
        }

        public static void WriteOptions(string[] options)
        {
            foreach ( var option in options )
            {
                Console.Write("  ");
                Console.WriteLine(options);
            }
        }

        public static void WriteQuestion(string question)
        {
            Console.Write(question);
        }

        public static void WriteDescription(string description)
        {
            Console.Write("  ");
            Console.WriteLine(description);
        }

        public static void WriteError(string error)
        {
            Console.Write("  ");
            Console.WriteLine(error);
        }
    }
}
