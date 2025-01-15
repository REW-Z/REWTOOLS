using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using LitJson;

# if UNITY_EDITOR
using UnityEditor;
#endif


namespace EllipseArcSimulate
{
    public class Segment : MonoBehaviour
    {
        public bool auto;

        [HideInInspector]
        public float length;

        [HideInInspector]
        public float curvature;

        [HideInInspector]
        public GameObject frameGenerated;

        [HideInInspector]
        public Material lineMaterial;

        




        public void Start()
        {
            //如果有拖动组件
            if(this.GetComponent<DragablePlaneObject>() != null)
            {
                this.GetComponent<DragablePlaneObject>().onPointerDown.AddListener(() => {
                    var builder = this.GetComponentInParent<FuselageBuilder>();
                    if (builder != null)
                    {
                        builder.SelectSegment(this);
                        builder.builderUI.Log("Segment Selected.");
                    }
                });
            }
        }



        public void UpdateLengthWithScale()
        {
            this.length = this.transform.localScale.x;
        }
        public void UpdateScaleWithLength()
        {
            this.transform.localScale = new Vector3(length, 0.0005f, this.transform.localScale.z);
        }
        public void EditorDrawNormal()
        {
            Vector3 normalL = Quaternion.Euler(0, 0, curvature * 0.5f) * this.transform.up;
            Vector3 normalR = Quaternion.Euler(0, 0, -curvature * 0.5f) * this.transform.up;
            Debug.DrawRay(this.transform.position + LeftDir(), normalL.normalized * 2f, Color.blue);
            Debug.DrawRay(this.transform.position + RightDir(), normalR.normalized * 2f, Color.blue);
        }


        public Vector3 LeftDir()
        {
            return -this.transform.right * length * 0.5f;
        }
        public Vector3 RightDir()
        {
            return this.transform.right * length * 0.5f;
        }
        public float Angle()
        {
            return Vector3.SignedAngle(Vector3.up, this.transform.up, -Vector3.forward);
        }


        public FuselageInfo GenerateFuselageInfo()
        {
            FuselageInfo fuseInfo = new FuselageInfo(PartType.CargoBayDoor);

            if(CurvatureDatabase.IsInited == false)
            {
                CurvatureDatabase.Init();
            }
            if (CurvatureDatabase.IsInited == false) 
            {
                Debug.LogAssertion("没有数据库信息，返回默认");
                return new FuselageInfo(PartType.Solid_1);
            } 

            if(this.curvature > 0f)
            {
                //width  / height  / clamp  
                float width = this.length / CurvatureDatabase.lenFac785;
                float ratio = CurvatureDatabase.chart0d785.Sample(Mathf.Abs(this.curvature));


                float height = width * ratio;
                fuseInfo.top = new Vector2(width, height);
                fuseInfo.bottom = new Vector2(width, height);
                fuseInfo.offset = new Vector3(0, 0.1f, 0);

                fuseInfo.topClamp0 = -0.6055943f;
                fuseInfo.topClamp1 = 0.6055943f;
                fuseInfo.topClamp2 = -1f;
                fuseInfo.topClamp3 = -0.785f;

                fuseInfo.bottomClamp0 = -0.6055943f;
                fuseInfo.bottomClamp1 = 0.6055943f;
                fuseInfo.bottomClamp2 = -1f;
                fuseInfo.bottomClamp3 = -0.785f;

                //position / rotation  
                fuseInfo.rotation = Quaternion.LookRotation(-this.transform.up, this.transform.forward);
                var localEdges = JNOFuselage.GetEdgesLocal(fuseInfo);
                localEdges[0] = new Vector3(localEdges[0].x, 0f, localEdges[0].z);
                localEdges[1] = new Vector3(localEdges[1].x, 0f, localEdges[1].z);
                Vector3[] edges = new Vector3[2];
                Matrix4x4 mat = Matrix4x4.Translate(this.transform.position) * Matrix4x4.Rotate(fuseInfo.rotation);//假设圆筒和片段同坐标的变换矩阵  

                edges[0] = mat * new Vector4(localEdges[0].x, localEdges[0].y, localEdges[0].z, 1f);
                edges[1] = mat * new Vector4(localEdges[1].x, localEdges[1].y, localEdges[1].z, 1f);

                var offset = ((edges[0] + edges[1]) / 2f) - this.transform.position;

                fuseInfo.position = this.transform.position - offset;
            }
            else if(this.curvature < 0f)
            {
                //base info  
                fuseInfo.partType = PartType.Solid_1;
                fuseInfo.top = new Vector2(1, 1);
                fuseInfo.bottom = new Vector2(1, 1);
                fuseInfo.offset = new Vector3(0, 0.1f, 0);

                float clamp = CurvatureDatabase.chartConcave.Sample(Mathf.Abs(this.curvature));

                fuseInfo.topClamp0 = clamp;
                fuseInfo.topClamp1 = 0.85f;
                fuseInfo.topClamp2 = -0.85f;
                fuseInfo.topClamp3 = -clamp;

                fuseInfo.bottomClamp0 = clamp;
                fuseInfo.bottomClamp1 = 0.85f;
                fuseInfo.bottomClamp2 = -0.85f;
                fuseInfo.bottomClamp3 = -clamp;


                //rotation  
                Quaternion rotbase = Quaternion.LookRotation(this.transform.right, this.transform.forward);
                Quaternion rot45 = Quaternion.Euler(0, 0, -45f);
                Quaternion rotFinal = rot45 * rotbase;
                fuseInfo.rotation = rotFinal;

                //修正缩放  
                JNOFuselage.GetEdgesLocal(fuseInfo);
                var localEdges = JNOFuselage.GetEdgesLocal(fuseInfo);
                localEdges[0] = new Vector3(localEdges[0].x, 0f, localEdges[0].z);
                localEdges[1] = new Vector3(localEdges[1].x, 0f, localEdges[1].z);
                Vector3[] edges = new Vector3[2];
                Matrix4x4 mat = Matrix4x4.Translate(this.transform.position) * Matrix4x4.Rotate(fuseInfo.rotation);//假设圆筒和片段同坐标的变换矩阵  
                edges[0] = mat * new Vector4(localEdges[0].x, localEdges[0].y, localEdges[0].z, 1f);
                edges[1] = mat * new Vector4(localEdges[1].x, localEdges[1].y, localEdges[1].z, 1f);
                float scale = this.length  /  (edges[1] - edges[0]).magnitude;
                fuseInfo.top = Vector2.one * scale;
                fuseInfo.bottom = Vector2.one * scale;

                //再次重新计算edges
                localEdges = JNOFuselage.GetEdgesLocal(fuseInfo);
                localEdges[0] = new Vector3(localEdges[0].x, 0f, localEdges[0].z);
                localEdges[1] = new Vector3(localEdges[1].x, 0f, localEdges[1].z);
                edges[0] = mat * new Vector4(localEdges[0].x, localEdges[0].y, localEdges[0].z, 1f);
                edges[1] = mat * new Vector4(localEdges[1].x, localEdges[1].y, localEdges[1].z, 1f);



                //position  
                var offset = ((edges[0] + edges[1]) / 2f) - this.transform.position;
                fuseInfo.position = this.transform.position - offset;
            }



            return fuseInfo;
        }


#if UNITY_EDITOR

        public void UnityDesigner_GenerateFrame()
        {
            if(this.frameGenerated != null)
            {
                //DestroyImmediate(this.frameGenerated);
                return;
            }

            JNOCraft craft = FindObjectOfType<JNOCraft>();

            GameObject cargobayPrefab = Resources.Load<GameObject>("DesignerPrefab/CargoBayDoor"); if (cargobayPrefab == null) { Debug.LogAssertion("找不到预制体"); return; }
            this.frameGenerated = GameObject.Instantiate(cargobayPrefab, craft.transform);

            if(CurvatureDatabase.IsInited)
            {
                var fuse = this.frameGenerated.GetComponent<JNOFuselage>();
                FuselageInfo info = GenerateFuselageInfo();
                fuse.UpdateByInfo(info);
            }
        }

        public void UnityDesigner_FillChart()
        {
            this.GetComponentInParent<Curve>().StartCoroutine(this.UnityDesigner_CoFillChart());
        }

        private IEnumerator UnityDesigner_CoFillChart()
        {
            if(this.frameGenerated == null)
            {
                UnityDesigner_GenerateFrame();
            }

            var fuse = this.frameGenerated.GetComponent<JNOFuselage>();
            fuse.clampMode = ClampMode.AccurateShape;
            fuse.Part.partType = PartType.CargoBayDoor;

            fuse.Part.UpdatePart();

            yield return null;
            yield return null;

            //0.785f
            {
                List<Vector2> points1 = new List<Vector2>(100);

                fuse.topClamp0 = -0.6055943f;
                fuse.topClamp1 = 0.6055943f;
                fuse.topClamp2 = -1f;
                fuse.topClamp3 = -0.785f;

                fuse.bottomClamp0 = -0.6055943f;
                fuse.bottomClamp1 = 0.6055943f;
                fuse.bottomClamp2 = -1f;
                fuse.bottomClamp3 = -0.785f;

                for (int i = 0; i < 100; i++)
                {
                    yield return null;
                    yield return null;

                    if (i == 0)
                    {
                        fuse.top = new Vector2(1f, 0.0001f);
                        fuse.bottom = new Vector2(1f, 0.0001f);
                    }
                    else
                    {
                        fuse.top = new Vector2(1f, i * 0.01f);
                        fuse.bottom = new Vector2(1f, i * 0.01f);
                    }


                    fuse.Part.UpdatePart();
                    fuse.Part.GetComponentInChildren<MeshFilter>().mesh.RecalculateNormals();

                    Debug.Log("Update Part");

                    yield return null;
                    yield return null;

                    var normals = fuse.GetEdgeNormals();
                    var normAngle = Vector3.Angle(normals[0], normals[1]);


                    points1.Add(new Vector2(normAngle, i * 0.01f));

                    Debug.Log("Add Point");


                    yield return null;
                    yield return null;
                }
                CurvatureDatabase.chart0d785 = new LineChart(points1);
                CurvatureDatabase.lenFac785 = 0.6055943f / 1f;

            }



            //0.885f
            {
                List<Vector2> points2 = new List<Vector2>(100);

                fuse.topClamp0 = -0.4541914f;
                fuse.topClamp1 = 0.4541914f;
                fuse.topClamp2 = -1f;
                fuse.topClamp3 = -0.885f;

                fuse.bottomClamp0 = -0.4541914f;
                fuse.bottomClamp1 = 0.4541914f;
                fuse.bottomClamp2 = -1f;
                fuse.bottomClamp3 = -0.885f;

                for (int i = 0; i < 100; i++)
                {
                    yield return null;

                    if (i == 0)
                    {
                        fuse.top = new Vector2(1f, 0.0001f);
                        fuse.bottom = new Vector2(1f, 0.0001f);
                    }
                    else
                    {
                        fuse.top = new Vector2(1f, i * 0.01f);
                        fuse.bottom = new Vector2(1f, i * 0.01f);
                    }

                    fuse.Part.UpdatePart();

                    yield return null;

                    var normals = fuse.GetEdgeNormals();
                    var normAngle = Vector3.Angle(normals[0], normals[1]);

                    points2.Add(new Vector2(normAngle, i * 0.01f));

                    yield return null;

                }


                CurvatureDatabase.chart0d885 = new LineChart(points2);
                CurvatureDatabase.lenFac885 = 0.4541914f / 1f;
            }

            
            fuse.Part.partType = PartType.Solid_1;//变更类型  
            fuse.Part.UpdatePart();

            yield return null;
            yield return null;

            //凹面  
            {
                List<Vector2> points = new List<Vector2>(100);


                fuse.topClamp0 = 0.56f;
                fuse.topClamp1 = 0.85f;
                fuse.topClamp2 = -0.85f;
                fuse.topClamp3 = -0.56f;

                fuse.bottomClamp0 = 0.56f;
                fuse.bottomClamp1 = 0.85f;
                fuse.bottomClamp2 = -0.85f;
                fuse.bottomClamp3 = -0.56f;

                for (int i = 0; i < 100; ++i)
                {
                    yield return null;
                    yield return null;

                    fuse.topClamp0 = 0.56f + (i * 0.004f); //0.56f ~ 0.96f
                    fuse.topClamp3 = -(0.56f + (i * 0.004f));
                    fuse.bottomClamp0 = 0.56f + (i * 0.004f);
                    fuse.bottomClamp3 = -(0.56f + (i * 0.004f));


                    fuse.Part.UpdatePart();
                    fuse.Part.GetComponentInChildren<MeshFilter>().mesh.RecalculateNormals();

                    yield return null;
                    yield return null;


                    var normals = fuse.GetEdgeNormals();
                    var normAngle = Vector3.Angle(normals[0], normals[1]);


                    points.Add(new Vector2(normAngle, Mathf.Abs(0.56f + (i * 0.004f))));


                    yield return null;
                }

                CurvatureDatabase.chartConcave = new LineChart(points);
            }


            //Save  
            JsonData jdata = new JsonData();
            var j0785 = jdata["clamp0785"] = new JsonData();
            j0785.SetJsonType(JsonType.Array);

            for (int i = 0; i < 100; ++i)
            {
                j0785.Add(i);
                j0785[i] = new JsonData();
                j0785[i].Add(0);
                j0785[i].Add(1);
                j0785[i][0] = CurvatureDatabase.chart0d785.points[i].x.ToString();
                j0785[i][1] = CurvatureDatabase.chart0d785.points[i].y.ToString();
            }

            var j0885 = jdata["clamp0885"] = new JsonData();
            j0885.SetJsonType(JsonType.Array);

            for (int i = 0; i < 100; ++i)
            {
                j0885.Add(i);
                j0885[i] = new JsonData();
                j0885[i].Add(0);
                j0885[i].Add(1);
                j0885[i][0] = CurvatureDatabase.chart0d885.points[i].x.ToString();
                j0885[i][1] = CurvatureDatabase.chart0d885.points[i].y.ToString();
            }

            var jconcave = jdata["concave"] = new JsonData();
            for (int i = 0; i < 100; ++i)
            {
                jconcave.Add(i);
                jconcave[i] = new JsonData();
                jconcave[i].Add(0);
                jconcave[i].Add(1);
                jconcave[i][0] = CurvatureDatabase.chartConcave.points[i].x.ToString();
                jconcave[i][1] = CurvatureDatabase.chartConcave.points[i].y.ToString();
            }





            jdata["len0785"] = CurvatureDatabase.lenFac785.ToString();
            jdata["len0885"] = CurvatureDatabase.lenFac885.ToString();




            string json = jdata.ToJson();
            string path = Application.streamingAssetsPath + "/curvatureDatabase.json";
            System.IO.File.WriteAllText(path, json);

            Debug.Log("Saved To : " + path);
        }


#endif
    }




    public class LineChart
    {
        public List<Vector2> points;
        
        public LineChart(List<Vector2> points)
        {
            this.points = points.OrderBy(p => p.x).ToList();
        }

        public float Sample(float x)
        {
            float y = 0;

            if (x <= points[0].x)
            {
                float k = (points[1].y - points[0].y) / (points[1].x - points[0].x);
                float xo = x - points[0].x;
                float yo = xo * k;

                y = points[0].y + yo;
            }
            else if(x >= points[points.Count - 1].x)
            {
                var pLast = points[points.Count - 1];
                var pPrev = points[points.Count - 2];

                float k = (pLast.y - pPrev.y) / (pLast.x - pPrev.x);
                float xo = x - pLast.x;
                float yo = xo * k;

                y = pLast.y + yo;
            }
            else
            {
                for (int i = 0; i < points.Count - 1; ++i)
                {
                    if(x > points[i].x && x < points[i + 1].x)
                    {
                        float t = (x - points[i].x) / (points[i + 1].x - points[i].x);
                        y = Mathf.Lerp(points[i].y, points[i + 1].y, t);
                        break;
                    }
                }
            }

            return y;
        }
    }
    public class CurvatureDatabase
    {
        public static LineChart chart0d885;
        public static LineChart chart0d785;

        public static LineChart chartConcave;

        public static float lenFac885 = 0.3f;
        public static float lenFac785 = 0.4f;

        private static bool inited = false; public static bool IsInited => inited;

        public static void Init()
        {
            if (inited == true) return;

            var textAsset = Assets.Scripts.Mod.Instance.ResourceLoader.LoadAsset<TextAsset>("Assets/Content/curvatureDatabase.json");

            if (textAsset == null) { Debug.LogAssertion("没有找到圆筒数据库Json"); return; }

            string json = textAsset.text;

            JsonData jdata = JsonMapper.ToObject(json);

            List<Vector2> points785 = new List<Vector2>();
            for (int i = 0; i < jdata["clamp0785"].Count; ++i)
            {
                points785.Add(new Vector2(
                    float.Parse(((string)jdata["clamp0785"][i][0])),
                    float.Parse(((string)jdata["clamp0785"][i][1]))
                    ));
            }
            chart0d785 = new LineChart(points785);

            List<Vector2> points885 = new List<Vector2>();
            for (int i = 0; i < jdata["clamp0885"].Count; ++i)
            {
                points885.Add(new Vector2(
                    float.Parse(((string)jdata["clamp0885"][i][0])),
                    float.Parse(((string)jdata["clamp0885"][i][1]))
                    ));
            }
            chart0d885 = new LineChart(points885);


            List<Vector2> pointsConcave = new List<Vector2>();
            for (int i = 0; i < jdata["concave"].Count; ++i)
            {
                pointsConcave.Add(new Vector2(
                    float.Parse(((string)jdata["concave"][i][0])),
                    float.Parse(((string)jdata["concave"][i][1]))
                    ));
            }
            chartConcave = new LineChart(pointsConcave);



            lenFac785 = float.Parse((string)jdata["len0785"]);
            lenFac885 = float.Parse((string)jdata["len0885"]);

            inited = true;
        }

        public static void Calc(float clampfac, float curvature)
        {
            float t = (clampfac - 0.785f) / (0.885f - 0.785f);

            float ratio785 = chart0d785.Sample(curvature);
            float ratio885 = chart0d885.Sample(curvature);

            float ratio = Mathf.Lerp(ratio785, ratio885, t);
        }
    }
}





#if UNITY_EDITOR
[CustomEditor(typeof(EllipseArcSimulate.Segment))]
public class CustomEllipseSimSegmentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("数据获取开始"))
        {
            (target as EllipseArcSimulate.Segment).UnityDesigner_FillChart();
        }

        if((target as EllipseArcSimulate.Segment).auto)
        {
            GUILayout.Label("curvature:" + (target as EllipseArcSimulate.Segment).curvature);
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("curvature ");
            (target as EllipseArcSimulate.Segment).curvature = EditorGUILayout.FloatField((target as EllipseArcSimulate.Segment).curvature);
            GUILayout.EndHorizontal();
        }
    }
}
#endif
