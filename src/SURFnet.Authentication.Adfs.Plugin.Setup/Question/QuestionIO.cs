using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        /// <summary>
        /// For single Setting Questions. Prints extra line.
        /// </summary>
        /// <param name="currentValue"></param>
        public static void WriteValue(string currentValue)
        {
            Console.Write(ValueIndent);
            Console.WriteLine(currentValue);
            Console.WriteLine();   // This is only for the questions, not for the Lists
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

        public static void WriteLine(string s = null)
        {
            if (s == null)
                Console.WriteLine();
            else
                Console.WriteLine(s);
        }

        public static char ReadKey()
        {
            char rc;

            string line = Console.ReadLine();

            line = line.Trim();
            if (line.Length == 0)
            {
                rc = '\r';
            }
            else if (line.Length == 1)
            {
                rc = line[0];
            }
            else
            {
                rc = '\0';
            }

            return rc;
        }
        
        public static string ReadLine()
        {
            return Console.ReadLine();
        }

        /// <summary>
        /// Forbidden method!!! Just for test!
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="msg"></param>
        public static void MySleep(int seconds, string msg)
        {
            Console.Write(msg);
            while (seconds > 0 )
            {
                Thread.Sleep(1000);
                Console.Write(".");
                seconds--;
            }

            Console.Write("\r                                                                     \r");
        }
    }
}
