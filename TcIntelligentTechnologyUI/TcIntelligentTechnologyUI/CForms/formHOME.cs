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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnderwaterTrackingInstrument;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formHOME : Form
    {
        #region Public

        #endregion

        #region Private


        #endregion

        #region FUNC
        public formHOME()
        {
            InitializeComponent();
            //绑定拍照处理事件
            ////////Static_Variable.cHikrobotHelper.ImageCallBackAction += getVisionProPoint;
        }

        #endregion

        #region Event
        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_formHome_Load(object sender, EventArgs e)
        {
            try
            {
                UserControl_Display userControl_Display = new UserControl_Display();
                userControl_Display.Dock = DockStyle.Fill;
                panel1.Controls.Add(userControl_Display);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载Home的Load事件错误-01");
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        #endregion
    }
}
