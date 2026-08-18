using System;
using System.Runtime.InteropServices;

namespace Little_Registry_Cleaner
{
    public enum TaskbarProgressBarStatus
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }

    [ComImport()]
    [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITaskbarList3
    {
        // ITaskbarList
        [PreserveSig]
        void HrInit();
        [PreserveSig]
        void AddTab(IntPtr hwnd);
        [PreserveSig]
        void DeleteTab(IntPtr hwnd);
        [PreserveSig]
        void ActivateTab(IntPtr hwnd);
        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2
        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        [PreserveSig]
        void SetProgressState(IntPtr hwnd, TaskbarProgressBarStatus tbpFlags);
        [PreserveSig]
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        [PreserveSig]
        void UnregisterTab(IntPtr hwndTab);
        [PreserveSig]
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        [PreserveSig]
        void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);
        [PreserveSig]
        void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, IntPtr pButton);
        [PreserveSig]
        void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, IntPtr pButton);
        [PreserveSig]
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
        [PreserveSig]
        void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        [PreserveSig]
        void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
        [PreserveSig]
        void SetThumbnailClip(IntPtr hwnd, IntPtr prcClip);
    }

    [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComImport()]
    internal class TaskbarInstance
    {
    }

    public static class TaskbarManager
    {
        private static ITaskbarList3 _taskbarList;
        private static bool _supported = false;
        private static bool _initialized = false;

        static TaskbarManager()
        {
            try
            {
                Version osVer = Environment.OSVersion.Version;
                _supported = (osVer.Major > 6) || (osVer.Major == 6 && osVer.Minor >= 1);
                if (_supported)
                {
                    _taskbarList = (ITaskbarList3)new TaskbarInstance();
                    _taskbarList.HrInit();
                    _initialized = true;
                }
            }
            catch
            {
                _supported = false;
                _taskbarList = null;
            }
        }

        public static void SetProgressState(IntPtr hwnd, TaskbarProgressBarStatus status)
        {
            if (!_initialized || _taskbarList == null || hwnd == IntPtr.Zero) return;
            try
            {
                _taskbarList.SetProgressState(hwnd, status);
            }
            catch { }
        }

        public static void SetProgressValue(IntPtr hwnd, ulong current, ulong total)
        {
            if (!_initialized || _taskbarList == null || hwnd == IntPtr.Zero) return;
            try
            {
                _taskbarList.SetProgressValue(hwnd, current, total);
            }
            catch { }
        }
    }
}
