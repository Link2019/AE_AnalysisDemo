using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE_AnalysisDemo
{
    class Global
    {
        public static string GdbPath = Application.StartupPath + @"\网络分析\网络分析.mdb";
        public static IWorkspace pWorkSpace;
        public static ITopology GlobalTopology;
    }
}
