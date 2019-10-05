using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace DpiAwarenessApp
{
    // Source: https://stackoverflow.com/a/49115675/27211
    internal enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }

    internal enum DPI_AWARENESS : int
    {
        DPI_AWARENESS_INVALID = -1,
        DPI_AWARENESS_UNAWARE = 0,
        DPI_AWARENESS_SYSTEM_AWARE = 1,
        DPI_AWARENESS_PER_MONITOR_AWARE = 2
    }

    class DpiUtils
    {
        [DllImport("SHcore.dll")]
        internal static extern int GetProcessDpiAwareness(IntPtr hWnd, out PROCESS_DPI_AWARENESS value);

        [DllImport("user32.dll")]
        internal static extern uint GetDpiForWindow(IntPtr hWnd);

        // When app.manifest has:
        //     <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
        // Values is:
        //     PROCESS_SYSTEM_DPI_AWARE
        //
        // When app.manifest has:
        //     <dpiAware>true/PM</dpiAware>
        // Value is:
        //     PROCESS_PER_MONITOR_DPI_AWARE
        public static void CheckOnInterval()
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                // Per process
                PROCESS_DPI_AWARENESS processAwareness;
                try
                {
                    GetProcessDpiAwareness(process.Handle, out processAwareness);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
                catch (Win32Exception ex) when (ex.ErrorCode == -2147467259)
                {
                    // Access denied error (E_FAIL)
                    continue;
                }

                // Filter to one app by changing the following filter.
                string[] nameFilters = new string[] { }; //  { "CefSharp.BrowserSubprocess", "PBIDesktop" }; // Filter example
                if (nameFilters != null && nameFilters.Length > 0 && !nameFilters.Contains(process.ProcessName))
                {
                    continue;
                }

                uint? windowDpi = null;
                //if (!String.IsNullOrEmpty(process.MainWindowTitle))
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    windowDpi = GetDpiForWindow(process.MainWindowHandle);
                }

                Console.WriteLine("Process: {0,-18} ID: {1,5} Awareness: {3,-30}  DPI: {4,3} Title: {2}",
                    process.ProcessName,
                    process.Id,
                    process.MainWindowTitle,
                    processAwareness,
                    windowDpi);
            }
        }
    }
}
