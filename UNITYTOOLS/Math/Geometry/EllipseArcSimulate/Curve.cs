using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EllipseArcSimulate
{
    
    public class UtilsAngle
    {
        public static float ClampPosNeg180(float angle)
        {
            if(angle > 180f)
            {
                return angle - 360f;
            }
            else if(angle < -180f)
            {
                return angle + 360f;
            }
            return angle;
        }
    }

    [ExecuteInEditMode]
    public class Curve : MonoBehaviour
    {
        [Header("顺时针")]
        public bool clockWise = true;

        [Header("激活")]
        public bool isActive = false;
        
        [HideInInspector]
        public bool segmentLengthManagedByBuilder = false;

        public void Start()
        {
            //如果有拖动组件
            if (this.GetComponent<SlidableChildObject>() != null)
            {
                this.GetComponent<SlidableChildObject>().onPointerDown.AddListener(() => {
                    var builder = this.GetComponentInParent<FuselageBuilder>();
                    if (builder != null)
                    {
                        builder.SelectCurve(this);
                        builder.builderUI.Log("Section Selected.");
                    }
                });
            }
        }

        public void Update()
        {
            if (isActive == false) return;

            Vector3 nextPoint = default;
            float nextNormal = 0;
            for (int i = 0; i < this.transform.childCount; ++i)
            {
                var seg = this.transform.GetChild(i).GetComponent<Segment>();

                if(segmentLengthManagedByBuilder == false)
                {
                    seg.UpdateLengthWithScale();
                }

                //顺时针
                if(clockWise)
                {
                    if (i == 0)
                    {
                        //第一个必然是手动的  
                        seg.auto = false;
                    }
                    else
                    {
                        if(seg.auto)
                        {
                            //法线靠齐
                            float halfNormalChange = (seg.Angle() - nextNormal);
                            seg.curvature = 2 * halfNormalChange;

                            //位置靠齐  
                            seg.transform.position = nextPoint + (-seg.LeftDir());
                        }
                    }

                    nextPoint = seg.transform.position + seg.RightDir();
                    nextNormal = UtilsAngle.ClampPosNeg180(seg.Angle() + seg.curvature * 0.5f);
                    //Debug.Log(i.ToString() + " set next normal:" + nextNormal);
                }
                //逆时针
                else
                {
                    if (i == 0)
                    {
                        //第一个必然是手动的  
                        seg.auto = false;
                    }
                    else
                    {
                        if(seg.auto)
                        {
                            //位置靠齐  
                            seg.transform.position = nextPoint + (-seg.RightDir());

                            //法线靠齐
                            float halfNormalChange = -(seg.Angle() - nextNormal);
                            seg.curvature = 2 * halfNormalChange;
                        }
                    }

                    nextPoint = seg.transform.position + seg.LeftDir();
                    nextNormal = UtilsAngle.ClampPosNeg180(seg.Angle() - seg.curvature * 0.5f);
                }



                seg.UpdateScaleWithLength();

                seg.EditorDrawNormal();
            }
        }

        public void UnBindFrame()
        {
            for (int i = 0; i < this.transform.childCount; ++i)
            {
                var seg = this.transform.GetChild(i).GetComponent<Segment>();

                if (seg.frameGenerated != null)
                {
                    seg.frameGenerated = null;
                }
            }
        }


#if UNITY_EDITOR
        public void Unity_GenerateFrames()
        {
            CurvatureDatabase.Init();

            for (int i = 0; i < this.transform.childCount; ++i)
            {
                var seg = this.transform.GetChild(i).GetComponent<Segment>();

                if (seg.frameGenerated != null)
                {
                    Debug.LogAssertion("请先去除绑定");
                    return;
                }

                seg.UnityDesigner_GenerateFrame();
            }
        }

#endif

        public void OnDisable()
        {
            this.isActive = false;
        }


    }


}






























#if UNITY_EDITOR
[CustomEditor(typeof(EllipseArcSimulate.Curve))]
public class EllipseCurveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("生成"))
        {
            (target as EllipseArcSimulate.Curve).Unity_GenerateFrames();
        }


        if (GUILayout.Button("解除框架绑定"))
        {
            (target as EllipseArcSimulate.Curve).UnBindFrame();
        }

        GUILayout.Label("");
        GUILayout.Label("---------------注意-----------------");
        GUILayout.Label("定斜线段优先的圆筒生成器");
        GUILayout.Label("扣洞解决方案1：边缘重叠");
        GUILayout.Label("扣洞解决方案2：强制clamp");
    }
}
#endif