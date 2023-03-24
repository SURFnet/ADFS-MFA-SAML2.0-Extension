using CommandLine;

namespace SURFnet.Authentication.Adfs.Plugin.Setup
{
    public class SetupOptions
    {
        [Option('c', HelpText = "Check/analyze existing installation only. if combined with another action will check first before executing that action.")]
        public bool Check
        {
            get;
            set;
        }

        [Option('i', HelpText = "Install (including automatic upgrade).", SetName = "optioni")]
        public bool Install
        {
            get;
            set;
        }

        [Option('r', HelpText = "Reconfigure existing installation.", SetName = "optionr")]
        public bool Reconfigure
        {
            get;
            set;
        }

        [Option('x', HelpText = "Uninstall.", SetName = "optionx")]
        public bool Uninstall
        {
            get;
            set;
        }
    }
}