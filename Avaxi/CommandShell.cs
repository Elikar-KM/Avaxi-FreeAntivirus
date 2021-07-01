﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avaxi
{
    class CommandShell
    {
        public string RunExternalExe(string filename, string arguments = null)
        {
            try
            {
                Process compiler = new Process();
                compiler.StartInfo.FileName = filename;
                compiler.StartInfo.Arguments = arguments;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.Start();
                return compiler.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
