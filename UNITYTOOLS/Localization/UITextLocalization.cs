using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class UITextLocalization : MonoBehaviour
{
    private Text txtCom;
    private TextMeshProUGUI tmproTxtCom;
    private bool inited = false;
    /// <summary>
    /// Pattern
    /// </summary>
    public string pattern;

    private LocalizationSystem.Language currentLanguage;
    private bool languageIsSet = false;



    private void Init()
    {
        if (inited) return;
        if (!LocalizationSystem.isInit) LocalizationSystem.Init();

        txtCom = GetComponent<Text>();
        tmproTxtCom = GetComponent<TextMeshProUGUI>();

        LocalizationSystem.langChangeEvent.AddListener(this.Refresh);

        inited = true;
    }

    private void Awake()
    {
        Init();
    }
    
    private void Start()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        LocalizationSystem.langChangeEvent.RemoveListener(this.Refresh);
    }


    public string Text { get { return this.pattern; } set { this.pattern = value; Refresh(); } }


    public void Refresh()
    {
        if (!inited) Init();

        //change language?
        if(!languageIsSet || LocalizationSystem.currentLanguage != this.currentLanguage)  // TODO: BUG：可能初始值刚刚好相等！！！
        {
            SetLanguage(LocalizationSystem.currentLanguage);
            languageIsSet = true;
        }
        
        //translate
        Translate();
    }


    private void SetLanguage(LocalizationSystem.Language newLanguage)
    {
        //change font
        if (txtCom != null)
        {
            txtCom.font = LocalizationSystem.fontLocalization.fonts[(int)newLanguage];
        }

        if (tmproTxtCom != null)
        {
            if (LocalizationSystem.fontLocalization == null) Debug.LogAssertion("fontLocalizaton为空!");
            if (LocalizationSystem.fontLocalization.fontAssets == null) Debug.LogAssertion("fontLocalizaton.fontAssets为空!");
            tmproTxtCom.font = LocalizationSystem.fontLocalization.fontAssets[(int)newLanguage];
        }

        this.currentLanguage = LocalizationSystem.currentLanguage;
    }

    private void Translate()
    {
        if (!inited) Init();
        if (pattern == null) return;

        string[] allSegments = pattern.Split('&');
        List<string> keys = new List<string>();
        List<string> replaces = new List<string>();
        
        // *** 提取Key值和Replace值 ***
        foreach(var seg in allSegments)
        {
            if( Regex.IsMatch(seg, "\\$.*\\$"))
            {
                replaces .Add (seg.Substring(1, seg.Length - 2));
            }
            else
            {
                keys.Add(seg);
            }
        }
        
        // *** 翻译分句并连接 *** 
        System.Text.StringBuilder strb = new System.Text.StringBuilder();
        foreach (var key in keys)
        {
            //不翻译的内容
            if (Regex.IsMatch(key, "@.*@"))
            {
                strb.Append(key.Substring(1, key.Length - 2));
            }
            //要翻译的内容
            else
            {
                strb.Append(LocalizationSystem.GetLocalizedValue(key));
            }
        }
        string concatText = strb.ToString();
        
        string finalText = concatText;

        // *** 转义逗号 ***  
        finalText = finalText.Replace('~', ',');


        // *** 替换模板 *** 
        for (int i = 0; i < replaces.Count; i++)
        {
            finalText = Regex.Replace(finalText, "{.*}", replaces[i]);
        }
        

        // *** 修改UI ***
        if (txtCom != null)
        {
            txtCom.text = finalText;
        }

        if(tmproTxtCom != null)
        {
            tmproTxtCom.text = finalText;
        }
    }
}

//格式注解  

    //示例：add&@:@&attck&{}&$100%$

// '&'连接符  
// '$'模板替换值  
// '@'固定值不翻译
// '~'逗号转义  



#if UNITY_EDITOR
[CustomEditor(typeof(UITextLocalization))]
public class CustomUITextLocalization : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("翻译"))
        {
            UITextLocalization tgt = (target as UITextLocalization);
            tgt.Refresh();
        }
    }
}
#endif


