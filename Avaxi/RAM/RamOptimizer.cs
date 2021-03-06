using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.VisualBasic.Devices;

namespace Avaxi
{
    /// <summary>
    /// System Cache Information structure for x86 working set
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SystemCacheInformation
    {
        internal uint CurrentSize;
        internal uint PeakSize;
        internal uint PageFaultCount;
        internal uint MinimumWorkingSet;
        internal uint MaximumWorkingSet;
        internal uint Unused1;
        internal uint Unused2;
        internal uint Unused3;
        internal uint Unused4;
    }

    /// <summary>
    /// System Cache Information structure for x64 working set
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SystemCacheInformation64Bit
    {
        internal long CurrentSize;
        internal long PeakSize;
        internal long PageFaultCount;
        internal long MinimumWorkingSet;
        internal long MaximumWorkingSet;
        internal long Unused1;
        internal long Unused2;
        internal long Unused3;
        internal long Unused4;
    }

    /// <summary>
    /// Token Privileges structure, used for adjusting token privileges
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokenPrivileges
    {
        internal int Count;
        // ReSharper disable once IdentifierTypo
        internal long Luid;
        internal int Attr;
    }

    /// <summary>
    /// Enum containing System Information class values
    /// </summary>
    internal enum SystemInformationClass
    {
        SystemFileCacheInformation = 0x0015,
        SystemMemoryListInformation = 0x0050
    }

    /// <summary>
    /// Sealed class containing methods to 'optimize' or clear memory usage in Windows
    /// </summary>
    internal sealed class RamOptimizer
    {
        #region Variables
        /// <summary>
        /// Constant int used for TokenPrivileges Attribute variable
        /// </summary>
        private const int PrivilegeEnabled = 2;
        /// <summary>
        /// Adjust memory quotas for a process
        /// </summary>
        private const string IncreaseQuotaName = "SeIncreaseQuotaPrivilege";
        /// <summary>
        /// Profile single process
        /// </summary>
        private const string ProfileSingleProcessName = "SeProfileSingleProcessPrivilege";
        /// <summary>
        /// Memory purge standby list
        /// </summary>
        private const int MemoryPurgeStandbyList = 4;
        /// <summary>
        /// The LogController object that can be called to add logs
        /// </summary>
        //private readonly LogController _logController;
        #endregion

        /// <summary>
        /// Initialize a new RamOptimizer object
        /// </summary>
        /// <param name="logController">The LogController object that can be used to add new logs</param>
        internal RamOptimizer(/*LogController logController*/)
        {
            //_logController = logController ?? throw new ArgumentNullException(nameof(logController));
        }

        /// <summary>
        /// Clear the working sets of all processes that are available to the application
        /// </summary>
        /// <param name="processExceptions">A list of processes that should be excluded from memory optimization</param>
        internal void EmptyWorkingSetFunction(List<string> processExceptions)
        {
            frmMain.PushLog("Emptying working set");

            if (processExceptions != null && processExceptions.Count > 0)
            {
                processExceptions = processExceptions.ConvertAll(d => d.ToLower());
            }

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (processExceptions == null || processExceptions.Count == 0 || !processExceptions.Contains(process.MainModule.FileName.ToLower()))
                    {
                        frmMain.PushLog("Emptying working set for process: " + process.ProcessName);
                        NativeMethods.EmptyWorkingSet(process.Handle);
                        frmMain.PushLog("Successfully emptied working set for process " + process.ProcessName);
                    }
                    else
                    {
                        frmMain.PushLog("Excluded process: " + process.ProcessName);
                    }
                }
                catch (Exception ex)
                {
                    frmMain.PushLog("Could not empty working set for process " + process.ProcessName + ": " + ex.Message);
                }
            }

            // _logController.AddLog("Done emptying working set"));
        }

        /// <summary>
        /// Clear the clipboard using the native Windows API
        /// </summary>
        internal void ClearClipboard()
        {
            frmMain.PushLog("Clearing clipboard");
            try
            {
                // Attempt to open the clipboard first and set associate its handle to the current task
                if (!NativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    throw new Exception("OpenClipboard: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }

                NativeMethods.EmptyClipboard();
                NativeMethods.CloseClipboard();

                frmMain.PushLog("Successfully cleared all clipboard data");
            }
            catch (Exception ex)
            {
                frmMain.PushLog(ex.ToString());
            }
        }

        /// <summary>
        /// Check whether the system is running a x86 or x64 working set
        /// </summary>
        /// <returns>A boolean to indicate whether or not the system is 64 bit</returns>
        private bool Is64BitMode()
        {
            frmMain.PushLog("Checking if 64 bit mode is enabled");
            bool is64Bit = Marshal.SizeOf(typeof(IntPtr)) == 8;
            frmMain.PushLog(is64Bit ? "64 bit mode is enabled" : "64 bit mode is disabled");

            return is64Bit;
        }

        /// <summary>
        /// Clear the FileSystem cache
        /// </summary>
        /// <param name="clearStandbyCache">Set whether or not to clear the standby cache</param>
        internal void ClearFileSystemCache(bool clearStandbyCache)
        {
            frmMain.PushLog("Clearing FileSystem cache");

            try
            {
                // Check if privilege can be increased
                if (SetIncreasePrivilege(IncreaseQuotaName))
                {
                    frmMain.PushLog("Privileges have successfully been increased");

                    uint ntSetSystemInformationRet;
                    int systemInfoLength;
                    GCHandle gcHandle;
                    // Depending on the working set, call NtSetSystemInformation using the right parameters
                    if (!Is64BitMode())
                    {
                        frmMain.PushLog("Clearing 32 bit FileSystem cache information");

                        SystemCacheInformation cacheInformation =
                            new SystemCacheInformation
                            {
                                MinimumWorkingSet = uint.MaxValue,
                                MaximumWorkingSet = uint.MaxValue
                            };
                        systemInfoLength = Marshal.SizeOf(cacheInformation);
                        gcHandle = GCHandle.Alloc(cacheInformation, GCHandleType.Pinned);
                        ntSetSystemInformationRet = NativeMethods.NtSetSystemInformation((int)SystemInformationClass.SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                        // If value is not equal to zero, things didn't go right :(
                        if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                        gcHandle.Free();

                        frmMain.PushLog("Done clearing 32 bit FileSystem cache information");
                    }
                    else
                    {
                        frmMain.PushLog("Clearing 64 bit FileSystem cache information");

                        SystemCacheInformation64Bit information64Bit =
                            new SystemCacheInformation64Bit
                            {
                                MinimumWorkingSet = -1L,
                                MaximumWorkingSet = -1L
                            };
                        systemInfoLength = Marshal.SizeOf(information64Bit);
                        gcHandle = GCHandle.Alloc(information64Bit, GCHandleType.Pinned);
                        ntSetSystemInformationRet = NativeMethods.NtSetSystemInformation((int)SystemInformationClass.SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                        // If value is not equal to zero, things didn't go right :(
                        if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                        gcHandle.Free();

                        frmMain.PushLog("Done clearing 64 bit FileSystem cache information");
                    }
                }

                // Clear the standby cache if we have to and if we can also increase the privileges
                // If we can't increase the privileges, it's pointless to even try
                if (!clearStandbyCache || !SetIncreasePrivilege(ProfileSingleProcessName)) return;
                {
                    frmMain.PushLog("Clearing standby cache");

                    int systemInfoLength = Marshal.SizeOf(MemoryPurgeStandbyList);
                    GCHandle gcHandle = GCHandle.Alloc(MemoryPurgeStandbyList, GCHandleType.Pinned);
                    uint ntSetSystemInformationRet = NativeMethods.NtSetSystemInformation((int)SystemInformationClass.SystemMemoryListInformation, gcHandle.AddrOfPinnedObject(), systemInfoLength);
                    if (ntSetSystemInformationRet != 0) throw new Exception("NtSetSystemInformation: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    gcHandle.Free();

                    frmMain.PushLog("Done clearing standby cache");
                }
            }
            catch (Exception ex)
            {
                frmMain.PushLog(ex.ToString());
            }
        }

        /// <summary>
        /// Increase the Privilege using a privilege name
        /// </summary>
        /// <param name="privilegeName">The name of the privilege that needs to be increased</param>
        /// <returns>A boolean value indicating whether or not the operation was successful</returns>
        private bool SetIncreasePrivilege(string privilegeName)
        {
            frmMain.PushLog("Increasing privilege: " + privilegeName);

            using (WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges))
            {
                TokenPrivileges tokenPrivileges;
                tokenPrivileges.Count = 1;
                tokenPrivileges.Luid = 0L;
                tokenPrivileges.Attr = PrivilegeEnabled;

                frmMain.PushLog("Looking up privilege value");
                // If we can't look up the privilege value, we can't function properly
                if (!NativeMethods.LookupPrivilegeValue(null, privilegeName, ref tokenPrivileges.Luid)) throw new Exception("LookupPrivilegeValue: ", new Win32Exception(Marshal.GetLastWin32Error()));
                frmMain.PushLog("Done looking up privilege value");


                frmMain.PushLog("Adjusting token privileges");
                // Enables or disables privileges in a specified access token
                int adjustTokenPrivilegesRet = NativeMethods.AdjustTokenPrivileges(current.Token, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero) ? 1 : 0;
                // Return value of zero indicates an error
                if (adjustTokenPrivilegesRet == 0) throw new Exception("AdjustTokenPrivileges: ", new Win32Exception(Marshal.GetLastWin32Error()));
                frmMain.PushLog("Done adjusting token privileges");
                return adjustTokenPrivilegesRet != 0;
            }
        }

        /// <summary>
        /// Fill the available RAM
        /// </summary>
        /// <param name="info">The ComputerInfo object that can be used to calculate usage percentages</param>
        /// <param name="maxRuns">The amount of times the RAM should be filled</param>
        internal void FillRam(ComputerInfo info, int maxRuns)
        {
            frmMain.PushLog("Attempting to fill the available RAM");
            int runs = 0;
            while (runs < maxRuns)
            {
                List<IntPtr> pointers = new List<IntPtr>();
                // Don't keep calling the info object because this can cause a Win32Exception
                double total = Convert.ToDouble(info.TotalPhysicalMemory);
                double usage = total - Convert.ToDouble(info.AvailablePhysicalMemory);
                double percentage = usage / total * 100;

                if (double.IsNaN(total) || double.IsInfinity(total)) throw new ArgumentException(nameof(total));
                if (double.IsNaN(usage) || double.IsInfinity(usage)) throw new ArgumentException(nameof(usage));
                if (double.IsNaN(percentage) || double.IsInfinity(percentage)) throw new ArgumentException(nameof(percentage));

                // 99 is the threshold for the amount of RAM that should be filled
                // Windows will do its best to manage the available memory while its filling up
                while (percentage < 99)
                {
                    try
                    {
                        IntPtr pointer = Marshal.AllocHGlobal(1024);
                        pointers.Add(pointer);

                        usage += 1024;
                        percentage = usage / total * 100;
                    }
                    catch (OutOfMemoryException ex)
                    {
                        frmMain.PushLog(ex.Message);
                        break;
                    }
                }

                foreach (IntPtr clearPtr in pointers)
                {
                    try
                    {
                        // Clean all previously acquired IntPtr objects pointing to the allocated memory
                        Marshal.FreeHGlobal(clearPtr);
                    }
                    catch (Exception ex)
                    {
                        frmMain.PushLog(ex.Message);
                    }
                }
                runs++;
            }
            frmMain.PushLog("Done filling available RAM");
        }
    }
}
