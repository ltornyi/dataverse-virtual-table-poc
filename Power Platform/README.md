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

This is just to check your setup and tooling. Complete [this exercise](https://learn.microsoft.com/en-us/training/modules/extend-plug-ins/exercise). The exercise uses Visual Studio, which is slightly different - [this tutorila](https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio-code) can be helpful; the key difference is to use the Command Palette to create the project.
