using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class IdPChoiceController : ShowListGetDigit
    {
        public int ChoosenIndex
        {
            get { return IdPChoiceUtil.Digit2Index(Value); }
        }

        public IdPChoiceController(List<Dictionary<string, string>> idpEnvironments, int defaultIndex) :
                    base(CreateOptionList(idpEnvironments), defaultIndex+1)  // list is 1 based, index 0 based
        {
        }

        /*
         * Static helpers for the constructor
         */
        private static OptionList CreateOptionList(List<Dictionary<string, string>> idpEnvironments)
        {
            string[] options = new string[idpEnvironments.Count];

            for (int index = 0; index < options.Length; index++)
            {
                var env = idpEnvironments[index];
                options[index] = IdPIndexToEnvString(env, index);
            }

            var ol = new OptionList()
            {
                Introduction = "There are different SecondFactorOnly servers, their names suggest their usage.",
                Options = options,
                Question = "Select a SecondFactorOnly gateway environment"
            };

            return ol;
        }

        private static string IdPIndexToEnvString(Dictionary<string, string> dict, int index)
        {
            return $"  {IdPChoiceUtil.Index2Digit(index)}. {dict[SetupConstants.IdPEnvironmentType]}  ({dict[ConfigSettings.IdPEntityId]})";
        }
    }
}
