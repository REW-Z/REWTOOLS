
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//using LitJson;
//using DG.Tweening;


//public enum GuideStepType
//{
//    Simple, //GetChild (i) at eleList[i].
//    Slide, //GetChild (0) slide between first and last eleList.
//}

//[System.Serializable]
//public class GuideStep
//{
//    public GuideStepType type;

//    public string focus;

//    public string guideText;

//    public bool tapToClose;

//    public Transform[] elements;

//    public Vector2Int[] elementReplaces;
//}


//public class StandaloneGuidesDataSerializer
//{
//    public const string prefName = "GuidedInfos";

//    private static List<string> guidedInfoList = null;


//    public static void GenList()
//    {
//        var strGuidedList = PlayerPrefs.GetString(prefName, "");
//        if (strGuidedList != "")
//        {
//            guidedInfoList = JsonMapper.ToObject<string[]>(strGuidedList).ToList();
//        }
//        else
//        {
//            guidedInfoList = new List<string>();
//        }
//    }

//    public static void AddGuidedInfo(string info)
//    {
//        if(guidedInfoList == null)
//        {
//            GenList();
//        }
//        guidedInfoList.Add(info);

//        Debug.Log("指引标记完成:" + info);

//        SaveData();
//    }

//    public static void RemoveGuidedInfo(string info)
//    {
//        if (guidedInfoList == null)
//        {
//            GenList();
//        }
//        if(guidedInfoList.Contains(info))
//        {
//            guidedInfoList.Remove(info);
//            SaveData();
//        }
//    }

//    public static bool CheckGuidedInfo(string info)
//    {
//        if (guidedInfoList == null)
//        {
//            GenList();
//        }

//        return guidedInfoList.Contains(info);
//    }


//    //Other
//    public static void SaveData()
//    {
//        if (guidedInfoList == null) return;

//        string json = JsonMapper.ToJson(guidedInfoList.ToArray());
//        PlayerPrefs.SetString(prefName, json);
//    }
//    public static void DeleteInfos()
//    {
//        if (PlayerPrefs.HasKey(prefName))
//        {
//            PlayerPrefs.DeleteKey(prefName);
//        }
//    }
//}


//public class SoftGuide : MonoBehaviour
//{
//    //serializable
//    [Header("引导延迟")]
//    public int delaySec;

//    [Header("点击判定半径")]
//    public float validateRadius = 99999;

//    [Header("锁定时间")]
//    public float lockTime = 2f;
    
//    [Header("最大存在时间")]
//    public float maxExistTime = 30f;

//    [Header("下一指引")]
//    public string nextGuide;

//#if SURVIVOR
//    [Header("暂停时间")]
//    public float pauseTime = 0f;
//    [Header("结束调用")]
//    public string endInvoke;
//#endif

//    [Header("引导步骤")]
//    public GuideStep[] guideSteps;


//    //coms
//    private CanvasScaler canvasScaler;

//    //status
//    private int currentGuideIdx = -1;

//    private bool startedGuide = false;      public bool StartedGuide => this.startedGuide;
//    private bool guided = false;            public bool Guided => this.guided;
//    private bool isEnd = false;


//    /// <summary>
//    /// Awake 初始化、检查问题
//    /// </summary>
//    void Awake()
//    {
//        canvasScaler = this.GetComponentInParent<CanvasScaler>();

//        CheckStepLength();
//    }


//    /// <summary>
//    /// Enable时自动开始引导
//    /// </summary>
//    private void OnEnable()
//    {

//        if (guided)
//        {
//            EndGuide();
//            return;
//        }
//        if (IsGuided())
//        {
//            //Debug.Log("找到了已引导信息");
//            EndGuide();
//            return;
//        }
//        else
//        {
//            //Debug.Log("未找到已引导信息");
//        }

//        //Default Status
//        AutoShowHide();


//        //Debug.LogAssertion("指引准备激活：" + this.gameObject.name);

//        //Invoke
//        Invoke("Next", delaySec);
//    }


//    /// <summary>
//    /// Update 判断点击事件
//    /// </summary>
//    void Update()
//    {
//        //未开始
//        if (currentGuideIdx < 0) return;


//        //可以点击结束步骤  
//        if (!canClick) { return; }
//        if(currentGuideIdx > -1 && guideSteps[currentGuideIdx].tapToClose)
//        {
//            //PC
//            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0))
//            {
//                if (ValidateScreenPos(Input.mousePosition))
//                {
//                    Next();
//                }
//            }
//            //Mobile
//            if (Input.touchCount == 1 && (Input.touches[0].phase == TouchPhase.Began || Input.touches[0].phase == TouchPhase.Ended))
//            {
//                if (ValidateScreenPos(Input.touches[0].position))
//                    Next();
//            }
//        }
//    }


//    /// <summary>
//    ///是已经完成的引导    
//    /// </summary>
//    /// <returns></returns>
//    public bool IsGuided()
//    {
//        if (finishedMarks.Contains(this.gameObject.name)) return true;

//        return StandaloneGuidesDataSerializer.CheckGuidedInfo(this.gameObject.name);
//    }

//    /// <summary>
//    /// 强制设置为未完成  
//    /// </summary>
//    public void ForceSetUnguided()
//    {
//        if (finishedMarks.Contains(this.gameObject.name))
//            finishedMarks.Remove(this.gameObject.name);

//        StandaloneGuidesDataSerializer.RemoveGuidedInfo(this.gameObject.name);
//    }




//    /// <summary>
//    /// 验证判定位置
//    /// </summary>
//    /// <param name="clickScreenPos"></param>
//    /// <returns></returns>
//    private bool ValidateScreenPos(Vector2 clickScreenPos)
//    {
//        Camera cam = Camera.current;
//        if (Camera.current == null) cam = Camera.main;
//        if (cam == null) return true;

//        //radius
//        float trueRadius = this.guideSteps[currentGuideIdx].type == GuideStepType.Slide ? 99999f : validateRadius;


//        //position
//        Vector2 fingerCanvasPos = new Vector2();
//        Vector2 fingerScreenPos = new Vector2();
//        switch (this.guideSteps[currentGuideIdx].type)
//        {
//            case GuideStepType.Simple:
//                fingerScreenPos = cam.WorldToScreenPoint(this.transform.GetChild(currentGuideIdx).GetChild(0).position);
//                fingerCanvasPos = UtilsScreenAndUI.ScreenPointToCanvasPos_MatchWidth(fingerScreenPos ,    this.canvasScaler ); //第一个图像位置
//                break;
//            default:
//                break;
//        }

//        Vector2 clickCanvasPos = UtilsScreenAndUI.ScreenPointToCanvasPos_MatchWidth(clickScreenPos, this.canvasScaler); //第一个图像位置

        

//        //validate (only simple)
//        if(this.canvasScaler.GetComponent<Canvas>().renderMode == RenderMode.WorldSpace)
//        {
//            if ((fingerCanvasPos - clickCanvasPos).sqrMagnitude < (trueRadius * trueRadius))
//                return true;
//            else
//                return false;
//        }
//        else
//        {
//            if ((fingerCanvasPos - clickCanvasPos).sqrMagnitude < (trueRadius * trueRadius))
//                return true;
//            else
//                return false;
//        }
        
//        return true;
//    }
    



//    /// <summary>
//    /// 检查引导数是否匹配
//    /// </summary>
//    private void CheckStepLength()
//    {
//        if (guideSteps.Length != this.transform.childCount)
//            throw new UnityException("引导不匹配");
//    }
    


//    /// <summary>
//    /// 自动隐藏其他步骤的引导
//    /// </summary>
//    private void AutoShowHide()
//    {
//        for (int i = 0; i < this.transform.childCount; i++)
//        {
//            if (currentGuideIdx == i)
//                this.transform.GetChild(i).gameObject.SetActive(true);
//            else
//                this.transform.GetChild(i).gameObject.SetActive(false);
//        }
//    }


//    /// <summary>
//    /// 下一个引导
//    /// </summary>
//    private void Next()
//    {
//        if (currentGuideIdx >= guideSteps.Length) return;
//        if (guided) return;

//        //开始第一步引导  
//        if (currentGuideIdx == -1)//First Step  
//        {
//            if(pauseTime > 0.1f)
//            {
//                Game.TryPause("Guide", pauseTime);
//            }

//            //LOG
//            //Debug.LogAssertion("指引已激活：" + this.gameObject.name);
//        }


//#if SURVIVOR
//        //黑边默认隐藏    
//        FindObjectOfType<UISoftGuideCurtain>().Hide();   
//#endif


//        //!! 引导互斥 -- Only Exist One
//        foreach(var guide in this.transform.parent.GetComponentsInChildren<SoftGuide>(true))
//        {
//            if (guide != this && guide.startedGuide) // is guiding ???
//            {
//                guide.EndGuide();
//            }
//        }


//        //Reset TImer
//        ResetLock();

//        //Start(First)
//        if (!startedGuide)
//            startedGuide = true;
//        //NExt
//        currentGuideIdx += 1;

//        //Auto ShowHide
//        AutoShowHide();




//        //Show Next Guide
//        if (currentGuideIdx < guideSteps.Length)
//        {
//            switch (guideSteps[currentGuideIdx].type)
//            {
//                case GuideStepType.Simple:
//                    {
//                        for (int i = 0; i < this.transform.GetChild(currentGuideIdx).childCount; i++)
//                        {
//                            if (i < this.guideSteps[currentGuideIdx].elements.Length)
//                            {
//                                var pointer = this.transform.GetChild(currentGuideIdx).GetChild(i).GetComponent<RectTransform>();
//                                //var target = JudgeElementAnchorPos(guideSteps[currentGuideIdx], i, canvasScaler);
//                                //pointer.anchoredPosition = target;

//                                pointer.transform.position = guideSteps[currentGuideIdx].elements[i].position;

//#if SURVIVOR
//                                //黑边  
//                                FindObjectOfType<UISoftGuideCurtain>().Focus(guideSteps[currentGuideIdx].elements[0], Vector2.one * 250f, guideSteps[currentGuideIdx].guideText);
//#endif
//                            }
//                            else
//                            {
//                                //停留原位置
//                            }
//                        }
//                    }
//                    break;
//                case GuideStepType.Slide:
//                    {
//                        for (int i = 0; i < this.transform.GetChild(currentGuideIdx).childCount; i++)
//                        {
//                            if(i == 0) //第一个为滑动元素
//                            {
//                                var slider = this.transform.GetChild(currentGuideIdx).GetChild(i).GetComponent<RectTransform>();
//                                //var target1 = JudgeElementAnchorPos(guideSteps[currentGuideIdx], 0, canvasScaler);
//                                //var target2 = JudgeElementAnchorPos(guideSteps[currentGuideIdx], guideSteps[currentGuideIdx].elements.Length - 1, canvasScaler);

//                                //slider.transform.DOComplete();
//                                //slider.anchoredPosition = target1;
//                                //slider.DOAnchorPos(target2, 1f).SetLoops(-1);

//                                var pos1 = guideSteps[currentGuideIdx].elements.First().position;
//                                var pos2 = guideSteps[currentGuideIdx].elements.Last().position;

//                                slider.transform.DOComplete();
//                                slider.transform.position = pos1;
//                                slider.transform.DOMove(pos2, 1f).SetLoops(-1);
//#if SURVIVOR
//                                //黑边  
//                                FindObjectOfType<UISoftGuideCurtain>().Focus(slider, Vector2.one * 250f, guideSteps[currentGuideIdx].guideText);
//#endif
//                            }
//                            else//其余为固定元素
//                            {
//                                if (i < guideSteps[currentGuideIdx].elements.Length - 1) //-1:忽略最后一个（最后一个是滑动元素的末位置）
//                                {
//                                    var pointer = this.transform.GetChild(currentGuideIdx).GetChild(i).GetComponent<RectTransform>();
//                                    var target = JudgeElementAnchorPos(guideSteps[currentGuideIdx], i, canvasScaler);
//                                    pointer.anchoredPosition = target;
//                                }
//                                else
//                                {
//                                    //停留原来位置
//                                }
//                            }
//                        }
//                    }
//                    break;
//            }


//            //auto next
//            AutoNext();
//        }
//        //No Guide
//        else
//        {
//            StandaloneGuidesDataSerializer.AddGuidedInfo(this.gameObject.name);
//            finishedMarks.Add(this.gameObject.name);
//            guided = true;
//            EndGuide();
//        }
//    }


//    /// <summary>
//    /// 结束引导
//    /// </summary>
//    private void EndGuide()
//    {
//        if (isEnd) return;
//        isEnd = true;

//        //Debug.LogAssertion("指引结束：" + this.gameObject.name);

//        this.gameObject.SetActive(false);


//#if SURVIVOR
//        if(!string.IsNullOrEmpty(endInvoke))
//        {
//            UtilsReflection.Call(endInvoke);
//        }
//#endif


//#if SURVIVOR
//        FindObjectOfType<UISoftGuideCurtain>().Hide();
//#endif

//        //埋点  
//        MaiDian.Mai("number", this.gameObject.name, "version", Infomanager.MaiDianVersion, "guide");



//        if (!string.IsNullOrEmpty(this.nextGuide))
//        {
//            SoftGuide.EnableGuide(this.nextGuide);
//        }
//    }

//    /// <summary>
//    /// 防止连点
//    /// </summary>
//    private bool canClick = false;
//    private void ResetLock()
//    {
//        canClick = false;
//        CancelInvoke("LockEnd");
//        Invoke("LockEnd", lockTime);
//    }
//    private void LockEnd()
//    {
//        canClick = true;
//    }

//    //超时自动到下一步（调用Next后触发）
//    private void AutoNext()
//    {
//        CancelInvoke("Next");
//        Invoke("Next", maxExistTime);
//    }




//    // -------- Finish Marks Temp ---------------------  
//    private static List<string> finishedMarks = new List<string>();
//    // --------(Static)Enable Guide --------------------------------
//    public static void EnableGuide(string guideName)
//    {
//        if(Game.Instance != null && Game.Instance.isLevelZero)
//        {
//            return;
//        }

//        if (finishedMarks.Contains(guideName))
//        {
//            return;
//        }

//        if (StandaloneGuidesDataSerializer.CheckGuidedInfo(guideName))
//        {
//            SoftGuide.finishedMarks.Add(guideName);
//            return;
//        }


//        var softguideList = UtilsScene.GetComponentsInScene<SoftGuide>(true).Where(s => s.gameObject != null).ToList();


//        //find target
//        var targetGuide = softguideList.FirstOrDefault(g => g.gameObject.name == guideName);
//        if (targetGuide == null)
//        {
//            return;
//        }


//        //enable
//        targetGuide.gameObject.SetActive(true);
//    }
//    public static void FinishGuide(string guideName)
//    {
//        if (Game.Instance != null && Game.Instance.isLevelZero)
//        {
//            return;
//        }
//        if (finishedMarks.Contains(guideName))
//        {
//            return;
//        }
//        if (StandaloneGuidesDataSerializer.CheckGuidedInfo(guideName))
//        {
//            SoftGuide.finishedMarks.Add(guideName);
//            return;
//        }


//        var softguideList = UtilsScene.GetComponentsInScene<SoftGuide>(true).Where(s => s.gameObject != null).ToList();

//        //find target
//        var targetGuide = softguideList.FirstOrDefault(g => g.gameObject.name == guideName);
//        if (targetGuide == null) return;

//        //Finish  
//        StandaloneGuidesDataSerializer.AddGuidedInfo(targetGuide.gameObject.name);
//        targetGuide.guided = true;
//        targetGuide.EndGuide();
//    }
//    public static bool IsFinished(string guideName)
//    {
//        if (finishedMarks.Contains(guideName)) return true;

//        return StandaloneGuidesDataSerializer.CheckGuidedInfo(guideName);
//    }


//    // --------(Static)(ApplicationCustom) DynamicGetPos ------------
//    private static System.Func<Vector2>[] presets = null; 
//    public static void HardCodeDynamicGetPosPreset()
//    {
//        presets = new System.Func<Vector2>[0];

//        //presets = new System.Func<Vector2>[2];

//        //presets[0] = () =>
//        //{
//        //    var scaler = FindObjectOfType<UIArenaCanvas>().GetComponent<CanvasScaler>();
//        //    var cell = FindObjectsOfType<ArenaGridCell>().OrderByDescending(c => c.State).FirstOrDefault(c => c.team == 0);
//        //    return GetAnchorPosition(cell.transform, scaler);
//        //};
//        //presets[1] = () =>
//        //{
//        //    var scaler = FindObjectOfType<UIArenaCanvas>().GetComponent<CanvasScaler>();
//        //    var cell = FindObjectsOfType<ArenaGridCell>().OrderByDescending(c => c.State).LastOrDefault(c => c.team == 0);
//        //    return GetAnchorPosition(cell.transform, scaler);
//        //};
//    }
//    public static Vector2 DynamicGetAnchorPos(int presetIdx)
//    {
//        if (presets == null) HardCodeDynamicGetPosPreset();


//        if (presets.Length > presetIdx)
//            return presets[presetIdx]();
//        else
//            return new Vector2();
//    }



//    // ----------------- Utils -----------------------
//    private static Vector2 JudgeElementAnchorPos(GuideStep step, int idx, CanvasScaler scaler)
//    {
//        if(idx > -1 && idx < step.elements.Length)
//        {
//            Transform element = step.elements[idx];
//            Vector2Int replace = step.elementReplaces.FirstOrDefault(r => r.x == idx);

//            if (element != null)
//                return GetAnchorPosition(step.elements[idx].transform, scaler);
//            if (replace != null)
//                return DynamicGetAnchorPos(replace.y);
//            else
//                return new Vector2();
//        }

//        return default;
//    }

//    private static Vector2 GetAnchorPosition(Transform trans, CanvasScaler canvasScaler)
//    {
//        if (trans is RectTransform)
//        {
//            var screenPos = UtilsScreenAndUI.WorldToScreenPoint(trans.position);
//            return UtilsScreenAndUI.ScreenPointToCanvasPos_MatchWidth(screenPos, canvasScaler);
//        }
//        else
//        {
//            return UtilsScreenAndUI.ScreenPointToCanvasPos_MatchWidth(UtilsScreenAndUI.WorldToScreenPoint(trans.position), canvasScaler);
//        }
//    }






//#if UNITY_EDITOR
    
//#endif

//}




//#if UNITY_EDITOR
//[UnityEditor.CustomEditor(typeof(SoftGuide))]
//public class CustomSoftGuide : UnityEditor.Editor
//{
//    private static GUIStyle commentStyle = null;

//    public override void OnInspectorGUI()
//    {
//        if(commentStyle == null)
//        {
//            commentStyle = new GUIStyle();
//            commentStyle.normal.textColor = new Color(0, 0.5f, 0, 0.8f);
//        }

//        GUILayout.Label("新手引导系统"
//            + "\n//软引导"
//            + "\n//Elements:引导元素，可以是场景内的Transform也可以是UI上的RecttTransform"
//            + "\n//Replace：引导元素不确定，改为通过硬编码的函数动态获取"
//            + "\n//Type: 类型。可以是固定和滑动的。"
//            + "\n//激活方式1：自动Enable"
//            + "\n//激活方式2：其他物体挂Enabler脚本"
//            + "\n//激活方式3：调用SoftGuide.EnableGuide(string guideName)静态方法"
//            + "\n//激活方式4：下一指引NextGuide字段填写"
//            , commentStyle);

//        base.OnInspectorGUI();
//    }
//}

//#endif