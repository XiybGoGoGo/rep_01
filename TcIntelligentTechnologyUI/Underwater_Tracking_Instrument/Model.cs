using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision_Control;

namespace UnderwaterTrackingInstrument
{
    //视觉工具信息
    public class Struct_VisionInfo
    {
        public int ID = 0;
        public string Name;
        public string Text;
        public Matrix RT;
        public Region SelectRegion;
        public List<string> Input = new List<string>();
        public List<string> Output = new List<string>();
        public string OutputResult;
    }
    /// <summary>
    /// 系统路径结构体
    /// </summary>
    public class StructPath
    {
        /// <summary>
        /// INI保存数据路径, 存储一些公共的参数
        /// </summary>
        public string INI_Path;

        /// <summary>
        /// 系统参数，保存Json类型数据路径
        /// </summary>
        public string Setting_Json_Path;

        /// <summary>
        /// 不同产品类型视觉配置文件的总的目录，CogTool工具路径 
        /// </summary>
        //public string CogTool_Path;

        /// <summary>
        /// 不同产品内容Json类型数据路径
        /// </summary>
        public string ProductInfo_Json_Path;

        /// <summary>
        /// 保存数据路径
        /// </summary>
        public string Data_Path;

        /// <summary>
        /// 图像保存数据路径
        /// </summary>
        public string Image_Path;

        /// <summary>
        /// Access保存数据路径
        /// </summary>
        public string Access_Path;

        /// <summary>
        /// Json视觉数据
        /// </summary>
        public string Vision_Json_Path;

        /// <summary>
        /// Json视觉共有部分
        /// </summary>
        public string Vision_Public_Json_Path;
    }

    /// <summary>
    /// 系统信息结构体
    /// </summary>
    public class StructProductInfo
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName;

        /// <summary>
        /// 相机数量
        /// </summary>
        public int CamNumber;

        /// <summary>
        /// 新建产品名称
        /// </summary>
        public string NewProductName;

        /// <summary>
        ///软件系统版本
        /// </summary>
        public string ProVersion;

        /// <summary>
        /// 是否保存OK图片
        /// </summary>
        public bool SaveOKImages;//是否保存OK图片

        /// <summary>
        /// 是否保存NG图片
        /// </summary>
        public bool SaveNGImages;//是否保存NG图片

        /// <summary>
        /// 保存图片天数
        /// </summary>
        public int SaveImageDays;//保存图片的时间限制

        /// <summary>
        /// 保存数据天数
        /// </summary>
        public int SaveAccessDay;//保存Access时间

        /// <summary>
        /// 计数--总数
        /// </summary>
        public int TotalRun;

        /// <summary>
        /// 计数--NG数
        /// </summary>
        public int TotalFail;
    }

    /// <summary>
    /// 相机参数结构体
    /// </summary>
    public class Struct_CameraInfo
    {
        /// <summary>
        /// 相机型号
        /// </summary>
        public string CamType;

        /// <summary>
        /// 相机ID
        /// </summary>
        public int CamID;

        /// <summary>
        /// 相机曝光
        /// </summary>
        public double GrabExpose;

        /// <summary>
        /// 相机SN号
        /// </summary>
        public string CamSerialNumber;

        /// <summary>
        /// 相机拍照次数
        /// </summary>
        public int TriggerNum;
    }

    public struct TCPoint3d
    {
        public double x;
        public double y;
        public double z;
    }

    public struct Point3f
    {
        public float x;
        public float y;
        public float z;
    }

    public struct Point2f
    {
        public float x;
        public float y;
    }

    public struct TCRTMatrix4
    {
        public double a00, a01, a02, a03, a10, a11, a12, a13, a20, a21, a22, a23, a30, a31, a32, a33;
    }

    //区域
    public class Region
    {
        public double CenterX = 0;
        public double CenterY = 0;
        public double CenterZ = 0;
        public double X = 0;
        public double Y = 0;
        public double Z = 0;
    }
}
