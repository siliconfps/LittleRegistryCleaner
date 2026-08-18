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
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace Little_Registry_Cleaner.Scanners
{
    public class RecentDocs : ScannerBase
    {
        public override string ScannerName
        {
            get { return Strings.RecentDocs; }
        }

        public static void Scan()
        {
            ScanMUICache();
            ScanExplorerDocs();
        }

        /// <summary>
        /// Checks MUI Cache for invalid file references
        /// </summary>
        private static void ScanMUICache()
        {
            try
            {
                // XP location
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\ShellNoRoam\MUICache"))
                {
                    ScanMUICacheKey(regKey);
                }

                // Vista / Windows 7 / 8 / 10 / 11 location
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache"))
                {
                    ScanMUICacheKey(regKey);
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

        private static void ScanMUICacheKey(RegistryKey regKey)
        {
            if (regKey == null)
                return;

            foreach (string valueName in regKey.GetValueNames())
            {
                if (string.IsNullOrEmpty(valueName) || valueName.StartsWith("@") || valueName == "LangID")
                    continue;

                // Strip .ApplicationCompany or .FriendlyAppName suffixes if present
                string cleanPath = valueName;
                int dotIndex = cleanPath.LastIndexOf(".ApplicationCompany", StringComparison.OrdinalIgnoreCase);
                if (dotIndex > 0) cleanPath = cleanPath.Substring(0, dotIndex);
                dotIndex = cleanPath.LastIndexOf(".FriendlyAppName", StringComparison.OrdinalIgnoreCase);
                if (dotIndex > 0) cleanPath = cleanPath.Substring(0, dotIndex);

                ScanDlg.CurrentScannedObject = cleanPath;

                if (!Utils.FileExists(cleanPath))
                    ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.Name, valueName);
            }
        }

        /// <summary>
        /// Recurses through the recent documents registry keys for invalid files
        /// </summary>
        private static void ScanExplorerDocs()
        {
            try 
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs"))
                {
                    if (regKey == null)
                        return;

                    Main.Logger.WriteLine("Cleaning invalid references in " + regKey.Name);

                    EnumMRUList(regKey);

                    foreach (string strSubKey in regKey.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = regKey.OpenSubKey(strSubKey))
                        {
                            if (subKey != null)
                                EnumMRUList(subKey);
                        }
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

        private static void EnumMRUList(RegistryKey regKey)
        {
            if (regKey == null)
                return;

            foreach (string strValueName in regKey.GetValueNames())
            {
                string filePath, fileArgs;

                // Ignore MRUListEx and others
                if (!Regex.IsMatch(strValueName, "[0-9]"))
                    continue;

                string fileName = ExtractUnicodeStringFromBinary(regKey.GetValue(strValueName));
                
                // If filename is empty -> remove it
                if (string.IsNullOrEmpty(fileName.Trim()))
                {
                    ScanDlg.StoreInvalidKey(Strings.InvalidRegKey, regKey.ToString(), strValueName);
                    continue;
                }
                else
                {
                    string shortcutPath = string.Format("{0}\\{1}.lnk", Environment.GetFolderPath(Environment.SpecialFolder.Recent), fileName);

                    ScanDlg.CurrentScannedObject = shortcutPath;

                    // See if file exists in Recent Docs folder
                    if (!Utils.FileExists(shortcutPath) || !Utils.ResolveShortcut(shortcutPath, out filePath, out fileArgs))
                    {
                        ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), strValueName);
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Converts registry value to filename
        /// </summary>
        /// <param name="keyObj">Value from registry key</param>
        private static string ExtractUnicodeStringFromBinary(object keyObj)
        {
            if (keyObj == null)
                return string.Empty;

            if (keyObj is byte[])
            {
                byte[] Bytes = (byte[])keyObj;
                char[] chars = Encoding.Unicode.GetChars(Bytes);
                StringBuilder sb = new StringBuilder();
                foreach (char bt in chars)
                {
                    if (bt != 0)
                        sb.Append(bt);
                    else
                        break;
                }
                return sb.ToString();
            }

            return keyObj.ToString();
        }
    }
}
