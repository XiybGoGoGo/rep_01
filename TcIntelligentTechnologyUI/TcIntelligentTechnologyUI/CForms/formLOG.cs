using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyBLL;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class formLOG : Form
    {
        #region Public

        #endregion

        #region Private
        ExcelHelper excelHelper = new ExcelHelper();
        #endregion

        #region FUNC
        public formLOG()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void BTN_Search_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;// DataTable dt = new DataTable();
            string strDate = dateTimePicker1.Value.Year.ToString() + "-" + dateTimePicker1.Value.Month.ToString("00") + "-" + dateTimePicker1.Value.Day.ToString("00");
            //string FileTime = dateTimePicker1.Value.Year.ToString() + "-" + dateTimePicker1.Value.Month.ToString();
            string _directoryPath = Static_Variable.strGPath + @"\Data";
            string _strSql = "select * from ProductInfo";
            if (!File.Exists(_directoryPath + @"\" + strDate + ".accdb"))
            {
                MessageBox.Show("不存在此日期的数据");
                return;
            }
            dataGridView1.DataSource = ExcelHelper.DbSelect(_strSql, _directoryPath + @"\" + strDate + ".accdb");
        }
        #endregion

    }
}
