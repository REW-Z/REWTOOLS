
#if REW_LEGACY  

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using LitJson;


public class FunctionUnlock
{
    public string name;
    public string scenePath;
    public System.Func<bool>[] conditions;


    //tmp info  
    public bool isUnlocked = false;
    private Transform node = null;
    public Transform Node()
    {
        if (node == null) node = UtilsScene.Find(scenePath);
        return node;
    }
}

public class FunctionUnlockManager
{
    public static FunctionUnlock[] functionUnlocks = null;
    public static bool isInited = false;


    private static void Init()
    {
        if (isInited) return;
        
        //Deserialize  
        Deserialize();

        //Init Unlock  
        foreach(var funcLock in functionUnlocks)
        {
            if (funcLock.conditions.All(c => c() == true))
            {
                funcLock.isUnlocked = true;
            }
            else
            {
                funcLock.isUnlocked = false;
            }
        }

        isInited = true;
    }

    private static void Deserialize()
    {
        string json = Resources.Load<TextAsset>("Json/FunctionUnlocks").text;

        JsonData jFunctions = JsonMapper.ToObject(json)["functionUnlocks"];

        FunctionUnlockManager.functionUnlocks = new FunctionUnlock[jFunctions.Count];
        for (int i = 0; i < jFunctions.Count; ++i)
        {
            functionUnlocks[i] = new FunctionUnlock();
            functionUnlocks[i].name = (string)jFunctions[i]["name"];
            functionUnlocks[i].scenePath = (string)jFunctions[i]["scenePath"];
            functionUnlocks[i].conditions = new System.Func<bool>[jFunctions[i]["conditions"].Count];
            for (int j = 0; j < jFunctions[i]["conditions"].Count; ++j)
            {
                functionUnlocks[i].conditions[j] = GuideDeserializer.ConvertCondition((string)jFunctions[i]["conditions"][j]);
            }
        }
    }

    public static void Check()
    {
        if (!isInited) Init();

        foreach(var funcUnlock in functionUnlocks)
        {
            List<FunctionUnlock> list = null;

            //未解锁
            if (!funcUnlock.isUnlocked) 
            {
                if (funcUnlock.conditions.All(c => c() == true)) //满足解锁条件
                {
                    //解锁功能  
                    funcUnlock.isUnlocked = true;
                    //if (funcUnlock.Node() != null)
                    //{
                    //    funcUnlock.Node().gameObject.SetActive(true);
                    //}
                    SetActive(funcUnlock, true);

                    // 加入提示列表  
                    if (list == null) { list = new List<FunctionUnlock>(); };
                    list.Add(funcUnlock);

                    Debug.LogAssertion("Function Active:" + funcUnlock.name);
                }
                else //不满足解锁条件
                {
                    //如果返回主菜单时UI功能处于激活，则关闭它  
                    if(IsActive(funcUnlock))
                    {
                        SetActive(funcUnlock, false);
                        Debug.LogAssertion("Function Deactive" + funcUnlock.name);
                    }
                }
            }
            //已经解锁
            else
            {
                //如果返回主菜单时功能没有激活，则激活它  
                if (!IsActive(funcUnlock))
                {
                    SetActive(funcUnlock, true);
                }
            }


            //UI显示提示窗口  
            if(list != null)
            {
                var mainMenu = GameObject.FindObjectOfType<MainMenuCanvas>();
                mainMenu.EnqueueWindow(() => { mainMenu.ShowHideWindowUnlocked(true, list); });
            }
        }
    }



    public static bool IsActive(FunctionUnlock funcUnlock)
    {
        if(funcUnlock.Node().GetComponent<UnlockableFunctionBase>() != null)
        {
            //Debug.LogAssertion(funcUnlock.name + "有Base组件"); 
            return funcUnlock.Node().GetComponent<UnlockableFunctionBase>().IsFunctionActive();
        }
        else
        {
            //Debug.LogAssertion(funcUnlock.name + "没有Base组件");
            return funcUnlock.Node().gameObject.activeSelf;
        }
    }
    public static void SetActive(FunctionUnlock funcUnlock, bool active)
    {
        //Debug.LogAssertion(funcUnlock.name + "正在激活或者关闭：" + active);
        if (funcUnlock.Node().GetComponent<UnlockableFunctionBase>() != null)
        {
            funcUnlock.Node().GetComponent<UnlockableFunctionBase>().SetFunctionActive(active);
        }
        else
        {
            funcUnlock.Node().gameObject.SetActive(active);
        }
    }

}

#endif