using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class ADUserAttributes : IDisposable
    {
        private bool isDisposed;

        public ADUserAttributes(DirectoryEntry userObject, IList<string> userGroups)
        {
            this.UserObject = userObject;
            this.UserGroups = userGroups;
        }

        public DirectoryEntry UserObject { get; private set; }

        public readonly IList<string> UserGroups;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.UserObject.Dispose();
                this.UserObject = null;
                this.UserGroups.Clear();
            }

            this.isDisposed = true;
        }
    }
}