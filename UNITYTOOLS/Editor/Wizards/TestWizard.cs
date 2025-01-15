using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestWizard : ScriptableWizard
{
    public int number;
    public string str;

    void OnWizardUpdate()
    {
        string helpString = "Log string number times";
        bool isValid = (number > 0) && (str != null);
    }

    void OnWizardCreate()
    {
        for(int i = 0; i < number; i++)
        {
            Debug.Log(str + " (" + i + ")");
        }
    }

    [MenuItem("自定义/Wizards/Test")]
    static void Test()
    {
        ScriptableWizard.DisplayWizard<TestWizard>(
            "Test Wizard", "Test!");
    }
}