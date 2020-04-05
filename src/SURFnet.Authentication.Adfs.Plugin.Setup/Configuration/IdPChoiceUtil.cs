using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public static class IdPChoiceUtil
    {
        public static int EntityID2Index(string entityID, List<Dictionary<string, string>> idpdictionaries)
        {
            int index = -1;

            for (int i = 0; i < idpdictionaries.Count; i++)
            {
                var env = idpdictionaries[i];
                string s1 = env[ConfigSettings.IdPEntityId];  // throws if error in json file.
                if (string.CompareOrdinal(s1, entityID) == 0)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }


        /*
         * Integer index and list ralated things.
         * The program works with 0 based indices. The UI works with 1 based digits.
         */
        public static int Digit2Index(char digit)
        {
            int i = digit - '0' - 1;
            return i;
        }

        public static char Index2Digit(int index)
        {
            char digit = (char)(index + 1 + '0');
            return digit;
        }
    }
}
