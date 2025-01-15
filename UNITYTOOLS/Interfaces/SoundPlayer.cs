using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DOTWEEN
using DG.Tweening;
#endif

public class SoundPlayer : MonoBehaviour
{
    /// <summary>
    /// 全局音效音量
    /// </summary>
    private static float volume = 1f;
    public static float Volume { get { return volume; } set { ChangeVolume(value); } }

    /// <summary>
    /// 全局背景音乐开关
    /// </summary>
    private static bool bgmOn = true;
    public static bool BGMOn { get { return bgmOn; } set { bgmOn = value; BgmEnable(value); } }


    //--------------- BGM -----------------
    private static AudioClip rawBgmClip = null;



    // ----------------- MONO ------------------------
    private void Awake()
    {
        Init();
    }


    // ------------------- INIT --------------------------
    private static bool isInited = false;
    private static void Init()
    {
        var bgmObj = GameObject.FindGameObjectWithTag("BGM").GetComponent<AudioSource>();

        rawBgmClip = bgmObj.clip;
        isInited = true;
    }





    // --------------- Functions ---------------------

    private static void BgmEnable(bool enable)
    {
        var bgmObj = GameObject.FindObjectOfType<BGM>();
        if (bgmObj == null) return;

        bgmObj.GetComponent<AudioSource>().volume = enable ? 1f : 0f;
    }

    /// <summary>
    /// 替换BGM
    /// </summary>
    /// <param name="name"></param>
    public static void ReplaceBGM(string name)
    {
        if (!isInited) Init();
        if (!bgmOn) return;

        var bgmObj = GameObject.FindObjectOfType<BGM>();
        if (bgmObj == null) return;
        var bgmAudio = bgmObj.GetComponent<AudioSource>();

#if DOTWEEN
        bgmAudio.DOFade(0f, 0.5f).OnComplete(() => {
            bgmAudio.clip = Resources.Load<AudioClip>("Sounds/" + name);
            bgmAudio.Play();
            bgmAudio.DOFade(1f, 0.5f);
        });
#else
        bgmAudio.clip = Resources.Load<AudioClip>("Sounds/" + name);
        bgmAudio.Play();
#endif

    }

    public static void ResetBGM()
    {
        if (!isInited) Init();

        var bgmObj = GameObject.FindObjectOfType<BGM>();
        if (bgmObj == null) return;
        var bgmAudio = bgmObj.GetComponent<AudioSource>();

        if (bgmOn)
        {
#if DOTWEEN
            bgmAudio.DOFade(0f, 0.5f).OnComplete(() => {
                bgmAudio.clip = rawBgmClip;
                bgmAudio.Play();
                bgmAudio.DOFade(1f, 0.5f);
            });
#else
        bgmAudio.clip = rawBgmClip;
        bgmAudio.Play();
#endif
        }
        else
        {
            bgmAudio.clip = rawBgmClip;
            bgmAudio.Play();
        }

    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="str"></param>
    public static void PlaySound2D(string str)//
    {
        try
        {
            GameObject.FindObjectOfType<BGM>().GetComponent<AudioSource>().PlayOneShot(ResourceBundle.Load<AudioClip>("Sounds/" + str));
        }
        catch
        {

        }
    }

    public static void PlaySound3D(string name, Vector3 pos, float volumMultipiler = 1f, float maxDistance = 50f)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Prefabs/AudioSource3D"));
        AudioSource asc = obj.GetComponent<AudioSource>();

        obj.transform.position = pos;

        asc.clip = Resources.Load<AudioClip>("Sounds/" + name);
        asc.maxDistance = maxDistance;

        if (!(asc.clip != null)) return;

        asc.volume = volume * volumMultipiler;
        asc.Play();
        GameObject.Destroy(obj, asc.clip.length);
    }

    public static void ApplyVolume()
    {
        foreach (var auS in FindObjectsOfType<AudioSource>())
        {
            auS.volume = volume;
        }
    }

    /// <summary>
    /// 改变声音
    /// </summary>
    /// <param name="newVolume"></param>
    private static void ChangeVolume(float newVolume)
    {
        volume = newVolume;
        foreach (var auS in FindObjectsOfType<AudioSource>())
        {
            auS.volume = newVolume;
        }
    }


    public static float GetVolume()
    {
        return volume;
    }






    // --------------------- This Game ------------------------
    public static void ClickSound()
    {
        PlaySound2D("click");
    }


}
