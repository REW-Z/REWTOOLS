using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PrefabReplace
{
    [MenuItem("Utils/Replace/ReplaceNull")]
    public static void ReplaceGlassSelected()
    {
        GameObject[] gameObjs = Selection.gameObjects;
        
        ReplaceObjsWith(gameObjs, "Prefabs/Items/Null");
    }


    public static void ReplaceObjsWith(GameObject[] gameObjs, string path)
    {
        foreach (var obj in gameObjs)
        {
            var newObj = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>(path), obj.transform.parent) as GameObject;
            newObj.transform.position = obj.transform.position;
            newObj.transform.rotation = obj.transform.rotation;
            newObj.transform.localScale = obj.transform.localScale;
            

        }

        for (int i = gameObjs.Length - 1; i > -1; i--)
        {
            GameObject.DestroyImmediate(gameObjs[i]);
        }
    }
}