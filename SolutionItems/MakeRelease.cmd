mkdir ..\release
SET release=..\release\SetupPackage-2.0.0
mkdir %release%
mkdir %release%\dist
mkdir %release%\config
       
copy ..\src\Setup\bin\Release\log4net.dll %release%
copy ..\src\Setup\bin\Release\Newtonsoft.Json.dll %release%
copy ..\src\Setup\bin\Release\Setup.exe %release%
copy ..\src\Setup\bin\Release\Setup.exe.config %release%
copy ..\src\Setup\bin\Release\Setup.log4net %release%
copy ..\src\Setup\bin\Release\SURFnet.Authentication.Adfs.Plugin.Setup.dll %release%
copy ..\src\Setup\bin\Release\SURFnet.Authentication.Adfs.Plugin.Setup.dll.config %release%

copy ..\src\Setup\bin\Release\config\gateway.pilot.stepup.surfconext.nl.xml %release%\config
copy ..\src\Setup\bin\Release\config\sa-gw.surfconext.nl.xml %release%\config
copy ..\src\Setup\bin\Release\config\sa-gw.test.surfconext.nl.xml %release%\config
copy ..\src\Setup\bin\Release\config\SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json %release%\config

copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\log4net.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Logging.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Protocols.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Tokens.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Tokens.Saml.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Tokens.xml %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Xml.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Newtonsoft.Json.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\SURFnet.Authentication.ADFS.MFA.Plugin.log4net %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\SURFnet.Authentication.Adfs.Plugin.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\SURFnet.Authentication.Adfs.Plugin.dll.config %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Sustainsys.Saml2.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Configuration.ConfigurationManager.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.AccessControl.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.Cryptography.Xml.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.Permissions.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.Principal.Windows.dll %release%\dist
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.ValueTuple.dll %release%\dist
