using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;
using TcIntelligentTechnologyMODEL;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formMAIN : Form
    {
        #region Public
        public uint ProgramStatus = 0;//0:停机状态，1：运行状态，2：复位状态
        #endregion

        #region Private
        JsonHelper jsonmethodbll = new JsonHelper();
        ClassFormCollection csClassFormCollection = new ClassFormCollection();
        bool _bColorShanShuo = false;
        #endregion

        #region FUNC
        public formMAIN()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            //加载全局参数
            Static_Variable.strGPath = jsonmethodbll.GetJsonString("GPath", Static_Variable.configPath_Global);
        }
        #endregion

        #region Event
        private void cbuttonFormHome_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmHome == null) ClassFormCollection.gFrmHome = new formHOME();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmHome, true);
        }

        private void cbuttonFormVisionControl_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmDisplay == null) ClassFormCollection.gFrmDisplay = new formDISPLAY();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmDisplay, true);
        }

        private void cbuttonFormCommunication_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmAxisAdlink == null) ClassFormCollection.gFrmAxisAdlink = new formAXIS();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmAxisAdlink, true);
        }

        private void m_formMain_Load(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmHome == null) ClassFormCollection.gFrmHome = new formHOME();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmHome, true);
            //if (ClassFormCollection.gFrmDisplay == null) ClassFormCollection.gFrmDisplay = new formDISPLAY();


            cbuttonAbort.Enabled = false;
            cbuttonTrigger.Enabled = false;
            timerShowInfo.Start();
            timerShowInfo.Enabled = true;
        }

        private void m_formMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                string str = ex.Message + ex.StackTrace;
                MessageBox.Show(str);
            }
        }

        private void cbuttonExit_Click(object sender, EventArgs e)
        {
            timerShowInfo.Enabled = false;
            timerShowInfo.Stop();
            Static_Variable.cHikrobotHelper.DeviceClose();
            this.Close();
        }

        private void cbuttonFormSetting_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmParameter == null) ClassFormCollection.gFrmParameter = new formParameter();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmParameter, true);
        }

        private void cbuttonFormUser_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmLogin == null) ClassFormCollection.gFrmLogin = new formLogin();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmLogin, true);
        }

        private void cbuttonFormLog_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmLog == null) ClassFormCollection.gFrmLog = new formLOG();
            ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmLog, true);
        }

        private void cbuttonTrigger_Click(object sender, EventArgs e)
        {
            try
            {
                //if (ClassFormCollection.gFrmHome.grabImages(Static_Variable.nCameraSelecteIndex)) MessageBox.Show("点云数据计算错误-00");
                if (ClassFormCollection.gFrmCamera == null) ClassFormCollection.gFrmCamera = new formCamera();
                ClassFormCollection.ShowForm(panel_Home, ClassFormCollection.gFrmCamera, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("点云数据计算错误-01");
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void timerShowInfo_Tick(object sender, EventArgs e)
        {
            try
            {
                labelNowTime.Text = DateTime.Now.ToString();
                labelUserName.Text = Static_Variable.cUserModel.UserName;
                labelProduct.Text = Static_Variable.nCameraSelecteIndex.ToString();
                if (_bColorShanShuo)
                {
                    _bColorShanShuo = false;
                    switch (ProgramStatus)
                    {
                        case 0:

                            cbuttonAbort.BackColor = Color.Red;
                            break;
                        case 1:

                            cbuttonStart.BackColor = Color.Green;
                            break;
                        case 2:
                            cbuttonInitialize.BackColor = Color.Orange;
                            break;
                    }
                }
                else
                {
                    _bColorShanShuo = true;
                    switch (ProgramStatus)
                    {
                        case 0:

                            cbuttonAbort.BackColor = Color.White;
                            break;
                        case 1:

                            cbuttonStart.BackColor = Color.White;
                            break;
                        case 2:
                            cbuttonInitialize.BackColor = Color.White;
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbuttonAbort_Click(object sender, EventArgs e)
        {
            cbuttonAbort.Enabled = false;
            cbuttonStart.Enabled = true;
            cbuttonTrigger.Enabled = false;
            cbuttonInitialize.Enabled = true;
            cbuttonStart.BackColor = Color.White;
            cbuttonInitialize.BackColor = Color.White;
            ProgramStatus = 0;
        }

        private void cbuttonStart_Click(object sender, EventArgs e)
        {
            if (ClassFormCollection.gFrmDisplay == null) ClassFormCollection.gFrmDisplay = new formDISPLAY();
            cbuttonStart.Enabled = false;
            cbuttonAbort.Enabled = true;
            cbuttonTrigger.Enabled = true;
            cbuttonInitialize.Enabled = false;
            cbuttonAbort.BackColor = Color.White;
            cbuttonInitialize.BackColor = Color.White;
            ProgramStatus = 1;

        }

        private void cbuttonInitialize_Click(object sender, EventArgs e)
        {
            try
            {
                ProgramStatus = 2;
                cbuttonStart.Enabled = false;
                cbuttonAbort.Enabled = false;
                cbuttonInitialize.Enabled = false;
                cbuttonStart.BackColor = Color.White;
                cbuttonAbort.BackColor = Color.White;

               // Static_Variable.cCard0.StopMove(Static_Variable.nCameraSelecteIndex); //停止速度运动
               // if (!ClassFormCollection.gFrmDisplay.mStartOrStopToChangeCheckBox(false)) MessageBox.Show("停止采集失败-00");

                //if (ClassFormCollection.gFrmMain != null) ClassFormCollection.gFrmMain.Dispose(); ClassFormCollection.gFrmMain = new m_formMAIN();

                //if (ClassFormCollection.gFrmHome != null) ClassFormCollection.gFrmHome.Dispose(); ClassFormCollection.gFrmHome = new m_formHOME();


                ClassFormCollection.gFrmMain.Initialize();

                //Static_Variable.cHikrobotHelper.DeviceClose();
                //Static_Variable.cHikrobotHelper.DeviceSearch();
                //if (Static_Variable.cHikrobotHelper.DeviceOpen() != TcIntelligentTechnologyMODEL.HIKROBOTCamErrorCode.MOK)
                //{
                //    MessageBox.Show("加载相机失败");
                //    Static_Variable.cHikrobotHelper.DeviceClose();
                //}

                //if (Static_Variable.cCognex == null) Static_Variable.cCognex = new Cognex_Helper();
                //string strToolBlockLoadPath = AppDomain.CurrentDomain.BaseDirectory + "VPP\\" + "Block_GetPoint.vpp";
                //Static_Variable.cCognex.mToolBlockLoadFromFile(strToolBlockLoadPath);

                //Static_Variable.cCard0.CloseCard();
                //if (Static_Variable.cCard0.InitializeCard(Static_Variable.cAdlinkModel.M_selectAxis, 0) == AdlinkControlErrorCode.MOK)
                //{
                //    if (Static_Variable.cCard0.LoadParamFromFile(Environment.CurrentDirectory + ConfigurationManager.ConnectionStrings["AdlinkConfigPath"].ConnectionString) != AdlinkControlErrorCode.MOK)
                //    {
                //        MessageBox.Show("加载运动控制卡配置文件失败");
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("加载运动控制卡失败");
                //}
                //if (Static_Variable.cCard0.SetServoOn() != AdlinkControlErrorCode.MOK)
                //{
                //    MessageBox.Show("开启运动控制卡使能失败");
                //}

                cbuttonStart.Enabled = true;
                cbuttonAbort.Enabled = false;
                cbuttonInitialize.Enabled = true;
                //ClassFormCollection.gFrmMain.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        #endregion

        private void cbuttonFormProduct_Click(object sender, EventArgs e)
        {
            try
            {
                Task task = new Task(() =>
                {
                    
                });
                task.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("计算失败");
            }

        }
    }
}
