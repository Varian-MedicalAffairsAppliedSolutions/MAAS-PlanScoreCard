using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Diagnostics;
using System.IO;
using System;

//[assembly: AssemblyVersion("1.0.0.1")]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            try
            {
                if (context.PlanSetup == null)
                {
                    Process.Start(AppExePath(), String.Format("\"{0};{1};\"",
                    context.Patient.Id, context.Course.Id));
                }
                else
                {
                    Process.Start(AppExePath(), String.Format("\"{0};{1};{2}\"",
                        context.Patient.Id, context.Course.Id, context.PlanSetup.Id));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to start application.");
            }
        }
        private string AppExePath()
        {
            return @"C:\Users\mschmidt\Desktop\PSC_current\MAAS - PlanScoreCard\PlanScoreCard\bin\Debug\PlanScoreCard.exe";
            //return @"C:\ESAPI\PlanScorecard_Plugin3_0_0_3\PlanScoreCard.exe";
        }
    }
}
