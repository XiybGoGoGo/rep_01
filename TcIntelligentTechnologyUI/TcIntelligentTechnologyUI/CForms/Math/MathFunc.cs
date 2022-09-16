using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcIntelligentTechnologyUI.CForms
{
    public partial class MathFunc
    {
        /// <summary>
        /// 希尔排序法
        /// </summary>
        /// <param name="_dis">需要排序的数组</param>
        public static double[] Sort(double[] _dis)
        {
            List<double> _dis_new = new List<double>();
            try
            {
                for (int m = 0; m < _dis.Length; m++)
                {
                    if (_dis[m] != 0)
                    {
                        _dis_new.Add(_dis[m]);
                    }
                }
                _dis = _dis_new.ToArray();

                for (int i = 0; i < _dis.Length - 1; i++)
                {
                    for (int j = 0; j < _dis.Length - 1 - i; j++)
                    {
                        if (_dis[j] > _dis[j + 1])
                        {
                            double temp = _dis[j];
                            _dis[j] = _dis[j + 1];
                            _dis[j + 1] = temp;
                        }
                    }
                }
                return _dis;
            }
            catch (Exception ex)
            {
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return _dis;
            }

        }

        /// <summary>
        /// 取平均值（各去掉首尾的10%）
        /// </summary>
        /// <param name="_distance">需要取平均值的集合</param>
        /// <returns>平均值</returns>
        public static double GetAverage(double[] _distance)
        {
            try
            {
                int num = 0;
                double Sum = 0;
                double Average = 0;

                Sum = 0;
                num = 0;
                for (int j = Convert.ToInt32(_distance.Length * 0.15); j < Convert.ToInt32(_distance.Length * 0.85); j++)
                {
                    Sum += _distance[j];
                    num++;
                }
                Average = Sum / num;
                return Average;
            }
            catch (Exception ex)
            {
                Global_Method.mWriteErrorMesToTxt(ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                return 0;
            }

        }
    }
}
