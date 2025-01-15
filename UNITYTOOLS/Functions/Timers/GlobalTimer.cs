#define ZTOOLS_GLOBALTIMER

#if ZTOOLS_GLOBALTIMER
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class RegisteredEvent
{
    public RegisteredEvent(UnityAction callback, int sec, int min = 0, int hr = 0)
    {
        this.callback = callback;

        this.second = sec;
        this.minute = min;
        this.hour = hr;
    }

    // init fields
    private UnityAction callback;
    public int second;
    public int minute;
    public int hour;
    
    // remain
    public int rSec;
    public int rMin;
    public int rHour;




    public void Reset()
    {
        rSec = second;
        rMin = minute;
        rHour = hour;
    }

    public void Check()
    {
        if(rSec < 0)
        {
            rSec += 60;
            rMin--;
        }
        if(rMin < 0)
        {
            rMin += 60;
            rHour--;
        }
        if (rHour < 0)
        {
            Debug.Log("RHOUR < 0 ！！！");
            InvokeEvent();
        }
    }

    private void InvokeEvent()
    {
        callback();
        Reset();
        Debug.Log("hr - min - sec: " + rHour + "-" + rMin + "-" + rSec);
    }
    
}

public class GlobalTimer : MonoBehaviour
{
    private static GlobalTimer inst = null;

    public static GlobalTimer Instance
    {
        get
        {
            if (inst == null)
                inst = CreateInstance();

            return inst;
        }
    }

    public static GlobalTimer CreateInstance()
    {
        if (inst != null) return inst;

        GameObject obj = new GameObject("GlobalTimer");
        DontDestroyOnLoad(obj);

        return obj.AddComponent<GlobalTimer>();
    }

    //Registers
    private List<RegisteredEvent> registeredEventList = new List<RegisteredEvent>();

    //EVENTS
    [HideInInspector] public UnityEvent onSecondChange = new UnityEvent();
    [HideInInspector] public UnityEvent onMinuteChange = new UnityEvent();
    [HideInInspector] public UnityEvent onHourChange = new UnityEvent();

    //TIMER STUFF
    private int hour;
    private int minute;
    private int second;
    private float tmpTime = 0f;

    //TIMER FLOAT
    private float timeTotal = 0f;







    // --------------------------- maiDian ---------------------------------
    private int[] maiTimes = new int[] { 0, 5, 30, 60, 90, 120, 180, 240, 300, 360, 420, 480, 540, 600, 1200, 1800, 2400, 3000, 3600 };
    private int currentMaiTarget = 0;
    // --------------------------- maiDian ---------------------------------



    // --------------------- MONO Behaviour -----------------
    void Start()
    {
        onSecondChange.AddListener(() => {
            registeredEventList.ForEach(e => {
                e.rSec -= 1;
                e.Check();
            });
        });
    }

    void Update()
    {
        //PLUS TIME
        timeTotal += Time.deltaTime;

        TmpTimePlus(Time.deltaTime);


        //-----------------MAI DIAN STUFF--------------
        if(currentMaiTarget < maiTimes.Length)
        {
            if (timeTotal > (float)maiTimes[currentMaiTarget])
            {
                //MaiDian.Mai("number", maiTimes[currentMaiTarget], "version", Infomanager.MaiDianVersion, "second");
                currentMaiTarget += 1;
            }
        }
        //-----------------MAI DIAN STUFF--------------
    }


    // ----------------- Private Methods -----------------------
    private void TmpTimePlus(float timeDelta)
    {
        tmpTime += timeDelta;
        
        while(tmpTime > 1f)
        {
            tmpTime -= 1f;
            SecondPlusOne();
        }
    }
    private void SecondPlusOne()
    {
        second += 1;
        if (second > 60)
        {
            second -= 60;
            MinutePlusOne();
        }
        onSecondChange.Invoke();
    }

    private void MinutePlusOne()
    {
        minute += 1;
        if(minute >= 60)
        {
            minute -= 60;
            HourPlusOne();
        }
        onMinuteChange.Invoke();
    }
    
    private void HourPlusOne()
    {
        hour += 1;
        onHourChange.Invoke();
    }


    // ----------------- Out Interface -----------------------
    public void RegisterEvent(UnityAction callback, int sec, int min = 0, int hour = 0)
    {
        RegisteredEvent revent = new RegisteredEvent(callback, sec, min, hour);
        revent.Reset();
        registeredEventList.Add(revent);
    }



    // ----------------- Test ----------------------
    [ContextMenu("TestThisScript")]
    public void OnTest()
    {
        Debug.Log(registeredEventList != null ? ("注册的定时事件数： " + registeredEventList.Count) : "没有注册的定时事件");
    }
}
#endif