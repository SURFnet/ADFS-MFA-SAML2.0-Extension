// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IServiceController.cs" company="Winvision bv">
//   Copyright (c) Winvision bv. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.ServiceProcess;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Controllers
{
    public interface IServiceControllerHost : IDisposable
    {
        ServiceControllerStatus Status { get; }

        void Refresh();

        void Start();

        void Stop();
    }
}