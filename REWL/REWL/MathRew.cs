﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REWL
{
    public class MathRew
    {

        #region 常用数学函数

        /// <summary>
        /// 根判别式
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double Rootjudge(double a, double b, double c)
        {
            return b * b - 4 * a * c;
        }
        /// <summary>
        /// 求根公式
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double[] Root(double a, double b, double c)
        {
            double root_1;
            double root_2;
            if (a != 0)
            {
                if (b * b - 4 * a * c >= 0)
                {
                    root_1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
                    root_2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
                    double[] roots = new double[2];
                    roots[0] = root_1;
                    roots[1] = root_2;
                    return roots;
                }
                else
                {
                    double[] roots = null;
                    return roots;
                }
            }
            else
            {
                double[] roots = new double[1];
                roots[0] = -(c / b);
                return roots;
            }

        }
        

        /// <summary>
        /// 浮点数组读取
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static float[] ReadFloatArrayStr(string str)
        {
            List<float> arrayList = new List<float>();
            string strCurrent = str;
            while (strCurrent.Contains(","))
            {
                arrayList.Add(Convert.ToSingle(strCurrent.Substring(0, strCurrent.IndexOf(','))));
                strCurrent = strCurrent.Substring(strCurrent.IndexOf(',') + 1);
            }
            arrayList.Add(Convert.ToSingle(strCurrent));
            return arrayList.ToArray();
        }
        public static int[] ReadIntArrayStr(string str)
        {
            List<int> arrayList = new List<int>();
            string strCurrent = str;
            while (strCurrent.Contains(","))
            {
                arrayList.Add(Convert.ToInt32(strCurrent.Substring(0, strCurrent.IndexOf(','))));
                strCurrent = strCurrent.Substring(strCurrent.IndexOf(',') + 1);
            }
            arrayList.Add(Convert.ToInt32(strCurrent));
            return arrayList.ToArray();
        }
        

        /// <summary>
        /// 浮点转换字符串
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ArrayToString(int[] array)
        {
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                strb.Append(array[i]);
                strb.Append(",");
            }
            strb.Remove(strb.Length - 1, 1);
            return strb.ToString();
        }

        #endregion

    }
}
