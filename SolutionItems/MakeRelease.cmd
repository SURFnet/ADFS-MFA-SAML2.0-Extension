@IF [%1] == [] (
    @echo Missing parameter.
	@echo.
	@echo Usage: MakeRelease.cmd ^<version^>
	@echo.
	goto :error
)

SET version=%1

mkdir ..\release
SET release=..\release\SetupPackage-%version%

@if exist %release%\ (
  @echo Deleting %release%
  rmdir /S /Q %release% || goto :error
)

@echo Creating %release%
mkdir %release% || goto :error
mkdir %release%\dist || goto :error
mkdir %release%\config || goto :error
   
@echo Copying files   
copy ..\src\Setup\bin\Release\log4net.dll %release% || goto :error
copy ..\src\Setup\bin\Release\Newtonsoft.Json.dll %release% || goto :error
copy ..\src\Setup\bin\Release\Setup.exe %release% || goto :error
copy ..\src\Setup\bin\Release\Setup.exe.config %release% || goto :error
copy ..\src\Setup\bin\Release\Setup.log4net %release% || goto :error
copy ..\src\Setup\bin\Release\SURFnet.Authentication.Adfs.Plugin.Setup.dll %release% || goto :error
copy ..\src\Setup\bin\Release\SURFnet.Authentication.Adfs.Plugin.Setup.dll.config %release% || goto :error

copy ..\src\Setup\bin\Release\config\gateway.pilot.stepup.surfconext.nl.xml %release%\config || goto :error
copy ..\src\Setup\bin\Release\config\sa-gw.surfconext.nl.xml %release%\config || goto :error
copy ..\src\Setup\bin\Release\config\sa-gw.test.surfconext.nl.xml %release%\config || goto :error
copy ..\src\Setup\bin\Release\config\SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json %release%\config || goto :error

copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\log4net.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Logging.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Protocols.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Tokens.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Tokens.Saml.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Microsoft.IdentityModel.Xml.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Newtonsoft.Json.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\SURFnet.Authentication.ADFS.MFA.Plugin.log4net %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\SURFnet.Authentication.Adfs.Plugin.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\SURFnet.Authentication.Adfs.Plugin.dll.config %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\Sustainsys.Saml2.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Configuration.ConfigurationManager.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.AccessControl.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.Cryptography.Xml.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.Permissions.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.Security.Principal.Windows.dll %release%\dist || goto :error
copy ..\src\SURFnet.Authentication.Adfs.Plugin\bin\Release\System.ValueTuple.dll %release%\dist || goto :error

copy ..\CHANGELOG %release% || goto :error
copy ..\LICENSE %release% || goto :error
copy ..\NOTICE %release% || goto :error
copy ..\INSTALL %release% || goto :error
copy ..\UPGRADE %release% || goto :error
copy ..\KNOWN_ISSUES %release% || goto :error

@set sign=0
@choice /m "Sign release?" /c YN
 @if "%errorlevel%" == "1" set sign=1

@if "%sign%" == "1" (
  @echo Signing Setup.exe
  signtool sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a %release%\Setup.exe || goto :error
)

@echo Making Self extracting archive
del %release%.exe
"C:\Program Files\7-Zip\7z.exe" a -bb3 -sfx7z.sfx -r %release%.exe %release%

@if "%sign%" == "1" (
  @echo Signing SetupPackage self extracting archive
  signtool sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a %release%.exe || goto :error
)

@echo Sucessfully created Release %release%.exe
@echo.
@exit /b 0

:error
@echo Building setup package failed
@echo.
@exit /b %errorlevel%