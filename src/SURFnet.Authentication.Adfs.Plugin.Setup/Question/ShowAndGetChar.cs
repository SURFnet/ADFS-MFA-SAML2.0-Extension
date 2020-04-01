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


        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if OK.</returns>
        public override bool Ask()
        {
            bool rc = false;
            bool ask = true;
            string error = null;

            while ( ask )
            {
                Show(); // let the base display it

                // get Key
                var input = QuestionIO.ReadKey();
                if (input.Key == ConsoleKey.Enter)
                {
                    if ( HasDefault )
                    {
                        // TODO: Remove when all ReadLine!!
                        QuestionIO.WriteDefaultEcho(DefaultChar);
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
                            else
                            {
                                // invalid answer => loop
                                QuestionIO.WriteLine();
                                error = "Invalid choice";
                            }
                            break;
                    }

                    // Always position on a new line
                    if ( null != error )
                    {
                        QuestionIO.WriteError(error);
                        error = null;
                    }
                    else
                        QuestionIO.WriteLine();
                }

            }
            return rc;
        }
    }
}
