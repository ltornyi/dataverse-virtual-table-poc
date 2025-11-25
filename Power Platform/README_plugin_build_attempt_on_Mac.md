# Power Platform implementation

## Create custom data provider plugin

### Set up developer environment and IDE

#### Set up VSCode

* Install [.NET SDK 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) or use Homebrew:

    brew install --cask dotnet-sdk

* Install the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
* Install [Power Platform tools extension](https://marketplace.visualstudio.com/items?itemName=microsoft-IsvExpTools.powerplatform-vscode)
* run the `pac` command to see what commands are available; for example `pac admin`.
* create your local PAC profile

    pac auth create --name profile_name --url your_instance_url

or

    pac auth create --environment your_environment_id

where `profile_name` is how you want this environment connection to appear, your_instance_url is the power platform environment URL and your_environment_id is the ID of the environment you want to connect to. 6b1f9fd8-9973-e858-92f4-7fb9e31b2743. You can get these for example from the maker portal, click the cogwheel (Settings icon), then Session details.

You can also manage the authentication profiles in the VSCode extension as well. Check your Power Platform connection with

    pac env list

and set your default environment with

    pac env select --environment [id_or_url_or_name]

### Create and deploy a sample plug-in

#### Create and build plug-in

Complete [this exercise](https://learn.microsoft.com/en-us/training/modules/extend-plug-ins/exercise). The exercise uses Visual Studio, which is quite different - [this tutorila](https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio-code) can be helpful. The key difference is to use the Command Palette to create the project. To add dependencies, use the Command Palette `NuGet: Add NuGet Package` command or right-click in the Solution explorer and select `Add NuGet package`. Use `Plugins` as your root folder.

To sign and build the project, follow the steps below

1. install mono and create signing key

    brew install mono
    sn -k signingkey.snk

2. Put signingkey.snk in the project folder. Do NOT commit the private key — add it to .gitignore.
3. Edit the project file (.csproj) to enable signing. Example change:

    // ...existing code...
    <PropertyGroup>
        <!-- Enable strong-name signing -->
        <SignAssembly>true</SignAssembly>
        <AssemblyOriginatorKeyFile>signingkey.snk</AssemblyOriginatorKeyFile>
        <!-- Optional: use PublicSign on build systems where private keys are not available -->
        <!-- <PublicSign>true</PublicSign> -->
    </PropertyGroup>

    <!-- Ensure the key file is included in the project (but ignored by git) -->
    <ItemGroup>
        <None Include="signingkey.snk" />
    </ItemGroup>
    // ...existing code...
4. Build the project from the solution explorer or by right-clicking on the .csproj file and selecting `Build`
5. Verify that the build is signed

    sn -v bin/Debug/net9.0/ExerciseProject.dll

The output should be `Assembly bin/Debug/net9.0/ExerciseProject.dll is strongnamed.`

#### Register the plug-in and steps

Continue following the [exercise](https://learn.microsoft.com/en-us/training/modules/extend-plug-ins/exercise) from Task 3. You will need a Windows machine to complete this step. Also, download the Plugin Registration Tool directly from [here](https://www.nuget.org/packages/Microsoft.CrmSdk.XrmTooling.PluginRegistrationTool). Once you have the nuget package downloaded, rename it to .zip, extract it and run the executable. You can also use [XrmToolbox](https://www.xrmtoolbox.com/).

You might have to unblock the ddl files after extracting the .zip; useful command:

    get-childitem "./tools" | unblock-file

THIS WON'T WORK - Dataverse plugins must target .net framework 4.6.2 which is Windows-only.