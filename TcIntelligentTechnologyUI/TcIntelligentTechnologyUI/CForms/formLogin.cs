using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formLogin : Form
    {
        #region Public

        #endregion

        #region Private

        #endregion

        #region FUNC
        public formLogin()
        {
            InitializeComponent();
        }

        public bool ChangeConrolsEnabled()
        {
            try
            {
                if (ClassFormCollection.gFrmAxisAdlink == null) ClassFormCollection.gFrmAxisAdlink = new formAXIS();
                if (ClassFormCollection.gFrmDisplay == null) ClassFormCollection.gFrmDisplay = new formDISPLAY();
                if (ClassFormCollection.gFrmParameter == null) ClassFormCollection.gFrmParameter = new formParameter();
                ClassFormCollection.gFrmParameter.ChangeControlsEnabled(Static_Variable.cUserModel.Level);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        #endregion

        #region Event
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string _userName = CbUser1.SelectedItem.ToString();
                string _userPwd = TbPwd1.Text;

                if (new Global_Method().GetUserLevel(_userName, _userPwd))
                {
                    ChangeConrolsEnabled();
                }
                else
                {
                    MessageBox.Show("账号密码错误");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("登录失败");
            }
            finally
            {
                CbUser1.SelectedIndex = 0;
                TbPwd1.Text = "";
            }

        }

        private void m_formLogin_Load(object sender, EventArgs e)
        {
            CbUser1.SelectedIndex = 0;
            CbUser2.SelectedIndex = 0;
        }

        private void BtnChange_Click(object sender, EventArgs e)
        {
            string _userName = CbUser2.SelectedItem.ToString();
            string _userPwdOld = TbPwd2.Text;
            string _uerPwdNew = TbNew2.Text;
            if (new Global_Method().ChangeUserPwd(_userName, _userPwdOld, _uerPwdNew))
            {
                MessageBox.Show("修改成功");

            }
            else
            {
                MessageBox.Show("修改失败");
            }
            TbPwd2.Text = "";
            TbNew2.Text = "";
        }

        private void BtnCancel1_Click(object sender, EventArgs e)
        {
            Static_Variable.cUserModel.UserName = "Operator1";
            Static_Variable.cUserModel.Level = 1;
            TbPwd1.Text = "";
            ChangeConrolsEnabled();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            TbPwd2.Text = "";
            TbNew2.Text = "";
        }
        #endregion 
    }
}
