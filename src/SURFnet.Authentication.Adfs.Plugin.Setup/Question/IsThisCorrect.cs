using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Question
{
    public class IsThisCorrect : ShowAndGetChar
    {
        public IsThisCorrect() : base("Is this correct?", "yn", 'y')
        { 
        }
    }
}
