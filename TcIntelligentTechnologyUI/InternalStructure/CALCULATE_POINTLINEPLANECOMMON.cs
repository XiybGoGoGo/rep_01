using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TcIntelligentTechnologyMODEL;
using Vision_Control;

namespace InternalStructure
{

    #region Public 
    public partial struct Size_zy
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public struct Point_xy
    {
        public double x;
        public double y;
        public Point_xy(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    #endregion

    #region Private

    public struct Image2dPointsWithIndex
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int index_2d { get; set; }
    }

    #endregion
    public partial class CalculatePointLinePlaneCommon
    {
        #region FUNC
        /// <summary>
        /// 相机内外参标定 
        /// </summary>
        /// <param name="chessImagePath">输入棋盘格图像的路径 </param>
        /// <param name="path_cameraMatrixXML">输入cameraMatrix.xml的路径 </param>
        /// <param name="path_rvecsXML">输入rvecs.xml的路径 </param>
        /// <param name="path_tvecsXML">输入tvecs.xml的路径 </param>
        /// <param name="path_distCoeffsXML">输入distCoeffs.xml的路径 </param>
        /// <param name="image_size">传入图像的实际尺寸(单位:像素) </param>
        /// <param name="corners_size">传入内部角点的实际数量(长*宽) </param>
        /// <param name="chess_square_size">传入棋盘格的实际尺寸(单位:毫米) </param>
        /// <param name="imageNum">传入目录下图片的数量 </param>
        /// <param name="total_error">将计算出来的误差结果保存在该参数中 </param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)] public extern static int cameraCalibration(char[] chessImagePath, char[] path_cameraMatrixXML, char[] path_rvecsXML, char[] path_tvecsXML, char[] path_distCoeffsXML, Size_zy image_size, Size_zy corners_size, Size_zy chess_square_size, int imageNum, ref int invalidImage, ref double total_error);

        /// <summary>
        /// 图片(中只有一根激光线)保存激光点数据到指定路径
        /// </summary>
        /// <param name="laserImagePath">输入激光图像的路径 </param>
        /// <param name="path_cameraMatrixXML">输入cameraMatrix.xml的路径 </param>
        /// <param name="path_rvecsXML">输入rvecs.xml的路径 </param>
        /// <param name="path_tvecsXML">输入tvecs.xml的路径 </param>
        /// <param name="imageNum">输入目录下图片的数量 </param>
        /// <param name="lightPointPath">传入激光点的保存路径(不添加后缀.xml) </param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)] public extern static void getLightPlaneParameter_singleLaser(char[] laserImagePath, char[] path_cameraMatrixXML, char[] path_rvecsXML, char[] path_tvecsXML, int imageNum, char[] lightPointPath);

        /// <summary>
        /// 图片(有多根激光线)保存激光点数据到指定路径 
        /// </summary>
        /// <param name="laserImagePath">将激光图像中找到的点保存在txt中, 传入txt的路径(.txt的上级目录)\ntxt中的2d点坐标格式为x0 y0</param>
        /// <param name="path_cameraMatrixXML">输入cameraMatrix.xml的路径 </param>
        /// <param name="path_rvecsXML">输入rvecs.xml的路径 </param>
        /// <param name="path_tvecsXML">输入tvecs.xml的路径 </param>
        /// <param name="imageNum">输入目录下图片的数量 </param>
        /// <param name="lightOrder">传入检测的是第几根激光(将这个参数作为laserPlaneParameter的后缀作为区分,从0开始)  </param>
        /// <param name="lightPointPath">将生成的激光光平面参数放置在该路径下（到.xml的上级目录） </param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)] public extern static void getLightPlaneParameter_multipleLaser(char[] laserImagePath, char[] path_cameraMatrixXML, char[] path_rvecsXML, char[] path_tvecsXML, int imageNum, int lightOrder, char[] lightPointPath);

        /// <summary>
        /// 生成点云（算法捕捉）
        /// </summary>
        /// <param name="path_laserPlaneParameterXML">输入laserPlaneParameternXML的路径(n表示后缀,与保存时的相对应)  </param>
        /// <param name="path_cameraMatrixXML">输入cameraMatrix.xml的路径</param>
        /// <param name="captureImagePath">输入生成点云所需要的抓拍的图像</param>
        /// <param name="imgNumber">点云图片数量</param>
        /// <param name="heightLowerLimit">需要计算点云的矩形区域的长度最小值</param>
        /// <param name="heightHigherLimit">需要计算点云的矩形区域的长度最大值</param>
        /// <param name="widthLowerLimit">需要计算点云的矩形区域的宽度最小值</param>
        /// <param name="widthHigherLimit">需要计算点云的矩形区域的宽度最大值</param>
        /// <param name="diameter_topSurface">上表面的实际尺寸（mm）</param>
        /// <param name="diameter_bottomSurface">下表面的实际尺寸（mm）</param>
        /// <param name="heightPointCloud">图片的最大宽度</param>
        /// <param name="offset">默认：0，无需修改</param>
        /// <param name="pnt_out">将生成的3d点放置在该参数中</param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)] public extern static void generateCylindricalPointCloud(char[] path_laserPlaneParameterXML, char[] path_cameraMatrixXML, char[] captureImagePath, int imgNumber, int heightLowerLimit, int heightHigherLimit, int widthLowerLimit, int widthHigherLimit, int diameter_topSurface, int diameter_bottomSurface, int heightPointCloud, double offset, ref TCPoint3d pnt_out);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path_laserPlaneParameterXML"></param>
        /// <param name="path_cameraMatrixXML"></param>
        /// <param name="captureImagePath"></param>
        /// <param name="imgNumber"></param>
        /// <param name="widthLowerLimit"></param>
        /// <param name="widthHigherLimit"></param>
        /// <param name="heightLowerLimit"></param>
        /// <param name="heightHigherLimit"></param>
        /// <param name="diameter_topSurface"></param>
        /// <param name="diameter_bottomSurface"></param>
        /// <param name="heightPointCloud"></param>
        /// <param name="addTheOffset"></param>
        /// <param name="pointX_WestNorth"></param>
        /// <param name="width_UninterestedRectangle"></param>
        /// <param name="step"></param>
        /// <param name="pnt_out"></param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)] public extern static void generateCylindricalPointCloud_cutImage(char[] path_laserPlaneParameterXML, char[] path_cameraMatrixXML, char[] captureImagePath, int imgNumber, int widthLowerLimit, int widthHigherLimit, int heightLowerLimit, int heightHigherLimit, int diameter_topSurface, int diameter_bottomSurface, int heightPointCloud, double addTheOffset, int pointX_WestNorth, int width_UninterestedRectangle, int step, ref TCPoint3d pnt_out);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points1"></param>
        /// <param name="points2"></param>
        /// <param name="points3"></param>
        /// <param name="imageIndex"></param>
        /// <param name="clawsIndex"></param>
        /// <param name="pointsSize"></param>
        /// <param name="lightOrder"></param>
        /// <param name="path_laserPlaneParameterXML"></param>
        /// <param name="path_cameraMatrixXML"></param>
        /// <param name="imgNumber"></param>
        /// <param name="addTheOffset"></param>
        /// <param name="distance_claws1_out"></param>
        /// <param name="distance_claws2_out"></param>
        /// <param name="distance_claws3_out"></param>
        /// <param name="_tcpoints3D"></param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)] public static extern void point2dToPoint3d(Point_xy[] points1, Point_xy[] points2, Point_xy[] points3, int[] imageIndex, int[] clawsIndex, int pointsSize, int lightOrder, char[] path_laserPlaneParameterXML, char[] path_cameraMatrixXML, int imgNumber, double addTheOffset, ref double distance_claws1_out, ref double distance_claws2_out, ref double distance_claws3_out, ref TCPoint3d _tcpoints3D);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="points1"></param>
        /// <param name="points2"></param>
        /// <param name="points3"></param>
        /// <param name="imageindex"></param>
        /// <param name="clawsindex"></param>
        /// <param name="distance_claws1_out"></param>
        /// <param name="distance_claws2_out"></param>
        /// <param name="distance_claws3_out"></param>
        /// <param name="_tcpoints3d"></param>
        /// <returns></returns>
        public static CalculateErrorCode point2dTo3d(Calibration_Model model, Point_xy[] points1, Point_xy[] points2, Point_xy[] points3, int[] imageindex, int[] clawsindex, ref double[] distance_claws1_out, ref double[] distance_claws2_out, ref double[] distance_claws3_out, ref TCPoint3d[] _tcpoints3d)
        {
            try
            {
                char[] laserPointPath = (model.LaserCalibrate.LightPointPath + "\\laserPlaneParameter" + model.LaserCalibrate.LightOrder.ToString() + ".xml").ToCharArray();

                point2dToPoint3d(points1, points2, points3, imageindex, clawsindex, points1.Length, Convert.ToInt32(model.LaserCalibrate.LightOrder), laserPointPath, model.Path_cameraMatrixXML.ToCharArray(), Convert.ToInt32(model.CalculatePC.imgNumber), model.CalculatePC.offset, ref distance_claws1_out[0], ref distance_claws2_out[0], ref distance_claws3_out[0], ref _tcpoints3d[0]);
                savePointCloudToTxt(model.CalculatePC.ProductLaserImagePath, _tcpoints3d);
                return CalculateErrorCode.MOK;
            }
            catch (Exception)
            {
                return CalculateErrorCode.GenerateCloudFailed;
            }

        }

        /// <summary>
        /// 相机内外参标定
        /// 如若其中个别图像有问题，则返回对应的图像编号，将其删除，并对其余的图像重新排序从0开始排序
        /// </summary>
        /// <param name="model">标定类</param>
        /// 需要用到的参数有
        /// ChessImagePath，输入棋盘格图像的路径
        /// Path_cameraMatrixXML，相机标定文件路径
        /// Path_rvecsXML，输入rvecs.xml的路径 
        /// Path_tvecsXML，输入tvecs.xml的路径
        /// Path_distCoeffsXML，输入distCoeffs.xml的路径
        /// Image_size_Width，传入图像的实际尺寸(单位:像素)
        /// Image_size_Height，传入图像的实际尺寸(单位:像素)
        /// Corners_size_Width，传入内部角点的实际数量(长*宽)
        /// Corners_size_Height，传入内部角点的实际数量(长*宽)
        /// Chess_square_size，传入棋盘格的实际尺寸(单位:毫米)
        /// ImageNum，传入目录下图片的数量 
        public static CalculateErrorCode cameraCalibrationCommon(Calibration_Model model)
        {
            try
            {
                char[] chessImagePath = model.CameraCalibrate.ChessImagePath.ToCharArray();
                char[] path_cameraMatrixXML = model.Path_cameraMatrixXML.ToCharArray();
                char[] path_rvecsXML = model.Path_rvecsXML.ToCharArray();
                char[] path_tvecsXML = model.Path_tvecsXML.ToCharArray();
                char[] path_distCoeffsXML = model.CameraCalibrate.Path_distCoeffsXML.ToCharArray();
                Size_zy image_size = new Size_zy
                {
                    Width = Convert.ToInt32(model.CameraCalibrate.Image_size_Width),
                    Height = Convert.ToInt32(model.CameraCalibrate.Image_size_Height)
                };
                Size_zy corners_size = new Size_zy
                {
                    Width = Convert.ToInt32(model.CameraCalibrate.Corners_size_Width),
                    Height = Convert.ToInt32(model.CameraCalibrate.Corners_size_Height)
                };
                Size_zy chess_square_size = new Size_zy
                {
                    Width = Convert.ToInt32(model.CameraCalibrate.Chess_square_size),
                    Height = Convert.ToInt32(model.CameraCalibrate.Chess_square_size)
                };
                int imageNum = Convert.ToInt32(model.ImageNum);
                int[] invalidImage = new int[imageNum];  // size = imgNum;
                double[] total_error = new double[imageNum + 1];  // size = imgNum+1;
                                                                  //{ model.Total_error, -1, -1, -1, 0, 0, 0, 0, 0, 0 };
                for (int index = 0; index < invalidImage.Length; index++)
                {
                    invalidImage[index] = -1;
                }

                //返回错误代码
                int ERROR_NUMBER = cameraCalibration(chessImagePath, path_cameraMatrixXML, path_rvecsXML, path_tvecsXML, path_distCoeffsXML, image_size, corners_size, chess_square_size, imageNum, ref invalidImage[0], ref total_error[0]);

                //如果错误代码为-1，则针对错误图片进行删除操作
                if (ERROR_NUMBER == -1)
                {
                    if (changeImageName(invalidImage, chessImagePath))
                    {
                        return CalculateErrorCode.CameraCalibrateChanged;
                    }
                    else
                    {
                        return CalculateErrorCode.CameraCalibrateFailed;
                    }

                }
                return CalculateErrorCode.MOK;
            }
            catch (Exception)
            {
                return CalculateErrorCode.CameraCalibrateFailed;
            }
        }

        /// <summary>
        /// 删除错误图片，并对其余图片重新从0开始排序
        /// </summary>
        /// <param name="_invalidImage"></param>
        /// <param name="chessImagePath"></param>
        private static bool changeImageName(int[] _invalidImage, char[] chessImagePath)
        {
            try
            {
                //删除错误图片
                for (int i = 0; i < _invalidImage.Length; i++)
                {
                    if (_invalidImage[i] != -1)
                    {
                        string _path = new string(chessImagePath) + "\\" + _invalidImage[i] + ".bmp";
                        if (File.Exists(_path))
                        {
                            File.Delete(_path);
                        }
                    }
                }
                //将图片从0开始重命名
                DirectoryInfo di = new DirectoryInfo(new string(chessImagePath));
                FileInfo[] fi = di.GetFiles("*.bmp");
                for (int i = 0; i < fi.Length; i++)
                {
                    fi[i].MoveTo(fi[i].DirectoryName + "\\" + i.ToString() + ".bmp");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 激光标定
        /// </summary>
        /// <param name="model">标定类</param>
        /// 需要用到的参数有
        /// LaserImagePath,将激光图像中找到的点保存在txt中, 传入txt的路径(.txt的上级目录)\ntxt中的2d点坐标格式为x0 y0
        /// Path_cameraMatrixXML,输入cameraMatrix.xml的路径
        /// Path_rvecsXML,输入rvecs.xml的路径 
        /// Path_tvecsXML,输入tvecs.xml的路径
        /// ImageNum,输入目录下图片的数量
        /// LightOrder,传入检测的是第几根激光(将这个参数作为laserPlaneParameter的后缀作为区分,从0开始)
        /// LightPointPath，将生成的激光光平面参数放置在该路径下（到.xml的上级目录）
        public static CalculateErrorCode multipleLaserCommon(Calibration_Model model)
        {
            try
            {
                char[] laserImagePath = model.LaserCalibrate.LaserImagePath.ToCharArray();
                char[] path_cameraMatrixXML = model.Path_cameraMatrixXML.ToCharArray();
                char[] path_rvecsXML = model.Path_rvecsXML.ToCharArray();
                char[] path_tvecsXML = model.Path_tvecsXML.ToCharArray();
                int imageNum = Convert.ToInt32(model.ImageNum);
                int lightOrder = Convert.ToInt32(model.LaserCalibrate.LightOrder);
                char[] laserPointPath = model.LaserCalibrate.LightPointPath.ToCharArray();
                getLightPlaneParameter_multipleLaser(laserImagePath, path_cameraMatrixXML, path_rvecsXML, path_tvecsXML, imageNum, lightOrder, laserPointPath);
                return CalculateErrorCode.MOK;
            }
            catch (Exception)
            {
                return CalculateErrorCode.CalibrateLaserFailed;
            }
        }

        /// <summary>
        /// 点云生成1.0（算法捕捉）
        /// </summary>
        /// <param name="model">标定类</param>
        /// 需要用到的参数有
        /// LightPointPath：将生成的激光光平面参数放置在该路径下（到.xml的上级目录）
        /// LightOrder：传入检测的是第几根激光(将这个参数作为laserPlaneParameter的后缀作为区分,从0开始)
        /// ProductLaserImagePath：产品激光图片路径
        /// imgNumber：点云图片数量（理论上）
        /// heightHigherLimit：需要计算点云的矩形区域的长度最大值
        /// heightLowerLimit：需要计算点云的矩形区域的长度最小值
        /// Path_cameraMatrixXML：相机标定文件路径
        /// widthLowerLimit：需要计算点云的矩形区域的宽度最小值
        /// widthHigherLimit：需要计算点云的矩形区域的宽度最大值
        /// diameter_topSurface：上表面的实际尺寸（mm）
        /// diameter_bottomSurface：下表面的实际尺寸（mm）
        /// heightPointCloud：图片的最大宽度
        /// offset：默认0，无需修改
        public static CalculateErrorCode generateCylindricalPointCloudCommon(Calibration_Model model, ref TCPoint3d[] tCPoint3D)
        {
            try
            {
                char[] laserPointPath = (model.LaserCalibrate.LightPointPath + "\\laserPlaneParameter" + model.LaserCalibrate.LightOrder.ToString() + ".xml").ToCharArray();
                char[] caputerImagePath = model.CalculatePC.ProductLaserImagePath.ToCharArray();
                tCPoint3D = new TCPoint3d[model.CalculatePC.imgNumber * Math.Abs(model.CalculatePC.heightHigherLimit - model.CalculatePC.heightLowerLimit)];
                generateCylindricalPointCloud(laserPointPath, model.Path_cameraMatrixXML.ToCharArray(), caputerImagePath, model.CalculatePC.imgNumber, model.CalculatePC.heightLowerLimit, model.CalculatePC.heightHigherLimit, model.CalculatePC.widthLowerLimit, model.CalculatePC.widthHigherLimit, model.CalculatePC.diameter_topSurface, model.CalculatePC.diameter_bottomSurface, model.CalculatePC.heightPointCloud, model.CalculatePC.offset, ref tCPoint3D[0]);
                savePointCloudToTxt(AppDomain.CurrentDomain.BaseDirectory + "CloudPoint.txt", tCPoint3D);
                return CalculateErrorCode.MOK;
            }
            catch (Exception)
            {
                return CalculateErrorCode.GenerateCloudFailed;
            }
        }

        /// <summary>
        /// 点云生成2.0（算法捕捉）
        /// </summary>
        /// <param name="model">标定类</param>
        /// 需要用到的参数有
        /// LightPointPath：将生成的激光光平面参数放置在该路径下（到.xml的上级目录）
        /// LightOrder：传入检测的是第几根激光(将这个参数作为laserPlaneParameter的后缀作为区分,从0开始)
        /// ProductLaserImagePath：产品激光图片路径
        /// imgNumber：点云图片数量（理论上）
        /// heightHigherLimit：需要计算点云的矩形区域的长度最大值
        /// heightLowerLimit：需要计算点云的矩形区域的长度最小值
        /// Path_cameraMatrixXML：相机标定文件路径
        /// widthLowerLimit：需要计算点云的矩形区域的宽度最小值
        /// widthHigherLimit：需要计算点云的矩形区域的宽度最大值
        /// diameter_topSurface：上表面的实际尺寸（mm）
        /// diameter_bottomSurface：下表面的实际尺寸（mm）
        /// heightPointCloud：图片的最大宽度
        /// offset：减去（200-产品直径）/2
        public static CalculateErrorCode generateCylindricalPointCloudCommon_cutImage(Calibration_Model model, ref TCPoint3d[] tCPoint3D)
        {
            try
            {
                char[] laserPointPath = (model.LaserCalibrate.LightPointPath + "\\laserPlaneParameter" + model.LaserCalibrate.LightOrder.ToString() + ".xml").ToCharArray();
                char[] caputerImagePath = model.CalculatePC.ProductLaserImagePath.ToCharArray();
                //tCPoint3D = new TCPoint3d[model.imgNumber * Math.Abs(model.heightHigherLimit - model.heightLowerLimit)];
                TCPoint3d[] tCPoint3D_1 = tCPoint3D;
                int width_UninterestedRectangle = model.CalculatePC.X2 - model.CalculatePC.X1;
                generateCylindricalPointCloud_cutImage(laserPointPath, model.Path_cameraMatrixXML.ToCharArray(), caputerImagePath, model.CalculatePC.imgNumber, model.CalculatePC.heightLowerLimit, model.CalculatePC.heightHigherLimit, model.CalculatePC.widthLowerLimit, model.CalculatePC.widthHigherLimit, model.CalculatePC.diameter_topSurface, model.CalculatePC.diameter_bottomSurface, model.CalculatePC.heightPointCloud, model.CalculatePC.offset, model.CalculatePC.X1, width_UninterestedRectangle, 2, ref tCPoint3D[0]);
                //foreach (var item in tCPoint3D)
                //{
                //    if (item.X != 0 || item.Y != 0 || item.Z != 0)
                //    {
                //        _tcPoint3D_1
                //        }
                //}
                savePointCloudToTxt(AppDomain.CurrentDomain.BaseDirectory + "CloudPoint.txt", tCPoint3D);
                return CalculateErrorCode.MOK;
            }
            catch (Exception)
            {
                return CalculateErrorCode.GenerateCloudFailed;
            }
        }

        /// <summary>
        /// 将点云数据保存到本地
        /// </summary>
        /// <param name="_path">本地路径</param>
        /// <param name="_tcPoint3D">点云数据</param>
        public static CalculateErrorCode savePointCloudToTxt(string _path, TCPoint3d[] _tcPoint3D)
        {
            try
            {
                _path += @"\CloudPoints.txt";
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
                //将点云数据存到本地
                using (FileStream fs = new FileStream(_path, FileMode.Create, FileAccess.Write))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    foreach (var item in _tcPoint3D)
                    {
                        if (item.X != 0 || item.Y != 0 || item.Z != 0)
                        {
                            string str = item.strAppend() + "\r\n";

                            sw.Write(str);
                        }
                    }
                }
                return CalculateErrorCode.MOK;
            }
            catch (Exception)
            {
                return CalculateErrorCode.SaveCloudTxtFailed;
            }
        }

        /*
        /// <summary>
        /// 生成点云（VisionPro捕捉）
        /// </summary>
        /// <param name="data">VisionPro工具处理后的点云数据（2D）</param>
        /// <param name="size">点云图片数量（实际）</param>
        /// <param name="lightOrder">传入检测的是第几根激光(将这个参数作为laserPlaneParameter的后缀作为区分,从0开始)</param>
        /// <param name="diameter_topSurface">上表面实际尺寸（mm）</param>
        /// <param name="diameter_bottomSurface">下表面实际尺寸（mm）</param>
        /// <param name="heightPointCloud">图片最大宽度</param>
        /// <param name="path_laserPlaneParameterXML">激光标定文件路径</param>
        /// <param name="path_cameraMatrixXML">相机标定文件路径</param>
        /// <param name="imgNumber">点云图片数量（理论上）</param>
        /// <param name="addTheOffset">点云文件中物体的大小和实际物体大小不一样时，输入一定的偏移量使他们看起来一致</param>
        /// <param name="data_out">点云数据（3D）</param>
        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static void point2dToPoint3d(Image2dPointsWithIndex[] data, int size, int lightOrder, int diameter_topSurface, int diameter_bottomSurface, int heightPointCloud, char[] path_laserPlaneParameterXML, char[] path_cameraMatrixXML, int imgNumber, double addTheOffset, ref TCPoint3d data_out);

        [DllImport("calibrationChessCorner.dll", CallingConvention = CallingConvention.StdCall)]
        public extern static void point2dToPoint3d(Point_xy[] points1, Point_xy[] points2, Point_xy[] points3, int[] imageIndex, int[] clawsIndex, int pointsSize, int lightOrder, char[] path_laserPlaneParameterXML, char[] path_cameraMatrixXML, int imgNumber, double addTheOffset, ref double distance_out);
        */
        #endregion

        #region Event

        #endregion

    }
}
