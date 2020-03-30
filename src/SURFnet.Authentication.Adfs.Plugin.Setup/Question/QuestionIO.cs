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
        // Use handler to set flag and provide: Poll() AtomicPollClear()  ?
        //

        private const string DescriptionIndent = "        "; // 8
        private const string ErrorIndent = "    ";           // 4
        private const string OptionsIndent = "    ";         // 4
        private const string ValueIndent = "    ";           // 4

        public static void WriteIntro(string intro)
        {
            Console.WriteLine(intro);
        }
        public static void WriteValue(string currentValue)
        {
            Console.Write(ValueIndent);
            Console.WriteLine(currentValue);
        }

        public static void WriteOptions(string[] options)
        {
            foreach ( var option in options )
            {
                Console.Write(OptionsIndent);
                Console.WriteLine(option);
            }
        }

        public static void WriteQuestion(string question)
        {
            Console.Write(question);
        }

        public static void WriteDescription(string[] description)
        {
            foreach (string s in description)
            {
                Console.Write(DescriptionIndent);
                Console.WriteLine(s);
            }
        }

        public static void WriteError(string error)
        {
            Console.Write(ErrorIndent);
            Console.WriteLine(error);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }
        
        public static string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
