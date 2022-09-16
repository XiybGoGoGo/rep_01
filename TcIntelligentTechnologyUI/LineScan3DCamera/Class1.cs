using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LineScan3DCamera
{
    public class Class1
    {
        [DllImport("LineScan3DCamera.dll")]
        public extern static void InitCalibrationPara(int imageNum, char[] pathBase, char[] chessImagePath, char[] laserImagePath);

        [DllImport("LineScan3DCamera.dll")]
        public extern static void cameraCalibration(SizeXY cornerSize, int sideLength_chessSquare, SizeXY imageSize, ref int emptyImage, ref int invalidImage, ref double errorResult);

        [DllImport("LineScan3DCamera.dll")]
        public extern static void laserCalibration(int whiteThreshold, ForScope height, ForScope width, int mode, byte[] color, ref int emptyImage_out);

        [DllImport("LineScan3DCamera.dll")]
        public extern static void generatePointCloud(TCPoint2d[] pts2d, int pts2dSize, ref TCPoint3d pts3d, int frame, int step, int methodNumber, ref double normOriginal_out, ref double normLine_out);

        /// <summary>
        /// 初始化参数,在使用其他方法时请一定要使用此方法
        /// [in] imageNum 传入图像的数量(应保持棋盘格图片和激光图片的数量一致)  
        /// [in] * pathBase 将在这个路径下存储所有的标定文件
        /// 在同一级目录下, 创建文件夹laserBeCircled将把激光点标记图放置在该路径下
        /// [in] *chessImagePath 输入棋盘格图像的路径(.bmp的上级目录)
        /// [in] * laserImagePath 输入激光图像的路径(.bmp的上级目录)
        /// </summary>
        /// <param name="model"></param>
        public static void InitCalibrationParaM(CalibrationModel model)
        {
            InitCalibrationPara(model.imageNum, model.pathBase, model.chessImagePath, model.laserImagePath);
        }

        /// <summary>
        /// 相机内外参标定 
        /// [in] cornersSize 传入内部角点的实际数量(长, 宽)
        ///      struct SizeXY { double x; double y; };
        /// [in] sideLength_chessSquare 传入棋盘格的实际边长(单位: 毫米)
        /// [in] imageSize 传入图像的实际尺寸(单位: 像素)
        ///      struct SizeXY { double x; double y; };
        /// [out] * emptyImage_out 将地址为空的图片下标保存在该参数中
        ///      因为图片下标是从0开始的, 为了跟0做出区分, 如果没有图片出错的话,将会返回 -1  
        /// [out] * invalidImage_out 将非法的图片下标保存在该参数中
        /// [out] *errorResult_out 将计算出来的误差结果保存在该参数中(该参数的规模大小应为图片数量imageNum+1)
        /// </summary>
        /// <param name="model"></param>
        public static void CameraCalibrationM(CalibrationModel model)
        {
            cameraCalibration(model.cornerSize, model.sideLength_chessSquare, model.imageSize, ref model.emptyImage, ref model.invalidImage, ref model.errorResult);
        }

        /// <summary>
        /// 激光光平面标定 
        /// [in] whiteThreshold 使用阈值分割图像时,输入一个阈值使得[whiteThreshold, 255]之间的灰度被视为白色  
        /// [in] height 寻找点的时候传入在高度上的范围∈[0, 1]
        ///  struct ForScope { double low; double high; };
        /// [in] width 寻找点的时候传入在宽度上的范围∈[0, 1]
        ///  struct ForScope { double low; double high; };
        /// [in] mode 当激光方向为竖直时, 传入1, 当激光方向为水平时, 传入2
        /// [in] *color color的规模应当为3, 每个分量分别代表R G B
        /// [out] *emptyImage_out 将地址为空的图片下标保存在该参数中
        /// 因为图片下标是从0开始的,为了跟0做出区分,如果没有图片出错的话,将会返回 -1  
        /// </summary>
        /// <param name="model"></param>
        public static void LaserCalibration(CalibrationModel model)
        {
            laserCalibration(model.whiteThreshold, model.height, model.width, model.mode, model.color, ref model.emptyImage_out);
        }

        /// <summary>
        /// 生成点云文件 
        /// [in] *pts2d 传入一组2d点  
        /// [in] pts2dSize 传入pts2d这组点的数量
        /// [out] *pts3d 将合成的3d点保存在该参数中(该参数的规模大小应与传入的2d点数量一致)
        /// [in] frame 传入抓拍图像的序号数(即这是第几张抓拍图像)
        /// [in] step 抓拍图像时每次移动的距离(单位:毫米)
        /// [in] methodNumber 该参数用来说明使用哪一种算法来生成点云, 传入1时保存的3d点为初始点云数据, 默认传入1
        /// [out] *normOriginal_out 将激光光平面的法向量保存在该参数中
        /// [out] *normLine_out 将计算出来的激光方向向量保存在该参数中
        /// </summary>
        /// <param name="model"></param>
        public static void GeneratePointCloud(PointCloud model)
        {
            generatePointCloud(model.pts2d, model.pts2dSize, ref model.pts3d, model.frame, model.step, model.methodNumber, ref model.normOriginal_out, ref model.normLine_out);
        }
    }
}
