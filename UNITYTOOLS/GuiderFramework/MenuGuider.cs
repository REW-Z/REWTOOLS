using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if ENABLE_MENUGUIDER

class MenuGuideInfo
{
    public int id;

    public System.Func<bool> whenToOpen;

    public System.Action openCallback;

    public bool tapAnyPlaceToClose;
    public RectTransform focus;

    public System.Action closeCallback;

    public string txt;

    public float duration;

    //自定义回调
    public System.Action btnCustomOnClick = null;

    public float bgDarkness = 0.65f;
}

public class MenuGuider : MonoBehaviour
{
    private static MenuGuider inst;

    private int currentId;

    private static int[] isGuided = new int[10];//!!!!!!!!!!!!Link To Infomanager.Userdate

    private List<MenuGuideInfo> guides;



    //temp
    private float timer = 0f;


    //pointers
    private MainMenuCanvas menuCanvas;

    private GameObject mask;
    private GameObject obtn;
    private GameObject finger;
    private Transform t2Follow;

    private void Awake()
    {
        Debug.Log("Awake");

        //isGuided = InfoManager.GetInstance().userData.guideProgress;//!!!!!!!!!!!!Link To Infomanager.Userdate

        inst = this;

        inst.menuCanvas = MonoBehaviour.FindObjectOfType<MainMenuCanvas>();
        inst.mask = this.transform.Find("GUIDEMASK").gameObject;
        inst.obtn = inst.mask.GetComponentInChildren<Button>().gameObject;
        inst.finger = inst.mask.transform.Find("GuidePress").gameObject;

        //guides init
        guides = new List<MenuGuideInfo>();

        //------------GUIDE示例---------------
        {
            MenuGuideInfo guide = new MenuGuideInfo();
            guide.id = 0;
            guide.txt = "点击这里进入教程关卡";
            guide.whenToOpen = () => { return true; };
            guide.openCallback = null;
            guide.tapAnyPlaceToClose = false;
            guide.focus = this.transform.Find("Windows").Find("WindowMain").Find("AccPanel").Find("LeftPanel").Find("ButtonMissions").GetComponent<RectTransform>();
            guide.btnCustomOnClick = () => {

            };
            guide.closeCallback = null;

            guides.Add(guide);
        }
        
    }

    void Start()
    {

    }


    void Update()
    {
        //---
        timer -= Time.deltaTime;
        //---



        Guide();//!!!!!!!!!!!!Invoke In Events To Reduce Cost

        //Btn follow
        if (t2Follow != null)
        {
            obtn.transform.position = t2Follow.position;
            obtn.transform.rotation = t2Follow.rotation;

            if (finger.activeInHierarchy)
            {
                finger.transform.position = t2Follow.position;
                finger.transform.rotation = t2Follow.rotation;
            }
        }
    }




    public static void Guide()
    {
        foreach (var guide in inst.guides)
        {
            int idTmp = guide.id;

            if (guide.whenToOpen() && isGuided[guide.id] == 0)
            {


                //Acivate Guide
                inst.mask.SetActive(true);
                //BG darkness
                inst.mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, guide.bgDarkness);

                //text
                inst.mask.GetComponentInChildren<Text>().text = "<b>" + guide.txt + "</b>";

                //openCallback
                guide.openCallback?.Invoke();

                //how to close
                if (guide.tapAnyPlaceToClose)
                {
                    inst.obtn.GetComponent<Animation>().enabled = false;

                    inst.obtn.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
                    inst.obtn.GetComponent<Image>().sprite = null;

                    inst.obtn.GetComponent<RectTransform>().anchoredPosition = new Vector2();
                    inst.obtn.GetComponent<RectTransform>().anchorMin = new Vector2();
                    inst.obtn.GetComponent<RectTransform>().anchorMax = new Vector2();
                    inst.obtn.GetComponent<RectTransform>().sizeDelta = new Vector2(5000, 5000);
                    inst.obtn.transform.localScale = Vector3.one;
                    inst.t2Follow = null;

                    //finger
                    inst.finger.gameObject.SetActive(false);
                }
                else
                {
                    inst.obtn.GetComponent<Animation>().enabled = true;

                    inst.obtn.GetComponent<Image>().color = guide.focus.GetComponent<Image>().color;
                    inst.obtn.GetComponent<Image>().sprite = guide.focus.GetComponent<Image>().sprite;

                    inst.obtn.transform.SetParent(null);
                    inst.obtn.GetComponent<RectTransform>().sizeDelta = guide.focus.sizeDelta;
                    inst.obtn.GetComponent<RectTransform>().pivot = guide.focus.pivot;

                    inst.obtn.transform.localScale = guide.focus.lossyScale;
                    inst.obtn.transform.SetParent(inst.mask.transform);
                    inst.obtn.transform.SetAsFirstSibling();
                    inst.t2Follow = guide.focus;


                    //finger
                    inst.finger.SetActive(true);
                    inst.finger.GetComponent<RectTransform>().anchoredPosition = Camera.main.WorldToViewportPoint(guide.focus.transform.position) * inst.menuCanvas.GetComponent<RectTransform>().sizeDelta;
                }

                inst.mask.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                inst.mask.GetComponentInChildren<Button>().onClick.AddListener(() => {
                    //Invoke Focus Button
                    if (guide.btnCustomOnClick != null)
                    {
                        guide.btnCustomOnClick();
                    }
                    else if (!guide.tapAnyPlaceToClose)
                    {
                        guide.focus.GetComponent<Button>().onClick?.Invoke();
                    }

                    //Hide Mask
                    inst.mask.SetActive(false);
                    isGuided[idTmp] = 2;

                    //callback
                    guide.closeCallback?.Invoke();
                });


                isGuided[guide.id] = 1;
            }
        }
    }

    
}

#endif