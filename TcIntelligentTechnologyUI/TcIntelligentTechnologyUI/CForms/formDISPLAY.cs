using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;
using TcIntelligentTechnologyMODEL;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formDISPLAY : Form
    {
        

        #region FUNC
        public formDISPLAY()
        {
            InitializeComponent();
        }

        

        #endregion

        #region Event
        private void buttonRun_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            

        }

        private void buttonSub_Click(object sender, EventArgs e)
        {
            

        }

        private void buttonSetRegion_Click(object sender, EventArgs e)
        {
            

        }

        private void m_formDisplay_Load(object sender, EventArgs e)
        {
           

        }

        private void BtnTrigger_Click(object sender, EventArgs e)
        {

        }
        private void BTN_SAVE_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "请选择要保存的路径";
                saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
                saveFileDialog.Filter = "Image Files (*.bmp)|*.bmp";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string curPath = saveFileDialog.FileName;
                    gImageFileTool.InputImage = gOriginalImage;
                    gImageFileTool.Operator.Open(curPath, Cognex.VisionPro.ImageFile.CogImageFileModeConstants.Write);
                    gImageFileTool.Run();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存图片错误-01");
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void BTN_SETREGION_CALIBRATION_Click(object sender, EventArgs e)
        {
            

        }

        private void BTN_IMAGE_IN_Click(object sender, EventArgs e)
        {
            

        }

        private void BTN_NORMAL_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void CKB_Grabbing_CheckedChanged(object sender, EventArgs e)
        {
            

        }

        private void CKB_Software_CheckedChanged(object sender, EventArgs e)
        {
            

        }

        private void RB_ContinueMode_CheckedChanged(object sender, EventArgs e)
        {
            

        }

        private void RB_TriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void CKB_Grabbing_Trigger_CheckedChanged(object sender, EventArgs e)
        {
            

        }

        private void BTN_SaveExposure_Click(object sender, EventArgs e)
        {
            

        }

        private void BTN_SaveMaxMin_Click(object sender, EventArgs e)
        {
            

        }

        private void btnChange1_Click(object sender, EventArgs e)
        {
            try
            {
                if (ChangeCameraIPAddress(0, tbIP_1.Text, tbMask_1.Text, tbDefaultWay_1.Text) == HIKROBOTCamErrorCode.MOK)
                {
                    MessageBox.Show("CAM1相机修改成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("相机IP更改错误-01");
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        private void BTN_IMAGE_OUT_Click(object sender, EventArgs e)
        {
            
        }
        #endregion

    }
}
