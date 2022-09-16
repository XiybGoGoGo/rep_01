using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyMODEL;
using TcIntelligentTechnologyBLL;
using System.IO;
using System.Threading;
using Cognex.VisionPro.ImageFile;
using Vision_Control;
using System.Windows.Forms.DataVisualization.Charting;

namespace InternalStructure
{
    public partial class formHome : UserControl
    {

        //public Hikrobot_Helper HikrobotHelper = new Hikrobot_Helper();

        public Adlink_204CHelper Adlink204CHelper = new Adlink_204CHelper();
        public Adlink_Model AdlinkModel = new Adlink_Model();
        public Cognex_Helper CognexHelper = new Cognex_Helper();
        public Action<string> AddInformation;
        public Action<string> WriteErrorMes;
        public Func<double[], double[]> Sort;
        public Func<double[], double> GetAverage;
        public uint ProgramStatus = 0;
        public uint nCameraSelecteIndex = 0;
        public string GPath { get; set; }
        string timeAccdb = "";
        Hikrobot_Helper hikRobot; Calibration_Model calibModel;
        List<Point_xy> listSingle1 = new List<Point_xy>();
        List<Point_xy> listSingle2 = new List<Point_xy>();
        List<Point_xy> listSingle3 = new List<Point_xy>();
        List<int> listSingleImgIndex = new List<int>();
        List<int> listSingleGZIndex = new List<int>();
        int singleGZIndex = 1;
        int singleErrorIndex = 0;
        int singleOKIndex = 0;
        bool singleAllowToAdd = false;

        public formHome(string gpath)
        {
            InitializeComponent();
            calibModel = Variable.calibModel;
            this.GPath = gpath;
            //为控件赋初始值
            osgView1.IsLoaded = true;
        }

        public void INITIALIZE()
        {
            try
            {
                ////加载相机所需的参数
                //string[] strSerialNumber = new string[4];
                //for (int i = 0; i < hikRobot.deviceNumber; i++)
                //{
                //    strSerialNumber[i] = hikRobot.SerialNumbers[i];
                //    Static_Variable.listCameramodel = Static_Variable.cXmlhelper.ReadCameraDataFromXml(strSerialNumber, Static_Variable.configPath_Camera);
                //    Static_Variable.cHikrobotHelper.Exposures[i] = Static_Variable.listCameramodel[i].CameraExposure;
                //}

            }
            catch (Exception)
            {
                MessageBox.Show("加载标定信息失败");
            }

        }

        #region 多次拍照一次处理
        /// <summary>
        /// 飞拍抓图
        /// </summary>
        /// <param name="_cameraIndex"></param>
        /// <returns></returns>
        public bool grabImages(int _cameraIndex)
        {
            try
            {
                if (hikRobot.deviceNumber == 0)
                {
                    MessageBox.Show("未连接到相机，请勿开始取像");
                    return false;
                }
                hikRobot.NumberOfAcqs[_cameraIndex] = 0;
                hikRobot.SetExprosure(_cameraIndex);
                calibModel.CalculatePC.ProductLaserImagePath = "Images" + @"\" + DateTime.Now.ToString("yyMMdd") + @"\" + DateTime.Now.ToString("HHmmss");
                timeAccdb = DateTime.Now.ToString("HHmmss");
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\" + calibModel.CalculatePC.ProductLaserImagePath))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\" + calibModel.CalculatePC.ProductLaserImagePath);

                }

                hikRobot.ImagePaths[_cameraIndex] = calibModel.CalculatePC.ProductLaserImagePath;

                Adlink204CHelper.SetPositionZero(AdlinkModel.M_selectAxis);//位置清零
                Adlink204CHelper.SetLinearTrigger(AdlinkModel.M_selectAxis, AdlinkModel.M_CompareStart, AdlinkModel.M_CompareTimes, AdlinkModel.M_CompareInterval);//设置飞拍参数,设备初次上电时初次设置起始次数会变成1，需多设置一次，
                Adlink204CHelper.SetLinearTrigger(AdlinkModel.M_selectAxis, AdlinkModel.M_CompareStart, AdlinkModel.M_CompareTimes, AdlinkModel.M_CompareInterval);//设置飞拍参数
                Adlink204CHelper.SetAccDecSpeed(AdlinkModel.M_selectAxis, AdlinkModel.M_Acc, AdlinkModel.M_Dec);
                AddInformation("位置清零完成");
                DGVDistance.Rows.Clear();
                if (Adlink204CHelper.IsLoadXmlFile && Adlink204CHelper.IsServeOn[AdlinkModel.M_selectAxis])
                {
                    Task task = new Task(() =>
                    {
                        Adlink204CHelper.MoveRelative(AdlinkModel.M_selectAxis, (int)AdlinkModel.M_Vm, AdlinkModel.M_Pulse);
                        AddInformation("等待运动完成");
                        //等待Motion Down完成
                        int motionStatusMdn = 5;
                        while ((Adlink204CHelper.GetMotionStatus(AdlinkModel.M_selectAxis) & 1 << motionStatusMdn) == 0)
                        {
                            Thread.Sleep(100);
                        }
                        AddInformation("运动完成,开始计算点云...");

                        if (ProgramStatus != 1)
                        {
                            AddInformation("程序暂停中");
                            return;
                        }
                        getVisionProPoints(calibModel);
                        AddInformation("点云计算完成");
                        //将图片从0开始重命名
                        //DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Image");
                        //FileInfo[] fi = di.GetFiles("*.bmp");
                        ////删除图片
                        //foreach (var item in fi)
                        //{
                        //    File.Delete(item.FullName);
                        //}
                        //getPointsCloud();
                    });
                    task.Start();
                }
                else
                {
                    MessageBox.Show("未加载配置文件或未开启使能");
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;

            }

        }

        /// <summary>
        /// 获取钩爪数据
        /// </summary>
        /// <param name="model">标定信息</param>
        public bool getVisionProPoints(Calibration_Model model)
        {
            try
            {
                List<Point_xy> List_1 = new List<Point_xy>();
                List<Point_xy> List_2 = new List<Point_xy>();
                List<Point_xy> List_3 = new List<Point_xy>();
                List<int> imgIndex = new List<int>();
                List<int> GZIndex_list = new List<int>();
                int GZIndex_int = 1;
                int ErrorIndex = 0;
                int OKIndex = 0;
                bool allowToAdd = false;
                DirectoryInfo di = new DirectoryInfo(GPath + "\\" + "Images\\220727\\160341");//model.ProductLaserImagePath
                FileInfo[] fi = di.GetFiles("*.bmp");
                if (fi.Length < 10)
                {
                    AddInformation("未获取到图像参数，请检查");
                    return false;
                }
                for (int i = 0; i < fi.Length; i++)//fi.Length
                {
                    CognexHelper.cogImageFile.Operator.Open(fi[i].DirectoryName + "\\" + i.ToString() + ".bmp", CogImageFileModeConstants.Read);
                    CognexHelper.cogImageFile.Run();
                    CognexHelper.cogToolBlock.Inputs["InputImage"].Value = CognexHelper.cogImageFile.OutputImage;
                    CognexHelper.cogToolBlock.Run();
                    bool result = Convert.ToBoolean(CognexHelper.cogToolBlock.Outputs["Result"].Value);
                    if (result)
                    {
                        double[] point1 = (double[])(CognexHelper.cogToolBlock.Outputs["Point_1"].Value);
                        double[] point2 = (double[])(CognexHelper.cogToolBlock.Outputs["Point_2"].Value);
                        double[] point3 = (double[])(CognexHelper.cogToolBlock.Outputs["Point_3"].Value);
                        Point_xy point_Xy1 = new Point_xy(point1[0], point1[1]);
                        Point_xy point_Xy2 = new Point_xy(point2[0], point2[1]);
                        Point_xy point_Xy3 = new Point_xy(point3[0], point3[1]);
                        List_1.Add(point_Xy1);
                        List_2.Add(point_Xy2);
                        List_3.Add(point_Xy3);
                        imgIndex.Add(i);
                        GZIndex_list.Add(GZIndex_int);
                        OKIndex++;
                        //ErrorIndex = 0;
                        if (OKIndex > 30)
                        {
                            allowToAdd = true;
                            //OKIndex = 0;
                            ErrorIndex = 0;
                        }
                    }
                    else
                    {
                        ErrorIndex++;
                        if (ErrorIndex > 30 && allowToAdd == true)
                        {
                            OKIndex = 0;
                            allowToAdd = false;
                            ErrorIndex = 0;
                            GZIndex_int++;
                            if (GZIndex_int > 3)
                            {
                                GZIndex_int = 1;
                            }
                        }
                    }
                }
                Point_xy[] point_Xies1 = List_1.ToArray();
                Point_xy[] point_Xies2 = List_2.ToArray();
                Point_xy[] point_Xies3 = List_3.ToArray();
                int[] imgIndex_Xies = imgIndex.ToArray();
                int[] GZIndex_list_Xies = GZIndex_list.ToArray();
                double[] distance_out1 = new double[point_Xies1.Length];
                double[] distance_out2 = new double[point_Xies2.Length];
                double[] distance_out3 = new double[point_Xies3.Length];
                //调用算法，获取点云数据
                TCPoint3d[] tCPoint3Ds = new TCPoint3d[point_Xies1.Length + point_Xies2.Length + point_Xies3.Length];
                CalculatePointLinePlaneCommon.point2dTo3d(calibModel, point_Xies1, point_Xies2, point_Xies3, imgIndex_Xies, GZIndex_list_Xies, ref distance_out1, ref distance_out2, ref distance_out3, ref tCPoint3Ds);
                //绘制折线图

                this.Invoke((EventHandler)delegate
                {
                    chart1.Series.Clear();
                    Series GZ1 = new Series("GZ1");
                    Series GZ2 = new Series("GZ2");
                    Series GZ3 = new Series("GZ3");
                    GZ1.ChartType = SeriesChartType.Spline;
                    //GZ1.IsValueShownAsLabel = true;
                    GZ1.Color = Color.Red;
                    GZ2.ChartType = SeriesChartType.Spline;
                    //GZ2.IsValueShownAsLabel = true;
                    GZ2.Color = Color.Green;
                    GZ3.ChartType = SeriesChartType.Spline;
                    //GZ3.IsValueShownAsLabel = true;
                    GZ3.Color = Color.Blue;

                    chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 1;
                    chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                    chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart1.ChartAreas[0].AxisX.IsMarginVisible = true;
                    chart1.ChartAreas[0].AxisX.Title = "Index";
                    chart1.ChartAreas[0].AxisX.TitleForeColor = Color.Crimson;
                    chart1.ChartAreas[0].AxisY.Title = "Value";
                    chart1.ChartAreas[0].AxisY.TitleForeColor = Color.Crimson;
                    chart1.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Horizontal;

                    List<double> distanceList_1 = new List<double>();
                    List<double> distanceList_2 = new List<double>();
                    List<double> distanceList_3 = new List<double>();
                    for (int a = 0; a < distance_out1.Length; a++)
                    {
                        if (distance_out1[a] > 2)
                        {
                            GZ1.Points.Add(distance_out1[a] + model.CalculatePC.offset_Avg, a + 1);
                            distanceList_1.Add(distance_out1[a] + model.CalculatePC.offset_Avg);
                        }
                    }
                    distance_out1 = distanceList_1.ToArray();
                    chart1.Series.Add(GZ1);

                    for (int b = 0; b < distance_out2.Length; b++)
                    {
                        if (distance_out2[b] > 2)
                        {
                            GZ2.Points.Add(distance_out2[b] + model.CalculatePC.offset_Avg, b + 1);
                            distanceList_2.Add(distance_out2[b] + model.CalculatePC.offset_Avg);
                        }
                    }
                    distance_out2 = distanceList_2.ToArray();
                    chart1.Series.Add(GZ2);

                    for (int c = 0; c < distance_out3.Length; c++)
                    {
                        if (distance_out3[c] > 2)
                        {
                            GZ3.Points.Add(distance_out3[c] + model.CalculatePC.offset_Avg, c + 1);
                            distanceList_3.Add(distance_out3[c] + model.CalculatePC.offset_Avg);
                        }

                    }
                    distance_out3 = distanceList_3.ToArray();
                    chart1.Series.Add(GZ3);

                    //数值排序
                    if (Sort(distance_out1).Length == 0) AddInformation("数值1排序失败-00");
                    if (Sort(distance_out2).Length == 0) AddInformation("数值2排序失败-00");
                    if (Sort(distance_out3).Length == 0) AddInformation("数值3排序失败-00");
                    //取平均值
                    double[] average = new double[] { GetAverage(distance_out1), GetAverage(distance_out2), GetAverage(distance_out3) };
                    bool Result = true;
                    foreach (var avg in average)
                    {
                        if (avg > Convert.ToDouble(calibModel.CalculatePC.compare_Max) || avg < Convert.ToDouble(calibModel.CalculatePC.compare_Min))
                        {
                            Result = false;
                            //Static_Variable._calModel.number_OK++;
                        }

                    }

                    if (!Result)
                    {
                        calibModel.CalculatePC.number_NG++;
                    }
                    else
                    {
                        calibModel.CalculatePC.number_OK++;
                    }

                    label_Total.Text = (calibModel.CalculatePC.number_OK + calibModel.CalculatePC.number_NG).ToString();
                    label_OK.Text = calibModel.CalculatePC.number_OK.ToString();
                    label_Percent.Text = ((calibModel.CalculatePC.number_OK / (calibModel.CalculatePC.number_OK + calibModel.CalculatePC.number_NG)) * 100).ToString("0.00");

                    //将平均值，最小值，最大值添加到datagirdview中
                    DGVDistance.Rows.Add("Avg", average[0].ToString("0.00"), average[1].ToString("0.00"), average[2].ToString("0.00"));
                    DGVDistance.Rows.Add("Min", distance_out1[0].ToString("0.00"), distance_out2[0].ToString("0.00"), distance_out3[0].ToString("0.00"));
                    DGVDistance.Rows.Add("Max", distance_out1[distance_out1.Length - 1].ToString("0.00"), distance_out2[distance_out2.Length - 1].ToString("0.00"), distance_out3[distance_out3.Length - 1].ToString("0.00"));

                    //取数组长度最大值
                    int _maxlength = 0;
                    if (distance_out1.Length > distance_out2.Length)
                    {
                        _maxlength = distance_out1.Length;
                    }
                    else
                    {
                        _maxlength = distance_out2.Length;
                    }

                    if (distance_out3.Length > _maxlength)
                    {
                        _maxlength = distance_out3.Length;
                    }

                    //将数值显示到datagridvie中
                    for (int i = 0; i < _maxlength; i++)
                    {
                        string number1 = "";
                        string number2 = "";
                        string number3 = "";
                        number1 = i < distance_out1.Length ? distance_out1[i].ToString("0.00") : "";
                        number2 = i < distance_out2.Length ? distance_out2[i].ToString("0.00") : "";
                        number3 = i < distance_out3.Length ? distance_out3[i].ToString("0.00") : "";

                        DGVDistance.Rows.Add(i, number1, number2, number3);
                    }
                    //显示点云图
                    osgView1.InputCloud = tCPoint3Ds;

                    string strDate = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00");

                    //string FileTime = dateTimePicker1.Value.Year.ToString() + "-" + dateTimePicker1.Value.Month.ToString();
                    string _directoryPath = GPath + @"\Data";
                    ExcelHelper.CreateAccdbFile(strDate, _directoryPath);
                    string _configPath = _directoryPath + @"\" + strDate + ".accdb";
                    string _strSql = "insert into ProductInfo(CheckTime,AverageNo1,AverageNo2,AverageNo3) Values (" + timeAccdb + "," + average[0] + "," + average[1] + "," + average[2] + ")";
                    ExcelHelper.DbOperation(_strSql, _configPath);
                });

                return true;
            }
            catch (Exception ex)
            {
                AddInformation("点云计算失败，请检查");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }
        #endregion

        #region 单次拍照单次处理
        /// <summary>
        /// 飞拍抓图
        /// </summary>
        /// <param name="_cameraIndex"></param>
        /// <returns></returns>
        public bool grabImage(int _cameraIndex)
        {
            try
            {
                if (hikRobot.deviceNumber == 0)
                {
                    MessageBox.Show("未连接到相机，请勿开始取像");
                    return false;
                }
                hikRobot.NumberOfAcqs[_cameraIndex] = 0;
                hikRobot.SetExprosure(_cameraIndex);
                calibModel.CalculatePC.ProductLaserImagePath = "Images" + @"\" + DateTime.Now.ToString("yyMMdd") + @"\" + DateTime.Now.ToString("HHmmss");
                timeAccdb = DateTime.Now.ToString("HHmmss");
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\" + calibModel.CalculatePC.ProductLaserImagePath))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"\" + calibModel.CalculatePC.ProductLaserImagePath);

                }

                hikRobot.ImagePaths[_cameraIndex] = calibModel.CalculatePC.ProductLaserImagePath;

                Adlink204CHelper.SetPositionZero(AdlinkModel.M_selectAxis);//位置清零
                Adlink204CHelper.SetLinearTrigger(AdlinkModel.M_selectAxis, AdlinkModel.M_CompareStart, AdlinkModel.M_CompareTimes, AdlinkModel.M_CompareInterval);//设置飞拍参数,设备初次上电时初次设置起始次数会变成1，需多设置一次，
                Adlink204CHelper.SetLinearTrigger(AdlinkModel.M_selectAxis, AdlinkModel.M_CompareStart, AdlinkModel.M_CompareTimes, AdlinkModel.M_CompareInterval);//设置飞拍参数
                Adlink204CHelper.SetAccDecSpeed(AdlinkModel.M_selectAxis, AdlinkModel.M_Acc, AdlinkModel.M_Dec);
                AddInformation("位置清零完成");
                DGVDistance.Rows.Clear();
                if (Adlink204CHelper.IsLoadXmlFile && Adlink204CHelper.IsServeOn[AdlinkModel.M_selectAxis])
                {
                    listSingle1.Clear();
                    listSingle2.Clear();
                    listSingle3.Clear();
                    listSingleImgIndex.Clear();
                    listSingleGZIndex.Clear();
                    singleGZIndex = 1;
                    singleErrorIndex = 0;
                    singleOKIndex = 0;
                    singleAllowToAdd = false;

                    Task task = new Task(() =>
                    {
                        Adlink204CHelper.MoveRelative(AdlinkModel.M_selectAxis, (int)AdlinkModel.M_Vm, AdlinkModel.M_Pulse);
                        AddInformation("等待运动完成");
                        //等待Motion Down完成
                        int motionStatusMdn = 5;
                        while ((Adlink204CHelper.GetMotionStatus(AdlinkModel.M_selectAxis) & 1 << motionStatusMdn) == 0)
                        {
                            Thread.Sleep(100);
                        }
                        AddInformation("运动完成,开始计算点云...");

                        if (ProgramStatus != 1)
                        {
                            AddInformation("程序暂停中");
                            return;
                        }
                        showInfomation(calibModel);

                        AddInformation("点云计算完成");
                        //将图片从0开始重命名
                        //DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Image");
                        //FileInfo[] fi = di.GetFiles("*.bmp");
                        ////删除图片
                        //foreach (var item in fi)
                        //{
                        //    File.Delete(item.FullName);
                        //}
                        //getPointsCloud();
                    });
                    task.Start();
                }
                else
                {
                    MessageBox.Show("未加载配置文件或未开启使能");
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;

            }

        }

        /// <summary>
        /// 获取钩爪数据
        /// </summary>
        /// <param name="model">标定信息</param>
        public bool getVisionProPoint(byte[] imageBuffer)
        {
            try
            {

                if (imageBuffer == null)
                {
                    AddInformation("未获取到图像参数，请检查");
                    return false;
                }

                Image outputImg;

                using (MemoryStream ms = new MemoryStream(imageBuffer))
                {
                    outputImg = Image.FromStream(ms);
                }

                CognexHelper.cogToolBlock.Inputs["InputImage"].Value = outputImg;
                CognexHelper.cogToolBlock.Run();
                bool result = Convert.ToBoolean(CognexHelper.cogToolBlock.Outputs["Result"].Value);
                if (result)
                {
                    double[] point1 = (double[])(CognexHelper.cogToolBlock.Outputs["Point_1"].Value);
                    double[] point2 = (double[])(CognexHelper.cogToolBlock.Outputs["Point_2"].Value);
                    double[] point3 = (double[])(CognexHelper.cogToolBlock.Outputs["Point_3"].Value);
                    Point_xy point_Xy1 = new Point_xy(point1[0], point1[1]);
                    Point_xy point_Xy2 = new Point_xy(point2[0], point2[1]);
                    Point_xy point_Xy3 = new Point_xy(point3[0], point3[1]);
                    listSingle1.Add(point_Xy1);
                    listSingle2.Add(point_Xy2);
                    listSingle3.Add(point_Xy3);
                    listSingleImgIndex.Add(Convert.ToInt32(hikRobot.NumberOfAcqs[nCameraSelecteIndex]));
                    singleOKIndex++;
                    //ErrorIndex = 0;
                    if (singleOKIndex > 30)
                    {
                        singleAllowToAdd = true;
                        //OKIndex = 0;
                        singleErrorIndex = 0;
                    }
                }
                else
                {
                    singleErrorIndex++;
                    if (singleErrorIndex > 30 && singleAllowToAdd == true)
                    {
                        singleOKIndex = 0;
                        singleAllowToAdd = false;
                        singleErrorIndex = 0;
                        singleGZIndex++;
                        if (singleGZIndex > 3)
                        {
                            singleGZIndex = 1;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AddInformation("点云计算失败，请检查");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }

        /// <summary>
        /// 处理信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool showInfomation(Calibration_Model model)
        {
            try
            {
                Point_xy[] point_Xies1 = listSingle1.ToArray();
                Point_xy[] point_Xies2 = listSingle2.ToArray();
                Point_xy[] point_Xies3 = listSingle3.ToArray();
                int[] imgIndex_Xies = listSingleImgIndex.ToArray();
                int[] GZIndex_list_Xies = listSingleGZIndex.ToArray();
                double[] distance_out1 = new double[point_Xies1.Length];
                double[] distance_out2 = new double[point_Xies2.Length];
                double[] distance_out3 = new double[point_Xies3.Length];
                //调用算法，获取点云数据
                TCPoint3d[] tCPoint3Ds = new TCPoint3d[point_Xies1.Length + point_Xies2.Length + point_Xies3.Length];
                CalculatePointLinePlaneCommon.point2dTo3d(model, point_Xies1, point_Xies2, point_Xies3, imgIndex_Xies, GZIndex_list_Xies, ref distance_out1, ref distance_out2, ref distance_out3, ref tCPoint3Ds);
                //绘制折线图

                this.Invoke((EventHandler)delegate
                {
                    chart1.Series.Clear();
                    Series GZ1 = new Series("GZ1");
                    Series GZ2 = new Series("GZ2");
                    Series GZ3 = new Series("GZ3");
                    GZ1.ChartType = SeriesChartType.Spline;
                    //GZ1.IsValueShownAsLabel = true;
                    GZ1.Color = Color.Red;
                    GZ2.ChartType = SeriesChartType.Spline;
                    //GZ2.IsValueShownAsLabel = true;
                    GZ2.Color = Color.Green;
                    GZ3.ChartType = SeriesChartType.Spline;
                    //GZ3.IsValueShownAsLabel = true;
                    GZ3.Color = Color.Blue;

                    chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 1;
                    chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                    chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
                    chart1.ChartAreas[0].AxisX.IsMarginVisible = true;
                    chart1.ChartAreas[0].AxisX.Title = "Index";
                    chart1.ChartAreas[0].AxisX.TitleForeColor = Color.Crimson;
                    chart1.ChartAreas[0].AxisY.Title = "Value";
                    chart1.ChartAreas[0].AxisY.TitleForeColor = Color.Crimson;
                    chart1.ChartAreas[0].AxisY.TextOrientation = TextOrientation.Horizontal;

                    List<double> distanceList_1 = new List<double>();
                    List<double> distanceList_2 = new List<double>();
                    List<double> distanceList_3 = new List<double>();
                    for (int a = 0; a < distance_out1.Length; a++)
                    {
                        if (distance_out1[a] > 2)
                        {
                            GZ1.Points.Add(distance_out1[a] + model.CalculatePC.offset_Avg, a + 1);
                            distanceList_1.Add(distance_out1[a] + model.CalculatePC.offset_Avg);
                        }
                    }
                    distance_out1 = distanceList_1.ToArray();
                    chart1.Series.Add(GZ1);

                    for (int b = 0; b < distance_out2.Length; b++)
                    {
                        if (distance_out2[b] > 2)
                        {
                            GZ2.Points.Add(distance_out2[b] + model.CalculatePC.offset_Avg, b + 1);
                            distanceList_2.Add(distance_out2[b] + model.CalculatePC.offset_Avg);
                        }
                    }
                    distance_out2 = distanceList_2.ToArray();
                    chart1.Series.Add(GZ2);

                    for (int c = 0; c < distance_out3.Length; c++)
                    {
                        if (distance_out3[c] > 2)
                        {
                            GZ3.Points.Add(distance_out3[c] + model.CalculatePC.offset_Avg, c + 1);
                            distanceList_3.Add(distance_out3[c] + model.CalculatePC.offset_Avg);
                        }

                    }
                    distance_out3 = distanceList_3.ToArray();
                    chart1.Series.Add(GZ3);

                    //数值排序
                    if (Sort(distance_out1).Length == 0) AddInformation("数值1排序失败-00");
                    if (Sort(distance_out2).Length == 0) AddInformation("数值2排序失败-00");
                    if (Sort(distance_out3).Length == 0) AddInformation("数值3排序失败-00");
                    //取平均值
                    double[] average = new double[] { GetAverage(distance_out1), GetAverage(distance_out2), GetAverage(distance_out3) };
                    bool Result = true;
                    foreach (var avg in average)
                    {
                        if (avg > Convert.ToDouble(calibModel.CalculatePC.compare_Max) || avg < Convert.ToDouble(calibModel.CalculatePC.compare_Min))
                        {
                            Result = false;
                            //Static_Variable._calModel.number_OK++;
                        }

                    }

                    if (!Result)
                    {
                        calibModel.CalculatePC.number_NG++;
                    }
                    else
                    {
                        calibModel.CalculatePC.number_OK++;
                    }

                    label_Total.Text = (calibModel.CalculatePC.number_OK + calibModel.CalculatePC.number_NG).ToString();
                    label_OK.Text = calibModel.CalculatePC.number_OK.ToString();
                    label_Percent.Text = ((calibModel.CalculatePC.number_OK / (calibModel.CalculatePC.number_OK + calibModel.CalculatePC.number_NG)) * 100).ToString("0.00");

                    //将平均值，最小值，最大值添加到datagirdview中
                    DGVDistance.Rows.Add("Avg", average[0].ToString("0.00"), average[1].ToString("0.00"), average[2].ToString("0.00"));
                    DGVDistance.Rows.Add("Min", distance_out1[0].ToString("0.00"), distance_out2[0].ToString("0.00"), distance_out3[0].ToString("0.00"));
                    DGVDistance.Rows.Add("Max", distance_out1[distance_out1.Length - 1].ToString("0.00"), distance_out2[distance_out2.Length - 1].ToString("0.00"), distance_out3[distance_out3.Length - 1].ToString("0.00"));

                    //取数组长度最大值
                    int _maxlength = 0;
                    if (distance_out1.Length > distance_out2.Length)
                    {
                        _maxlength = distance_out1.Length;
                    }
                    else
                    {
                        _maxlength = distance_out2.Length;
                    }

                    if (distance_out3.Length > _maxlength)
                    {
                        _maxlength = distance_out3.Length;
                    }

                    //将数值显示到datagridvie中
                    for (int i = 0; i < _maxlength; i++)
                    {
                        string number1 = "";
                        string number2 = "";
                        string number3 = "";
                        number1 = i < distance_out1.Length ? distance_out1[i].ToString("0.00") : "";
                        number2 = i < distance_out2.Length ? distance_out2[i].ToString("0.00") : "";
                        number3 = i < distance_out3.Length ? distance_out3[i].ToString("0.00") : "";

                        DGVDistance.Rows.Add(i, number1, number2, number3);
                    }
                    //显示点云图
                    osgView1.InputCloud = tCPoint3Ds;

                    string strDate = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00");

                    //string FileTime = dateTimePicker1.Value.Year.ToString() + "-" + dateTimePicker1.Value.Month.ToString();
                    string _directoryPath = GPath + @"\Data";
                    ExcelHelper.CreateAccdbFile(strDate, _directoryPath);
                    string _configPath = _directoryPath + @"\" + strDate + ".accdb";
                    string _strSql = "insert into ProductInfo(CheckTime,AverageNo1,AverageNo2,AverageNo3) Values (" + timeAccdb + "," + average[0] + "," + average[1] + "," + average[2] + ")";
                    ExcelHelper.DbOperation(_strSql, _configPath);
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        ///// <summary>
        ///// 显示加载进度
        ///// </summary>
        ///// <param name="str">显示加载内容</param>
        ///// <param name="iProcess">显示加载进度条</param>
        //private bool ShowProcess(string str)
        //{
        //    try
        //    {
        //        this.Invoke((EventHandler)delegate
        //        {
        //            label_information.Items.Add(str);
        //            label_information.SelectedIndex = label_information.Items.Count - 1;
        //        });
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
        //        return false;
        //    }

        //}
    }
}
