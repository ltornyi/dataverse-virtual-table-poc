# Run Windows apps on Mac

## Install VirtualBox

Go to the [download page](https://www.virtualbox.org/wiki/Downloads) and pick your macOS build based on your hardware. 

## Download Windows 11 image from Microsoft

For Apple silicon, download the [Arm version](https://www.microsoft.com/en-us/software-download/windows11arm64), for Intel-based Macs, download the [x64 version](https://www.microsoft.com/en-us/software-download/windows11).

## Create a new VM

Run VirtualBox, create a new VM:

1. Specify name of the VM, make sure OS is "Microsoft Windows" and OS Version is "Windows 11 on ARM (64-bit)"
2. DON'T select the Windows install ISO at this point yet.
3. Allocate memory and CPU to the VM; the more the better. Allocate the size of virtual disk, the default 80 GB should be fine. Make sure EFI is selected.

## Adjust VM settings

1. Expert -> General -> Features -> Shared Clipboard: set it to Bidirectional for convenience.
2. Expert -> Display -> Screen: make sure Video Memory is set to the maximum. Also disable 3D Acceleration just to be on the safe side.
3. Expert -> Storage: selected the empty optical drive. Click on the disk icon, select "Choose a disk file..." and select the Windows installer ISO downloaded earlier.
4. Expert -> Network: make sure network adapter 1 is enabled and attached to NAT.

Also allow VirtualBox to monitor keyboard input and control your computer under MacOS Settings -> Privacy&Security -> Input monitoring and Settings -> Privacy&Security -> Accessibility.

## Start up the VM and install Windows 11

1. Press Enter when prompted.
2. From the VM menu, select view -> Enable Scaled mode. Resize the window and continue the installation.
3. Select language, time and currency format, keyboard layout.
4. Pick "Install Windows 11" and continue.
5. Select "Windows 11 Pro", agree to the licence terms.
6. Click "Create Partition", leave the default size and click "Apply".
7. Review the summary and click "Install".
8. Pick the "set up for personal use" option when prompted.

Wait until the installation finishes, it will check and install updates. Remove the installation ISO from the optical disk drive (Devices -> Optical drives).

## Install VirtualBox Guest Additions

Once Windows is installed and functioning, complete the following steps:

1. Devices -> Insert Guest Additions ISO image
2. In Windows, open file manager, select the CD drive and run the `VBoxWindowsAdditions-arm64` executable.
3. Turn off Scaled mode in VirtualBox and restart Windows to apply changes

With the Guest Additions installed, graphics should be better and full screen mode becomes useful. Play with Settings -> System -> Display -> Scale&layout settings. Also Control Panel -> Power options -> create new power plan -> pick high performance mode.

## Setup shared folder

Add a shared folder to your VM settings - it can be used to copy files to/from the virtual Windows machine.