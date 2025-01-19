#if REW_LEGACY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Recorder : MonoBehaviour
{
    private static Recorder inst = null;


    public static GameObject CreateInstance()
    {
        if (inst != null)
        {
            Debug.Log("Exist Instance!");
            return inst.gameObject;
        }

        GameObject recorderObj = new GameObject("Recorder");

        recorderObj.AddComponent<Recorder>();

        return recorderObj;
    }



    // -------------- Times --------------------
    private static float timeSinceLastStartRecord = 0f; //距离上一次开始录制
    public static float TimeSinceLastStartRecord => timeSinceLastStartRecord;





    private void Awake()
    {
        inst = this;
        DontDestroyOnLoad(inst);

#if TOUTIAO
        var recorder = StarkSDKSpace.StarkSDK.API.GetStarkGameRecorder();
        recorder.SetEnabled(true);
#endif

    }


    void Update()
    {
        timeSinceLastStartRecord += Time.deltaTime;
    }


    /// <summary>
    /// 开始录制
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    /// <param name="maxTimeSec"></param>
    public static void StartRecord(UnityAction successCallback, UnityAction<int, string> failCallback, int maxTimeSec)
    {
#if TOUTIAO
        inst.StartCoroutine(inst.CoStartRecord(successCallback, failCallback, maxTimeSec));
#endif
        timeSinceLastStartRecord = 0f;
    }

    /// <summary>
    /// 结束录制
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public static void StopRecord(UnityAction<string> successCallback, UnityAction<int, string> failCallback)
    {
#if TOUTIAO
        var recorder = StarkSDKSpace.StarkSDK.API.GetStarkGameRecorder();
        recorder.SetEnabled(false);
        recorder.StopRecord((path) => { successCallback(path); }, (code, msg) => { failCallback(code, msg); });
#endif
    }

    /// <summary>
    /// 获取已录屏的时长（毫秒）
    /// </summary>
    /// <returns></returns>
    public static int GetRecordTime()
    {
#if TOUTIAO
        var recorder = StarkSDKSpace.StarkSDK.API.GetStarkGameRecorder();

        return recorder.GetRecordDuration();
#endif
        return 0;
    }













#if TOUTIAO
    public static StarkSDKSpace.StarkGameRecorder.VideoRecordState GetRecordState()
    {
        var recorder = StarkSDKSpace.StarkSDK.API.GetStarkGameRecorder();

        var state = recorder.GetVideoRecordState();

        return state;
    }
    
#endif



    public static void ShareVideo(UnityAction successCallback, UnityAction<string> failCallback, UnityAction cancelCallBack)
    {
#if UNITY_EDITOR
        successCallback();
#elif TOUTIAO
        inst.StartCoroutine(inst.CoShareVideo(successCallback, failCallback, cancelCallBack));
#endif

    }


#if TOUTIAO
    private IEnumerator CoStartRecord(UnityAction successCallback, UnityAction<int, string> failCallback, int maxTimeSec)
    {
        yield return new WaitForSeconds(1.5f);

        var recorder = StarkSDKSpace.StarkSDK.API.GetStarkGameRecorder();
        recorder.SetEnabled(true);
        recorder.StartRecord(true, maxTimeSec, () => { successCallback(); }, (code, msg) => { failCallback(code, msg); });
    }

    private IEnumerator CoShareVideo(UnityAction successCallback, UnityAction<string> failCallback, UnityAction cancelCallBack)
    {
        yield return new WaitForSeconds(1.5f);

        string title = "怪物精灵大冒险";
        List<string> list = new List<string>();
        list.Add("#怪物精灵大冒险");

        var recorder = StarkSDKSpace.StarkSDK.API.GetStarkGameRecorder();
        recorder.ShareVideoWithTitleTopics((arg) => { successCallback(); }, (msg) => { failCallback(msg); }, () => { cancelCallBack(); }, title, list);
    }
#endif
}


#endif