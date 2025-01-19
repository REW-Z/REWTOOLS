using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class WindowTest : EditorWindow
{
    public static WindowTest window;


    //root objs
    private GameObjectView[] rootGameObjViews;
    //tmp
    private int eleDrawCount = 0;


    [MenuItem("自定义/窗口/WindowTest")]
    public static void OpenWindow()
    {
        window = EditorWindow.GetWindow<WindowTest>(false, "WindowTest", true); //Instantiate
        window.Show(); //Show
    }

    public void OnGUI()
    {
        if(GUILayout.Button("刷新") || this.rootGameObjViews == null)
        {
           this.rootGameObjViews = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Select(obj => new GameObjectView(obj)).ToArray();
        }

        this.eleDrawCount = 0;
        foreach (var obj in this.rootGameObjViews)
        {
            GameObjectLayout(obj);
        }
    }


    private void GameObjectLayout(GameObjectView obj)
    {
        if(obj.gameObject.transform.childCount > 0)
        {
            obj.foldout = EditorGUI.BeginFoldoutHeaderGroup(new Rect((50 * obj.depth), 100 + (25 * eleDrawCount), 200, 20), obj.foldout, "");

            if(GUI.Button(new Rect(50 + (50 * obj.depth), 100 + (25 * eleDrawCount), 200, 20), obj.gameObject.name))
            {
                EditorGUIUtility.PingObject(obj.gameObject);
            }

            EditorGUI.EndFoldoutHeaderGroup();
            eleDrawCount += 1;

            if (obj.foldout)
            {
                for (int i = 0; i < obj.gameObject.transform.childCount; i++)
                {
                    GameObjectLayout(obj.childViews[i]);
                }
            }

            //obj.foldout = EditorGUILayout.Foldout(obj.foldout, obj.gameObject.name);
            //if (obj.foldout)
            //{
            //    for (int i = 0; i < obj.gameObject.transform.childCount; i++)
            //    {
            //        GameObjectLayout(obj.childViews[i]);
            //    }
            //}
        }
        else
        {
            if (GUI.Button(new Rect(50 + (50 * obj.depth), 100 + (25 * eleDrawCount), 200, 20), obj.gameObject.name))
            {
                EditorGUIUtility.PingObject(obj.gameObject);
            }
            eleDrawCount += 1;
        }
    }
}


public class GameObjectView
{
    public GameObject gameObject;

    public GameObjectView[] childViews;

    public int depth;

    public bool foldout;

    public bool hide;

    public GameObjectView(GameObject obj, int depth = 0)
    {
        this.gameObject = obj;
        this.depth = depth;
        this.foldout = false;
        this.hide = false;

        SetChildViews(depth + 1);
    }

    private void SetChildViews(int depth)
    {
        childViews = new GameObjectView[this.gameObject.transform.childCount];
        for (int i = 0; i < this.gameObject.transform.childCount; i++)
        {
            childViews[i] = new GameObjectView(this.gameObject.transform.GetChild(i).gameObject, depth);
        }
    }
}