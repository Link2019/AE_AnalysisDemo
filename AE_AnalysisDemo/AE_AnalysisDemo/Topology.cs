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
using ESRI.ArcGIS.Geometry;

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
            Global.GlobalTopology = create_topology(Global.pWorkSpace, "Road_Analysis", "Topology_Dataset");
            if (Flag)
            {
                //打开拓扑数据集
                Global.GlobalTopology = OpenToplogyFromFeatureWorkspace((IFeatureWorkspace)Global.pWorkSpace, "Road_Analysis", "Topology_Dataset"); //???
                MessageBox.Show("已经存在拓扑数据集");
                this.Cursor = Cursors.Default;
                return;
            }
            IFeatureClass pTempFt = null;
            //向拓扑数据集中添加拓扑元素，可以添加多个
            AddSingleElement(Global.GlobalTopology, "Road_Analysis", "南宁路网", out pTempFt);
            //添加单个要素的拓扑规则, 相同图层内的先不能相交
            AddRuleToTopology(Global.GlobalTopology, esriTopologyRuleType.esriTRTLineNoIntersection, "NoIntersection", pTempFt);
            //Global.GlobalTopology将强转为IGeoDataset获得Extent
            IGeoDataset GDS = Global.GlobalTopology as IGeoDataset;
            //调用自定义方法添加拓扑规则
            ValidateTopology(Global.GlobalTopology, GDS.Extent);
            MessageBox.Show("拓扑数据集创建成功！");
            this.Cursor = Cursors.Default;
        }
        /// <summary>
        /// 验证拓扑错误
        /// </summary>
        /// <param name="topology">拓扑集</param>
        /// <param name="envelope">t拓扑集的Extent</param>
        private void ValidateTopology(ITopology topology, IEnvelope envelope)
        {
            //实例化一个Polygon存储Topology的Extent
            IPolygon localPolygon = new PolygonClass();
            //获取Topology的外接矩形
            ISegmentCollection segmentCollection = (ISegmentCollection)localPolygon;
            segmentCollection.SetRectangle(envelope);
            //赋值Topology的阴影区域
            IPolygon polygon = topology.get_DirtyArea(localPolygon);
            if (!polygon.IsEmpty)
            {
                //赋值参数并Validate拓扑错误
                IEnvelope areaToValidate = polygon.Envelope;
                IEnvelope areaValidated = topology.ValidateTopology(areaToValidate);
            }
        }

        /// <summary>
        /// 添加单个要素的拓扑规则
        /// </summary>
        /// <param name="topology">拓扑数据集</param>
        /// <param name="ruleType">拓扑规则</param>
        /// <param name="ruleName">规则名称</param>
        /// <param name="featureClass">参与制定规则的要素</param>
        private void AddRuleToTopology(ITopology topology, esriTopologyRuleType ruleType, string ruleName, IFeatureClass featureClass)
        {
            //实例化拓扑规则
            ITopologyRule topologyRule = new TopologyRuleClass();
            //拓扑规则
            topologyRule.TopologyRuleType = ruleType;
            //规则名称
            topologyRule.Name = ruleName;
            //规则面向的要素类
            topologyRule.OriginClassID = featureClass.FeatureClassID;
            topologyRule.AllOriginSubtypes = true;
            ITopologyRuleContainer topologyRuleContainer = (ITopologyRuleContainer)topology;
            if (topologyRuleContainer.get_CanAddRule(topologyRule))
            {
                //调用.AddRule方法添加规则
                topologyRuleContainer.AddRule(topologyRule);
            }
            else
            {
                MessageBox.Show("规则添加失败, 不适用于拓扑集");
            }
        }

        /// <summary>
        /// 向拓扑数据集中添加拓扑元素
        /// </summary>
        /// <param name="myTopology">拓扑数据集</param>
        /// <param name="DSName">数据集名</param>
        /// <param name="FtName">要素名</param>
        /// <param name="pFtClass">输出参与拓扑的单个元素</param>
        private void AddSingleElement(ITopology myTopology, string DSName, string FtName, out IFeatureClass pFtClass)
        {
            //打开工作空间
            IFeatureWorkspace pFtWsp = Global.pWorkSpace as IFeatureWorkspace;
            //打开数据集
            IFeatureDataset myFDS = pFtWsp.OpenFeatureDataset(DSName);
            //在数据集中打开要素
            IFeatureClassContainer myFCContainer = myFDS as IFeatureClassContainer;
            IFeatureClass pTempFt = myFCContainer.get_ClassByName(FtName);
            pFtClass = pTempFt;
            //调用ITopology.AddClass方法添加要素
            myTopology.AddClass(pTempFt, 5, 1, 1, false);
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
        /// <param name="myWSp">工作空间</param>
        /// <param name="FtDSName">要素集名称</param>
        /// <param name="TopologyName">拓扑数据集名</param>
        /// <returns></returns>
        public ITopology create_topology(IWorkspace myWSp, string FtDSName, string TopologyName)
        {
            IFeatureWorkspace pFtWsp = myWSp as IFeatureWorkspace;
            IFeatureDataset myFDS = pFtWsp.OpenFeatureDataset(FtDSName);
            IFeatureClassContainer myFCContainer = myFDS as IFeatureClassContainer;
            ITopologyContainer myTopologyContainer = myFDS as ITopologyContainer;
            ITopology myTopology = null;
            try
            {
                myTopology = myTopologyContainer.CreateTopology(TopologyName, myTopologyContainer.DefaultClusterTolerance, -1, "");
                //myTopology = myTopologyContainer.CreateTopology(TopologyName, myTopologyContainer.DefaultClusterTolerance, -1, "");
            }
            catch (Exception ee)
            {
                Flag = true;
                return null;
            }
            return myTopology;

        }
    }
}
