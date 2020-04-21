using SURFnet.Authentication.Adfs.Plugin.Setup.Common;
using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public class V1CheckAndFix
    {
        public static int FixLog4netCfg()
        {
            const string wrongtext = "level value=\"Information\"";
            const string newtext = "level value=\"INFO\"";

            int rc = 0;

            string filepath = FileService.OurDirCombine(FileDirectory.AdfsDir, Values.Log4netCfgFilename);
            try
            {
                var fi = new FileInfo(filepath);
                if ( fi.Exists )
                {
                    LogService.Log.Info("Found a log4net configuration file");

                    string text;
                    using (var sr = fi.OpenText() )
                    {
                        text = sr.ReadToEnd();
                    }

                    if ( (text!=null) && (-1 != text.IndexOf(wrongtext)) )
                    {
                        LogService.Log.Info("    It does contain the wrong level.");
                        // gnarf, the bug is there!
                        Console.WriteLine("The V1.0.* log4net configuration contains a bug, it logs too much.");
                        if ( 'y' == AskYesNo.Ask("       We recommend STRONGLY to let us fix this", 'y') )
                        {
                            LogService.Log.Info("    Permission to fix.");
                            // OK, write.
                            text = text.Replace(wrongtext, newtext);
                            text = Regex.Replace(text, "(?<!\r)\n", "\r\n"); // while we are there anyway, make it PC world....
                            using (var sw = new StreamWriter(fi.OpenWrite()) )
                            {
                                sw.Write(text);
                                sw.Flush();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogService.WriteFatalException($"Error while analyzing file {Values.Log4netCfgFilename}", e);
            }

            return rc;
        }
    }
}
