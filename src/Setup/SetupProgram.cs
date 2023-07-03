using CommandLine;

using SURFnet.Authentication.Adfs.Plugin.Setup.Configuration;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using SURFnet.Authentication.Adfs.Plugin.Setup.Util;

using System;
using System.Linq;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    /// <summary>
    /// Class Program.
    /// </summary>
    public static class SetupProgram
    {
        public static bool UseMock = false;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// 
        /// The [STAThread] is really important. Many GUI things need STA model for COM.
        /// Default is MTA. We do use Certificate and Folder dialogs!
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length > 0
                && args.Any(x => x.Equals("useMock", StringComparison.OrdinalIgnoreCase)))
            {
                UseMock = true;
                FileService.InitFileServiceMock();
            }

#if DEBUG
            if (!UseMock)
            {
                Console.Write("Attach remote debugger and press any key to continue...");
                Console.ReadLine();
            }
#endif

            var response = Parser.Default.ParseArguments<SetupOptions>(args)
                                 .MapResult(
                                     ParseOptions,
                                     _ => ReturnOptions.Failure);

            Console.WriteLine("Result: {0}", response);
            Console.Write("Hit any key to exit.");
            Console.ReadLine();
            return (int)response;
        }

        private static ReturnOptions ParseOptions(SetupOptions opts)
        {
            var response = PrepareForSetup(out var state);
            if (response != ReturnOptions.Success)
            {
                return response;
            }

            try
            {
                if (opts.Check)
                {
                    RulesAndChecks.ExtraChecks(state);
                }

                if (opts.Install)
                {
                    return SetupActions.Install(state);
                }

                if (opts.Uninstall)
                {
                    return SetupActions.Uninstall(state);
                }

                if (opts.Reconfigure)
                {
                    return SetupActions.Reconfigure(state);
                }
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("LastResort catch(). Caught in Main()", ex);
                return ReturnOptions.ExtremeFailure;
            }

            return ReturnOptions.Success;
        }

        private static ReturnOptions PrepareForSetup(out SetupState state)
        {
            state = new SetupState();
            Console.WriteLine(state.SetupProgramVersion.VersionToString("Setup program version"));

            if (!UAC.HasAdministratorPrivileges())
            {
                Console.WriteLine("Must be a member of local Administrators and run with local");
                Console.WriteLine("Administrative privileges.");
                Console.WriteLine("\"Run as Administrator\" or start from an elevated command prompt");
                return ReturnOptions.Failure;
            }

            if (0 != SetupIO.InitializeLog4net())
            {
                return ReturnOptions.FatalFailure;
            }

            LogService.Log.Info("Setup program version: " + state.SetupProgramVersion);
            // TODO: get version of fundamental OS file. line "CMD.EXE" as OS version detection.

            try
            {
                FileService.InitFileService();

                var idPEnvironments = state.IdPEnvironments = ConfigurationFileService.LoadIdPDefaults();
                if (idPEnvironments == null || idPEnvironments.Count == 0)
                {
                    // Darn, no error check at low level?
                    throw new ApplicationException("Error reading the SFO server (IdP) environment descriptions.");
                }

                if (false == DetectAndRead.TryDetectAndReadCfg(state))
                {
                    Console.WriteLine("Stopping after attempted Version detection.");
                    Console.WriteLine("Check the logfile of the setup program for diagnostics.");
                    return ReturnOptions.FatalFailure;
                }

                if (0 != DetectAndRead.TryAndDetectAdfs(state))
                {
                    LogService.Log.Warn("Something failed in ADFS detection");
                    return ReturnOptions.FatalFailure;
                }

                // set default SP entityID. Has a very essential side effect:
                //   - It fills the setting Dictionary!
                ConfigSettings.SPEntityID.DefaultValue = state.IsPrimaryComputer
                    ? $"http://{state.AdfsConfig.AdfsProps.HostName}/stepup-mfa"
                    : null;

                LogService.Log.Info("Successful end of PrepareforSetup()");
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Fatal failure in Setup preparation.", ex);
                return ReturnOptions.FatalFailure;
            }

            return ReturnOptions.Success;
        }
    }
}