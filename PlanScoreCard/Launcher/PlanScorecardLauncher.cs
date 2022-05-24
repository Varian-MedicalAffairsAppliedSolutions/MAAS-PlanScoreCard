using System.Windows;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using System.Diagnostics;
using System.IO;
using System;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context)
        {
            try
            {
                string launcherPath = Path.GetDirectoryName(GetSourceFilePath());
                //string esapiStandaloneExecutable = @"SubDirectory/PlanScoreCard.exe";
                string esapiStandaloneExecutable = @"PlanScoreCard.exe";
                string arguments = context.PlanSetup == null
                                    ? string.Format("\"{0};{1};\"", context.Patient.Id, context.Course.Id)
                                    : string.Format("\"{0};{1};{2}\"", context.Patient.Id, context.Course.Id, context.PlanSetup.Id);
                Process.Start(Path.Combine(launcherPath, esapiStandaloneExecutable), arguments);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to start application.");
            }
        }

        public string GetSourceFilePath([CallerFilePath] string sourceFilePath = "")
        {
            return sourceFilePath;
        }
    }
}
