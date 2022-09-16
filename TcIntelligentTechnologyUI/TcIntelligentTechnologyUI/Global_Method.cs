using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;
using TcIntelligentTechnologyMODEL;

namespace TcIntelligentTechnologyUI.CForms
{
    public class Global_Method
    {



        public bool GetUserLevel(string _username, string _userpwd)
        {
            try
            {
                Static_Variable.cUserModel = Static_Variable.cXmlhelper.ReadUserModel(Static_Variable.configPath_User, _username);

                if (MD5Helper.Md5Decrypt(Static_Variable.cUserModel.PassWord) != _userpwd || Static_Variable.cUserModel.PassWord == "Op")
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception)
            {
                //加载User所需的参数
                Static_Variable.cUserModel.UserName = "Operator1";
                //GLOBAL_VARIABLE._userModel.PassWord = "Op";
                Static_Variable.cUserModel.Level = 1;
                return false;
            }
        }

        /// <summary>
        /// 修改用户密码
        /// </summary>
        /// <param name="_username">用户名</param>
        /// <param name="_userpwdOld">旧密码</param>
        /// <param name="_userpwdNew">新密码</param>
        /// <returns>结果，false：密码错误，true：修改成功</returns>
        public bool ChangeUserPwd(string _username, string _userpwdOld, string _userpwdNew)
        {
            try
            {
                USERMODEL _userModel = Static_Variable.cXmlhelper.ReadUserModel(Static_Variable.configPath_User, _username);
                if (MD5Helper.Md5Decrypt(_userModel.PassWord) != _userpwdOld || _userModel.PassWord == "Op")
                {
                    return false;
                }
                else
                {
                    Static_Variable.cXmlhelper.ChangeUserPwd(Static_Variable.configPath_User, _username, MD5Helper.Md5Encrypt(_userpwdNew));
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 保存错误信息到本地
        /// </summary>
        /// <param name="strMessage">错误信息</param>
        /// <returns></returns>
        public static bool mWriteErrorMesToTxt(string strMessage)
        {
            try
            {
                string strDirectory = Static_Variable.strGPath + "\\ErrorMessage";
                if (!Directory.Exists(strDirectory))
                {
                    Directory.CreateDirectory(strDirectory);
                }
                string strPath = strDirectory + @"\" + "ErrorMessage_" + DateTime.Now.ToString("yyMMdd") + ".txt";
                string strTime = DateTime.Now.ToString("HHmmss");
                return Static_Variable.cTxtheloer.WriteTXT(strPath, strTime + ":" + strMessage);  
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
