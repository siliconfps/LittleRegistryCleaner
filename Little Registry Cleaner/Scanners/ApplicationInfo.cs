/*
    Little Registry Cleaner
    Copyright (C) 2008 Little Apps (http://www.little-apps.org/)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Little_Registry_Cleaner.Scanners
{
    public class ApplicationInfo : ScannerBase
    {
        public override string ScannerName
        {
            get { return Strings.ApplicationInfo; }
        }

        /// <summary>
        /// Verifies installed programs in add/remove list
        /// </summary>
        public static void Scan()
        {
            try
            {
                Main.Logger.WriteLine("Verifying programs in Add/Remove list");

                ScanUninstallKey(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"));
                ScanUninstallKey(Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"));

                if (Utils.Is64BitOS)
                {
                    ScanUninstallKey(Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"));
                    ScanUninstallKey(Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"));
                }

                Main.Logger.WriteLine("Verifying registry entries in Add/Remove Cache");

                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Management\ARPCache"))
                {
                    checkARPCache(rk);
                }
                using (RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Management\ARPCache"))
                {
                    checkARPCache(rk);
                }
                if (Utils.Is64BitOS)
                {
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Management\ARPCache"))
                    {
                        checkARPCache(rk);
                    }
                }
            }
            catch (System.Security.SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private static void ScanUninstallKey(RegistryKey regKey)
        {
            if (regKey == null)
                return;

            try
            {
                foreach (string strProgName in regKey.GetSubKeyNames())
                {
                    using (RegistryKey regKey2 = regKey.OpenSubKey(strProgName))
                    {
                        if (regKey2 != null)
                        {
                            ScanDlg.CurrentScannedObject = regKey2.ToString();

                            Common_Tools.ProgramInfo progInfo = new Common_Tools.ProgramInfo(regKey2);

                            if (regKey2.ValueCount <= 0 && regKey2.SubKeyCount <= 0)
                            {
                                ScanDlg.StoreInvalidKey(Strings.InvalidRegKey, regKey2.ToString());
                                continue;
                            }

                            if (progInfo.WindowsInstaller)
                                continue;

                            if (string.IsNullOrEmpty(progInfo.DisplayName) && (!progInfo.Uninstallable))
                            {
                                ScanDlg.StoreInvalidKey(Strings.InvalidRegKey, regKey2.ToString());
                                continue;
                            }

                            // Check display icon
                            if (!string.IsNullOrEmpty(progInfo.DisplayIcon))
                                if (!Utils.IconExists(progInfo.DisplayIcon))
                                    ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey2.ToString(), "DisplayIcon");

                            // Check install location 
                            if (!string.IsNullOrEmpty(progInfo.InstallLocation))
                                if ((!Utils.DirExists(progInfo.InstallLocation)) && (!Utils.FileExists(progInfo.InstallLocation)))
                                    ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey2.ToString(), "InstallLocation");

                            // Check install source 
                            if (!string.IsNullOrEmpty(progInfo.InstallSource))
                                if ((!Utils.DirExists(progInfo.InstallSource)) && (!Utils.FileExists(progInfo.InstallSource)))
                                    ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey2.ToString(), "InstallSource");

                            // Check ARP Cache
                            if (progInfo.SlowCache)
                            {
                                if (!string.IsNullOrEmpty(progInfo.FileName))
                                    if (!Utils.FileExists(progInfo.FileName))
                                        ScanDlg.StoreInvalidKey(Strings.InvalidRegKey, progInfo.SlowInfoCacheRegKey);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (regKey != Registry.LocalMachine && regKey != Registry.CurrentUser)
                    regKey.Close();
            }
        }

        /// <summary>
        /// Do a cross-reference check on ARP Cache keys
        /// </summary>
        /// <param name="regKey"></param>
        private static void checkARPCache(RegistryKey regKey)
        {
            if (regKey == null)
                return;

            foreach (string subKey in regKey.GetSubKeyNames())
            {
                bool found = false;
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + subKey))
                {
                    if (rk != null) found = true;
                }
                if (!found && Utils.Is64BitOS)
                {
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\" + subKey))
                    {
                        if (rk != null) found = true;
                    }
                }

                if (!found)
                    ScanDlg.StoreInvalidKey(Strings.ObsoleteRegKey, string.Format(@"{0}\{1}", regKey.Name, subKey));
            }
        }
    }
}
