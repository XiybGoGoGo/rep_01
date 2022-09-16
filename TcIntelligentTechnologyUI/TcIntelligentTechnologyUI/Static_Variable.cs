using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using TcIntelligentTechnologyBLL;
using TcIntelligentTechnologyMODEL;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class Static_Variable
    {
        //GPath需要先加载
        public static string strGPath = System.AppDomain.CurrentDomain.BaseDirectory;

        public static string configPath_Calibration = strGPath + ConfigurationManager.ConnectionStrings["CalibrationConfigPath"].ConnectionString;
        public static string configPath_User = strGPath + ConfigurationManager.ConnectionStrings["UserConfigPath"].ConnectionString;
        public static string configPath_ADLINK_Json = strGPath + ConfigurationManager.ConnectionStrings["AdlinkJsonConfigPath"].ConnectionString;
        public static string configPath_Camera = strGPath + ConfigurationManager.ConnectionStrings["CameraConfigPath"].ConnectionString;
        public static string configPath_ADLINK = Environment.CurrentDirectory + ConfigurationManager.ConnectionStrings["AdlinkConfigPath"].ConnectionString;
        public static string configPath_Region = Environment.CurrentDirectory + ConfigurationManager.ConnectionStrings["RegionConfigPath"].ConnectionString;
        public static string configPath_Global = Static_Variable.strGPath + ConfigurationManager.ConnectionStrings["GlobalConfigPath"].ConnectionString;
        /// <summary>
        /// //BLL方法类
        /// </summary>
        public static JsonHelper cJsonmethodbll = new JsonHelper();
        /// <summary>
        /// //轴卡类
        /// </summary>
        //public static Calibration_Model cCalModel = new Calibration_Model();
        /// <summary>
        /// //轴卡类
        /// </summary>
        public static Adlink_Model cAdlinkModel = new Adlink_Model();
        public static readonly Adlink_204CHelper cCard0 = new Adlink_204CHelper();
        /// <summary>
        /// //康耐视类
        /// </summary>
        public static Cognex_Helper cCognex = new Cognex_Helper();
        /// <summary>
        /// //用户类
        /// </summary>
        public static USERMODEL cUserModel = new USERMODEL();
        /// <summary>
        /// //XML读取类
        /// </summary>
        public static XmlHelper cXmlhelper = new XmlHelper();

        public static TxtHelper cTxtheloer = new TxtHelper();
        /// <summary>
        /// //海康类
        /// </summary>
        public static Hikrobot_Helper cHikrobotHelper = new Hikrobot_Helper();
        /// <summary>
        /// //相机类
        /// </summary>
        public static List<CAMERAMODEL> listCameramodel;

        /// <summary>
        /// 相机索引
        /// </summary>
        public static int nCameraSelecteIndex = 0;
    }
}
