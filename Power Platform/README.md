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
9. Continue with the assembly and plug-in steps registrations.
10. Test the plugin; create/update a contact and observe how only the digits remain in the Business Phone field.

The sample code is in the `ExerciseProject` folder; feel free to complete the rest of the exercise. Once you are confident you can build and deploy a plug-in assembly, unregister the sample assembly from Dataverse.

### Create custom data provider and deploy to Dataverse

Build the project and register the assembly.

1. Review the `FacilityDataProvider` project
2. Modify the `Retrieve` and `RetrieveMultiple` plug-ins and add your function app endpoint and function keys.
3. Configure signing and build the project
4. Register the assembly

Register the data provider and data source.

1. Go to the maker portal and create an unmanaged solution - this will have your data provider and virtual table.
2. Register the data provider; specify your solution and the FacilityDataProvider assembly.
3. Under Data Source Entity, choose to create a new Data Source. Fill in display name, plural name, solution and internal name. This will create a new table (it's not the virtual table).
4. Pick the event handlers - choose "Not implemented" for the create, update and delete events
5. Register the data provider

At this point, your solution will have the data source table and the data provider added. We will have to create a record in the data source table.

1. go to legacy settings i.e. cogwheel -> advanced settings
2. settings -> administration -> virtual entity data sources
3. click New, select FacilityDataProvider
4. specify a name, for example "My Facility Data Source"
5. click save&close

### Create virtual table and use the custom data source

According to the [documentation](https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-virtual-entities#open-an-unmanaged-solution), you still have to use the classic solution experience. For example click on Solutions in the maker portal and then click *Switch to classic* on the menu.

1. Open your unmanaged solution your virtual table should be part of.
2. Select *Entities* on the left and then select *New*
3. Specify mandatory fields (Display name, Plural name, Name, External Name, External Collection Name)
4. Select the data source record you created earlier.
5. click save

Your virtual table was created, continue adding columns. The clientid column should be a lookup to Accounts, otherwise be mindful of types, lengths and internal names (see the `Facility.ToEntity` method).

### Enable tracing and test your virtual table

The [documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/tutorial-write-plug-in#view-trace-logs) says to go to the classic admin experience.

1. Settings -> System -> Administration -> System Settings
2. in the customization tab, set *Enable logging to plug-in trace log* to *All*
3. Click OK.

Now you can try querying the virtual table, for example in the maker portal. There's a user interface for plug-in trace logs in the classic admin experience, under Settings -> Customization -> Plug-in Trace log.

Disable plug-in trace logs once testing is complete.

## Create customizations in Dataverse

### Customize tables

1. Create a custom table called Ticket with a mandatory lookup to Accounts. The primary name field should be called Title. add choice colunms Priority and Ticket Status.
2. Create a custom table called Ticket Facility with two mandatory lookups, one to Ticket and one to Facility. Make the primary name field optional.

### Customize views and forms

1. Create a view for the Ticket table to be used as a view for the Ticket subgrid on the Accounts main form.
2. Create a new main form for Accounts, add a Tickets section and add a subgrid component to show related Tickets. Use the view created in the previous step.
3. Create a view for the Ticket Facilities table to be used as a view for the Ticket Facilities subgrid on the Ticket main form.
3. Edit the Ticket main form, add the account lookup, ticket status and priority fields.
4. Still on the Ticket main form, add a section and subgrid to show related Ticket Facility records. Used the view created in step 3.
5. Customize the Ticket Facility main form, remove the name field and add the Facility and Ticket fields. Make the ticket field read-only.

Create a model driven app to bring Accounts, Tickets and Facilities together. Try navigating the relationships through the subgrid and the related views.

## Export and store solution in source control

Useful `pac` commands:

    pac solution list
    pac solution export --name VirtualtablePoC --overwrite
    pac solution unpack --zipfile VirtualtablePoC.zip --folder ./Solutions/VirtualtablePoC
