using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineScan3DCamera
{
    public struct SizeXY
    {
        public double x;
        public double y;
        public SizeXY(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    };

    public struct ForScope
    {
        public double low;
        public double high;
        public ForScope(double low, double high)
        {
            this.low = low;
            this.high = high;
        }
    };

    public struct TCPoint2d
    {
        public double x;
        public double y;
        public TCPoint2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct TCPoint3d
    {
        public double x;
        public double y;
        public double z;
        public TCPoint3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class CalibrationModel
    {
        //Calibration  Init
        [Category("Calibration")]
        [Description("Initialize")]
        public int imageNum;
        public char[] pathBase;
        public char[] chessImagePath;
        public char[] laserImagePath;
        //Calibration  Camera
        public SizeXY cornerSize;
        public int sideLength_chessSquare;
        public SizeXY imageSize;
        public int emptyImage;
        public int invalidImage;
        public double errorResult;
        //Calibration Laser
        public int whiteThreshold;
        public ForScope height;
        public ForScope width;
        public int mode;
        public byte[] color;
        public int emptyImage_out;
    }

    public class PointCloud
    {
        public TCPoint2d[] pts2d;
        public int pts2dSize;
        public TCPoint3d pts3d;
        public int frame;
        public int step;
        public int methodNumber;
        public double normOriginal_out;
        public double normLine_out;
    }
}
