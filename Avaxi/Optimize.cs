using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ReaLTaiizor;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Avaxi
{
    class Optimize
    {
        [DllImport("psapi")]
        private static extern bool EnumProcesses(
         [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out] UInt32[] processIds,
           UInt32 arraySizeBytes,
           [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

        public Optimize(System.Windows.Forms.ListView origin, ReaLTaiizor.Controls.CircleProgressBar oLabel)
        {
            try
            {
                Thread enumProcessThread = new Thread(EnumProcesses);
                pList = origin;
                pProcess = oLabel;
                //pTotalRamSpace = oTotalRam;
                //pUsedRamSpace = oUsedRam;
                enumProcessThread.IsBackground = true;
                _clearingMemory = false;
                _ramController = new RamController
                    (
                        RamClearingCompleted,
                        5000,
                        true,
                        100
                    );
                _ramController.EnableRamStatistics = true;
                _ramController.MaxUsageHistoryCount = 100;
                _ramController.SetProcessExceptionList(null);
                _ramController.EmptyWorkingSets = true;
                _ramController.ClearFileSystemCache = true;
                _ramController.ClearStandbyCache = true;
                _ramController.FillRam = true;
                _ramController.FillRamMaxRuns = 1;
                _ramController.InvokeGarbageCollector = true;
                _ramController.SetRamUpdateTimerInterval(5000);
                _ramController.AutoOptimizeTimed(false, 600000);
                _ramController.AutoOptimizePercentage = false;
                _ramController.SetAutoOptimizeThreshold(75);
                _ramController.ClearClipboard = true;
                //_ramController.EnableMonitor();

                enumProcessThread.Start();
            }
            catch (Exception ex)
            {
                frmMain.PushLog(ex.Message);
            }
        }

        private static void RamClearingCompleted()
        {
            double ramSavings = _ramController.RamSavings / 1024 / 1024;
            string message;
            if (ramSavings < 0)
            {
                ramSavings = Math.Abs(ramSavings);
                frmMain.PushLog("RAM usage increase: " + ramSavings.ToString("F2") + " MB");
                message = string.Format("RamUsageIncreased: {0}MB", ramSavings.ToString("F2"));
            }
            else
            {
                frmMain.PushLog("RAM usage decrease: " + ramSavings.ToString("F2") + " MB");
                message = string.Format("RamUsageSaved: {0}MB", ramSavings.ToString("F2"));
            }

            //System.Windows.Forms.MessageBox.Show(message, "MemPlus");
        }

        public static void Dispose()
        {
            _ramController?.Dispose();
        }
        public static void OptimizePC()
        {

        }

        public static void EnumProcesses()
        {
            try
            {
                while (true)
                {
                    pList.SuspendLayout();

                    Process[] processlist = Process.GetProcesses();
                    pProcess.Text = String.Format("{0}%", pProcess.Value);

                    List<string> AddProcessNames = new List<string>();
                    List<string> DeleteProcessNames = new List<string>(ProcessNames);

                    foreach (Process theprocess in processlist)
                    {
                        Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                        string szProcess = String.Format("{0}[{1}]", theprocess.ProcessName, theprocess.Id);

                        if (ProcessNames.FindIndex(x => x == szProcess) == -1)
                        {
                            AddProcessNames.Add(szProcess);
                        }
                        else
                        {
                            DeleteProcessNames.Remove(szProcess);
                        }
                    }
                    //save to ProcessNames
                    AddProcessNames.ForEach(x => ProcessNames.Add(x));
                    DeleteProcessNames.ForEach(x => ProcessNames.Remove(x));

                    //resume listbox
                    //AddProcessNames.ForEach(x => pList.AddItem(x));
                    //DeleteProcessNames.ForEach(x => pList.RemoveItem(x));
                    AddProcessNames.ForEach(x => pList.Items.Add(x));
                    pList.ResumeLayout();

                    //Update RAM usage
                    _ramController.UpdateRamUsage();
                    double used = _ramController._ramUsageHistory[_ramController._ramUsageHistory.Count - 1].TotalUsed;
                    double total = _ramController._ramUsageHistory[_ramController._ramUsageHistory.Count - 1].RamTotal;
                    int percent = (int)_ramController._ramUsageHistory[_ramController._ramUsageHistory.Count - 1].UsagePercentage;

                    string szTotal = $"{total / 1024 / 1024 / 1024}";
                    string szUsed = $"{used / 1024 / 1024 / 1024}";
                    string szpercent = $"{percent}";

                    //pTotalRamSpace.Text = String.Format("TotalRamSpace: {0}GB", szTotal.Remove(4, szTotal.Length - 4));
                    //pUsedRamSpace.Text = String.Format("UsedRamSpace({0}%): {1,5}GB", szpercent.Remove(4, szpercent.Length - 4), szUsed.Remove(4, szUsed.Length - 4));
                    pProcess.Value = percent;
                    //System.Windows.Forms.MessageBox.Show(percent.ToString());
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                //get component again
                frmMain.PushLog(ex.Message);
            }
        }

        public async static Task ClearMemory(int index)
        {
            if (_clearingMemory) return;
            if (!_ramController.EmptyWorkingSets && !_ramController.ClearFileSystemCache && !_ramController.ClearClipboard && !_ramController.FillRam) return;

            frmMain.PushLog("Clearing RAM Memory");
            _clearingMemory = true;

            try
            {
                //BtnClearMemory.IsEnabled = false;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (index)
                {
                    case 0:
                        await _ramController.ClearMemory();
                        break;
                    case 1:
                        await _ramController.ClearWorkingSets();
                        break;
                    case 2:
                        await _ramController.ClearFileSystemCaches();
                        break;
                    case 3:
                        await _ramController.FillRamData();
                        break;
                }
            }
            catch (Exception ex)
            {
                frmMain.PushLog(ex.Message);
                //MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //BtnClearMemory.IsEnabled = true;
            _clearingMemory = false;

            frmMain.PushLog("Done clearing RAM memory");
        }

        private static System.Windows.Forms.ListView pList;
        //private static ReaLTaiizor.Controls.MaterialLabel pTotalRamSpace;
        //private static ReaLTaiizor.Controls.MaterialLabel pUsedRamSpace;
        private static ReaLTaiizor.Controls.CircleProgressBar pProcess;
        private static List<string> ProcessNames = new List<string>();

        private static bool _clearingMemory;
        private static RamController _ramController;
    }
}
