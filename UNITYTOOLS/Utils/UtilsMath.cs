using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//-------------------------------------------------------------------------------- Math ------------------------------------------------------------------------------------

public static class MathZQJ
{
    public class Line
    {
        public Vector3 point;

        public Vector3 direction;

        public Line(Vector3 p, Vector3 dir)
        {
            point = p;
            direction = dir;
        }
    }
    public class Segment
    {
        public Vector3 pointA;

        public Vector3 pointB;

        public Segment(Vector3 a, Vector3 b)
        {
            pointA = a;
            pointB = b;
        }
    }


    public static int GetFirstPos(int num)
    {
        int res = num;
        if (res < 0) res = -res;
        while(res > 9)
        {
            res = res / 10;
        }

        return res;
    }

    /// <summary>
    /// 线与平面交点  
    /// </summary>
    /// <param name="point"></param>
    /// <param name="direct"></param>
    /// <param name="planeNormal"></param>
    /// <param name="planePoint"></param>
    /// <returns></returns>
    public static Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
    {
        float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);

        return d * direct.normalized + point;
    }

    /// <summary>
    /// 点到直线的带符号距离
    /// </summary>
    public static float GetSignedDistancePoint2Line2D(Vector2 p, Line ab)
    {
        //左法向量
        Vector2 leftNormal = new Vector2(-ab.direction.y, ab.direction.x).normalized;
        //AP在左法向量上的投影即带符号距离
        Vector2 ap = p - new Vector2(ab.point.x, ab.point.y);
        //带符号距离
        float signDistance = Vector2.Dot(ap, leftNormal);

        return signDistance;
    }

    /// <summary>
    /// 直线PG与线段ab的交点
    /// </summary>
    public static bool GetIntersectOfLineAndSegment2D(Line line, Segment seg, out Vector2 intersection)
    {
        intersection = new Vector2();


        //A到PQ的带符号距离
        float signDis1 = GetSignedDistancePoint2Line2D(seg.pointA, line); 

        //B到PQ的带符号距离
        float signDis2 = GetSignedDistancePoint2Line2D(seg.pointB, line); 

        //两个带符号距离异号异号或者一个为0 ==> 交点存在
        if (signDis1 * signDis2 <= 0)
        {
            //绝对值距离
            float absDis1 = Mathf.Abs(signDis1);
            float absDis2 = Mathf.Abs(signDis2);


            float k;
            if (absDis1 + absDis2 != 0)
            {
                k = absDis1 / (absDis1 + absDis2);
            }
            //absD1和absD2均为0 ==> 线段和直线重合
            else
            {
                k = 0.5f;
            }

            Vector2 mid = seg.pointA + (seg.pointB - seg.pointA) * k;

            intersection = mid;
            return true;
        } 
        // ==> 交点不存在
        else
        {
            return false;
        }
    }



    /// <summary>
    /// 从一列整数中获取不重复的一组
    /// </summary>
    /// <param name="from">include</param>
    /// <param name="to">exclude</param>
    /// <param name="getCount">get count</param>
    /// <returns></returns>
    public static int[] GetNonRepeatNumbers(int from, int to, int getCount) // 0 - 10 (0123456789)
    {
        //Assert
        if (getCount > (to - from)) return null;


        int[] result = new int[getCount];

        List<int> remainList = new List<int>();
        for (int i = from; i < to; i++)
        {
            remainList.Add(i);
        }

        for (int i = 0; i < getCount; i++)
        {
            int idx = UnityEngine.Random.Range(0, remainList.Count);
            int value = remainList[idx];
            result[i] = value;
            remainList.RemoveAt(idx);
        }
        return result;
    }
}


public static class GeometryZQJ
{

    //1. 已验证
    //2. 行列方式： R[row, col]
    public static float[] MatrixToQuaternionTest(float[,] R)
    {
        float[] q = new float[4];

        float trace = R[0, 0] + R[1, 1] + R[2, 2];

        if (trace > 0f)
        {
            float s = Mathf.Sqrt(trace + 1.0f);
            q[3] = s * 0.5f;

            float t = 0.5f / s;
            q[0] = (R[2, 1] - R[1, 2]) * t;
            q[1] = (R[0, 2] - R[2, 0]) * t;
            q[2] = (R[1, 0] - R[0, 1]) * t;
        }
        else
        {
            int i = 0;
            if (R[1, 1] > R[0, 0]) i = 1;
            if (R[2, 2] > R[i, i]) i = 2;

            int[] next = new int[3] { 1, 2, 0 };
            int j = next[i];
            int k = next[j];

            float s = Mathf.Sqrt(R[i, i] - R[j, j] - R[k, k] + 1.0f);

            q[i] = s * 0.5f;

            float t;
            if (s != 0.0f)
            {
                t = 0.5f / s;
            }
            else
            {
                t = s;
            }

            q[3] = (R[k, j] - R[j, k]) * t;
            q[j] = (R[j, i] + R[i, j]) * t;
            q[3] = (R[k, i] + R[i, k]) * t;
        }

        return q;
    }
}


