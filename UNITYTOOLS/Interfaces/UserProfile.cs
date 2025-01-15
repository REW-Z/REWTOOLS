using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if TOUTIAO
using StarkSDKSpace;
#endif

public class UserInfoManager
{

#if TOUTIAO
    public static ScUserInfo scUserInfo = null;

    private static bool isLogin = false;
    private static bool isSetUserInfo = false;

    public static void Login(UnityAction callbackSuccess, UnityAction callbackFail)
    {
        StarkSDK.API.GetAccountManager().Login((code, anonymousCode, isLogin) => {

            isLogin = true;

            SetUserInfo( callbackSuccess,  callbackFail);
        }, (errMsg) => {
            Debug.Log("登录出错，错误信息:" + errMsg);
            callbackFail();
        });
        
    }
    private static void SetUserInfo(UnityAction callbackSuccess, UnityAction callbackFail)
    {
        if (!isLogin) return;

        StarkSDK.API.GetAccountManager().GetScUserInfo((ref ScUserInfo uinfo) => {
            scUserInfo = uinfo;
            isSetUserInfo = true;
        }, (errMsg) => {
            Debug.Log("获取抖音用户信息出错，错误信息:" + errMsg);
            callbackFail();
        });
    }
#endif
}
