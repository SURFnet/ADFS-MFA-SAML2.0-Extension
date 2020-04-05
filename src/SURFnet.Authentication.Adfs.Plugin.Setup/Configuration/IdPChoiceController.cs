using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class IdPChoiceController
    {
        private readonly List<Dictionary<string, string>> IdPEnvironments;

        public int IdPindex { get; private set; }

        private OptionList idpOptions;

        public IdPChoiceController(List<Dictionary<string, string>> idpEnvironments, Setting idpSetting)
        {
            IdPEnvironments = idpEnvironments;
            CreateOptionList();
        }


        public bool Ask()
        {
            bool ok = false;



            QuestionIO.WriteEndSeparator();
            return ok;
        }

        /* * * * * * * * * * * */


        private void CreateOptionList()
        {
            string[] options = new string[IdPEnvironments.Count];

            for (int index = 0; index < options.Length; index++)
            {
                options[index] = IdPIndexToEnvString(index);
            }

            var ol = new OptionList()
            {
                Introduction = "There are different SingleFactorOnly gateways, their names suggest their usage.",
                Options = options,
                Question = "Select a SingleFactorOnly gateway environment"
            };
        }

        private string IdPIndexToEnvString(int index)
        {
            var env = IdPEnvironments[index];
            return $"  {IdPChoiceUtil.Index2Digit(index)}. {env[SetupConstants.GwEnvironmentType]}";
        }
    }
}
