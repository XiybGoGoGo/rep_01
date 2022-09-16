using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using TcIntelligentTechnologyBLL;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.ImageFile;
using TcIntelligentTechnologyMODEL;
using System.Net;

namespace InternalStructure
{
    public partial class formDisplay : UserControl
    {
        public formDisplay()
        {
            InitializeComponent();
        }

        #region Public

        #endregion

        #region Private
        int number_Photo;
        XmlHelper xmlhelper = new XmlHelper();
        Hikrobot_Helper hikHelper = new Hikrobot_Helper();
        CogCircle gCircle;
        CogRectangle gRegion_Laser = new CogRectangle();
        /// <summary>
        /// 标定时所用搜索区域
        /// </summary>
        Double ix = new Double();
        Double iy = new Double();
        Double iwidth = new Double();
        Double iheight = new Double();
        /// <summary>
        /// 图形处理时所用搜索区域
        /// </summary>      
        CogRectangleAffine gRegion_Affine = new CogRectangleAffine();
        List<double[]> gRectangleAffinePoint = new List<double[]>();
        CogRectangle gRegion_Normal = new CogRectangle();
        /// <summary>
        /// 状态信息
        /// </summary>
        bool isAlive = false;
        /// <summary>
        /// COGNEX工具
        /// </summary>
        CogToolBlock gToolBlock_Cal = new CogToolBlock();
        CogImageFileTool gImageFileTool = new CogImageFileTool();
        CogImage8Grey gOriginalImage = new CogImage8Grey();

        public Action<string> AddInformation;
        public Action<string> WriteErrorMes;
        #endregion

        private bool CameraInfoShow()
        {
            try
            {
                grbCamera1.Text = "Camera1: " + hikHelper.netExports[0];
                tbIP_1.Text = hikHelper.currentIps[0];
                tbMask_1.Text = hikHelper.currentSubNetMasks[0];
                tbDefaultWay_1.Text = hikHelper.currentDefultGateWays[0];
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }

        private bool CheckChangedForRB()
        {
            try
            {
                if (RB_TriggerMode.Checked)
                {
                    CKB_Grabbing_Continue.Enabled = false;
                    CKB_Grabbing_Trigger.Enabled = true;
                    CKB_Software.Enabled = true;
                }

                if (RB_ContinueMode.Checked)
                {
                    CKB_Grabbing_Trigger.Enabled = false;
                    CKB_Software.Enabled = false;
                    CKB_Grabbing_Continue.Enabled = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }
        }

        public bool CALIBRATION_INITIALIZE()
        {
            try
            {
                string strToolBlock = Variable.gPath + "\\Vpp\\Block.vpp";
                gToolBlock_Cal = CogSerializer.LoadObjectFromFile(strToolBlock) as CogToolBlock;
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }

        private bool textBox1_Show()
        {
            try
            {
                textBox1.Text = number_Photo.ToString();
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }

        private void ImportImage()
        {
            try
            {
                cogDisplay1.InteractiveGraphics.Clear();

                // 打开指定路径的图片,将其中的激光点存储
                string fileName = "Calibration\\linep\\" + number_Photo + ".bmp";
                gImageFileTool.Operator.Open(fileName, CogImageFileModeConstants.Read);
                gImageFileTool.Run();
                //传输图像到视觉工具
                gToolBlock_Cal.Inputs["image"].Value = gImageFileTool.OutputImage;
                //界面显示
                cogDisplay1.StaticGraphics.Clear();
                cogDisplay1.InteractiveGraphics.Clear();
                cogDisplay1.Image = gImageFileTool.OutputImage;
                cogDisplay1.DrawingEnabled = true;
                cogDisplay1.Fit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开激光图片错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        public bool ChangeControlsEnabledAll(int _level)
        {
            try
            {
                ChangeControlsEnabled(_level, this.groupBox1);
                ChangeControlsEnabled(_level, this.tableLayoutPanel_Camera);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool ChangeControlsEnabled(int _level, Control _control)
        {
            try
            {
                foreach (var group in _control.Controls)
                {
                    if (group is GroupBox)
                    {
                        GroupBox tap = (GroupBox)group;

                        if (Convert.ToInt32(tap.Tag) <= _level)
                        {
                            //foreach (var grb in tap.Controls)
                            //{
                            //    if (grb is GroupBox)
                            //    {
                            tap.Enabled = true;
                            //}

                        }
                        else
                        {
                            //foreach (var grb in tap.Controls)
                            //{
                            //    if (grb is GroupBox)
                            //    {
                            tap.Enabled = false;
                            //}

                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }
        }

        private HIKROBOTCamErrorCode ChangeCameraIPAddress(int _index, string _ip, string _mask, string _way)
        {
            try
            {
                if (hikHelper.deviceNumber == 0)
                {
                    MessageBox.Show("暂未搜寻到相机");
                    return HIKROBOTCamErrorCode.INIT_NO_CAMERA_FOUND;
                }

                // ch:IP转换 | en:IP conversion
                IPAddress clsIpAddr;
                if (false == IPAddress.TryParse(_ip, out clsIpAddr))
                {
                    MessageBox.Show("IP地址错误");
                    return HIKROBOTCamErrorCode.MNG;
                }
                long nIp = IPAddress.NetworkToHostOrder(clsIpAddr.Address);

                // ch:掩码转换 | en:Mask conversion
                IPAddress clsSubMask;
                if (false == IPAddress.TryParse(_mask, out clsSubMask))
                {
                    MessageBox.Show("子网掩码错误");
                    return HIKROBOTCamErrorCode.MNG;
                }
                long nSubMask = IPAddress.NetworkToHostOrder(clsSubMask.Address);

                // ch:网关转换 | en:Gateway conversion
                IPAddress clsDefaultWay;
                if (false == IPAddress.TryParse(_way, out clsDefaultWay))
                {
                    MessageBox.Show("网关转换错误");
                    return HIKROBOTCamErrorCode.MNG;
                }
                long nDefaultWay = IPAddress.NetworkToHostOrder(clsDefaultWay.Address);

                hikHelper.HandleCreate();

                hikHelper.ChangeGigeIp(_index, nIp, nSubMask, nDefaultWay);


                GC.Collect();

                return HIKROBOTCamErrorCode.MOK;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return HIKROBOTCamErrorCode.MNG;
            }

        }

        private void ckbCameraEnabled1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (ckbCameraEnabled1.Checked)
                {
                    if (!mResetCheckBoxStatus()) MessageBox.Show("复位勾选框失败-00");
                    ckbCameraEnabled2.Checked = false;
                    Static_Variable.nCameraSelecteIndex = 0;
                    Static_Variable.cAdlinkModel.M_selectAxis = 0;
                    if (Static_Variable.cHikrobotHelper.deviceNumber > 0)
                    {
                        TBExposure.Text = Static_Variable.listCameramodel[Static_Variable.nCameraSelecteIndex].CameraExposure.ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("启用1号设备错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void ckbCameraEnabled2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (ckbCameraEnabled2.Checked)
                {
                    if (!mResetCheckBoxStatus()) MessageBox.Show("复位勾选框失败-00");
                    ckbCameraEnabled1.Checked = false;
                    Static_Variable.nCameraSelecteIndex = 0;
                    Static_Variable.cAdlinkModel.M_selectAxis = 1;
                    if (Static_Variable.cHikrobotHelper.deviceNumber > 0)
                    {
                        TBExposure.Text = Static_Variable.listCameramodel[Static_Variable.nCameraSelecteIndex].CameraExposure.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("启用2号设备错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private bool mResetCheckBoxStatus()
        {
            try
            {
                RB_TriggerMode.Checked = true;
                CKB_Software.Checked = false;
                CKB_Grabbing_Trigger.Checked = false;
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }

        public bool mStartOrStopToChangeCheckBox(bool _status)
        {
            try
            {
                if (_status)
                {
                    RB_TriggerMode.Checked = true;
                    CKB_Grabbing_Trigger.Checked = true;
                    CKB_Software.Checked = false;
                }
                else
                {
                    CKB_Grabbing_Trigger.Checked = false;
                    CKB_Software.Checked = false;
                }
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }
        }

        public bool mFormLoadCheckBox()
        {
            try
            {
                ckbCameraEnabled1.Checked = true;
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return false;
            }

        }

        private void buttonSetRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonSetRegion.Text.ToString() == "设置区域_激光")
                {
                    gRegion_Laser.GraphicDOFEnable = CogRectangleDOFConstants.All;
                    gRegion_Laser.Interactive = true;
                    buttonSetRegion.Text = "保存";
                    gRegion_Laser.SelectedColor = CogColorConstants.Blue;
                    cogDisplay1.InteractiveGraphics.Add(gRegion_Laser, "Searchregion", false);
                }
                else
                {
                    buttonSetRegion.Text = "设置区域_激光";
                    ix = gRegion_Laser.X;
                    iy = gRegion_Laser.Y;
                    iwidth = gRegion_Laser.Width;
                    iheight = gRegion_Laser.Height;
                    File.WriteAllText(Static_Variable.configPath_Region, ix.ToString() + "," + iy.ToString() + "," + iwidth.ToString() + "," + iheight.ToString(), Encoding.Default);
                    gToolBlock_Cal.Inputs["SearchRegion"].Value = gRegion_Laser;
                    cogDisplay1.InteractiveGraphics.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置激光区域失败-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void buttonImportImage_Click(object sender, EventArgs e)
        {
            try
            {
                ImportImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开图片失败-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            try
            {
                gToolBlock_Cal.Inputs["SearchRegion"].Value = gRegion_Laser;
                gToolBlock_Cal.Run();
                string s = gToolBlock_Cal.Outputs["res"].Value.ToString();
                if (s.Contains(";"))
                {
                    string[] str = s.Split('-');
                    string[] s1 = str[0].Split(';');
                    string[] s3 = str[1].Split(';');

                    for (int i = 0; i < s1.Length - 1; i++)
                    {
                        gCircle = new CogCircle();
                        gCircle.Radius = 1.5;
                        string[] s2 = s1[i].Split(' ');
                        gCircle.CenterX = Convert.ToDouble(s2[0]);
                        gCircle.CenterY = Convert.ToDouble(s2[1]);
                        gCircle.Color = CogColorConstants.Green;
                        cogDisplay1.InteractiveGraphics.Add(gCircle, "Point" + i, false);
                    }

                    string path = "Calibration\\txt\\" + number_Photo + ".txt";
                    if (!File.Exists(path))
                    {
                        File.Create(path);
                    }

                    File.WriteAllText(path, str[0].Replace(";", "\r\n"), Encoding.Default);
                    textBox1_Show();
                }
                cogDisplay1.DrawingEnabled = true;
                cogDisplay1.Fit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("生成txt文件错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                ++number_Photo;
                textBox1_Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("图片名称修改错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void buttonSub_Click(object sender, EventArgs e)
        {
            try
            {
                if (number_Photo > 0)
                {
                    --number_Photo;
                    if (!textBox1_Show())
                    {
                        MessageBox.Show("修改图片名称失败-00");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改图片名称错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void BTN_SETREGION_CALIBRATION_Click(object sender, EventArgs e)
        {
            try
            {
                if (BTN_SETREGION_CALIBRATION.Text.ToString() == "设置区域_棋盘格")
                {
                    gRegion_Affine.GraphicDOFEnable = CogRectangleAffineDOFConstants.All;
                    gRegion_Affine.Interactive = true;
                    BTN_SETREGION_CALIBRATION.Text = "保存";
                    gRegion_Affine.SelectedColor = CogColorConstants.Green;
                    cogDisplay1.InteractiveGraphics.Add(gRegion_Affine, "SearchregionAffine", false);
                }
                else
                {
                    //BTN_SETREGION_CALIBRATION.Text = "设置区域_棋盘格";
                    //gRectangleAffinePoint.Add(new double[] { gRegion_Affine.CornerOriginX, gRegion_Affine.CornerOriginY });


                    //iy = gRegion_Affine.Y;
                    //iwidth = gRegion_Affine.Width;
                    //iheight = gRegion_Affine.Height;
                    //File.WriteAllText(pathRegion, ix.ToString() + "," + iy.ToString() + "," + iwidth.ToString() + "," + iheight.ToString(), Encoding.Default);
                    //gToolBlock_Cal.Inputs["SearchRegion"].Value = gRegion_Affine;
                    //cogDisplay1.InteractiveGraphics.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置棋盘格区域错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void BTN_NORMAL_Click(object sender, EventArgs e)
        {
            try
            {
                if (BTN_NORMAL.Text.ToString() == "设置飞拍检测区域")
                {
                    gRegion_Normal.GraphicDOFEnable = CogRectangleDOFConstants.All;
                    gRegion_Normal.Interactive = true;
                    BTN_NORMAL.Text = "保存";
                    gRegion_Normal.SelectedColor = CogColorConstants.Blue;
                    cogDisplay1.InteractiveGraphics.Add(gRegion_Normal, "Searchregion", false);
                }
                else
                {
                    BTN_NORMAL.Text = "设置飞拍检测区域";

                    model.widthLowerLimit = Convert.ToInt32(gRegion_Normal.Y);
                    Static_Variable.cCalModel.widthHigherLimit = Convert.ToInt32(gRegion_Normal.Y + gRegion_Normal.Height);
                    Static_Variable.cCalModel.heightLowerLimit = Convert.ToInt32(gRegion_Normal.X);
                    Static_Variable.cCalModel.heightHigherLimit = Convert.ToInt32(gRegion_Normal.X + gRegion_Normal.Width);
                    cogDisplay1.InteractiveGraphics.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置飞拍检测区域错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (button2.Text.ToString() == "设置不感兴趣区域")
                {
                    gRegion_Normal.GraphicDOFEnable = CogRectangleDOFConstants.All;
                    gRegion_Normal.Interactive = true;
                    button2.Text = "保存";
                    gRegion_Normal.SelectedColor = CogColorConstants.Blue;
                    cogDisplay1.InteractiveGraphics.Add(gRegion_Normal, "Searchregion", false);
                }
                else
                {
                    button2.Text = "设置不感兴趣检测区域";

                    Static_Variable.cCalModel.X1 = Convert.ToInt32(gRegion_Normal.X);
                    Static_Variable.cCalModel.Y1 = Convert.ToInt32(gRegion_Normal.Y);
                    Static_Variable.cCalModel.X2 = Convert.ToInt32(gRegion_Normal.X + gRegion_Normal.Width);
                    Static_Variable.cCalModel.Y2 = Convert.ToInt32(gRegion_Normal.Y);
                    cogDisplay1.InteractiveGraphics.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置不感兴趣区域错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void RB_ContinueMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (Static_Variable.cHikrobotHelper.deviceNumber == 0)
                {
                    return;
                }
                if (RB_ContinueMode.Checked)
                {
                    Static_Variable.cHikrobotHelper.SetTriggerSourceAsContinue(Static_Variable.nCameraSelecteIndex);
                    CKB_Grabbing_Continue.Checked = false;
                    CKB_Grabbing_Continue.Enabled = true;
                }
                else
                {
                    CKB_Grabbing_Continue.Checked = false;
                    CKB_Grabbing_Continue.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("切换取像模式错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void CKB_Grabbing_Continue_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (Static_Variable.cHikrobotHelper.deviceNumber == 0)
                {
                    return;
                }
                if (CKB_Grabbing_Continue.Checked)
                {
                    Static_Variable.cHikrobotHelper.IsNeedToStopGrabs[Static_Variable.nCameraSelecteIndex] = true;
                    // ch:标志位置位true | en:Set position bit true
                    Static_Variable.cHikrobotHelper.IsGrabbings[Static_Variable.nCameraSelecteIndex] = true;//

                    //m_hReceiveThread = new Thread(Static_Variable._hikrobotHelper.ReceiveThreadProcess(0));//
                    //m_hReceiveThread.Start();
                    //开始前清除流长度
                    Static_Variable.cHikrobotHelper.SetFrameLen(Static_Variable.nCameraSelecteIndex);
                    // ch:开始采集 | en:Start Grabbing
                    int nRet = Static_Variable.cHikrobotHelper.StartGrabbing(Static_Variable.nCameraSelecteIndex);
                    if (nRet != 0)
                    {
                        Static_Variable.cHikrobotHelper.IsGrabbings[Static_Variable.nCameraSelecteIndex] = false;//
                        //m_hReceiveThread.Join();
                        MessageBox.Show("Start Grabbing Fail!");
                        return;
                    }
                }
                else
                {
                    Static_Variable.cHikrobotHelper.IsNeedToStopGrabs[Static_Variable.nCameraSelecteIndex] = false;
                    // ch:标志位设为false | en:Set flag bit false
                    Static_Variable.cHikrobotHelper.IsGrabbings[Static_Variable.nCameraSelecteIndex] = false;//
                    //m_hReceiveThread.Join();

                    // ch:停止采集 | en:Stop Grabbing
                    int nRet = Static_Variable.cHikrobotHelper.StopGrabbing(Static_Variable.nCameraSelecteIndex);
                    if (nRet != 0)
                    {
                        MessageBox.Show("Stop Grabbing Fail!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("开启采集错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void CKB_Grabbing_Trigger_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (Static_Variable.cHikrobotHelper.deviceNumber == 0)
                {
                    return;
                }
                if (CKB_Grabbing_Trigger.Checked)
                {
                    Static_Variable.cHikrobotHelper.IsNeedToStopGrabs[Static_Variable.nCameraSelecteIndex] = true;
                    Static_Variable.cHikrobotHelper.StartGrabbing(Static_Variable.nCameraSelecteIndex);
                }
                else
                {
                    Static_Variable.cHikrobotHelper.IsNeedToStopGrabs[Static_Variable.nCameraSelecteIndex] = false;
                    Static_Variable.cHikrobotHelper.StopGrabbing(Static_Variable.nCameraSelecteIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("切换取像模式错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void RB_TriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (Static_Variable.cHikrobotHelper.deviceNumber == 0)
                {
                    return;
                }
                if (RB_TriggerMode.Checked)
                {
                    Static_Variable.cHikrobotHelper.SetTriggerSourceAsTrigger(Static_Variable.nCameraSelecteIndex);
                    CKB_Grabbing_Trigger.Checked = false;
                    CKB_Grabbing_Trigger.Enabled = true;
                    CKB_Software.Checked = false;
                    CKB_Software.Enabled = true;
                }
                else
                {
                    CKB_Grabbing_Trigger.Checked = false;
                    CKB_Grabbing_Trigger.Enabled = false;
                    CKB_Software.Checked = false;
                    CKB_Software.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("切换取像模式错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void CKB_Software_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (Static_Variable.cHikrobotHelper.deviceNumber == 0)
                {
                    return;
                }
                if (CKB_Software.Checked)
                {
                    Static_Variable.cHikrobotHelper.IsGrabbings[Static_Variable.nCameraSelecteIndex] = true;
                    Static_Variable.cHikrobotHelper.SetTriggerSourceAsSoftware(Static_Variable.nCameraSelecteIndex);
                }
                else
                {
                    Static_Variable.cHikrobotHelper.IsGrabbings[Static_Variable.nCameraSelecteIndex] = false;
                    Static_Variable.cHikrobotHelper.SetTriggerSourceAsDevice(Static_Variable.nCameraSelecteIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("切换软硬触发错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void BTN_IMAGE_IN_Click(object sender, EventArgs e)
        {
            try
            {
                cogDisplay1.InteractiveGraphics.Clear();

                // 打开指定路径的图片,将其中的激光点存储
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "请选择要打开的路径";
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog.Filter = "Image Files (*.bmp)|*.bmp";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string curPath = openFileDialog.FileName;
                    gImageFileTool.Operator.Open(curPath, CogImageFileModeConstants.Read);
                    gImageFileTool.Run();
                    //传输图像到视觉工具
                    //gToolBlock_Cal.Inputs["image"].Value = gImageFileTool.OutputImage;
                    //界面显示
                    cogDisplay1.StaticGraphics.Clear();
                    cogDisplay1.InteractiveGraphics.Clear();
                    cogDisplay1.Image = gImageFileTool.OutputImage;
                    cogDisplay1.DrawingEnabled = true;
                    cogDisplay1.Fit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入图片错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void BTN_IMAGE_OUT_Click(object sender, EventArgs e)
        {
            try
            {
                // 打开指定路径的图片,将其中的激光点存储
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "请选择要保存的路径";
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                saveFileDialog.Filter = "Image Files (*.bmp)|*.bmp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string curPath = saveFileDialog.FileName;
                    gImageFileTool.InputImage = cogDisplay1.Image;
                    gImageFileTool.Operator.Open(curPath, CogImageFileModeConstants.Write);
                    gImageFileTool.Run();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存图片错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void BTN_TRIGGER_Click(object sender, EventArgs e)
        {
            try
            {
                Static_Variable.cHikrobotHelper.GetOneImage(Static_Variable.nCameraSelecteIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("取像错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void BTN_SaveExposure_Click(object sender, EventArgs e)
        {
            try
            {
                Static_Variable.listCameramodel[Static_Variable.nCameraSelecteIndex].CameraSerialNumber = Static_Variable.cHikrobotHelper.SerialNumbers[Static_Variable.nCameraSelecteIndex];
                Static_Variable.listCameramodel[Static_Variable.nCameraSelecteIndex].CameraExposure = Convert.ToInt64(TBExposure.Text);
                Static_Variable.cHikrobotHelper.Exposures[Static_Variable.nCameraSelecteIndex] = Convert.ToInt64(TBExposure.Text);
                Static_Variable.cHikrobotHelper.SetExprosure(Static_Variable.nCameraSelecteIndex);
                xmlhelper.GetCameraDataFromXml(Static_Variable.listCameramodel[Static_Variable.nCameraSelecteIndex], Static_Variable.configPath_Camera, Static_Variable.nCameraSelecteIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存曝光错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void BTN_SaveMaxMin_Click(object sender, EventArgs e)
        {
            try
            {
                Static_Variable.cCalModel.compare_Max = NUD_Max.Value;
                Static_Variable.cCalModel.compare_Min = NUD_Min.Value;
                List<ClassModel> model = new List<ClassModel>();
                model.Add(Static_Variable.cCalModel);

                if (Static_Variable.cJsonmethodbll.SetJsonString(model, Static_Variable.configPath_Calibration))
                {
                    MessageBox.Show("保存成功");

                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存最大最小区间错误-01");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }

        private void formDisplay_Load(object sender, EventArgs e)
        {
            try
            {
                if (!CALIBRATION_INITIALIZE())
                {
                    MessageBox.Show("加载视觉工具失败-00");
                }
                TBExposure.Text = Static_Variable.cHikrobotHelper.Exposures[0].ToString();
                string[] txtStr = File.ReadAllText(Static_Variable.configPath_Region, Encoding.Default).Split(',');
                ix = Convert.ToDouble(txtStr[0]);
                iy = Convert.ToDouble(txtStr[1]);
                iwidth = Convert.ToDouble(txtStr[2]);
                iheight = Convert.ToDouble(txtStr[3]);
                gRegion_Laser.X = ix;
                gRegion_Laser.Y = iy;
                gRegion_Laser.Width = iwidth;
                gRegion_Laser.Height = iheight;
                gToolBlock_Cal.Inputs["SearchRegion"].Value = gRegion_Laser;
                textBox1_Show();
                if (!ChangeControlsEnabledAll(Static_Variable.cUserModel.Level))
                {
                    MessageBox.Show("更改权限失败-00");
                }
                Static_Variable.cHikrobotHelper.IsGrabbings[0] = false;
                Static_Variable.cHikrobotHelper.IntrHandle = cogDisplay1.Handle;


                CheckChangedForRB();
                NUD_Max.Value = Static_Variable.cCalModel.compare_Max;
                NUD_Min.Value = Static_Variable.cCalModel.compare_Min;
                if (!CameraInfoShow())
                {
                    MessageBox.Show("相机信息展示失败-00");
                }
                //m_hReceiveThread.IsBackground = true;
                //m_hReceiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("DisplayLoad加载失败");
                WriteErrorMes(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
        }
    }
}
