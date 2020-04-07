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
        private const string EndSeparator = "-  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -";

        public static void WriteIntro(string intro)
        {
            Console.WriteLine(intro);
            Console.WriteLine();
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
            Console.WriteLine();
        }

        public static void WriteQuestion(string question)
        {
            Console.Write(question);
        }

        public static void WriteDefaultEcho(char c)
        {
            // TODO: remove this temp fix for new Keyreader!
            Console.WriteLine(c);  // old situation ENTER not echoed
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

        public static void WriteEndSeparator()
        {
            Console.WriteLine(EndSeparator);
            Console.WriteLine();
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
