using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//using DG.Tweening;




#if ENABLE_INGAMEGUIDER

public class GuideInfo
{
    public int id;
    public System.Func<bool> whenToOpen;
    public System.Func<bool> whenToClose;

    public System.Action opencallback;
    public System.Action closecallback;

    public bool useDefaultImage;

    public int radius;

    public string guideTxt;

    public RectTransform fingerTarget;

    public Vector2 fingerMovement;



    public GuideInfo()
    {
        opencallback = () => { };
        closecallback = () => { };

        fingerTarget = null;
        fingerMovement = new Vector2();
    }
}


public class InGameGuider : MonoBehaviour
{
    private static InGameGuider inst;


    //guide
    private List<GuideInfo> guideInfos;
    private static int currentGuideId;
    private static int[] isGuided;


    //dest gizmos
    private static Vector3[] destinationPos;
    private static int currentDestIndex;

    //player
    private GameObject player;


    //obj
    private UICanvas uicanvas;
    private GameObject destinationGizmos;

    public GameObject fingerAnim = null;

    //del
    private System.Func<bool> whenToCloseGuide;
    private System.Func<bool> whenToOpenGuide;


    //time tmp
    private float timer = 0f;
    private int frameCount = 0;

    //isShowing
    private bool isShowing = false;

    //------finger tmp------
    private Sequence seq = null;

    //-------tmp-------
    //...



    private void Awake()
    {
        //硬编码
        inst = this;
        isGuided = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        destinationPos = new Vector3[3] { new Vector3(0f, 0f, 100f), new Vector3(0f, 0f, 200f), new Vector3(100f, 0f, 300f) };

        currentGuideId = 0;
        currentDestIndex = 0;
    }

    private void Start()
    {
        //destinationGizmos = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Destination"), new Vector3(99999f, 100f, 99999f), new Quaternion());
        //destinationGizmos.SetActive(false);


#region GUIDES
        
        //Guides
        {
            GuideInfo info0 = new GuideInfo();
            info0.id = 0;
            info0.whenToOpen = (() => {
                return false;
            });
            info0.whenToClose = () => {
                return false;
            };
            info0.opencallback = (() => {
                
            });

            info0.guideTxt = "你好，欢迎来到战场！";


            guideInfos.Add(info0);
        }
        
#endregion

        
        


        //init
        whenToOpenGuide = guideInfos.Find(g => g.id == currentGuideId).whenToOpen;
    }


    private void Update()
    {
        //---
        timer -= Time.deltaTime;
        //---
        frameCount++;
        //---

        //inst.whenToOpenGuide();

        if (frameCount % 5 == 0)
        {
            if (whenToOpenGuide != null && whenToOpenGuide())
            {
                Guide();
            }


            if (whenToCloseGuide != null && whenToCloseGuide())
            {
                HideGuideStuff();
                whenToCloseGuide = (() => { return false; });
            }
        }
    }


    public static void Guide(float delay)
    {
        inst.StartCoroutine(inst.CoGuide(delay));
    }

    private IEnumerator CoGuide(float delay)
    {
        yield return new WaitForSeconds(delay);

        Guide();
    }

    public static void Guide()
    {

        GuideInfo info = inst.guideInfos.Find(g => g.id == currentGuideId);


        inst.ShowGuideStuff(info.guideTxt, info.fingerTarget, info.fingerMovement, -1f);

        isGuided[currentGuideId] = 1;

        inst.whenToCloseGuide = info.whenToClose;

        if (info.opencallback != null)
        {
            info.opencallback();
        }
        else
        {
        }


        //++++
        currentGuideId++;//!!!!!!!!!!!!!!

        if (inst.guideInfos.FirstOrDefault(g => g.id == currentGuideId) != null)
        {
            inst.whenToOpenGuide = inst.guideInfos.Find(g => g.id == currentGuideId).whenToOpen;

        }
        else
        {
            inst.whenToOpenGuide = () => { return false; };
        }

    }


    private void ShowGuideStuff(string txt, RectTransform fingerTarget, Vector2 fingerMovement, float showTime = -1f)
    {
        if (isShowing) HideGuideStuff();

        isShowing = true;

        //show
        uicanvas.transform.Find("Guider").gameObject.SetActive(true);
        uicanvas.transform.Find("Guider").GetComponentInChildren<Text>().text = txt;


        //finger anim
        if (fingerTarget != null)
        {
            if (seq != null) seq.Kill();

            RectTransform finger = uicanvas.transform.Find("Finger").GetComponent<RectTransform>();

            finger.gameObject.SetActive(true);

            finger.position = fingerTarget.position;

            Vector2 anchorPosTmp = finger.anchoredPosition;

            //finger.DOAnchorPos(finger.anchoredPosition + fingerMovement, 1f).OnStart(() =>
            //{
            //    finger.GetChild(0).gameObject.SetActive(false);
            //    finger.GetChild(1).gameObject.SetActive(true);
            //}).OnComplete(() =>
            //{
            //    finger.GetChild(0).gameObject.SetActive(true);
            //    finger.GetChild(1).gameObject.SetActive(false);
            //}).SetLoops(-1);



            seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                finger.anchoredPosition = anchorPosTmp;
                finger.GetChild(0).gameObject.SetActive(true);
                finger.GetChild(1).gameObject.SetActive(false);
            }).AppendInterval(
                1f
            ).AppendCallback(() =>
            {
                finger.DOAnchorPos(anchorPosTmp + fingerMovement, 1f);
                finger.GetChild(0).gameObject.SetActive(false);
                finger.GetChild(1).gameObject.SetActive(true);
            }).AppendInterval(
                1.2f
            ).SetLoops(-1);

        }

        //auto hide
        if (showTime > 0f)
        {
            Invoke("HideGuideStuff", showTime);
        }
    }

    private void HideGuideStuff()
    {
        isShowing = false;

        if (currentGuideId - 1 > -1)
        {
            isGuided[currentGuideId - 1] = 2;
            inst.guideInfos.Find(guideInfos => guideInfos.id == currentGuideId - 1).closecallback();
        }

        //hide
        uicanvas.transform.Find("Guider").gameObject.SetActive(false);

        //fingetAnim
        uicanvas.transform.Find("Finger").GetComponent<RectTransform>().DOKill();
        uicanvas.transform.Find("Finger").gameObject.SetActive(false);


        //TimeScale
        //...
    }


    
}
#endif