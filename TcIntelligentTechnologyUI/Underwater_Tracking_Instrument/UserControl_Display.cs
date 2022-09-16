using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cognex.VisionPro;
using System.Threading;
using System.Runtime.InteropServices;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ImageFile;
using System.IO;
using Newtonsoft.Json.Linq;
using TcIntelligentTechnologyBLL;
using Vision_Control;

namespace UnderwaterTrackingInstrument
{
    public partial class UserControl_Display : UserControl
    {
        public enum InitilizationError
        {
            INIT_OK = 0,
            INIT_PATH_ERROR = 1,
            INIT_SYSTEM_PARA_ERRPR = 2,
            INIT_CONTROL_ERROR = 3,
            INIT_COGNEX_COLTROL_ERROR = 4,
            INIT_COMMUNICATION_ERROR = 5,
        }

        public CogAcqFifoTool[] CamLocate = new CogAcqFifoTool[5];

        public CogToolBlock[] TbCheckBoard = new CogToolBlock[5];

        public CogToolBlock[] TbLocate = new CogToolBlock[5];

        public CogImageFileTool GSaveImage = new CogImageFileTool();

        public CogImage8Grey[] GOriginalImage = new CogImage8Grey[5];

        public CogRecordDisplay[] CogRecordDisplay = new CogRecordDisplay[3];

        public string GPath { get; set; }

        public string GProductName { get; set; }

        public string ExtrinsicsPath { get; set; }

        public string IintrinsicsPath { get; set; }

        public string Result;

        public double StandardX;

        public double StandardY;

        public double StandardR;

        public double ToleranceX = 5;

        public double ToleranceY = 5;

        public double ToleranceR = 5;

        public float[] fx = new float[6];
        public float[] fy = new float[6];
        public TCRTMatrix4[] StandardRT = new TCRTMatrix4[1];
        public TCPoint3d[] P3dd = new TCPoint3d[3];
        public static StructPath structPath = new StructPath();
        public static StructProductInfo structProductInfo = new StructProductInfo();
        public static Struct_CameraInfo strctCameraInfo = new Struct_CameraInfo();
        public static Struct_VisionInfo structVisionInfo = new Struct_VisionInfo();
        public static JsonHelper jsonHelper = new JsonHelper();

        private OpenFileDialog openFileDialog1 = new OpenFileDialog();

        /// <summary>
        /// 视觉工具参数
        /// </summary>
        public Dictionary<int, List<Struct_VisionInfo>> gVisionTool_Info = new Dictionary<int, List<Struct_VisionInfo>>();

        #region Method

        public class Get3DInformation_ZY
        {
            [DllImport("tcHKCalibration.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void __2dTo3d(Point2f[] centers1, int size_centers1, Point2f[] centers2, int size_centers2, char[] path_ex, char[] path_in, ref Point3f pnts_out);

            /// <summary>
            /// 计算变换矩阵
            /// </summary>
            /// <param name="srcPoints">输入原始坐标系</param>
            /// <param name="pointArray2">输入目标坐标系</param>
            /// <param name="size">数组大小</param>
            /// <param name="rt">输出变换矩阵</param>
            [DllImport("coordinateTransform.dll", CallingConvention = CallingConvention.StdCall)]
            //三维空间中多组点,srcPoints:输入原始坐标系，dstPoints：输入目标坐标系，rt：输出变换矩阵
            public static extern void Get3DR_TransMatrix(ref TCPoint3d srcPoints, ref TCPoint3d pointArray2, int size, ref TCRTMatrix4 rt);

            /// <summary>
            /// 三维点转换
            /// </summary>
            /// <param name="srcPoints">输入</param>
            /// <param name="pointArray2"></param>
            /// <param name="size"></param>
            /// <param name="rt"></param>
            [DllImport("coordinateTransform.dll", CallingConvention = CallingConvention.StdCall)]
            //两个坐标系，空间中三维点之间的变换 srcPoints:输入原始坐标系点，dstPoints：输出目标坐标系点，rt：输入变换矩阵
            public static extern void point3dTransform(TCPoint3d srcPoints, ref TCPoint3d pointArray2, int size, TCRTMatrix4 rt);

            [DllImport("coordinateTransform.dll", CallingConvention = CallingConvention.StdCall)]
            //根据输入的平面上的三个点输入srcPoints[],输入pointSize点的大小必须是3，输入baseNorm基准向量，输出欧拉角的度数（角度）
            public static extern void rotationVectorToEuler(ref TCPoint3d srcPoints, int pointSize, TCPoint3d baseNorm, ref TCPoint3d outRotation);
        }

        /// <summary>
        /// 将2D点转换为3D点
        /// </summary>
        /// <param name="center1"></param>
        /// <param name="center2"></param>
        /// <returns></returns>
        public TCPoint3d transferPoint(Point2f[] center1, Point2f[] center2)
        {
            TCPoint3d[] pp = new TCPoint3d[1];
            try
            {
                string path_exstr = @"D:\test\extrinsics.yml";//ExtrinsicsPath;
                char[] path_ex = path_exstr.ToCharArray();

                string path_instr = @"D:\test\intrinsics.yml";//IintrinsicsPath;//
                char[] path_in = path_instr.ToCharArray();

                Point3f[] p3 = new Point3f[1];

                Get3DInformation_ZY.__2dTo3d(center1, 1, center2, 1, path_ex, path_in, ref p3[0]);

                TCPoint3d np = new TCPoint3d();

                np.x = p3[0].x;
                np.y = p3[0].y;
                np.z = p3[0].z;

                //Method.gMotion.AddInformation("三维坐标   X：" + np.x.ToString());
                //Method.gMotion.AddInformation("三维坐标   Y：" + np.y.ToString());
                //Method.gMotion.AddInformation("三维坐标   Z：" + np.z.ToString());
                Get3DInformation_ZY.point3dTransform(np, ref pp[0], 1, StandardRT[0]);
            }
            catch (Exception)
            {

            }

            return pp[0];

        }

        /// <summary>
        /// 点位转换
        /// </summary>
        /// <param name="index"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point2f[] changeform(int index, float[] x, float[] y)
        {
            Point2f[] p2 = new Point2f[1];
            p2[0].x = x[index];
            p2[0].y = y[index];
            return p2;
        }

        /// <summary>
        /// 初始化转换矩阵
        /// </summary>
        public void IniRT()
        {

            float[] fx1 = new float[] { 827.140f, 1403.61f, 955.722f };
            float[] fy1 = new float[] { 1069.410f, 1078.54f, 1021.57f };

            float[] fx2 = new float[] { 1405.036f, 1983.32f, 1367.31f };
            float[] fy2 = new float[] { 1189.51f, 1186.95f, 1138.57f };
            Point2f[] p2 = new Point2f[1];
            p2 = changeform(0, fx1, fy1);
            Point2f[] p2p = new Point2f[1];
            p2p = changeform(0, fx2, fy2);

            Point2f[] p21 = new Point2f[1];
            p21 = changeform(1, fx1, fy1);
            Point2f[] p2p1 = new Point2f[1];
            p2p1 = changeform(1, fx2, fy2);

            Point2f[] p22 = new Point2f[1];
            p22 = changeform(2, fx1, fy1);
            Point2f[] p2p2 = new Point2f[1];
            p2p2 = changeform(2, fx2, fy2);

            Point3f[] p3 = new Point3f[3];

            string path_exstr = @"D:\test\extrinsics.yml";
            char[] path_ex = path_exstr.ToCharArray();

            string path_instr = @"D:\test\intrinsics.yml";
            char[] path_in = path_instr.ToCharArray();

            Get3DInformation_ZY.__2dTo3d(p2, 1, p2p, 1, path_ex, path_in, ref p3[0]);
            Get3DInformation_ZY.__2dTo3d(p21, 1, p2p1, 1, path_ex, path_in, ref p3[1]);
            Get3DInformation_ZY.__2dTo3d(p22, 1, p2p2, 1, path_ex, path_in, ref p3[2]);

            TCPoint3d[] p3d = new TCPoint3d[3];
            TCPoint3d[] p3dd = new TCPoint3d[3];
            TCRTMatrix4[] m4 = new TCRTMatrix4[3];

            p3d[0].x = p3[0].x;
            p3d[0].y = p3[0].y;
            p3d[0].z = p3[0].z;
            p3d[1].x = p3[1].x;
            p3d[1].y = p3[1].y;
            p3d[1].z = p3[1].z;
            p3d[2].x = p3[2].x;
            p3d[2].y = p3[2].y;
            p3d[2].z = p3[2].z;

            p3dd[0].x = 0;
            p3dd[0].y = 0;
            p3dd[0].z = 0;
            p3dd[1].x = 490;
            p3dd[1].y = 0;
            p3dd[1].z = 0;
            p3dd[2].x = 0;
            p3dd[2].y = 400;
            p3dd[2].z = 0;

            Get3DInformation_ZY.Get3DR_TransMatrix(ref p3d[0], ref p3dd[0], 3, ref StandardRT[0]);
        }

        /// <summary>
        /// 统一坐标系，计算出坐标变换矩阵
        /// </summary>
        public bool calibCheck()
        {
            try
            {
                float[] fx1 = new float[] { fx[0], fx[1], fx[2] };
                float[] fy1 = new float[] { fy[0], fy[1], fy[2] };

                float[] fx2 = new float[] { fx[3], fx[4], fx[5] };
                float[] fy2 = new float[] { fy[3], fy[4], fy[5] };

                Point2f[] p2 = new Point2f[1];
                p2 = changeform(0, fx1, fy1);
                Point2f[] p2p = new Point2f[1];
                p2p = changeform(0, fx2, fy2);

                Point2f[] p21 = new Point2f[1];
                p21 = changeform(1, fx1, fy1);
                Point2f[] p2p1 = new Point2f[1];
                p2p1 = changeform(1, fx2, fy2);

                Point2f[] p22 = new Point2f[1];
                p22 = changeform(2, fx1, fy1);
                Point2f[] p2p2 = new Point2f[1];
                p2p2 = changeform(2, fx2, fy2);

                Point3f[] p3 = new Point3f[3];

                string path_exstr = @"D:\test\extrinsics.yml";
                char[] path_ex = path_exstr.ToCharArray();

                string path_instr = @"D:\test\intrinsics.yml";
                char[] path_in = path_instr.ToCharArray();

                Get3DInformation_ZY.__2dTo3d(p2, 1, p2p, 1, path_ex, path_in, ref p3[0]);
                Get3DInformation_ZY.__2dTo3d(p21, 1, p2p1, 1, path_ex, path_in, ref p3[1]);
                Get3DInformation_ZY.__2dTo3d(p22, 1, p2p2, 1, path_ex, path_in, ref p3[2]);

                TCPoint3d[] p3d = new TCPoint3d[3];

                TCRTMatrix4[] m4 = new TCRTMatrix4[3];

                p3d[0].x = p3[0].x;
                p3d[0].y = p3[0].y;
                p3d[0].z = p3[0].z;
                p3d[1].x = p3[1].x;
                p3d[1].y = p3[1].y;
                p3d[1].z = p3[1].z;
                p3d[2].x = p3[2].x;
                p3d[2].y = p3[2].y;
                p3d[2].z = p3[2].z;

                AddInformation("一点3D坐标X！" + p3d[0].x.ToString());
                AddInformation("一点3D坐标Y！" + p3d[0].y.ToString());
                AddInformation("一点3D坐标Z！" + p3d[0].z.ToString());
                AddInformation("二点3D坐标X！" + p3d[1].x.ToString());
                AddInformation("二点3D坐标Y！" + p3d[1].y.ToString());
                AddInformation("二点3D坐标Z！" + p3d[1].z.ToString());
                AddInformation("三点3D坐标X！" + p3d[2].x.ToString());
                AddInformation("三点3D坐标Y！" + p3d[2].y.ToString());
                AddInformation("三点3D坐标Z！" + p3d[2].z.ToString());

                Get3DInformation_ZY.Get3DR_TransMatrix(ref p3d[0], ref P3dd[0], 3, ref StandardRT[0]);

                AddInformation("坐标转换成功！");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 将10进制数转换成16进制
        /// </summary>
        /// <param name="data">转换的数值，，范围为-32768到32767</param>
        /// <returns></returns>
        public string TransformCode(Int16 data)
        {
            string res = "";
            string transdata;

            transdata = Convert.ToString(data, 16).ToUpper();

            res = transdata;

            if (transdata.Length < 4)
            {
                if (data >= 0)
                {
                    for (int i = 0; i < 4 - transdata.Length; i++)
                    {
                        res = "0" + res;
                    }
                }
                else
                {
                    for (int i = 0; i < 4 - transdata.Length; i++)
                    {
                        res = "F" + res;
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// 初始化Cognex视觉工具
        /// </summary>
        public InitilizationError Init_CogVisionTool()
        {
            try
            {
                CamLocate[1] = (CogAcqFifoTool)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "定位相机1.vpp");
                CamLocate[2] = (CogAcqFifoTool)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "定位相机2.vpp");
                TbCheckBoard[1] = (CogToolBlock)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "定位标定1.vpp");
                TbCheckBoard[2] = (CogToolBlock)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "定位标定2.vpp");
                TbLocate[1] = (CogToolBlock)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "定位1.vpp");
                TbLocate[2] = (CogToolBlock)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "定位2.vpp");
                TbLocate[3] = (CogToolBlock)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "图像二值化1.vpp");
                TbLocate[4] = (CogToolBlock)Cognex.VisionPro.CogSerializer.LoadObjectFromFile(GPath + "\\" + GProductName + "\\" + "VPP" + "\\" + "图像二值化2.vpp");
                return InitilizationError.INIT_OK;
            }
            catch (Exception)
            {
                return InitilizationError.INIT_COGNEX_COLTROL_ERROR;
            }
        }

        /// <summary>
        /// 初始化系统中路径
        /// </summary>
        public InitilizationError Init_Path()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(string.Format(@"{0}..\..\..\..\", Application.StartupPath));
                string Path = di.FullName;
                structPath.INI_Path = Path + "Underwater_Tracking_Instrument\\TcIntelligentTechnologySetting\\Setting.dat";
                structPath.Setting_Json_Path = Path + "Underwater_Tracking_Instrument\\TcIntelligentTechnologySetting\\Info.Json";

                GPath = Path + "Underwater_Tracking_Instrument\\TcIntelligentTechnologyType";
                structPath.Vision_Json_Path = Path + "Underwater_Tracking_Instrument\\TcIntelligentTechnologyType\\";//Info.Json";
                structPath.Vision_Public_Json_Path = Path + "Underwater_Tracking_Instrument\\TcIntelligentTechnologyType\\";//Info_To.Json";
                return InitilizationError.INIT_OK;
            }
            catch (Exception ex)
            {
                string mes = ex.Message + ex.StackTrace;
                return InitilizationError.INIT_PATH_ERROR;
            }
        }

        /// <summary>
        /// 初始化系统参数
        /// </summary>
        public InitilizationError Init_SystemPara()
        {
            try
            {
                //系统参数
                int Index = 0, i = 0;
                int Length = 1;
                List<object> Value_Json = new List<object>();
                if (File.Exists(structPath.Setting_Json_Path) == true)
                    Value_Json = jsonHelper.Read_Json(structPath.Setting_Json_Path);

                //for (i = 0; i < Length; i++)
                //{
                JObject value_1 = new JObject();
                if (Index < Value_Json.Count) value_1 = (JObject)Value_Json[Index];
                GProductName = string.IsNullOrWhiteSpace(Convert.ToString(value_1["ProductName"])) ? "Defult" : Convert.ToString(value_1["ProductName"]);

                structPath.Vision_Json_Path = structPath.Vision_Json_Path + GProductName + "\\Info.Json";//";
                structPath.Vision_Public_Json_Path = structPath.Vision_Public_Json_Path + structProductInfo.ProductName + "\\Info_To.Json";//";

                structProductInfo.CamNumber = string.IsNullOrWhiteSpace(Convert.ToString(value_1["CamNumber"])) ? 1 : Convert.ToInt32(value_1["CamNumber"]);
                structProductInfo.ProVersion = string.IsNullOrWhiteSpace(Convert.ToString(value_1["ProVersion"])) ? "V1.0" : Convert.ToString(value_1["ProVersion"]);
                structProductInfo.SaveOKImages = string.IsNullOrWhiteSpace(Convert.ToString(value_1["SaveOKImages"])) ? false : Convert.ToBoolean(value_1["SaveOKImages"]);
                structProductInfo.SaveNGImages = string.IsNullOrWhiteSpace(Convert.ToString(value_1["SaveNGImages"])) ? false : Convert.ToBoolean(value_1["SaveNGImages"]);
                structProductInfo.SaveImageDays = string.IsNullOrWhiteSpace(Convert.ToString(value_1["SaveNGImageDays"])) ? 30 : Convert.ToInt32(value_1["SaveNGImageDays"]);
                structProductInfo.SaveAccessDay = string.IsNullOrWhiteSpace(Convert.ToString(value_1["SaveAccessDay"])) ? 90 : Convert.ToInt32(value_1["SaveAccessDay"]);
                structProductInfo.TotalRun = string.IsNullOrWhiteSpace(Convert.ToString(value_1["TotalRun"])) ? 0 : Convert.ToInt32(value_1["TotalRun"]);
                structProductInfo.TotalFail = string.IsNullOrWhiteSpace(Convert.ToString(value_1["TotalPass"])) ? 0 : Convert.ToInt32(value_1["TotalPass"]);
                Index++;
                //}

                for (i = 0; i < Length; i++)
                {
                    JObject value_3 = new JObject();
                    if (Index < Value_Json.Count) value_3 = (JObject)Value_Json[Index];
                    structPath.Data_Path = Convert.ToString(value_3["Data_Path"]);
                    structPath.Image_Path = Convert.ToString(value_3["Image_Path"]);
                    Index++;
                }
                for (i = 0; i < Length; i++)
                {
                    JObject value_4 = new JObject();
                    if (Index < Value_Json.Count) value_4 = (JObject)Value_Json[Index];
                    strctCameraInfo.CamID = Convert.ToInt32(value_4["CamID"]);
                    strctCameraInfo.CamSerialNumber = Convert.ToString(value_4["CamSerialNumber"]);
                    strctCameraInfo.GrabExpose = Convert.ToDouble(value_4["GrabExpose"]);
                    Index++;
                }
                //for (i = 0; i < Length; i++)
                //{
                //    JObject value_5 = new JObject();
                //    if (Index < Value_Json.Count) value_5 = (JObject)Value_Json[Index];
                //    SystemPara.gSocket.IP = string.IsNullOrWhiteSpace(Convert.ToString(value_5["IP"])) ? "127.0.0.1" : Convert.ToString(value_5["IP"]);
                //    SystemPara.gSocket.Port = string.IsNullOrWhiteSpace(Convert.ToString(value_5["Port"])) ? 4000 : Convert.ToInt32(value_5["Port"]);
                //    Index++;
                //}
                //for (i = 0; i < Length; i++)
                //{
                //    JObject value_6 = new JObject();
                //    Struct_SQL iMonitor = new Struct_SQL();
                //    if (Index < Value_Json.Count) value_6 = (JObject)Value_Json[Index];
                //    SystemPara.gSQLInfo.SQL_Type = string.IsNullOrWhiteSpace(Convert.ToString(value_6["SQL_Type"])) ? "Access" : Convert.ToString(value_6["SQL_Type"]);
                //    SystemPara.gSQLInfo.IP = string.IsNullOrWhiteSpace(Convert.ToString(value_6["IP"])) ? "127.0.0.1" : Convert.ToString(value_6["IP"]);
                //    SystemPara.gSQLInfo.Port = string.IsNullOrWhiteSpace(Convert.ToString(value_6["Port"])) ? "4000" : Convert.ToString(value_6["Port"]);
                //    SystemPara.gSQLInfo.UserID = string.IsNullOrWhiteSpace(Convert.ToString(value_6["UserID"])) ? "admin" : Convert.ToString(value_6["UserID"]);
                //    SystemPara.gSQLInfo.PassWord = string.IsNullOrWhiteSpace(Convert.ToString(value_6["PassWord"])) ? "admin" : Convert.ToString(value_6["PassWord"]);
                //    SystemPara.gSQLInfo.DataBaseName = string.IsNullOrWhiteSpace(Convert.ToString(value_6["DataBaseName"])) ? "DataBase" : Convert.ToString(value_6["DataBaseName"]);
                //    SystemPara.gSQLInfo.TableName = string.IsNullOrWhiteSpace(Convert.ToString(value_6["TableName"])) ? "Table" : Convert.ToString(value_6["TableName"]);

                //    Index++;
                //}

                //读取视觉参数
                List<Struct_VisionInfo> visionInfos = new List<Struct_VisionInfo>();
                gVisionTool_Info.Clear();
                gVisionTool_Info.Add(0, visionInfos);
                Value_Json.Clear();
                int Last = 0;
                if (File.Exists(structPath.Vision_Json_Path) == true)
                    Value_Json = jsonHelper.Read_Json(structPath.Vision_Json_Path);

                if (Value_Json != null)
                {
                    for (i = 0; i < Value_Json.Count; i++)
                    {
                        Struct_VisionInfo visionInfo = new Struct_VisionInfo();
                        JObject value = new JObject();
                        if (i < Value_Json.Count) value = (JObject)Value_Json[i];

                        if (Convert.ToInt32(value["ID"]) - Last == 1) visionInfos = new List<Struct_VisionInfo>();
                        visionInfo.ID = Convert.ToInt32(value["ID"]);
                        visionInfo.Name = Convert.ToString(value["Name"]);
                        visionInfo.Text = Convert.ToString(value["Text"]);
                        Matrix matrix = new Matrix();
                        if (value["RT"].ToString().Length > 0)
                        {
                            JObject value_RT = (JObject)value["RT"];
                            matrix.Matrix00 = Convert.ToDouble(value["RT"]["Matrix00"]); matrix.Matrix01 = Convert.ToDouble(value["RT"]["Matrix01"]);
                            matrix.Matrix02 = Convert.ToDouble(value["RT"]["Matrix02"]); matrix.Matrix10 = Convert.ToDouble(value["RT"]["Matrix10"]);
                            matrix.Matrix11 = Convert.ToDouble(value["RT"]["Matrix11"]); matrix.Matrix12 = Convert.ToDouble(value["RT"]["Matrix12"]);
                            matrix.Matrix20 = Convert.ToDouble(value["RT"]["Matrix20"]); matrix.Matrix21 = Convert.ToDouble(value["RT"]["Matrix21"]);
                            matrix.Matrix22 = Convert.ToDouble(value["RT"]["Matrix22"]); matrix.Matrix30 = Convert.ToDouble(value["RT"]["Matrix30"]);
                            matrix.Matrix31 = Convert.ToDouble(value["RT"]["Matrix31"]); matrix.Matrix32 = Convert.ToDouble(value["RT"]["Matrix32"]);
                            matrix.TranslationX = Convert.ToDouble(value["RT"]["TranslationX"]); matrix.TranslationY = Convert.ToDouble(value["RT"]["TranslationY"]);
                            matrix.TranslationZ = Convert.ToDouble(value["RT"]["TranslationZ"]); matrix.TranslationU = Convert.ToDouble(value["RT"]["TranslationU"]);
                        }
                        visionInfo.RT = matrix;

                        Region region = new Region();
                        if (value["SelectRegion"].ToString().Length > 0)
                        {
                            JObject value_Region = (JObject)value["SelectRegion"];
                            region.CenterX = Convert.ToDouble(value["SelectRegion"]["CenterX"]);
                            region.CenterY = Convert.ToDouble(value["SelectRegion"]["CenterY"]);
                            region.CenterZ = Convert.ToDouble(value["SelectRegion"]["CenterX"]);
                            region.X = Convert.ToDouble(value["SelectRegion"]["X"]);
                            region.Y = Convert.ToDouble(value["SelectRegion"]["Y"]);
                            region.Z = Convert.ToDouble(value["SelectRegion"]["Z"]);
                        }
                        visionInfo.SelectRegion = region;

                        List<string> Input = new List<string>();
                        JArray jarry = JArray.Parse(value["Input"].ToString());

                        if (jarry.Count > 0)
                        {
                            for (int W = 0; W < jarry.Count; W++)
                                Input.Add(jarry[W].ToString());
                        }
                        visionInfo.Input = Input;

                        List<string> Output = new List<string>();
                        JArray jarry1 = JArray.Parse(value["Output"].ToString());

                        if (jarry1.Count > 0)
                        {
                            for (int W = 0; W < jarry1.Count; W++)
                                Output.Add(jarry1[W].ToString());
                        }
                        visionInfo.Output = Output;

                        visionInfos.Add(visionInfo);
                        if (gVisionTool_Info.ContainsKey(visionInfo.ID)) gVisionTool_Info.Remove(visionInfo.ID);
                        gVisionTool_Info.Add(visionInfo.ID, visionInfos);

                        Last = visionInfo.ID;
                    }
                }
                return InitilizationError.INIT_OK;
            }
            catch (Exception)
            {
                return InitilizationError.INIT_SYSTEM_PARA_ERRPR;
            }
        }

        /// <summary>
        /// 视觉程序运行
        /// </summary>
        /// <param name="index">图像处理与标定处理选择</param>
        /// <param name="function">index为1的情况下，根据功能号进行对应处理 0---手动测试，控件显示所有点位结果  1---自动流程计算偏移值</param>
        public bool VppRun(int index, int function)
        {
            try
            {
                if (index == 1)
                {
                    Result = "";
                    for (int i = 1; i <= 2; i++)
                    {
                        if (GOriginalImage[i] == null)
                        {
                            AddInformation("图像传输失败!请检查相机是否连接正常。");
                            return false;
                        }
                    }
                    for (int j = 1; j <= 2; j++)
                    {
                        TbCheckBoard[j].Inputs["InputImage"].Value = GOriginalImage[j];
                        TbCheckBoard[j].Run();
                        TbLocate[j].Inputs["OutputImage"].Value = TbCheckBoard[j].Outputs["OutputImage"].Value as CogImage8Grey; //CogTool.GOriginalImage[index] as CogImage8Grey;
                        TbLocate[j].Run();
                    }

                    for (int k = 1; k <= 2; k++)
                    {
                        if (TbLocate[k].RunStatus.Result == CogToolResultConstants.Error)
                        {
                            CogRecordDisplay[1].Image = GOriginalImage[1];
                            CogRecordDisplay[2].Image = GOriginalImage[2];
                            //WriteCsv("0,0");
                            AddInformation("图像处理失败!请检查算法。");
                            return false;
                        }
                        else
                        {
                            CogRecordDisplay[k].Record = TbLocate[k].CreateLastRunRecord().SubRecords[0];
                            CogRecordDisplay[k].AutoFit = true;
                        }
                    }

                    /////接收该点位
                    double centerx1 = Convert.ToDouble(TbLocate[1].Outputs["CenterX"].Value);
                    double centery1 = Convert.ToDouble(TbLocate[1].Outputs["CenterY"].Value);
                    double centerx2 = Convert.ToDouble(TbLocate[2].Outputs["CenterX"].Value);
                    double centery2 = Convert.ToDouble(TbLocate[2].Outputs["CenterY"].Value);

                    ////显示找到点位信息
                    AddInformation("相机1像素坐标   X：" + centerx1.ToString());
                    AddInformation("相机1像素坐标   Y：" + centery1.ToString());
                    AddInformation("相机2像素坐标   X：" + centerx2.ToString());
                    AddInformation("相机2像素坐标   Y：" + centery2.ToString());

                    Point2f[] orgPoint1 = new Point2f[1];
                    Point2f[] orgPoint2 = new Point2f[1];
                    Point2f[] rPoint1 = new Point2f[1];
                    Point2f[] rPoint2 = new Point2f[1];
                    Point2f[] rPoint3 = new Point2f[1];
                    Point2f[] rPoint4 = new Point2f[1];
                    Point2f[] rPoint5 = new Point2f[1];
                    Point2f[] rPoint6 = new Point2f[1];
                    TCPoint3d outPoint = new TCPoint3d();

                    TCPoint3d[] srcPoints = new TCPoint3d[3];
                    TCPoint3d normbase = new TCPoint3d();
                    TCPoint3d rotation = new TCPoint3d();

                    orgPoint1[0].x = Convert.ToSingle(centerx1);
                    orgPoint1[0].y = Convert.ToSingle(centery1);
                    orgPoint2[0].x = Convert.ToSingle(centerx2);
                    orgPoint2[0].y = Convert.ToSingle(centery2);

                    rPoint1[0].x = Convert.ToSingle(Convert.ToDouble(TbLocate[1].Outputs["Results_GetCircle_CenterX"].Value));
                    rPoint1[0].y = Convert.ToSingle(Convert.ToDouble(TbLocate[1].Outputs["Results_GetCircle_CenterY"].Value));

                    rPoint2[0].x = Convert.ToSingle(Convert.ToDouble(TbLocate[1].Outputs["Results_GetCircle_CenterX1"].Value));
                    rPoint2[0].y = Convert.ToSingle(Convert.ToDouble(TbLocate[1].Outputs["Results_GetCircle_CenterY1"].Value));

                    rPoint3[0].x = Convert.ToSingle(Convert.ToDouble(TbLocate[1].Outputs["Results_GetCircle_CenterX2"].Value));
                    rPoint3[0].y = Convert.ToSingle(Convert.ToDouble(TbLocate[1].Outputs["Results_GetCircle_CenterY2"].Value));

                    rPoint4[0].x = Convert.ToSingle(Convert.ToDouble(TbLocate[2].Outputs["Results_GetCircle_CenterX"].Value));
                    rPoint4[0].y = Convert.ToSingle(Convert.ToDouble(TbLocate[2].Outputs["Results_GetCircle_CenterY"].Value));

                    rPoint5[0].x = Convert.ToSingle(Convert.ToDouble(TbLocate[2].Outputs["Results_GetCircle_CenterX1"].Value));
                    rPoint5[0].y = Convert.ToSingle(Convert.ToDouble(TbLocate[2].Outputs["Results_GetCircle_CenterY1"].Value));

                    rPoint6[0].x = Convert.ToSingle(Convert.ToDouble(TbLocate[2].Outputs["Results_GetCircle_CenterX2"].Value));
                    rPoint6[0].y = Convert.ToSingle(Convert.ToDouble(TbLocate[2].Outputs["Results_GetCircle_CenterY2"].Value));

                    outPoint = transferPoint(orgPoint1, orgPoint2);

                    AddInformation("现实坐标   X：" + outPoint.x.ToString());
                    AddInformation("现实坐标   Y：" + outPoint.y.ToString());
                    AddInformation("现实坐标   Z：" + outPoint.z.ToString());

                    srcPoints[0] = transferPoint(rPoint1, rPoint4);
                    srcPoints[1] = transferPoint(rPoint2, rPoint5);
                    srcPoints[2] = transferPoint(rPoint3, rPoint6);

                    normbase.x = 0;
                    normbase.y = 1;
                    normbase.z = 0;
                    //////010向量下，rotation.z为偏移角度

                    Get3DInformation_ZY.rotationVectorToEuler(ref srcPoints[0], 3, normbase, ref rotation);

                    AddInformation("角度   X：" + rotation.x.ToString());
                    AddInformation("角度   Y：" + rotation.y.ToString());
                    AddInformation("角度   Z：" + rotation.z.ToString());

                    if (function == 1)
                    {
                        //////function为1时，自动流程，需要对rotation的角度值进行判断，超出合理范围，需要发送角度偏移量给上位机
                        //////rotation的角度值在范围内时，需要发送角度确认命令
                        //////角度确认完成后，需要对XY偏移量进行确认，超出合理范围，需要发送XY偏移量给上位机
                        //////XY偏移量在范围内时，需要发送XY偏移量确认命令
                        if (TbLocate[1].Outputs["Result"].Value.ToString() == "Tech" && TbLocate[2].Outputs["Result"].Value.ToString() == "Tech")
                        {
                            if ((rotation.z - StandardR) <= ToleranceR)
                            {
                                if ((outPoint.x - StandardX) <= ToleranceX & (outPoint.y - StandardY) <= ToleranceY)
                                {
                                    Result = "Confirm";
                                }
                                else
                                {
                                    double dataX, dataY;
                                    if ((outPoint.x - StandardX) > 327.67)
                                    {
                                        dataX = 32767;
                                    }
                                    else if ((outPoint.x - StandardX) < -327.68)
                                    {
                                        dataX = -32768;
                                    }
                                    else
                                    {
                                        dataX = Convert.ToDouble((outPoint.x - StandardX).ToString("0.00")) * 100;
                                    }

                                    if ((outPoint.y - StandardY) > 327.67)
                                    {
                                        dataY = 32767;
                                    }
                                    else if ((outPoint.y - StandardY) < -327.68)
                                    {
                                        dataY = -32768;
                                    }
                                    else
                                    {
                                        dataY = Convert.ToDouble((outPoint.y - StandardY).ToString("0.00")) * 100;
                                    }

                                    Result = TransformCode(Convert.ToInt16(dataX)) + TransformCode(Convert.ToInt16(dataY)) + "0000000000000000";
                                }
                            }
                            else
                            {
                                double data;
                                if ((rotation.z - StandardR) > 327.67)
                                {
                                    data = 32767;
                                }
                                else if ((rotation.z - StandardR) < -327.68)
                                {
                                    data = -32768;
                                }
                                else
                                {
                                    data = Convert.ToDouble((rotation.z - StandardR).ToString("0.00")) * 100;
                                }
                                Result = "00000000" + TransformCode(Convert.ToInt16(data)) + "000000000000";
                            }
                        }
                        else
                        {
                            Result = "";
                        }

                    }
                    else
                    {
                        //////function为0时，为手动测试角度和偏移值都需要计算
                        //Method.gMotion.AddInformation("相机1像素坐标   X：" + centerx1.ToString());
                        //Method.gMotion.AddInformation("相机1像素坐标   Y：" + centery1.ToString());
                        //Method.gMotion.AddInformation("相机2像素坐标   X：" + centerx2.ToString());
                        //Method.gMotion.AddInformation("相机2像素坐标   Y：" + centery2.ToString());

                        //Method.gMotion.AddInformation("现实坐标   X：" + outPoint.x.ToString());
                        //Method.gMotion.AddInformation("现实坐标   Y：" + outPoint.y.ToString());
                        //Method.gMotion.AddInformation("现实坐标   Z：" + outPoint.z.ToString());

                        //Method.gMotion.AddInformation("角度   X：" + rotation.x.ToString());
                        //Method.gMotion.AddInformation("角度   Y：" + rotation.y.ToString());
                        //Method.gMotion.AddInformation("角度   Z：" + rotation.z.ToString());
                    }

                }
                else
                {
                    for (int m = 1; m <= 2; m++)
                    {
                        if (GOriginalImage[m] == null)
                        {
                            AddInformation("请检查输入图像");
                            return false;
                        }
                    }

                    for (int n = 3; n <= 4; n++)
                    {
                        TbLocate[n].Inputs["InputImage"].Value = GOriginalImage[n - 2] as CogImage8Grey;
                        TbLocate[n].Run();
                    }

                    for (int o = 3; o <= 4; o++)
                    {
                        if (TbLocate[o].RunStatus.Result == CogToolResultConstants.Error)
                        {
                            CogRecordDisplay[1].Image = GOriginalImage[1];
                            CogRecordDisplay[2].Image = GOriginalImage[2];
                            //WriteCsv("0,0");
                            AddInformation("图像处理失败!");
                            return false;
                        }
                    }
                    for (int p = 3; p <= 4; p++)
                    {
                        CogRecordDisplay[p - 2].Record = TbLocate[p].CreateLastRunRecord().SubRecords[2];
                        CogRecordDisplay[p - 2].AutoFit = true;
                        SaveImage(AppDomain.CurrentDomain.BaseDirectory, (CogImage8Grey)TbLocate[p].Outputs["OutputImage"].Value, p.ToString());
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 保存图像
        /// </summary>
        /// <param name="IMG"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool SaveImage(string path, ICogImage IMG, string code)
        {
            try
            {
                if (!System.IO.Directory.Exists(path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + code))
                {
                    System.IO.Directory.CreateDirectory(path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + code);
                }

                if (IMG != null)
                {
                    CogImageFileTool tool = new CogImageFileTool();
                    tool.InputImage = IMG;
                    tool.Operator.Open(path + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + code + "\\" + DateTime.Now.ToString("HH-mm-ss-fff") + ".bmp", CogImageFileModeConstants.Write);
                    tool.Run();
                    tool.Dispose();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 计算出当前位置的像素坐标，准备标定
        /// </summary>
        /// <param name="index">当前位置的序号</param>
        public bool pickPoint(int index)
        {
            try
            {
                GrabImage(1);
                Thread.Sleep(500);
                GrabImage(2);

                for (int i = 1; i <= 2; i++)
                {
                    if (GOriginalImage[i] == null)
                    {
                        AddInformation("请检查输入图像");
                        return false;
                    }
                }
                for (int j = 1; j <= 2; j++)
                {
                    TbCheckBoard[j].Inputs["InputImage"].Value = GOriginalImage[j];
                    TbCheckBoard[j].Run();
                    TbLocate[j].Inputs["InputImage"].Value = TbCheckBoard[j].Outputs["OutputImage"].Value as CogImage8Grey;//CogTool.GOriginalImage[index] as CogImage8Grey;
                    TbLocate[j].Run();
                }

                for (int k = 1; k <= 2; k++)
                {
                    if (TbLocate[k].RunStatus.Result != CogToolResultConstants.Error)
                    {
                        CogRecordDisplay[1].Image = GOriginalImage[1];
                        CogRecordDisplay[2].Image = GOriginalImage[2];
                        //WriteCsv("0,0");
                        AddInformation("图像处理失败!");
                        return false;
                    }
                }

                for (int m = 1; m <= 2; m++)
                {
                    CogRecordDisplay[m].Record = TbLocate[m].CreateLastRunRecord().SubRecords[0];
                    CogRecordDisplay[m].AutoFit = true;
                    fx[index - 1] = Convert.ToSingle(Convert.ToDouble(TbLocate[m].Outputs["CenterX"].Value));
                    fy[index - 1] = Convert.ToSingle(Convert.ToDouble(TbLocate[m].Outputs["CenterY"].Value));
                }

                AddInformation("相机1像素坐标   X：" + fx[index - 1].ToString());
                AddInformation("相机1像素坐标   Y：" + fy[index - 1].ToString());
                AddInformation("相机2像素坐标   X：" + fx[index + 2].ToString());
                AddInformation("相机2像素坐标   Y：" + fy[index + 2].ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// 相机取像
        /// </summary>
        /// <param name="index">根据数值选择取像相机</param>
        //取像
        public bool GrabImage(int index)
        {
            try
            {
                // CogTool.GOriginalImage[index] = null;
                CamLocate[index].Run();
                GOriginalImage[index] = CamLocate[index].OutputImage as CogImage8Grey;
                switch (index)
                {
                    case 1:
                        CogRecordDisplay[1].Image = GOriginalImage[index];
                        break;

                    case 2:
                        CogRecordDisplay[2].Image = null;
                        CogRecordDisplay[2].Record = null;
                        CogRecordDisplay[2].StaticGraphics.Clear();
                        CogRecordDisplay[2].Image = GOriginalImage[index];
                        CogRecordDisplay[2].Fit();
                        break;
                }
                SaveImage(AppDomain.CurrentDomain.BaseDirectory, GOriginalImage[index], index.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        #endregion
        public UserControl_Display()
        {
            InitializeComponent();

            this.CogRecordDisplay[1] = cogRecordDisplay1;
            this.CogRecordDisplay[2] = cogRecordDisplay2;
            IniRT();
            if (Init_Path() != InitilizationError.INIT_OK) MessageBox.Show("加载视觉系统路径错误");

            if (Init_SystemPara() != InitilizationError.INIT_OK) MessageBox.Show("加载视觉系统参数错误");

            //if (Init_Control() != InitilizationError.INIT_OK) MessageBox.Show("加载界面控件参数错误");

            if (Init_CogVisionTool() != InitilizationError.INIT_OK) MessageBox.Show("加载视觉系统工具失败");
        }

        /// <summary>
        /// 显示软件运行信息
        /// </summary>
        /// <param name="value"></param>
        public void AddInformation(string value)
        {
            this.Invoke((EventHandler)delegate
            {
                //必须在窗口主线程中调用
                if (label_information.Items.Count > 200) label_information.Items.Clear();
                //Class_Form.gFrmCogVision.label_information.Items.Insert(0, value);
                label_information.Items.Add(value);
                label_information.TopIndex = label_information.Items.Count - 1;
            });
        }

        /// <summary>
        /// 打开图片
        /// </summary>
        private void LoadPic()
        {
            try
            {
                if (comboBox1.SelectedItem != null)
                {
                    string strFileName = null;

                    openFileDialog1.FileName = null;
                    openFileDialog1.Filter = "Image Files(*.*)|*.*";
                    openFileDialog1.ShowDialog();
                    strFileName = openFileDialog1.FileName;
                    if (strFileName != "")
                    {
                        GSaveImage.Operator.Open(strFileName, Cognex.VisionPro.ImageFile.CogImageFileModeConstants.Read);
                        GSaveImage.Run();
                        GOriginalImage[Convert.ToInt32(comboBox1.SelectedItem)] = GSaveImage.OutputImage as CogImage8Grey;
                        switch (comboBox1.SelectedItem)
                        {
                            case "1":
                                cogRecordDisplay1.Image = GOriginalImage[Convert.ToInt32(comboBox1.SelectedItem)];
                                cogRecordDisplay1.StaticGraphics.Clear();
                                cogRecordDisplay1.Fit();
                                break;

                            case "2":
                                cogRecordDisplay2.Image = GOriginalImage[Convert.ToInt32(comboBox1.SelectedItem)];
                                cogRecordDisplay2.StaticGraphics.Clear();
                                cogRecordDisplay2.Fit();
                                break;

                            default:
                                MessageBox.Show(@"请先选择正确的编号");
                                break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"请先选择编号");
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(@"打开图片出错：" + exception.ToString());
            }
        }
        private void but_LoadPic_Click(object sender, EventArgs e)
        {
            switch (((Control)sender).Name.Replace("but_", ""))
            {
                //主页
                case "LoadPic":
                    LoadPic();
                    break;

                case "GrabImage":
                    GrabImage(1);
                    Thread.Sleep(200);
                    GrabImage(2);
                    break;

                case "RunImage":
                    if ((string)comboBox1.SelectedItem == "标定取像")  ////此功能是将获取到的图像进行二值化，方便进行标定
                    {
                        VppRun(2, 0);
                    }
                    else
                    {
                        VppRun(1, 0);
                    }
                    break;

                case "cordConfirm":
                    //ConfirmPoint();
                    break;
                case "p1":
                    pickPoint(1);
                    break;

                case "p2":
                    pickPoint(2);
                    break;

                case "p3":
                    pickPoint(3);
                    break;

                case "calib":
                    P3dd[0].x = Convert.ToDouble(textBox8.Text);
                    P3dd[0].y = Convert.ToDouble(textBox9.Text);
                    P3dd[0].z = Convert.ToDouble(textBox14.Text);
                    P3dd[1].x = Convert.ToDouble(textBox10.Text);
                    P3dd[1].y = Convert.ToDouble(textBox11.Text);
                    P3dd[1].z = Convert.ToDouble(textBox15.Text);
                    P3dd[2].x = Convert.ToDouble(textBox12.Text);
                    P3dd[2].y = Convert.ToDouble(textBox13.Text);
                    P3dd[2].z = Convert.ToDouble(textBox16.Text);
                    calibCheck();
                    break;
            }
        }

        private void but_RunImage_Click(object sender, EventArgs e)
        {
            switch (((Control)sender).Name.Replace("but_", ""))
            {
                //主页
                case "LoadPic":
                    LoadPic();
                    break;

                case "GrabImage":
                    GrabImage(1);
                    Thread.Sleep(200);
                    GrabImage(2);
                    break;

                case "RunImage":
                    if ((string)comboBox1.SelectedItem == "标定取像")  ////此功能是将获取到的图像进行二值化，方便进行标定
                    {
                        VppRun(2, 0);
                    }
                    else
                    {
                        VppRun(1, 0);
                    }
                    break;

                case "cordConfirm":
                    //ConfirmPoint();
                    break;
                case "p1":
                    pickPoint(1);
                    break;

                case "p2":
                    pickPoint(2);
                    break;

                case "p3":
                    pickPoint(3);
                    break;

                case "calib":
                    P3dd[0].x = Convert.ToDouble(textBox8.Text);
                    P3dd[0].y = Convert.ToDouble(textBox9.Text);
                    P3dd[0].z = Convert.ToDouble(textBox14.Text);
                    P3dd[1].x = Convert.ToDouble(textBox10.Text);
                    P3dd[1].y = Convert.ToDouble(textBox11.Text);
                    P3dd[1].z = Convert.ToDouble(textBox15.Text);
                    P3dd[2].x = Convert.ToDouble(textBox12.Text);
                    P3dd[2].y = Convert.ToDouble(textBox13.Text);
                    P3dd[2].z = Convert.ToDouble(textBox16.Text);
                    calibCheck();
                    break;
            }
        }
    }
}