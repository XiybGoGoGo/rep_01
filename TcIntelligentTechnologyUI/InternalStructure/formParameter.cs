using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcIntelligentTechnologyMODEL;

namespace InternalStructure
{
    public partial class formParameter : UserControl
    {
        public formParameter()
        {
            InitializeComponent();
        }

        private void LoadCalibrationModel()
        {
            Variable.calibModel = (Calibration_Model)Variable.jsonHelper.JsonFileToObject(Variable.configPath, Variable.calibModel);
            propertyGrid1.SelectedObject = Variable.calibModel;
        }
    }
}
