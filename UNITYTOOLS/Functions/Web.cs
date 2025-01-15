using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using System.Threading;
using System.Threading.Tasks;


public class Web : MonoBehaviour
{
    //instance
    private static Web inst = null;
    public static Web Instance
    {
        get
        {
            if(inst == null)
            {
                inst = FindObjectOfType<Web>();
            }
            if (inst == null)
            {
                inst = CreateInstance();
            }
            return inst;
        }
    }

    public static Web CreateInstance()
    {
        if (inst != null) return inst;
        if (FindObjectOfType<Web>() != null) return (FindObjectOfType<Web>());

        GameObject obj = new GameObject("Web");
        DontDestroyOnLoad(obj);

        inst = obj.AddComponent<Web>();

        return inst;
    }


    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 通用网络请求函数
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postBytes"></param>
    /// <param name="successCallback"></param>
    /// <param name="successCallback"></param>
    /// <returns></returns>
    public static void StartRequest(string method, string url, UnityAction<string> successCallback = null, UnityAction errorCallback = null, byte[] postBytes = null)
    {
        Instance.StartCoroutine(CoRequest(method, url, successCallback, errorCallback, postBytes));
    }
    private static IEnumerator CoRequest(string method, string url, UnityAction<string> successCallback = null, UnityAction errorCallback = null, byte[] postBytes = null)
    {
        UnityWebRequest request = new UnityWebRequest(url, method);

        if(method == "POST") request.uploadHandler = (UploadHandler)new UploadHandlerRaw(postBytes);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        //Debug.Log("Status Code: " + request.responseCode);

        if (request.isNetworkError || request.isHttpError)
        {
            //Debug.Log("network error or http error");
            //UIMiniPoper.PopText("网络未连接！");
            if (errorCallback != null) errorCallback();
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            if (successCallback != null) successCallback(request.downloadHandler.text);
        }
    }

}

/// <summary>
/// 简单异步请求  
/// </summary>
public static class SimpleRequest
{
    public static async void Send<T>(Task<T> task, System.Action<T> callback)
    {
        var res = await task;
        callback(task.Result);
    }
}
