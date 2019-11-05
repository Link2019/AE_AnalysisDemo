using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE_AnalysisDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadMapDoc2();
        }
        /// <summary>
        /// 运用MapDocument对象中的Open方法的函数加载mxd文档
        /// </summary>
        private void loadMapDoc2()
        {
            IMapDocument mapDocument = new MapDocumentClass();
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "打开地图文档";
                ofd.Filter = "map documents(*.mxd)|*.mxd";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string pFileName = ofd.FileName;
                    //filePath——地图文档的路径, ""——赋予默认密码
                    mapDocument.Open(pFileName, "");
                    for (int i = 0; i < mapDocument.MapCount; i++)
                    {
                        //通过get_Map(i)方法逐个加载
                        axMapControl1.Map = mapDocument.get_Map(i);
                    }
                    axMapControl1.Refresh();
                }
                else
                {
                    mapDocument = null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }
        private void 缓冲区分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BufferArea(5000);
        }
        /// <summary>
        /// 缓冲区分析函数
        /// </summary>
        /// <param name="BuffDistance">缓冲区距离</param>
        private void BufferArea(double BuffDistance)
        {
            //以主地图为缓冲区添加对象
            IGraphicsContainer graphicsContainer = axMapControl1.Map as IGraphicsContainer;
            //删除之前存留的所有元素
            graphicsContainer.DeleteAllElements();
            //选中索引值为0的图层
            ILayer layer = axMapControl1.get_Layer(0);
            //此循环用于查找图层名为LayerName的图层索引
            /*
            ILayer layer = null;
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name.Equals("Layer-Name"))
                {
                    layer = axMapControl1.get_Layer(i);
                    break;
                }
            }
            */
            //将图层名为LayerName的图层强转成要素选择集
            IFeatureSelection pFtSel = (IFeatureLayer)layer as IFeatureSelection;
            //将图层名为LayerName的图层中的所有要素加入选择集
            pFtSel.SelectFeatures(null, esriSelectionResultEnum.esriSelectionResultNew, false);

            ICursor pCursor;
            //获得遍历选择集中所有要素的游标
            pFtSel.SelectionSet.Search(null, false, out pCursor);
            IFeatureCursor pFtCursor = pCursor as IFeatureCursor;
            IFeature pFt = pFtCursor.NextFeature();
            //遍历所有选择集中的所有要素, 逐个要素地创建缓冲区
            while (pFt != null)
            {
                //将要素的几何对象(pFt.Shape)强转成ITopologicalOperator
                //pFt.Shape即为创建缓冲区的操作对象
                ITopologicalOperator topologicalOperator = pFt.Shape as ITopologicalOperator;
                //注意: BuffDIstance输入为正时向外缓冲, 为负时向内缓冲
                IPolygon polygon = topologicalOperator.Buffer(BuffDistance) as IPolygon;
                //实例化要素以装载缓冲区
                IElement element = new PolygonElement();
                //将几何要素赋值为多边形
                element.Geometry = polygon;
                //逐个显示
                graphicsContainer.AddElement(element, 0);
                //指向下一个
                pFt = pFtCursor.NextFeature();
            }
            //这里清除选择集, 以免高亮显示的要素与缓冲结果相互混淆
            pFtSel.Clear();
            //刷新axMapControl1
            axMapControl1.Refresh();
        }

        private void 拓扑分析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Topology frmTopo = new Topology();
            frmTopo.Show();
        }
    }
}
