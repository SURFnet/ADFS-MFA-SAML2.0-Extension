using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration
{
    public class ADUserAttributes : IDisposable
    {
        private bool isDisposed;

        public DirectoryEntry UserObject { get; private set; }

        public IList<GroupPrincipal> UserGroups { get; private set; }

        public ADUserAttributes(DirectoryEntry userObject, IList<GroupPrincipal> userGroups)
        {
            UserObject = userObject;
            UserGroups = userGroups;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                UserObject.Dispose();
                UserObject = null;
                
                foreach (var group in UserGroups)
                {
                    group.Dispose();
                }

                UserGroups.Clear();
            }

            isDisposed = true;
        }

    }
}
