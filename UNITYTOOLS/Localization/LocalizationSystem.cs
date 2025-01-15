using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class LocalizationSystem
{
    public enum Language
    {
        English = 0,
        Japanese = 1,
        Chinese = 2,
        BrazilPortuguese = 3,
        TChinese = 4,
        Russian = 5,
        Spanish = 6,
        German = 7,
        Polish = 8,
        Italian = 9,
        Turkish = 10,
        French = 11,
        Korean = 12,
        Hungarian = 13
    }

    // ------------------------ OBJ --------------------------------
    public static CSV csvObject;
    public static ImageLocalization imageLocalization;
    public static FontLocalization fontLocalization;




    // ---------------------- STATUS ---------------------------------
    public static bool isInit;
    public static Language currentLanguage;


    // ----------------------- EVENT ---------------------------------
    public static UnityEvent langChangeEvent = new UnityEvent();




    // ------------------------------------------------------------------------------------

    public static void Init()
    {
        csvObject = new CSV();
        imageLocalization = Resources.Load<ImageLocalization>("ImageLocalization");
        fontLocalization = Resources.Load<FontLocalization>("FontLocalization");

        if (imageLocalization == null)
        {
            Debug.LogAssertion("未找到图片本地化信息 ！！！");
        }
        else
        {
            Debug.Log("已找到图片本地化信息。");
        }

        //语言切换事件
        langChangeEvent.AddListener(() => { Debug.Log("语言已经切换为：" + LocalizationSystem.currentLanguage.ToString()); });
        
        
        //初始化结束
        isInit = true;
    }
    

    public static void ChangeLanguage(Language lang)
    {
        LocalizationSystem.currentLanguage = lang;
        langChangeEvent.Invoke();
    }

    public static Dictionary<string, string> GetDictionaryForEditor()
    {
        if (!isInit)
        {
            Init();
        }
        return csvObject.LanguageDictionaries["ch"];
    }

    public static string GetLocalizedValue(string key)
    {
        if (!isInit)
        {
            Init();
        }
        if(key == null)
        {
            return "";
        }
        if(!(csvObject != null))
        {
            Debug.LogAssertion("NULL CSV !!");
        }

        string value = key;
        switch (currentLanguage)
        {
            case Language.English:
                csvObject.LanguageDictionaries["en"].TryGetValue(key, out value);
                break;
            case Language.Japanese:
                csvObject.LanguageDictionaries["jp"].TryGetValue(key, out value);
                break;
            case Language.Chinese:
                csvObject.LanguageDictionaries["ch"].TryGetValue(key, out value);
                break;
            case Language.BrazilPortuguese:
                csvObject.LanguageDictionaries["br"].TryGetValue(key, out value);
                break;
            case Language.TChinese:
                csvObject.LanguageDictionaries["tc"].TryGetValue(key, out value);
                break;
            case Language.Russian:
                csvObject.LanguageDictionaries["ru"].TryGetValue(key, out value);
                break;
            case Language.Spanish:
                csvObject.LanguageDictionaries["sp"].TryGetValue(key, out value);
                break;
            case Language.German:
                csvObject.LanguageDictionaries["gr"].TryGetValue(key, out value);
                break;
            case Language.Polish:
                csvObject.LanguageDictionaries["pl"].TryGetValue(key, out value);
                break;
            case Language.Italian:
                csvObject.LanguageDictionaries["it"].TryGetValue(key, out value);
                break;
            case Language.Turkish:
                csvObject.LanguageDictionaries["tr"].TryGetValue(key, out value);
                break;
            case Language.French:
                csvObject.LanguageDictionaries["fr"].TryGetValue(key, out value);
                break;
            case Language.Korean:
                csvObject.LanguageDictionaries["kr"].TryGetValue(key, out value);
                break;
            case Language.Hungarian:
                csvObject.LanguageDictionaries["hu"].TryGetValue(key, out value);
                break;
        }


        if (value != null && value != "")
            return value;
        else
            return key;
    }
}
