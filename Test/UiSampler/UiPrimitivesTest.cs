using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SURFnet.Authentication.Adfs.Plugin.Setup.Question;

namespace UiSampler
{
    /// <summary>
    /// Scratch program to enable single stepping through Setup UI primitives.
    /// </summary>
    static class UiPrimitivesTest
    {
        static void Main(string[] args)
        {
            bool more;

            var q1 = new ShowAndGetYesNo("Q1 YesNo - Continue without default");
            var q2 = new ShowAndGetYesNo("Q2 YesNo - Continue with a default reponse", 'y');

            more = true;
            while (more)
            {
                if (q1.Ask())
                {
                    // valid answer
                    Console.WriteLine("OK, That is: " + q1.Value);
                    more = false;
                }
                else
                {
                    if (q1.IsAbort)
                    {
                        Console.WriteLine("OK, stopping.");
                        more = false;
                    }
                    else if (q1.WantsDescription)
                    {
                        Console.WriteLine("Description for Q1.");
                    }
                }
            } // more q1

            more = true;
            while (more)
            {
                if (q2.Ask() )
                {
                    // valid answer
                    Console.WriteLine("OK, That is: " + q2.Value);
                    more = false;
                }
                else
                {
                    if ( q2.IsAbort )
                    {
                        Console.WriteLine("OK, stopping.");
                        more = false;
                    }
                    else if ( q2.WantsDescription )
                    {
                        Console.WriteLine("Description for Q2.");
                    }
                   
                }
            } // more q2

            var q3 = new ShowAndGetString("De Vraag");
            more = true;
            while (more)
            {
                if (q3.Ask())
                {
                    // valid answer
                    Console.WriteLine("OK, That is: " + q3.Value);
                    more = false;
                }
                else
                {
                    if (q3.IsAbort)
                    {
                        Console.WriteLine("OK, stopping.");
                        more = false;
                    }
                    else if (q3.WantsDescription)
                    {
                        Console.WriteLine("Description for Q3.");
                    }
                }
            } // more q3
        } // end main
    }
}
