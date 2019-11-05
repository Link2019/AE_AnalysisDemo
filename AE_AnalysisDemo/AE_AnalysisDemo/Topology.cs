using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;

namespace AE_AnalysisDemo
{
    public partial class Topology : Form
    {
        /// <summary>
        /// 判断拓扑是否创建成功，true表示成功，false表示失败 
        /// </summary>
        private bool Flag = false;

        public Topology()
        {
            InitializeComponent();
        }

        private void Topology_Load(object sender, EventArgs e)
        {
            axMapControl1.LoadMxFile(Application.StartupPath + @"\网络分析\无标题.mxd");
            OpenGDB();
        }
        /// <summary>
        /// 打开GDB
        /// </summary>
        private void OpenGDB()
        {
            IWorkspaceFactory workspaceFactory = new AccessWorkspaceFactoryClass();//改动
          
            IWorkspace workspace = workspaceFactory.OpenFromFile(Global.GdbPath, 0) as IWorkspace;
            Global.pWorkSpace = workspace;
        }
        /// <summary>
        /// 创建拓扑数据集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreateTopo_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Global.GlobalTopology = CreateTopology(Global.pWorkSpace, "Road_Analysis", "Topology_Dataset");
            if(Flag)
            {
                //打开拓扑数据集
                Global.GlobalTopology = OpenToplogyFromFeatureWorkspace((IFeatureWorkspace)Global.pWorkSpace, "Road_Analysis", "Topology_Dataset"); //???
                MessageBox.Show("已经存在拓扑数据集");
                this.Cursor = Cursors.Default;
                return;
            }
           
        }

        

        /// <summary>
        /// 打开拓扑数据集(如果拓扑数据集已经创建)
        /// </summary>
        /// <param name="featureWorkspace">工作空间</param>
        /// <param name="featureDatasetName">普通数据集</param>
        /// <param name="topologyName">拓扑集名</param>
        /// <returns>返回拓扑数据集</returns>
        private ITopology OpenToplogyFromFeatureWorkspace(IFeatureWorkspace featureWorkspace, string featureDatasetName, string topologyName)
        {
            //打开特征数据集
            IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
            //将featureDataset转换为ITopologyContainer
            ITopologyContainer topologyContainer = (ITopologyContainer)featureDataset;
            //ITopology get_TopologyByName(string Name);
            //打开拓扑
            ITopology topology = topologyContainer.get_TopologyByName(topologyName);
            //返回拓扑
            return topology;
        }
        /// <summary>
        /// 创建拓扑数据集（前提是要素集内没有数据集）
        /// </summary>
        /// <param name="featureWorkspace">工作空间</param>
        /// <param name="featureDatasetName">要素集名称</param>
        /// <param name="topologyName">拓扑数据集名</param>
        /// <returns></returns>
        private ITopology CreateTopology(IWorkspace featureWorkspace, string featureDatasetName, string topologyName)
        {
            IFeatureWorkspace pFtWsp = featureWorkspace as IFeatureWorkspace;
            IFeatureDataset myFDS = pFtWsp.OpenFeatureDataset(featureDatasetName);
            IFeatureClassContainer myFCContainer = myFDS as IFeatureClassContainer;
            ITopologyContainer myTopologyContainer = myFDS as ITopologyContainer;
            ITopology myTopology = null;
            try
            {
                myTopology = myTopologyContainer.CreateTopology(topologyName, myTopologyContainer.DefaultClusterTolerance, -1, "");
            }
            catch (Exception ex)
            {
                Flag = true;
                return null;
            }
            return myTopology;

        }
    }
}
