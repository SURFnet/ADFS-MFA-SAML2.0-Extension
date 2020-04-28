using SURFnet.Authentication.Adfs.Plugin.Setup.Question;
using SURFnet.Authentication.Adfs.Plugin.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SURFnet.Authentication.Adfs.Plugin.Setup.Configuration
{
    public class CertImportPfx
    {
        public X509Certificate2 ResultCertificate { get; private set; }

        public void Cleanup()
        {
            if ( ResultCertificate != null )
                ResultCertificate = SetupCertService.CertDispose(ResultCertificate);
        }

        public int Doit()
        {
            int rc = -1;
            string pfxpath = null;
            string pwd = null;

            OpenFileDialog dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "PFX files (*.pfx)|*.pfx|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = FileService.RegistrationDataFolder,
                Multiselect = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pfxpath = dialog.FileName;  // is a fullpath

                // First temporary load to display the cert! It will not be in a proper provider!
                bool tryPwd = true;
                while (tryPwd)
                {
                    if (false == AskPwd(out pwd))
                    {
                        // abort
                        LogService.Log.Info("Break from ask PFX pwd");
                        rc = -1;
                        break;
                    }
                    else
                    {
                        LogService.Log.Info("Try importing Cert");
                        rc = Import(pfxpath, pwd); //temp load.
                        if ( rc == 0 )
                        {
                            // Yes Ephemeral loaded
                            tryPwd = false; // Apparently the pwd works
                            
                            // like it?
                            if (0 != SPCertController.DisplayAndAskOK(ResultCertificate, out bool accepted) )
                            {
                                // abort
                                LogService.Log.Warn("  Abort on first imported cert");
                                rc = -2;
                                break; // from tryPwd loop
                            }
                            else
                            {
                                LogService.Log.Info("  Temp imported.");
                                if (accepted)
                                {
                                    rc = 0; // likes it; goto real import.
                                }
                                else
                                {
                                    LogService.Log.Warn("  However, not satisfactory for admin.....");
                                    rc = 1; // stop with this one.
                                }
                            }

                            // Clear memory of Ephemeral!
                            ResultCertificate = SetupCertService.CertDispose(ResultCertificate);
                        }
                        // else: import failed. rc!=0; and will retry.
                    }
                } // tryPwd 

                // If it was ok so far load permanent and add to store.
                if ( rc==0 )
                {
                    // Cert was OK, add to store.
                    rc = Import(pfxpath, pwd, true);
                    if ( rc == 0 )
                    {
                        // Now it is in the real provider. And we can "Validate" it.
                        LogService.Log.Info("  Loaded for store.");
                        if ( SetupCertService.IsValidSPCert(ResultCertificate) )
                        {
                            X509Store store = new X509Store("MY", StoreLocation.LocalMachine);
                            store.Open(OpenFlags.ReadWrite);
                            store.Add(ResultCertificate);
                            store.Close();
                            LogService.Log.Info("  Added to store.");
                        }
                        else
                        {
                            LogService.Log.Info("  Cert validation failed, should delete from disk.");
                            // mmm, key is on disk
                            // TODONOW:  Must delete the key!!
                            DeleteKey(ResultCertificate);
                            ResultCertificate = SetupCertService.CertDispose(ResultCertificate);
                        }
                    }
                }
            }
            else
            {
                LogService.Log.Warn("OpenFileDialog abort");
                rc = -2;
            }

            return rc;
        }

        private void DeleteKey(X509Certificate2 resultCertificate)
        {
            QuestionIO.WriteError("DeleteKey not yet implement!!!!");

            // create CSPParms(Containername); alg = new RSAProvider(CSPParms); alg.PersistKeyInCsp=false; alg.Clear()
        }

        public int  Import(string filepath, string password, bool persist = false)
        {
            int rc = -1;


            var cert = new X509Certificate2();
            try
            {
                X509KeyStorageFlags flags;
                if (persist)
                    flags = X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet;
                else
                    flags = (X509KeyStorageFlags)32; // X509KeyStorageFlags.EphemeralKeySet; But not in this .NET????

                cert.Import(filepath, password, flags);

                ResultCertificate = cert;
                rc = 0;
            }
            catch (CryptographicException crex)
            {
                LogService.Log.Warn("CryptographicException: " + crex.Message);
                QuestionIO.WriteError(crex.Message);
            }
            catch (Exception ex)
            {
                LogService.WriteFatalException("Import certificate failed", ex);
                if (cert != null)
                {
                    cert = SetupCertService.CertDispose(cert);

                }
            }

            return rc;
        }

        private bool AskPwd(out string pwd)
        {
            bool ok = false;
            pwd = String.Empty;

            var x = new ShowAndGetString("  Give password for PFX file");
            ok = x.Ask();
            if (ok)
                pwd = x.Value;

            return ok;
        }
    }
}
