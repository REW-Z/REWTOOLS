using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using LitJson;

using UniRx;



// ------------------------- New Logic ---------------------------------------

public class GuideManager : MonoBehaviour
{
    //data
    private bool inited = false;
    public GuideData guideData;
    public GuideInfo[] guides;


    //temp  
    public GuideInfo currentGuide = null;
    public Queue<SoftGuideStep> steps = new Queue<SoftGuideStep>();

    //singleton
    private static GuideManager inst = null;
    public static GuideManager Instance
    {
        get
        {
            //Get Or Create
            if (inst == null)
            {
                inst = FindObjectOfType<GuideManager>();
            }
            if (inst == null)
            {
                inst = CreateInstance();
            }

            //AUTO INIT  
            if(inst != null && !inst.inited)
                inst.Init();
            
            return inst;
        }
    }
    public static GuideManager CreateInstance()
    {
        if (inst != null) return inst;
        if (FindObjectOfType<GuideManager>() != null) return (FindObjectOfType<GuideManager>());


        GameObject obj = new GameObject("GuideManager");
        DontDestroyOnLoad(obj);

        inst = obj.AddComponent<GuideManager>();
        return inst;
    }



    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }



    public void Init()
    {
        guides = Deserialize();
        guideData = StandaloneData.Read<GuideData>(); if (guideData == null) { guideData = new GuideData(); }
    }


    public static GuideInfo[] Deserialize() //硬编码Or反序列化  
    {
        return GuideDeserializer.Deserialize();


        //return new GuideInfo[] {
        //    new GuideInfo(){
        //        id = "test",
        //        conditions = new System.Func<bool>[]{
        //            () => FindObjectOfType<MainMenuCanvas>() != null
        //        },
        //        steps = new SoftGuideStep[]{
        //            new SoftGuideStep(){ text = "测试", focus = "MainMenuCanvas/Windows/WindowMain/ButtonStartGame", cloneButton = false, tapAnyPosition = false }
        //        }
        //    }
        //};
    }


    public void Check()
    {
        if (steps != null && steps.Count > 0) return;

        UIGuideMask guideMask = GameObject.FindObjectOfType<UIGuideMask>();
        if (guideMask == null) {  return; }

        GuideInfo validGuide = null;
        foreach (var guide in guides)
        {
            if (guide.conditions.All(c => { return c() == true; }))//condition ?? 
            {
                if (!guideData.marks.Contains(guide.id)) // finished ??  
                {
                    validGuide = guide;
                    break;
                }
            }
        }
        if (validGuide != null)
        {
            StartGuide(validGuide);
        }
        else
        {
            //Debug.LogAssertion("No Valid Guide");
        }
    }


    public void StartGuide(GuideInfo newGuide)
    {
        if (currentGuide != null) return; //已经有正在进行的指引了

        this.currentGuide = newGuide;
        this.steps = new Queue<SoftGuideStep>(currentGuide.steps);


        UIGuideMask guideMask = GameObject.FindObjectOfType<UIGuideMask>();
        if (guideMask == null) return;

        SoftGuideStep firstStep = steps.Dequeue();
        if (firstStep == null) return;


        DoStep(firstStep);
    }

    public void NextStep()
    {
        if (currentGuide == null || steps == null) return;

        //CHECK NEXT
        if (steps.Count < 1)
        {
            //Finish  
            FinishGuide();
            return;
        }
        SoftGuideStep currentStep = steps.Dequeue();


        //GET MASK  
        UIGuideMask guideMask = GameObject.FindObjectOfType<UIGuideMask>();
        if (guideMask == null) return;


        DoStep(currentStep);
    }

    public void FinishGuide()
    {
        if (currentGuide == null) return;

        //Mark As Guided  
        this.steps.Clear();
        guideData.marks.Add(currentGuide.id);
        StandaloneData.Save(guideData);

        this.steps.Clear();
        this.currentGuide = null;
    }

    public void DoStep(SoftGuideStep step)
    {
        UIGuideMask guideMask = GameObject.FindObjectOfType<UIGuideMask>();
        if (guideMask == null) return;

        //clone Button  
        if(step.cloneButton)
        {
            //WIP
        }
        else
        {
            //tap any position ??  
            float radius = 100; 
            //TODO: Auto Fit Radiuis

            Transform focusTarget = UtilsScene.Find(step.focus);
            RectTransform focusRectt = focusTarget != null ? focusTarget.GetComponent<RectTransform>() : null;

            if(focusRectt != null)
            {
                //是否在Scroll中并且scroll为true(是则滑动到)    
                ScrollRect scroll = focusRectt.GetComponentInParent<ScrollRect>();
                if (scroll != null && step.scroll == true)
                {
                    scroll.ScrollToElement(focusRectt, true);
                }


                //显示文本  
                guideMask.ShowText(step.text);
                //强指引激活  
                guideMask.MaskTarget(focusRectt, true, radius, step.tapAnyPosition, () => {
                    NextStep();
                });
            }
            else
            {
                Debug.LogAssertion("找不到引导对象！");
            }
        }

    }
}

public class GuideInfo
{
    public string id;

    public System.Func<bool>[] conditions; //lvl  //mainline  //other  

    public SoftGuideStep[] steps;
}

public class SoftGuideStep
{
    public string text;

    public string focus;

    public bool scroll;

    public bool cloneButton; //默认false

    public bool tapAnyPosition;
}




public class GuideDeserializer
{
    public static GuideInfo[] Deserialize()
    {
        string json = Resources.Load<TextAsset>("Json/Guides").text;

        JsonData jGuides = JsonMapper.ToObject(json)["guides"];

        GuideInfo[] infos = new GuideInfo[jGuides.Count];

        for (int i = 0; i < jGuides.Count; ++i)
        {
            infos[i] = new GuideInfo();
            infos[i].id = (string)jGuides[i]["id"];

            //CONDITIONS  
            var conditionStrArr = UtilsLitJson.GetArray<string>(jGuides[i]["conditions"]);
            infos[i].conditions = new System.Func<bool>[conditionStrArr.Length + 1];
            for (int j = 0; j < conditionStrArr.Length; ++j)
            {
                infos[i].conditions[j] = GuideDeserializer.ConvertCondition(conditionStrArr[j]);
            }
            infos[i].conditions[infos[i].conditions.Length - 1] = () =>
            {
                var mainMenu = GameObject.FindObjectOfType<MainMenuCanvas>();
                return mainMenu == null || (mainMenu.UnimportantWindowClear());
            };

            //STEPS  
            JsonData jSteps = jGuides[i]["steps"];
            infos[i].steps = new SoftGuideStep[jSteps.Count];
            for (int j = 0; j < jSteps.Count; ++j)
            {
                infos[i].steps[j] = new SoftGuideStep();
                infos[i].steps[j].text = (string)jSteps[j]["text"];
                infos[i].steps[j].focus = (string)jSteps[j]["focus"];
                infos[i].steps[j].scroll = (bool)jSteps[j]["scroll"];
                infos[i].steps[j].cloneButton = (bool)jSteps[j]["cloneButton"];
                infos[i].steps[j].tapAnyPosition = (bool)jSteps[j]["tapAnyPosition"];
            }
        }

        return infos;
    }

    public static System.Func<bool> ConvertCondition(string conditionStr)
    {
        string[] arr = conditionStr.Split(':');
        switch (arr[0])
        {
            case "lvl":
                {
                    int lvlNeed = int.Parse(arr[1]); //Debug.LogAssertion("Needlvl " + arr[1]);
                    return () => Infomanager.Instance.userdata.accountLvl >= lvlNeed;
                }
            case "mainline":
                {
                    int mainlineNeed = int.Parse(arr[1]); //Debug.LogAssertion("Need Mainline " + arr[1]);
                    return () => Infomanager.Instance.userdata.unlockedLevels.Contains(mainlineNeed);
                }
            case "active":
                {
                    string path = arr[1]; //Debug.LogAssertion("Need Active " + arr[1]);
                    return () =>
                    {
                        var node = UtilsScene.Find(path);
                        if (node != null)
                        {
                            if(node.GetComponent<UnlockableFunctionBase>() != null)
                            {
                                return (node.GetComponent<UnlockableFunctionBase>().IsFunctionActive());
                            }
                            else
                            {
                                return (node.gameObject.activeSelf);
                            }
                        }
                        return false;
                    };
                }
        }

        return () => false;
    }
}