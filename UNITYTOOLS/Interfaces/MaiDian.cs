using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaiDian : MonoBehaviour
{
    public static void Mai(string key1, int value1, string key2, int value2, string name)
    {
        //埋点
        Dictionary<string, int> dic = new Dictionary<string, int>();
        dic[key1] = value1;
        dic[key2] = value2;
#if TOUTIAO
        StarkSDKSpace.StarkSDK.API.ReportAnalytics(name, dic);
#endif

        Debug.Log("=====埋点触发:" + name + " " + key1 + ":" + value1 + "  " + key2 + ":" + value2 + " ===== ");
    }




    ///埋点示例
    ///MaiDian.Mai("number", 1, "version", Infomanager.Version, "goInterface");
    ///MaiDian.Mai("number", 4, "version", Infomanager.Version, "startVideo");
    ///MaiDian.Mai("number", 4, "version", Infomanager.Version, "endVideo");
    ///MaiDian.Mai("number", InfoManager.GetInstance().userData.totalGames, "version", Infomanager.Version, "startLevel");
    ///MaiDian.Mai("second", Mathf.RoundToInt(gametime), "version", Infomanager.Version, "second");
}
