using System.ServiceProcess;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Controllers
{
    public class ServiceControllerMock : IServiceControllerHost
    {
        public ServiceControllerMock()
        {
            this.Status = ServiceControllerStatus.Running;
        }

        public ServiceControllerStatus Status { get; private set; }

        public void Refresh()
        {
        }

        public void Start()
        {
            this.Status = ServiceControllerStatus.Running;
        }

        public void Stop()
        {
            this.Status = ServiceControllerStatus.Stopped;
        }

        public void Dispose()
        {
        }
    }
}