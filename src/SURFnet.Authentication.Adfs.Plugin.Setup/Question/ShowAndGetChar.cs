using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetChar : ShowAndGet<char>
    {
        private string ValidCharacters { get; set; }

        private char DefaultChar { get; set; }

        public ShowAndGetChar(string question, string validChars) : base(CreateValidChoices(question,validChars))
        {
            ValidCharacters = validChars.ToLowerInvariant();
        }
        public ShowAndGetChar(string question, string validChars, char defaultChar) : base(CreateValidChoices(question, validChars), char.ToLowerInvariant(defaultChar))
        {
            ValidCharacters = validChars.ToLowerInvariant();
        }

        private static string CreateValidChoices(string question, string validChars)
        {
            return $"{question} ({validChars.ToLowerInvariant()}x?)";
        }


        public override bool Ask()
        {
            bool rc = false;
            bool ask = true;

            while ( ask )
            {
                Show();

                // get Key
                var input = Console.ReadKey();
                if (input.Key == ConsoleKey.Enter)
                {
                    if ( HasDefault )
                    {
                        IsDefault = true;
                        Value = DefaultValue;
                        ask = false;
                        rc = true;
                    }
                } // TODO: No need to test for other specials? ctrl-Break?
                else
                {
                    char answer = char.ToLowerInvariant(input.KeyChar);
                    switch (answer)
                    {
                        case '?':
                            WantsDescription = true;
                            ask = false;
                            // rc remains false
                            break;

                        case 'x':
                            IsAbort = true;
                            ask = false;
                            // rc remains false
                            break;

                        default:
                            if (ValidCharacters.Contains(answer))
                            {
                                Value = answer;
                                ask = false;
                                rc = true;
                            }
                            // else: invalid answer => loop
                            break;
                    }

                    Console.WriteLine();  // Always position on a new line
                }

            }
            return rc;
        }
    }
}
