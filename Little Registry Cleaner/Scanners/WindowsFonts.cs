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
    public class WindowsFonts : ScannerBase
    {
        public override string ScannerName
        {
            get { return Strings.WindowsFonts; }
        }

        /// <summary>
        /// Finds invalid font references
        /// </summary>
        public static void Scan()
        {
            try
            {
                Main.Logger.WriteLine("Scanning for invalid fonts");

                string systemFontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                string userFontsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Fonts");

                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts"))
                {
                    ScanFontsKey(regKey, systemFontsDir);
                }

                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts"))
                {
                    ScanFontsKey(regKey, Directory.Exists(userFontsDir) ? userFontsDir : systemFontsDir);
                }

                if (Utils.Is64BitOS)
                {
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Fonts"))
                    {
                        ScanFontsKey(regKey, systemFontsDir);
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

        private static void ScanFontsKey(RegistryKey regKey, string fontsFolder)
        {
            if (regKey == null)
                return;

            foreach (string strFontName in regKey.GetValueNames())
            {
                string strValue = regKey.GetValue(strFontName) as string;

                if (string.IsNullOrEmpty(strValue))
                    continue;

                if (Utils.FileExists(strValue))
                    continue;

                string strFontPath = Path.Combine(fontsFolder, strValue);
                ScanDlg.CurrentScannedObject = strFontPath;

                if (!Utils.FileExists(strFontPath))
                    ScanDlg.StoreInvalidKey(Strings.InvalidFile, regKey.ToString(), strFontName);
            }
        }
    }
}
