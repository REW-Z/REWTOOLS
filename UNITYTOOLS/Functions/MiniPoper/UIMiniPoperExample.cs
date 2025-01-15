
#if ZTOOLS_UIMINIPOPER
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public enum MiniPopType
{
    金币不够,
    道具不足,
    重复道具,
    兑换失败,
    兑换成功,
    任务完成,
    最大进化,
    获取道具,
    其他,
}

public class UIMiniPoper : MonoBehaviour
{
    private static UIMiniPoper instance = null;
    private Transform popPrefabContainer;

    private List<GameObject> displayingList = new List<GameObject>();


    void Start()
    {
        instance = this;
        popPrefabContainer = this.transform.Find("Container");
    }

    void Update()
    {

    }


    public static void Pop(MiniPopType popType, object b = null, string msg = "")
    {
        if (instance == null) return;

        GameObject newPoper = null;
        switch (popType)
        {
            case MiniPopType.金币不够:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("NotEnoughMoney").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.道具不足:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("NotEnoughItem").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.重复道具:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("RepeatItem").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.兑换失败:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("ExchangeNo").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.兑换成功:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("ExchangeYes").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.任务完成:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("MissionFinished").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.最大进化:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("MaxPromote").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                }
                break;
            case MiniPopType.获取道具:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("Got").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);
                    //...
                }
                break;
            case MiniPopType.其他:
                {
                    GameObject prefab = instance.popPrefabContainer.Find("Other").gameObject;
                    newPoper = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation, instance.transform);

                    newPoper.GetComponentInChildren<Text>().text = msg;
                }
                break;
            default:
                break;
        }


        //Add To DisplayingList
        instance.displayingList.Add(newPoper);

        //Recttransform
        var rectt = newPoper.GetComponent<RectTransform>();
        foreach (var obj in instance.displayingList)
        {
            obj.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, rectt.sizeDelta.y + 20);//padding = 20
        }

        //Show And Fade Anim
        rectt.DOBlendableLocalMoveBy(Vector3.zero, 0.5f).OnComplete(() => {

            foreach (Image img in newPoper.GetComponentsInChildren<Image>(true))
            {
                img.DOColor(new Color(1, 1, 1, 0), 2.75f);
            }
            foreach (Text txt in newPoper.GetComponentsInChildren<Text>(true))
            {
                txt.DOColor(new Color(1, 1, 1, 0), 2.75f);
            }
            rectt.DOBlendableLocalMoveBy(new Vector3(0, 200, 0), 3f).OnComplete(() => {

                instance.displayingList.Remove(newPoper);
                Destroy(newPoper);

            });
        });
    }

    public static void PopText(string txt)
    {
        Pop(MiniPopType.其他, null, txt);
    }
}







#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(UIMiniPoper))]
public class CustomUIMiniPOPer: UnityEditor.Editor
{
    private GUIStyle commentStyle = new GUIStyle() ;
    public void OnEnable()
    {
        Debug.Log("OnEnable");
        commentStyle.normal.textColor = Color.green;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.TextArea("Container设置为Inactive\n下面所有子节点设置为Active!", commentStyle);
    }
}
#endif


#endif