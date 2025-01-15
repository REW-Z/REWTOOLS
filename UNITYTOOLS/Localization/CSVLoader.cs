using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class CSV
{
    private char LF = '\n';
    private char DQ = '"';

    private TextAsset csvFile;

    private Dictionary<string, Dictionary<string, string>> languageDictionaries = null;
    public Dictionary<string, Dictionary<string, string>> LanguageDictionaries
    {
        get
        {
            if(this.csvFile == null)
                this.csvFile = Resources.Load<TextAsset>("localization");

            if (this.languageDictionaries == null)
                GenDics();

            return this.languageDictionaries;
        }
    }

    

    public void GenDics()
    {
        Dictionary<string, Dictionary<string, string>> languageDic = new Dictionary<string, Dictionary<string, string>>();

        Regex regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        string[] rows = csvFile.text.Split(LF);                         //行数组
        
        //row 0 (title)
        string[] languageNames = regex.Split(rows[0]);                                  //第一行 元素 数组
        for (int e = 0; e < languageNames.Length; e++) 
        {
            languageNames[e] = languageNames[e].TrimStart(' ', DQ);
            languageNames[e] = languageNames[e].TrimEnd(DQ);
            
            languageDic[languageNames[e]] = new Dictionary<string, string>();
        }

        // 1~n rows
        for (int j = 1; j < rows.Length; j++) //行循环
        {
            string row = rows[j];
            string[] eleArr = regex.Split(row);
            for (int k = 0; k < eleArr.Length; k++) //去除空格和引号
            {
                eleArr[k] = eleArr[k].TrimStart(' ', DQ);
                eleArr[k] = eleArr[k].TrimEnd(DQ);
            }
            

            if (eleArr.Length >= languageNames.Length) //元素长度判断
            {
                string key = eleArr[0];

                for(int langId = 0; langId < languageNames.Length; langId++) //元素循环
                {
                    string languageName = languageNames[langId];
                    if (!languageDic[languageName].ContainsKey(key))
                    {
                        string value = eleArr[langId];
                        languageDic[languageName].Add(key, value);
                    }
                }
            }
        }
        this.languageDictionaries =  languageDic;
    }
}
