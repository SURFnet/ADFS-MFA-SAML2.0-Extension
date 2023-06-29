@echo off

@rem List the publickey token used in the strong name signature of the relevant asseblies
@rem The publickey token is the ASCII hex representation of a hash that identifies the keypair (snk) that was 
@rem used to create the strong name 
@rem The publickeytoken hash is first 8 bytes of the (20 byte long) SHA-1 hash of the public key.

@rem SURFnet.Authentication.Adfs.Plugin.public.snk contains the public key of SURFnet.Authentication.Adfs.Plugin.snk
@echo SURFnet release builds must be signed with this token:
@echo |set /p dummy=SURFnet.Authentication.Adfs.Plugin.public.snk 
sn.exe -q -t SURFnet.Authentication.Adfs.Plugin.public.snk
@echo:

set project=..\src\SURFnet.Authentication.Adfs.Plugin\bin
for %%r in (Release Debug) do (
	for %%a in (SURFnet.Authentication.Adfs.Plugin.dll Sustainsys.Saml2.dll) do (
	   @echo | set /p dummy=%%r\%%a 
	   sn.exe -q -T %project%\\%%r\\%%a
	)
	echo:
)