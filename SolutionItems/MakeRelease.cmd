@rem Get the root directory of the project
@pushd %~dp0\..
@set root_dir=%CD%
@popd

@set build=Release
@set sign=0

@if [%1] == [-debug] (
  @echo Enabled debug release
  @set build=Debug
  @shift
)

@if [%1] == [-sign] (
  @echo Enabled signing release
  @set sign=1
  @shift
)

@if [%1] == [] (
  @echo Missing parameter: version
  @echo.
  @echo Usage: MakeRelease.cmd [-debug] [-sign] ^<version^>
  @echo.
  @echo Create a SetupPackage for release to customers from the VisualStudio build of the solution. 
  @echo Optionally signs the release.
  @echo.
  @echo ^<version^>   String describing the released version, this becomes part of the SetupPackage 
  @echo             name and directory. It has no effect on the files included in the SetupPackage.
  @echo.
  @echo -debug      Make a debug release. This includes files from bin/Debug instead of from the
  @echo             bin/Release directories. By default a release build is created.
  @echo.
  @echo -sign       Signs the release.
  goto :error
)

@set version=%1

mkdir %root_dir%\release
@set release=%root_dir%\release\SetupPackage-%version%
@if [%build%] == [Debug] (
	set release=%root_dir%\release\SetupPackage-debug-%version%
)

@if exist %release%\ (
  @echo Deleting %release%
  rmdir /S /Q %release% || goto :error
)

@echo Creating %release%
mkdir %release% || goto :error
mkdir %release%\dist || goto :error
mkdir %release%\config || goto :error
mkdir %release%\extensions || goto :error
   
@echo Copying files   
copy %root_dir%\src\Setup\bin\%build%\CommandLine.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\log4net.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Newtonsoft.Json.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Setup.exe %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Setup.exe.config %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Setup.log4net %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\SURFnet.Authentication.Adfs.Plugin.Setup.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\SURFnet.Authentication.Adfs.Plugin.Setup.dll.config %release% || goto :error

copy %root_dir%\src\Setup\bin\%build%\config\sa-gw.surfconext.nl.xml %release%\config || goto :error
copy %root_dir%\src\Setup\bin\%build%\config\sa-gw.test.surfconext.nl.xml %release%\config || goto :error
copy %root_dir%\src\Setup\bin\%build%\config\SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json %release%\config || goto :error

copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\log4net.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Microsoft.IdentityModel.Logging.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Microsoft.IdentityModel.Protocols.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Microsoft.IdentityModel.Tokens.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Microsoft.IdentityModel.Tokens.Saml.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Microsoft.IdentityModel.Xml.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Newtonsoft.Json.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\SURFnet.Authentication.ADFS.MFA.Plugin.log4net %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\SURFnet.Authentication.Adfs.Plugin.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\SURFnet.Authentication.Adfs.Plugin.dll.config %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\Sustainsys.Saml2.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\System.Configuration.ConfigurationManager.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\System.Security.AccessControl.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\System.Security.Cryptography.Xml.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\System.Security.Permissions.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\System.Security.Principal.Windows.dll %release%\dist || goto :error
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin\bin\%build%\System.ValueTuple.dll %release%\dist || goto :error

copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin.Extensions.Samples\bin\%build%\SURFnet.Authentication.Adfs.Plugin.Extensions.Samples.dll %release%\extensions || goto :error

copy %root_dir%\CHANGELOG %release% || goto :error
copy %root_dir%\LICENSE %release% || goto :error
copy %root_dir%\NOTICE %release% || goto :error
copy %root_dir%\INSTALL.md %release% || goto :error
copy %root_dir%\UPGRADE.md %release% || goto :error
copy %root_dir%\KNOWN_ISSUES %release% || goto :error

@if "%sign%" == "0" (
	@choice /m "Sign release?" /c YN
	@if "%errorlevel%" == "1" set sign=1
)

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