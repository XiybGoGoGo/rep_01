using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;

namespace TcIntelligentTechnologyUI.CForms
{
    partial class ClassFormCollection
    {
        /// <summary>
        /// 系统Winform
        /// </summary>
        public static formMAIN gFrmMain;
        public static formHOME gFrmHome;
        public static formAXIS gFrmAxisAdlink;
        public static formParameter gFrmParameter;
        public static formDISPLAY gFrmDisplay;
        public static formLogin gFrmLogin;
        public static formLOG gFrmLog;
        public static formCamera gFrmCamera;

        /// <summary>
        /// 显示当前Form
        /// </summary>
        /// <param name="iControl">显示Form的控件</param>
        /// <param name="sender">显示Form</param>
        /// <param name="IsClearControl">是否清除当前显示控件</param>
        public static void ShowForm(Control iControl, Form sender, bool IsClearControl)
        {
            try
            {
                if (IsClearControl == true)
                {
                    iControl.Controls.Clear();
                }
                sender.TopLevel = false;
                sender.Show();
                sender.Dock = DockStyle.Fill;
                iControl.Controls.Add(sender);
                sender.Show();
            }
            catch (Exception ex)
            {
                string str = ex.Message + ex.StackTrace;
            }
        }

        /// <summary>
        /// 关闭所有窗体
        /// </summary>
        public void ClosedAllForm(Form form)
        {
            try
            {
                //if (Class_Form.gFrmSetting != null && Class_Form.gFrmSetting != form) { ClosedForm(Class_Form.gFrmSetting); Class_Form.gFrmSetting = null; }
            }
            catch (Exception ex)
            {
                string str = ex.Message + ex.StackTrace;
            }
        }
    }
}
