using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.Events;

#if CAONI

public class AdManager : MonoBehaviour
{
    public static AdManager inst = null;

    [HideInInspector] public static string strVideoAdId = "dg00afl8k0ck18x2wy";

    [HideInInspector] public static string strInterAdId = "3g567n131533gxerkp";

    [HideInInspector] public static string strBannerAdId = "di5k6k2a13gh42cens";

    private static float timer60 = 60f;
    private static float timer30 = 30f;
    private static float timer2 = 2f;
    
    public static GameObject CreateInstance()
    {
        if (inst != null)
        {
            Debug.Log("Exist Instance!");
            return inst.gameObject;
        }

        GameObject adManagerObj = new GameObject("AdManager");

        adManagerObj.AddComponent<AdManager>();

        return adManagerObj;
    }




    // --------------------------- instance fields -----------------------------

#if TOUTIAO
    private static StarkSDKSpace.StarkAdManager.BannerStyle bannerStyle = null;
    private static StarkSDKSpace.StarkAdManager.BannerAd bannerAd = null;

    private static StarkSDKSpace.StarkAdManager.InterstitialAd inteAd = null;
#endif

    // ----------------------------  EVENTS -------------------------------------
    public class AdEvent : UnityEvent<string> { }
    public static AdEvent onVideoAdSuccess = new AdEvent();
    public static AdEvent onVideoAdFail = new AdEvent();


    // ---------------------------- Settings-------------------------------------
    private static bool disabledAd = false;
    public static bool DisableAd {
        get { return disabledAd; }
        set { disabledAd = value; }
    }


    // --------------------------- Mono fields -----------------------------

    private void Awake()
    {
        inst = this;
        DontDestroyOnLoad(inst);
    }


    void Start()
    {
        //禁止销毁
        DontDestroyOnLoad(this.gameObject);

#if TOUTIAO
        //广告模块启动
        StarkSDKSpace.StarkSDK.API.GetStarkAdManager();
#endif
        timer30 = 16f;
        timer60 = 16f;
    }
    


    void Update()
    {
        //ad timer
        timer60 -= Time.deltaTime;
        timer30 -= Time.deltaTime;
        timer2 -= Time.deltaTime;
    }


    public static void VideoAd(UnityAction successCallback, UnityAction failCallback, string adPoint = "")
    {
        // ******** 游戏的广告限制 ********  
        bool valid = Infomanager.AdPoint_Check(adPoint);
        if (!valid) { UIMiniPoper.PopText("现在不能看广告"); return; }
        // ******************************  


#if UNITY_EDITOR
        if (disabledAd)
        {
            Displayer.Display("广告调用。。。(广告关闭中)");
            successCallback();
            onVideoAdSuccess.Invoke(adPoint);
        }
        else
        {
            Displayer.Display("广告调用。。。(广告开启中)");
            successCallback();
            onVideoAdSuccess.Invoke(adPoint);
        }
#else
        if (disabledAd)
        {
            Displayer.Display("广告调用。。。(广告关闭中)");
            successCallback();
            onVideoAdSuccess.Invoke(adPoint);
        }
        else
        {
            //广告逻辑
            FindObjectOfType<MiliPay>().u3dToJava_msg("99", new Callback(() => {
                successCallback();
                onVideoAdSuccess.Invoke(adPoint);
            }), new Callback(() => {
                failCallback();
                onVideoAdFail.Invoke(adPoint);
            }));
        }
#endif
    }

    public static void InterstitialOrBannerAd(UnityEngine.UI.Button.ButtonClickedEvent[] eventsToAddTo, UnityAction bannerGotCallback = null)
    {

        //插屏或者banner
        if (timer2 < 0f)
        {

            if (timer60 < 0f)
            {
#if TOUTIAO
                try
                {
                    StarkSDKSpace.StarkAdManager.IsShowLoadAdToast = false;
                    inteAd = StarkSDKSpace.StarkSDK.API.GetStarkAdManager().CreateInterstitialAd(AdManager.strInterAdId);

                    inst.StartCoroutine(inst.CoShowIntAd(inteAd, eventsToAddTo));
                }
                catch
                {

                }
                finally
                {
                    timer60 = 60f;
                }
#else
                
                    FindObjectOfType<MiliPay>().u3dToJava_msg("98", () => { }, () => { });
#endif
            }
            else
            {
#if TOUTIAO

                Debug.Log("BannerTest  -Start");

                try
                {

                    StarkSDKSpace.StarkAdManager.IsShowLoadAdToast = false;


                    //Size Set
                    bannerStyle = new StarkSDKSpace.StarkAdManager.BannerStyle();
                    bannerStyle.width = 314;
                    bannerStyle.left = 10;
                    bannerStyle.top = 100;
                    



                    //Create bannder

                    bannerAd = StarkSDKSpace.StarkSDK.API.GetStarkAdManager().CreateBannerAd(AdManager.strBannerAdId, bannerStyle, 60, 
                    (code,msg) => {
                        Debug.Log("BannerTest Err --code:" + code + " --msg:" + msg);
                    }, () => {
                        Debug.Log("BannerTest Load");

                        if (bannerGotCallback != null) bannerGotCallback();
                        inst.StartCoroutine(inst.CoSetBannerAd());
                    }, (x, y) => {
                        Debug.Log("BannerTest Resize:  --" + x + ":" + y);
                    });
                    

                    //Late Process


                }
                catch(System.Exception exc)
                {
                }
                finally
                {
                }
#else
                FindObjectOfType<MiliPay>().u3dToJava_msg("97", () => { }, () => { });
#endif
            }

            timer2 = 2f;
        }

    }

    public static void CloseInteAndBannerAd()
    {
#if TOUTIAO
        try
        {
            if(bannerAd != null)
            {
                bannerAd.Hide();
                bannerAd.Destory();
            }
            if(inteAd != null)
            {
                inteAd.Destory();
            }
        }
        catch
        {

        }
#endif
    }

    private static void MoreJingCai()
    {
        FindObjectOfType<MiliPay>().u3dToJava_msg("66", () => { }, () => { });
    }

#if TOUTIAO

    private static int px2dp(int px) => (int)(px * (160 / Screen.dpi));


    private IEnumerator CoShowIntAd(StarkSDKSpace.StarkAdManager.InterstitialAd ad, UnityEngine.UI.Button.ButtonClickedEvent[] eventsToAddTo)
    {
        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => { return ad.IsLoaded(); });

        ad.Show();

        if (eventsToAddTo != null)
        {
            foreach (var evt in eventsToAddTo)
            {
                evt.AddListener(() =>
                {
                    ad.Destory();
                });
            }
        }
    }

    private IEnumerator CoSetBannerAd()
    {
        if (!(bannerAd != null)) yield break;

        bannerAd.Show();

        yield return new WaitForSeconds(0.01f);


        int w = 314; //竖屏下Banner宽度始终为314. (固定长宽比2.88，高度为112)
        int h = 112;
        int sw = px2dp(Screen.width);
        int sh = px2dp(Screen.height);


        bannerStyle.top = sh - h;//底部
        bannerStyle.left = sw / 2 - w / 2;//中央
        bannerStyle.width = w; //（只有横屏能Resize宽度）


        bannerAd.ReSize(bannerStyle);
    }
    
#endif
}


#endif