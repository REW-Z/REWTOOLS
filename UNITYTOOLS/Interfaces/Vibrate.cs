using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if TOUTIAO
using StarkSDKSpace;
#endif

public class Vibrate
{
    private static bool vibrateOn = true;

    public static bool VibrateOn { get { return vibrateOn; } set { vibrateOn = value; } }



    public static void DoVibrate(int mSec = 1000)
    {
        if (!vibrateOn) return;

#if TOUTIAO
        StarkSDKSpace.StarkSDK.API.Vibrate(new long[2] { 0, mSec });
//#elif CAONI
//        GameObject.FindObjectOfType<MiliPay>().u3dToJava_msg("9999", new Callback(() => { }), new Callback(() => { }));
#else
        Handheld.Vibrate();
#endif
    }

    public static void DoVibrateArr(long[] arr)
    {
        if (!vibrateOn) return;

#if TOUTIAO
        StarkSDKSpace.StarkSDK.API.Vibrate(arr);
//#elif CAONI
//        GameObject.FindObjectOfType<MiliPay>().u3dToJava_msg("9999", new Callback(() => { }), new Callback(() => { }));
#else
        Handheld.Vibrate();
#endif
    }
}
