# Power Platform implementation

## Create custom data provider plugin

### Set up developer environment and IDE

You will need a Windows machine. If you are on a Mac, see running Windows in a virtual machine in `README_Windows_on_Mac.md`.

### Install Visual Studio

1. Download the Visual Studio Community Edition installer from [here](https://visualstudio.microsoft.com/downloads/). Start the installer.
2. From the list of workloads, select ".NET desktop development". On the right-hand side, under Installation details make sure to also select the item ".NET Framework 4.6.2 - 4.7.1 development tools".
3. Launch Visual Studio

### Create sample plugin and deploy to Dataverse

To check the end-to-end process and setup, complete [this exercise](https://learn.microsoft.com/en-us/training/modules/extend-plug-ins/exercise). 

1. When you are creating the project, start from the "Class library (.NET Framework)" template. Check "Place solution and project in the same directory" and doublecheck the framework selected is ".NET Framework 4.6.2".
2. Add the microsoft.crmsdk.coreassemblies NuGet package to the project.
3. Follow the instructions to create the *PreOperationFormatPhoneCreateUpdate* class.
4. Configure signing the assembly as the exercise prescribes.
5. Build the project, check you have the build output in the `bin/Debug` folder.
6. Download the Plugin Registration Tool (PRT) directly from [here](https://www.nuget.org/packages/Microsoft.CrmSdk.XrmTooling.PluginRegistrationTool).
7. Once you have the PRT NuGet package downloaded, rename it to .zip, extract it and run the executable. You might want to create a shortcut on your desktop for convenience.
8. Follow the instructions to create a connection but DON'T enter a username or a password - Microsoft login will be launched and will guide you through the login process.
9. Continue with the assembly and plug-on steps registrations.
10. Test the plugin; create/update a contact and observe how only the digits remain in the Business Phone field.

The sample code is in the `ExerciseProject` folder; feel free to complete the rest of the exercise. Once you are confident you can build and deploy a plug-in assembly, unregister the sample assembly from Dataverse.