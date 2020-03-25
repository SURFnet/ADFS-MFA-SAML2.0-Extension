using SURFnet.Authentication.Adfs.Plugin.Setup.Assemblies;
using SURFnet.Authentication.Adfs.Plugin.Setup.Models;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Versions
{
    public class StepupComponent : ISetupHandler
    {
        public string ComponentName { get; set; }
        public AssemblySpec[] Assemblies { get; set; }
        public string ConfigFilename { get; set; }
        public FileDirectory ConfigFileDirectory = FileDirectory.AdfsDir;

        public virtual int Verify()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug($"Verifying: {ComponentName}.");
            foreach ( var spec in Assemblies )
            {
                int tmprc;
                tmprc = spec.Verify(Path.Combine(FileService.Enum2Directory(spec.Directory), spec.InternalName));
                LogService.Log.Debug("Verifying: " + spec.InternalName);
                if (tmprc != 0)
                {
                    if (rc == 0)
                        rc = tmprc;
                }
            }

            if ( null!=ConfigFilename )
            {
                string tmp = FileService.OurDirCombine(ConfigFileDirectory, ConfigFilename);
                LogService.Log.Debug($"Removing Configuration of: {ComponentName}, File: {tmp}");
                try
                {
                    if (!File.Exists(tmp))
                    {
                        // ugh no configuration file!
                        rc = -1;
                        string error = $"Configurationfile '{tmp}' missing in component:  {ComponentName}.";
                        Console.WriteLine(error);
                        LogService.Log.Fatal(error);
                    }
                }
                catch (Exception ex)
                {
                    rc = -1;
                    string error = $"Failed locate configuration of: {ComponentName}, filename: {tmp}. ";
                    Console.WriteLine(error+ex.Message);
                    LogService.Log.Fatal(error+ex.ToString());
                }
            }

            return rc;
        }

        public virtual List<Setting> ReadConfiguration()
        {
            return null;
        }

        public virtual int WriteConfiguration(List<Setting> settings)
        {
            return -1;
        }

        public virtual int Install(List<Setting> settings)
        {
            // Copy configuration

            // Copy assemblies
            return -1;
        }

        public virtual int UnInstall()
        {
            int rc = 0; // assume OK

            LogService.Log.Debug("UnInstall: "+ComponentName);

            // Delete assemblies
            foreach (var spec in Assemblies)
            {
                // bug check.
                if ( string.IsNullOrWhiteSpace(spec.FilePath) )
                {
                    LogService.Log.Fatal("StepUpComponent.UnInstall(): BugCheck! Untested file: "+spec.InternalName);
                    return -1;
                }

                try
                {
                    // no need to test for existence. It was there on Verify()!
                    LogService.Log.Debug("Deleting: " + spec.FilePath);
                    File.Delete(spec.FilePath);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"File.Delete on {spec.FilePath} failed with: {ex.Message}");
                    LogService.Log.Fatal($"File.Delete on {spec.FilePath} failed with: {ex.ToString()}");
                    rc = -1;
                }
            }

            // Delete Configuration file
            if ( null != ConfigFilename )
            {
                string tmp = FileService.OurDirCombine(ConfigFileDirectory, ConfigFilename);
                try
                {
                    // no need to test for existence. It was there on Verify()!
                    LogService.Log.Debug("Deleting configuration: " + tmp);
                    File.Delete(tmp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File.Delete on {tmp} failed with: {ex.Message}");
                    LogService.Log.Fatal($"File.Delete on {tmp} failed with: {ex.ToString()}");
                    rc = -1;
                }
            }

            return rc;
        }
    }
}
