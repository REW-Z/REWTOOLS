using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CleanAllMissing
{
    [MenuItem("Utils/Cleanup Missing Scripts(include all child transform)")]
    static void CleanupMissingScripts()
    {
        GameObject[] selectedObjs = Selection.gameObjects;

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            CleanThisObject(selectedObjs[i]);
        }
    }

    public static void CleanThisObject(GameObject gameobj)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameobj);

        foreach (Transform trans in gameobj.transform)
        {
            CleanThisObject(trans.gameObject);
        }
    }
}