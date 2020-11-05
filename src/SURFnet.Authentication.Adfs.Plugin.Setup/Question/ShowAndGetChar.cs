using System.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class ShowAndGetChar : ShowAndGet<char>
    {
        private string ValidCharacters { get; set; }

        private char DefaultChar { get; set; }

        private readonly bool showHelpChar;

        public ShowAndGetChar(string question, string validChars, bool showHelpChar) : 
            base(CreateValidChoices(question,validChars, showHelpChar))
        {
            this.ValidCharacters = validChars.ToLowerInvariant();
            this.showHelpChar = showHelpChar;
        }

        public ShowAndGetChar(string question, string validChars, char defaultChar, bool showHelpChar) : 
            base(CreateValidChoices(question, validChars, showHelpChar), char.ToLowerInvariant(defaultChar))
        {
            this.ValidCharacters = validChars.ToLowerInvariant();
            this.showHelpChar = showHelpChar;
        }

        private static string CreateValidChoices(string question, string validChars, bool showHelpChar)
        {
            var helpChar = showHelpChar ? "?" : string.Empty;
            return $"{question} ({validChars.ToLowerInvariant()}x{helpChar})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if OK.</returns>
        public override bool Ask()
        {
            bool ok = false;
            bool ask = true;
            //string error = null;

            while (ask)
            {
                Show(); // let the base display it

                // get Key

                char input = QuestionIO.ReadKey();
                char answer = char.ToLowerInvariant(input);
                switch (answer)
                {
                    case '\0':
                        // multiple characters
                        break;

                    case '\r':
                        if (HasDefault)
                        {
                            IsDefault = true;
                            Value = DefaultValue;
                            ask = false;
                            ok = true;
                        }

                        break;

                    case '?':
                        // Only set WantsDescription when showHelpChar is set 
                        if (showHelpChar)
                        {
                            WantsDescription = true;
                            ask = false;
                        }                       
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
                            ok = true;
                        }
                        else
                        {
                            // invalid answer => loop
                            QuestionIO.WriteError("Invalid choice");
                        }
                        break;
                }
            }
            return ok;
        }
    }
}
