using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formLOAD : Form
    {
        #region Public

        #endregion

        #region Private
        private System.Timers.Timer gTimer;
        #endregion

        #region FUNC
        public formLOAD()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// 显示加载进度
        /// </summary>
        /// <param name="str">显示加载内容</param>
        /// <param name="iProcess">显示加载进度条</param>
        private void ShowProcess(string str, int iProcess)
        {
            this.Invoke((EventHandler)delegate
            {
                label1.Text = str;
                progressBar1.Value = iProcess;
            });
        }
        #endregion

        #region Event
        private void m_formLoad_Load(object sender, EventArgs e)
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;
                gTimer = new System.Timers.Timer();
                gTimer.AutoReset = false;
                gTimer.Elapsed += new System.Timers.ElapsedEventHandler(timerCall);
                gTimer.Start();
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load窗体Load事件加载错误-01");
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void timerCall(object sender, System.Timers.ElapsedEventArgs e)//
        {
            try
            {
                ShowProcess("加载视觉系统路径", 10);
                if (ClassFormCollection.gFrmMain == null) ClassFormCollection.gFrmMain = new formMAIN();
                if (ClassFormCollection.gFrmHome == null) ClassFormCollection.gFrmHome = new formHOME();



                ShowProcess("加载视觉系统路径", 30);
                ClassFormCollection.gFrmMain.Initialize();

                ShowProcess("加载视觉系统工具", 40);
                Static_Variable.cHikrobotHelper.DeviceSearch();
                if (Static_Variable.cHikrobotHelper.DeviceOpen() != TcIntelligentTechnologyMODEL.HIKROBOTCamErrorCode.MOK)
                {
                    MessageBox.Show("加载相机失败");
                    Static_Variable.cHikrobotHelper.DeviceClose();
                }

                ShowProcess("加载视觉系统工具", 60);
                if (Static_Variable.cCognex == null) Static_Variable.cCognex = new Cognex_Helper();

                string strToolBlockLoadPath = AppDomain.CurrentDomain.BaseDirectory + "VPP\\" + "Block_GetPoint.vpp";
                Static_Variable.cCognex.mToolBlockLoadFromFile(strToolBlockLoadPath);

                //if (ClassFormCollection.gFrmDisplay == null) ClassFormCollection.gFrmDisplay = new m_formDISPLAY();
                //ClassFormCollection.gFrmDisplay.CALIBRATION_INITIALIZE();

                ShowProcess("加载运动控制卡", 80);
                //if (!ClassFormCollection.gFrmAxisAdlink.InitialCard()) MessageBox.Show("加载运动控制卡失败");
                //////////////if (Static_Variable.cCard0.InitializeCard(Static_Variable.cAdlinkModel.M_selectAxis, 0) == AdlinkControlErrorCode.MOK)
                //////////////{
                //////////////    if (Static_Variable.cCard0.LoadParamFromFile(Environment.CurrentDirectory + ConfigurationManager.ConnectionStrings["AdlinkConfigPath"].ConnectionString) != AdlinkControlErrorCode.MOK)
                //////////////    {
                //////////////        MessageBox.Show("加载运动控制卡配置文件失败");
                //////////////    }
                //////////////}
                //////////////else
                //////////////{
                //////////////    MessageBox.Show("加载运动控制卡失败");
                //////////////}
                //////////////if (Static_Variable.cCard0.SetServoOn() != AdlinkControlErrorCode.MOK)
                //////////////{
                //////////////    MessageBox.Show("开启运动控制卡使能失败");
                //////////////}

                ShowProcess("加载参数", 90);
                //////new Global_Method().INITIALIZE();

                ShowProcess("加载完成", 100);

                //ClassFormCollection.gFrmDisplay.CALIBRATION_INITIALIZE();
                //ShowProcess("加载标定系统工具", 80);

                //ClassFormCollection.gFrmAxisAdvantech.OpenBorad();
                //ShowProcess("加载视觉系统参数", 100);

                //string[] Num = { "00C35631569", "00D86274356" };
                //Class_Form.gFrmMain.Init_Cam(Num);
                //Class_Form.gFrmMain.Init_Cam();
                //ShowProcess("加载视觉系统相机", 60);

                //string IP = "127.0.0.1";string Port = "4000";
                //Class_Form.gFrmMain.Init_Communication (IP,Port);
                //ShowProcess("加载视觉系统相机", 60);

                //ClassFormCollection.gFrmMain.Init_VisionTool();
                //ShowProcess("加载视觉系统工具", 100);
            }
            catch (Exception ex)
            {
                gTimer.Stop();
                gTimer.Enabled = false;
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                this.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value == 100)
            {
                timer1.Stop();
                if (ClassFormCollection.gFrmMain == null) ClassFormCollection.gFrmMain = new formMAIN();
                ClassFormCollection.gFrmMain.Show();
                //progressBar1.Active = false;
                this.Hide();
            }
        }
        #endregion

    }
}
