# GuardRex.JsonToCsprojConverter
This custom tooling will take a `project.json` version of `<my_app>.csproj` and convert it into a `<my_app>.csproj` file. If this tooling could be run `prerestore`*, it would offer a path to using a JSON app config file to developers that would automatically be converted into a `.csproj` file for use by MSBuild when the project is restored/built/packed/published.

*Note: `prerestore` is not currently available for `dotnet cli`. See: https://github.com/dotnet/cli/issues/3436

### Basis
[Microsoft.AspNetCore.Server.IISIntegration.Tools](https://github.com/aspnet/IISIntegration/tree/dev/src/Microsoft.AspNetCore.Server.IISIntegration.Tools)

### Installation
**FUTURE:** Add the following to your `project.json`:
```json
"scripts": {
    "prerestore": "guardrex-json-to-csproj-converter --framework %publish:FullTargetFramework%"
},
```
Note: `prerestore` is not currently available for `dotnet cli`. As a temporary workaround just to see the tool run, change `prerestore` to `precompile`, which is working right now. To see the tool run, you will need to build the project. See: https://github.com/dotnet/cli/issues/3436

**CURRENTLY:** Add the following to your `project.json`:
```json
"scripts": {
    "precompile": "guardrex-json-to-csproj-converter --framework %publish:FullTargetFramework%"
},
```

You will also need the `guardrex-json-to-csproj-converter.exe` application available via the System PATH settings. Conversely, you can provide the full path to the executable in the `precompile` script value.
```json
"scripts": {
    "precompile": "c:\<path_to_the_tool_exe>\guardrex-json-to-csproj-converter --framework %publish:FullTargetFramework%"
},
```

### Operation
This version of the tool will look for a file named `project2.json` in the application. When a `dotnet restore` is run manually or via Visual Studio, the app will be executed and the JSON file will be converted into a `.csproj` file using the app name.

Create a file named `project2.json` at the content root path (app base path) of your application with the contents shown below, which is a mocked version of a `.csproj` file in JSON format. This would be the file that you would be editing manually in the future.

When the tool runs, it will produce the `.csproj` file shown below. This the file that would be used by MSBuild to manage the project for restoring packages, building the app, packaging, and publishing. In order for this scheme to work, it will be important for MS to make `prerestore` script execution available; otherwise, the tool would need to be attached to other events (e.g., Gulp watch, hot key execution, [GuardRex Status Bar Tasks](https://marketplace.visualstudio.com/items?itemName=GuardRex.status-bar-tasks)).

### Sample `project2.json` input file:
```json
{
  "Project": {
    "@xmlns": "http://schemas.microsoft.com/developer/msbuild/2003",
    "@ToolsVersion": "14.0",
    "@DefaultTargets": "Build",
    // Here is a JSON5 inline comment
    "Import": [
      {
        "@Project": "$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props",
        "@Condition": "Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')"
      },
      { "@Project": "$(MSBuildToolsPath)\\Microsoft.CSharp.Targets" }
    ],
    /*
      Here is a
      multiline
      JSON5 comment
    */
    "PropertyGroup": [
      {
        "MinimumVisualStudioVersion": "14.0",
        "Configuration": {
          "@Condition": "'$(Configuration)' == ''",
          "#text": "Debug"
        },
        "Platform": {
          "@Condition": "'$(Platform)' == ''",
          "#text": "x64"
        },
        "ProjectGuid": "{5E37EDEB-4E0A-4599-815E-7D0DEE966FCD}",
        "OutputType": "Exe",
        "AppDesignerFolder": "Properties",
        "RootNamespace": "coreApp",
        "AssemblyName": "coreApp",
        "DefaultLanguage": "en-US",
        "FileAlignment": "512",
        "TargetFrameworkIdentifier": ".NETCoreApp",
        "TargetFrameworkVersion": "v1.0",
        "AddAdditionalExplicitAssemblyReferences": "false",
        "NuGetTargetMoniker": ".NETCoreApp,Version=v1.0",
        "BaseNuGetRuntimeIdentifier": "win7",
        "HostExe": "dotnet",
        "HostExtension": ".exe",
        "NoStdLib": "true",
        "NoWarn": "$(NoWarn);1701",
        "UseVSHostingProcess": "false"
      },
      {
        "@Condition": "'$(Configuration)|$(Platform)' == 'Debug|x64'",
        "DebugSymbols": "true",
        "OutputPath": "bin\\x64.Debug.Win",
        "DefineConstants": "DEBUG;TRACE",
        "DebugType": "full",
        "PlatformTarget": "x64"
      },
      {
        "@Condition": "'$(Configuration)|$(Platform)' == 'Release|x64'",
        "OutputPath": "bin\\x64.Release.Win",
        "DefineConstants": "TRACE",
        "Optimize": "true",
        "NoStdLib": "true",
        "DebugType": "pdbonly",
        "PlatformTarget": "x64"
      },
      {
        "_TargetFrameworkDirectories": "$(MSBuildThisFileDirectory)",
        "_FullFrameworkReferenceAssemblyPaths": "$(MSBuildThisFileDirectory)",
        "AutoUnifyAssemblyReferences": "true",
        "AutoGenerateBindingRedirects": "false",
        "StartAction": "Program",
        "StartProgram": "$(TargetDir)dotnet.exe",
        "StartArguments": "$(TargetPath)",
        "DebugEngines": "{2E36F1D4-B23C-435D-AB41-18E608940038}"
      }
    ],
    "ItemGroup": [
      {
        "None": { "@Include": "project.json" }
      },
      {
        "Compile": [
          { "@Include": "Program.cs" },
          { "@Include": "Properties\\AssemblyInfo.cs" }
        ]
      },
      {
        "ProjectReference": {
          "@Include": "..\\NetCoreLibrary\\NetCoreLibrary.csproj",
          "Project": "{e5ed1d08-3ae9-4c05-8ca4-9e2981a0f9cf}",
          "Name": "NetCoreLibrary"
        }
      }
    ]
  }
}
```

### Sample `<my_app>.csproj` output file:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="14.0" DefaultTargets="Build">
  <!-- Here is a JSON5 inline comment-->
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.Targets" />
  <!--
      Here is a
      multiline
      JSON5 comment
    -->
  <PropertyGroup>
    <MinimumVisualStudioVersion>14.0</MinimumVisualStudioVersion>
    <Configuration Condition="'$(Configuration)' == ''">Debug</Configuration>
    <Platform Condition="'$(Platform)' == ''">x64</Platform>
    <ProjectGuid>{5E37EDEB-4E0A-4599-815E-7D0DEE966FCD}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>coreApp</RootNamespace>
    <AssemblyName>coreApp</AssemblyName>
    <DefaultLanguage>en-US</DefaultLanguage>
    <FileAlignment>512</FileAlignment>
    <TargetFrameworkIdentifier>.NETCoreApp</TargetFrameworkIdentifier>
    <TargetFrameworkVersion>v1.0</TargetFrameworkVersion>
    <AddAdditionalExplicitAssemblyReferences>false</AddAdditionalExplicitAssemblyReferences>
    <NuGetTargetMoniker>.NETCoreApp,Version=v1.0</NuGetTargetMoniker>
    <BaseNuGetRuntimeIdentifier>win7</BaseNuGetRuntimeIdentifier>
    <HostExe>dotnet</HostExe>
    <HostExtension>.exe</HostExtension>
    <NoStdLib>true</NoStdLib>
    <NoWarn>$(NoWarn);1701</NoWarn>
    <UseVSHostingProcess>false</UseVSHostingProcess>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x64'">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\x64.Debug.Win</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <DebugType>full</DebugType>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
  <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Release|x64'">
    <OutputPath>bin\x64.Release.Win</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <Optimize>true</Optimize>
    <NoStdLib>true</NoStdLib>
    <DebugType>pdbonly</DebugType>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
  <PropertyGroup>
    <_TargetFrameworkDirectories>$(MSBuildThisFileDirectory)</_TargetFrameworkDirectories>
    <_FullFrameworkReferenceAssemblyPaths>$(MSBuildThisFileDirectory)</_FullFrameworkReferenceAssemblyPaths>
    <AutoUnifyAssemblyReferences>true</AutoUnifyAssemblyReferences>
    <AutoGenerateBindingRedirects>false</AutoGenerateBindingRedirects>
    <StartAction>Program</StartAction>
    <StartProgram>$(TargetDir)dotnet.exe</StartProgram>
    <StartArguments>$(TargetPath)</StartArguments>
    <DebugEngines>{2E36F1D4-B23C-435D-AB41-18E608940038}</DebugEngines>
  </PropertyGroup>
  <ItemGroup>
    <None Include="project.json" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\NetCoreLibrary\NetCoreLibrary.csproj">
      <Project>{e5ed1d08-3ae9-4c05-8ca4-9e2981a0f9cf}</Project>
      <Name>NetCoreLibrary</Name>
    </ProjectReference>
  </ItemGroup>
</Project>
```

### Version History
Version | Changes Made
------- | ------------
1.0.0   | Initial Release
