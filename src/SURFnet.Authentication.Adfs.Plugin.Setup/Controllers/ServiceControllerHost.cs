using System;
using System.ServiceProcess;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Controllers
{
    public class ServiceControllerHost : IServiceControllerHost
    {
        private readonly ServiceController serviceController;

        public ServiceControllerHost()
        {
            this.serviceController = new ServiceController("adfssrv");
        }

        public ServiceControllerStatus Status => this.serviceController.Status;

        public void Refresh()
        {
            this.serviceController.Refresh();
        }

        public void Start()
        {
            this.serviceController.Start();
        }

        public void Stop()
        {
            this.serviceController.Stop();
        }

        public void Dispose()
        {
            this.serviceController.Dispose();
        }
    }
}