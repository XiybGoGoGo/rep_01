using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;
using TcIntelligentTechnologyMODEL;
using TcIntelligentTechnologyCOMMON;
using System.IO;
using Calib3DExtend;
using Cognex.VisionPro;
using System.Configuration;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formParameter : Form
    {
        #region Public | 公有
        public Action cameraCalibrationEvent;
        public Calibration_Model calibModel = new Calibration_Model(); 
        #endregion

        #region Private | 私有

        #endregion

        #region FUNC | 函数
        public formParameter()
        {
            InitializeComponent();
        }

        

        

        public void ChangeControlsEnabled(int _level)
        {
            try
            {
                foreach (var group in this.Controls)
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
            }
            catch (Exception)
            {
                MessageBox.Show("切换权限中断");
            }
        }

 
        #endregion

        #region Event | 事件
        private void m_formVisionControl_Load(object sender, EventArgs e)
        {
            Initialize();
            //cameraCalibrationEvent += 
            ChangeControlsEnabled(Static_Variable.cUserModel.Level);
        }

        private void buttonJsonWrite_Click(object sender, EventArgs e)
        {
            changeCalModel();
            List<ClassModel> model = new List<ClassModel>();
            model.Add(calibModel);

            if (Static_Variable.cJsonmethodbll.SetJsonString(model, Static_Variable.configPath_Calibration))
            {
                MessageBox.Show("保存成功");

            }
            else
            {
                MessageBox.Show("保存失败");
            }
        }

        private void buttonCameraCalibrate_Click(object sender, EventArgs e)
        {
            cameraCalibrationEvent();
        }

        private void buttonMultipleLaser_Click(object sender, EventArgs e)
        {
            //CalculatePointLinePlaneCommon.multipleLaserCommon(Static_Variable.cCalModel);
        }

        private void BTNINITIALIZE_Click(object sender, EventArgs e)
        {
            Initialize();
        }
        #endregion

        //POINTCLOUD_HELPER pcHelper = new POINTCLOUD_HELPER
        //{
        //    _pointSizeX = Convert.ToSingle(0.0225),
        //    _pointSizeY = Convert.ToSingle(0.0225)
        //};
    }
}
