# Little Registry Cleaner
Little Registry Cleaner is an open source program that is designed to cleanup Microsoft Windows Registry. It removes obsolete or unwanted items that build up in the registry over time in order to improve the stability and performance of your computer.

## Key Features

* **Registry Optimization & Stability**: Cleans obsolete, broken, and orphaned entries safely to improve Windows stability and performance.
* **Full 64-Bit & Modern Windows Support**: Native 64-bit pointer handling (`IntPtr`), multi-architecture support (`Wow6432Node`), and tested on Windows 7, 8, 10, and 11.
* **Automatic Backups & System Restore**: Creates full XML registry backups prior to cleaning, allowing 1-click restore.
* **Native Taskbar Progress**: Seamless Windows Taskbar progress integration via COM (`ITaskbarList3`).
* **Startup Manager (`Little Startup Manager`)**: Manage, enable, disable, and configure auto-start applications and services.
* **Uninstall Manager (`Little Uninstall Manager`)**: Clean and force-remove broken, obsolete, or leftover installed software entries (both HKLM and per-user HKCU).
* **Multilingual Support**: Built-in translations for 17+ different languages.
* **100% Free & Open Source**: Licensed under GPLv3.

## System Requirements

* **Operating System**: Windows 7 / 8 / 8.1 / 10 / 11 (32-bit or 64-bit)
* **Runtime**: Microsoft .NET Framework 4.0 or higher (pre-installed on Windows 10/11)
* **Privileges**: Administrator rights (required for registry inspection and cleanup)

## Building from Source

You can build the complete solution using Visual Studio 2010 or newer, or directly via command line with MSBuild:

```powershell
MSBuild.exe "Little Registry Cleaner.sln" /p:Configuration=Release
```

## Versions & Changelog

See [CHANGELOG.md](CHANGELOG.md) for the detailed version history and release notes.

## Licensing

Little Registry Cleaner is licensed under the [GNU General Public License v3](http://www.gnu.org/licenses/gpl.html).

## Credits
 
**Thanks to the following libraries and examples:**

 * [Advanced TreeView for .NET by Andrey Gliznetsov](http://www.codeproject.com/KB/tree/treeviewadv.aspx)
 * [XP Progress Bar by Marcos Meli](http://www.codeproject.com/KB/cpp/XpProgressBar.aspx)
 * [Import/Export registry sections as XML by Sam DenHartog](http://www.codeproject.com/KB/XML/registryxml.aspx)
 * [JumpTo RegEdit by Reto Ravasio](http://www.codeproject.com/KB/cs/RegEdit_JumpTo.aspx)
 * [Task Scheduler Managed Wrapper by David Hall](http://taskscheduler.codeplex.com/)
 * [AutoUpdater.NET by RBSoft](http://autoupdaterdotnet.codeplex.com/)

**Thanks to the following people for their contributions to LRC:**

#### Maintainers

 * Nick H.

#### Developers

 * Jonathan N. (Beta tester) 
 * Erik Y. (Beta tester) 
 * Jomy M. (Bugs & Fixes) 

#### Translators

 * Maciej M. - Polish 
 * Artur M. - Russian 
 * E. Staas - Dutch 
 * Sándor L. - Hungarian 
 * Alisson C. - Portuguese 
 * Radoslaw M - Polish 
 * Alexander P. - Swedish 
 * Jim B. - Greek 
 * Vladislav Z. - Lithuanian and Russian 
 * Erkan M. - Turkish 
 * Ken O. - Chinese (Simplified & Traditional) 
 * Dmitriy P. - Russian (Setup) 
 * Saeed D. - Persian 
 * Björn N. - Swedish 
 * Fitoschido - Spanish 
 * Chris G. - German 
 * Daniel - German (Fixed minor mistakes) 
 * Tulip V. - French

**And a big thanks to everyone else for supporting this project!**
