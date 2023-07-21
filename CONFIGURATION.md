Configuration
=============

Basic configuration of the plugin is performed using the Setup.exe program. Advanced configuration requires manual editing of the configuration files. See the included [INSTALL.md](INSTALL.md) and [UPGRADE.md](UPGRADE.md) files for more information on using the Setup program.

Dynamic LoA configuration
-------------------------
Since version 2.1.0 the of the plugin supports setting the `minimalLoa` that is used during the authentication of a user based on the user's group memberships in AD. Configuration of this feature is not supported by the Setup.exe program and must be performed manually after installation of the plugin.

To use dynamic LoA, add a json file to the `Windows\ADFS` directory with a mapping of the AD group names to the LoA to use. When a user is a member of a group in this file, the LoA specified for that group is used instead of the `minimalLoa` set in the `SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file. This can be both a higher or a lower LoA. If a user is a member of multiple groups in the file, the first matching group is used, so the order of the groups in the file can be important.
The group to LoA mappings a evaluated top to bottom.

The dynamic LoA file to load must be specified in the `dynamicLoaFile` attribute of the `SfoMfaExtension` element in the `SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SfoMfaExtension schacHomeOrganization="institution-b.nl" 
                 activeDirectoryUserIdAttribute="employeeNumber" 
                 minimalLoa="http://test.surfconext.nl/assurance/sfo-level15" 
                 NameIdAlgorithm="UserIdFromADAttr"
                 dynamicLoaFile="SURFnet.Authentication.ADFS.Plugin.config.dynamicLoa.json" />
```

In this example the `SURFnet.Authentication.ADFS.Plugin.config.dynamicLoa.json` file is loaded from the `Windows\ADFS` directory. A dynamicLoa file must contain a JSON dictionary mapping the AD group names to level of assurance identifiers.
Note that when a user is a member of multiple groups in this file, the first matching group is used. So in the example below the if a user is a member of both the "Domain Admins" and the "Domain Users" group, the "Domain Admins" group is used and the user is authenticated at LoA `http://test.surfconext.nl/assurance/sfo-level3`.

The `SURFnet.Authentication.ADFS.Plugin.config.dynamicLoa.json` file`:
```json
{
  "Domain Admins": "http://test.surfconext.nl/assurance/sfo-level3",
  "LOA2": "http://test.surfconext.nl/assurance/sfo-level2",
  "Domain Users": "http://test.surfconext.nl/assurance/sfo-level1.5",
}
```

When the plugin starts it will log the dynamic LoA configuration to it's event log. For each authentication where a dynamic LoA is used, the plugin will log the actual LoA that was selected for the authentication together the user's group membership.

Custom NameID Algorithm
-----------------------

Since version 2.0.4 of the plugin it is possible to customise the algorithm used to calculate the NameID of the user. The NameID is the identifier of the user at the Stepup-Gateway.

The `NameIDAlgorithm` parameter in the `SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file in the `C:\Windows\ADFS` directory selects the NameIDAlgorithm that is used. The default NameIDAlgorithm is `UserIdFromADAttr`, this is the algorithm that is used in earlier versions of the plugin. Two new NameIDAlgorithm types were added:

1. `UserIdAndShoFromADAttr` – This algorithm reads the values of both the uid and the schacHomeOrganization from AD and is included int he plugin.
2. `NameIDFromType` – This algorithm loads a .NET assembly with a NameIDBuilder and allows anyone to add a new NameID algorithm without having to recompile the plugin. See the `SURFnet.Authentication.Adfs.Plugin.Extensions.Samples` project for a sample of such a plugin. A prebuild version of this sample is included in the extensions directory of the SetupPackage, but is not installed by the Setup program.

### Custom NameID Algorithm Configuration

The Setup program always uses the `UserIdFromADAttr` algorithm. If setup detects that another NameIDAlgorithm is being used, it displays a warning that the current configuration is not supported. Setup will attempt to, and should, leave an existing configuration intact. Setup creates a backup of any files it removes or replaces. This way Setup can still be used to upgrade the plugin or to reconfigure the other plugin settings when a custom NameIDAlgorithm is used.

Configuration of another NameIDAlgorithm must be done by editing the `SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file manually.

#### UserIdAndShoFromADAttr

The `UserIdAndShoFromADAttr` algorithm requires one additional parameter `activeDirectoryShoAttribute` that specifies the attribute in AD from which to read the value to use for schacHomeOrganization. Example configuration using the `UserIdAndShoFromADAttr` algorithm:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<SfoMfaExtension 
  NameIdAlgorithm="UserIdAndShoFromADAttr"
  minimalLoa="http://test.surfconext.nl/assurance/sfo-level2"
  activeDirectoryShoAttribute="department"
  activeDirectoryUserIdAttribute="employeeNumber" />
```

In this example the user's schacHomeOrganization is read from the "department" attribute in AD and the user's uid is read from the "employeeNumber" attribute in AD.

#### NameIDFromType

The `NameIDFromType` algorithm requires one additional parameter `GetNameIDTypeName`
with the .NET TypeName of the class to load. This class must implement the `SURFnet.Authentication.Adfs.Plugin.NameIdConfiguration.IGetNameID` interface.
See the `SURFnet.Authentication.Adfs.Plugin.Extensions.Samples` examples project for an example of such en extension. This example is included in the SetupPackage.

To use this example copy `SURFnet.Authentication.Adfs.Plugin.Extensions.Samples.dll` from the extensions directory in the SetupPackage to the `C:\Windows\ADFS` directory. Next update the `SURFnet.Authentication.ADFS.Plugin.config.xml` configuration file to load the example extension. E.g.:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<SfoMfaExtension 
  minimalLoa="http://test.surfconext.nl/assurance/sfo-level2"
  NameIdAlgorithm="NameIDFromType"
  GetNameIDTypeName="SURFnet.Authentication.Adfs.Plugin.Extensions.Samples.NameIDBuilder, SURFnet.Authentication.Adfs.Plugin.Extensions.Samples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
  UidAttribute1="employeeNumber"
  Domain1="D2012"
  Sho1="institution-a.example.com"
  UidAttribute2="sAMAccountName"
  Domain2="D2019"
  Sho2="institution-b.example.com">
</SfoMfaExtension>
```

The NameIDBuilder in this example uses different AD attributes to read the user's uid and schacHomeOrganization from, based on the domain of the user's account. The `UidAttribute1`, `Domain1` and `Sho1` parameters are used when the user's account is in the "D2012" domain. The `UidAttribute2`, `Domain2` and `Sho2` parameters are used when the user's account is in the "D2019" domain.
