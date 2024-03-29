@rem Get the root directory of the project
@pushd %~dp0\..
@set root_dir=%CD%
@popd

@rem Default to an unsigned release build
@set build=Release
@set sign=0

@rem Parse command line arguments
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

@echo.
@echo ------------------------------------------------------------
@echo Version: %version%
@echo Build:   %build%
@if %sign% == 1 (
  @echo Signing: Enabled
) else (
  @echo Signing: Disabled
)
@echo ------------------------------------------------------------
@echo.

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
@rem Copy the files for running Setup.exe to the root of the release directory   
copy %root_dir%\src\Setup\bin\%build%\CommandLine.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\log4net.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Newtonsoft.Json.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Setup.exe %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Setup.exe.config %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\Setup.log4net %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\SURFnet.Authentication.Adfs.Plugin.Setup.dll %release% || goto :error
copy %root_dir%\src\Setup\bin\%build%\SURFnet.Authentication.Adfs.Plugin.Setup.dll.config %release% || goto :error

@rem Copy the Setup configuration for SURF's SURFsecureID service to the config directory
copy %root_dir%\src\Setup\bin\%build%\config\sa-gw.surfconext.nl.xml %release%\config || goto :error
copy %root_dir%\src\Setup\bin\%build%\config\sa-gw.test.surfconext.nl.xml %release%\config || goto :error
copy %root_dir%\src\Setup\bin\%build%\config\SURFnet.Authentication.ADFS.MFA.Plugin.Environments.json %release%\config || goto :error

@rem Copy the plugin binaries and their dependencies to the dist directory
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

@rem Copy the NameIDfromType examples extension to the extensions directory so that is is included in the release
@rem It is not installed by default
copy %root_dir%\src\SURFnet.Authentication.Adfs.Plugin.Extensions.Samples\bin\%build%\SURFnet.Authentication.Adfs.Plugin.Extensions.Samples.dll %release%\extensions || goto :error

@rem Copy the documentation to the root of the release directory
copy %root_dir%\CHANGELOG %release% || goto :error
copy %root_dir%\LICENSE %release% || goto :error
copy %root_dir%\NOTICE %release% || goto :error
copy %root_dir%\INSTALL.md %release% || goto :error
copy %root_dir%\UPGRADE.md %release% || goto :error
copy %root_dir%\KNOWN_ISSUES.md %release% || goto :error
copy %root_dir%\CONFIGURATION.md %release% || goto :error

@rem Message when signing is not enabled
@if "%sign%" == "0" (
  @echo.
  @echo ------------------------------------------------------------
  @echo Signing is not enabled, the release will not be signed  
  @echo To sign the release, run MakeRelease.cmd -sign %version%
  @echo ------------------------------------------------------------
  @echo.
)

@rem Use signtool.exe from the windows SDK to create timestamped signature is requested
@rem First we sign Setup.exe, later we sign the self extracting archive

@rem Set the RFC 3161 timestamp service to use
@rem @set timestampservice=http://timestamp.digicert.com
@set timestampservice=http://timestamp.sectigo.com

@if "%sign%" == "1" (
  @echo Signing Setup.exe
  @echo signtool.exe sign /tr %timestampservice% /td sha256 /fd sha256 /a %release%\Setup.exe
  signtool.exe sign /tr %timestampservice% /td sha256 /fd sha256 /a %release%\Setup.exe || goto :error
)

@echo Making Self extracting archive
@rem delete the self extracting archive if it already exists
@if exist %release%.exe (
  @echo Deleting %release%.exe
  del %release%.exe || goto :error
)
"C:\Program Files\7-Zip\7z.exe" a -bb3 -sfx7z.sfx -r %release%.exe %release%  || goto :error

@if "%sign%" == "1" (
  @echo Signing SetupPackage self extracting archive
  echo signtool.exe sign /tr %timestampservice% /td sha256 /fd sha256 /a %release%.exe
  signtool.exe sign /tr %timestampservice% /td sha256 /fd sha256 /a %release%.exe || goto :error
)

@echo Sucessfully created Release %release%.exe
@echo.
@echo.
@exit /b 0

:error
@echo Building setup package failed
@echo.
@echo.
@exit /b %errorlevel%