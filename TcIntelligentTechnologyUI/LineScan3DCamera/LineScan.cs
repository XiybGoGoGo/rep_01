using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LineScan3DCamera
{
    public partial class LineScan : UserControl
    {
        CalibrationModel model = new CalibrationModel();
        public LineScan()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = model;
        }

    }
}
