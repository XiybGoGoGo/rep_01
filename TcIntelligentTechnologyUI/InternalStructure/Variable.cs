using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcIntelligentTechnologyBLL;
using TcIntelligentTechnologyMODEL;

namespace InternalStructure
{
    public class Variable
    {
        public static string configPath = @"\INFO\CALIBRATION.Json";

        public static string gPath;

        public static JsonHelper jsonHelper = new JsonHelper();

        public static Calibration_Model calibModel = new Calibration_Model();
    }
}
