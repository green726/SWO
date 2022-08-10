# Installation

<aside>
ℹ️ This page contains the documentation / examples for the SWO installation

</aside>

# Tutorial:

1. Download the latest SWO Installer from the [SWO Github releases page](https://github.com/green726/SWO/releases)
2. Run the installer to install with default options. Run it with `-h|--help` to view all options. You can also run the installer from the terminal with the `--ui` option for a more guided install process. 
3. Wait for the installer to finish
4. Done!

---

# Build From Source:

1. Clone the [SWO Github](https://github.com/green726/SWO)
2. `cd` into the language folder
3. Run `dotnet publish` 
4. Move the contents of `/bin/Debug/net6.0/youros/publish` to your desired install location
5. Add said location to the path
6. Go back into the base directory
7. Move the Resources folder into the same directory that you placed the SWO language
8. (Optional) If you want SAP, you can repeat steps 1-5 but replace the SWO Github and folders with the [SAP](https://github.com/green726/SAP) respective items. 

---

# About:

The SWO installer simply downloads some zips from the github, extracts them, and adds them to your path. Currently, Linux and Windows are supported; however, it has only been tested on Windows 10 and EndeavourOS (Arch distro), so there are no guarantees that this will work.