using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.Processes
{
    /// <summary>
    /// プロセス系のラッパーです。
    /// </summary>
    public class ProcessW
    {
        public static  bool IsAlreadyRunning()
        {
            // プロセス名を取得
            string processName = Process.GetCurrentProcess().ProcessName;
            // 同じプロセス名のプロセスを取得
            Process[] processes = Process.GetProcessesByName(processName);
            // 現在のプロセスを除外して、同じ名前のプロセスが存在するか確認
            return processes.Length > 1;
        }


        public static void KillProcessByName(string processName)
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to kill process {process.ProcessName}: {ex.Message}");
                }
            }
        }
    }
}
