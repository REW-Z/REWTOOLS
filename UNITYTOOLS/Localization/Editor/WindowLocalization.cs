using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WindowImageLocalization : EditorWindow
{
    public static WindowImageLocalization window;

    public Vector2 currnetScrollPos = new Vector2();

    [MenuItem("本地化/图片本地化管理")]
    public static void OpenWindow()
    {
        window = EditorWindow.GetWindow<WindowImageLocalization>(false, "本地化", true); //Instantiate
        window.Show(); //Show
        //windowSize
        window.position = new Rect(0, 0, 1600, 650);

        if (!LocalizationSystem.isInit)
        {
            LocalizationSystem.Init();
        }
    }

    public void OnGUI()
    {
        if (!LocalizationSystem.isInit)
        {
            LocalizationSystem.Init();
        }

        int left = 20;
        int top = 20;
        int _x = left;
        int _y = top;
        int cellW = 100;
        int cellH = 100;
        int parding = 2;


        // First ROW
        var langNames = System.Enum.GetNames(typeof(LocalizationSystem.Language));
        EditorGUI.LabelField(new Rect(_x, _y, cellW, cellH), "Image组件位置");
        _x += cellW;
        for (int l = 0; l < langNames.Length; l++)
        {
            EditorGUI.LabelField(new Rect(_x, _y, cellW, cellH), langNames[l]);
            _x += cellW;
        }
        _x = left;
        _y += cellH;


        //TEST SCROLL
        int scrollW = cellW * (langNames.Length + 1);
        int scrollH = cellH * 4;
        int scrollEndY = (cellH * 4) + _y;
        int scrollTotalH = cellH * LocalizationSystem.imageLocalization.imgReplaces.Length;

        currnetScrollPos = GUI.BeginScrollView(new Rect(_x, _y, scrollW, scrollH), currnetScrollPos, new Rect(_x, _y, scrollW, scrollTotalH));

        // 1 ~  N ROW
        for (int i = 0; i < LocalizationSystem.imageLocalization.imgReplaces.Length; i++) //行循环
        {
            EditorGUI.DrawRect(new Rect(_x, _y, cellW, cellH), Color.grey);
            LocalizationSystem.imageLocalization.imgReplaces[i].imgName = GUI.TextField(new Rect(_x, _y, 100, 100), LocalizationSystem.imageLocalization.imgReplaces[i].imgName);
            _x += cellW;

            for (int j = 0; j < LocalizationSystem.imageLocalization.imgReplaces[i].sprites.Length; j++)//元素循环
            {
                EditorGUI.DrawRect(new Rect(_x, _y, cellW, cellH), Color.black);
                LocalizationSystem.imageLocalization.imgReplaces[i].sprites[j] = (Sprite)EditorGUI.ObjectField(new Rect(_x + parding, _y + parding, cellW - (parding * 2), cellH - (parding * 2)), LocalizationSystem.imageLocalization.imgReplaces[i].sprites[j], typeof(Sprite), true);
                _x += cellW;
            }

            _y += cellH;
            _x = left;
        }
        
        GUI.EndScrollView();

        _x = left;
        _y = scrollEndY;

        //BUTTONS
        if (GUI.Button(new Rect(_x, _y, 100, 100), "+"))
        {
            var newArr = new ImageReplace[LocalizationSystem.imageLocalization.imgReplaces.Length + 1];
            for (int i = 0; i < newArr.Length; i++)
            {
                if (i < LocalizationSystem.imageLocalization.imgReplaces.Length)
                {
                    newArr[i] = LocalizationSystem.imageLocalization.imgReplaces[i];
                }
                else
                {
                    newArr[i] = new ImageReplace();
                }
            }
            LocalizationSystem.imageLocalization.imgReplaces = newArr;
        }
        _x += 100;
        if (GUI.Button(new Rect(_x, _y, 100, 100), "-"))
        {
            var newArr = new ImageReplace[Mathf.Clamp( LocalizationSystem.imageLocalization.imgReplaces.Length - 1, 0, int.MaxValue) ];
            for (int i = 0; i < newArr.Length; i++)
            {
                newArr[i] = LocalizationSystem.imageLocalization.imgReplaces[i];
            }
            LocalizationSystem.imageLocalization.imgReplaces = newArr;
        }
        _x += 100;
        if (GUI.Button(new Rect(_x, _y, 100, 100),"Save"))
        {
             var existAsset = AssetDatabase.LoadAssetAtPath("Assets/Resources/ImageLocalization.asset", typeof(ImageLocalization));
            if(existAsset != null)
            {
                AssetDatabase.DeleteAsset("Assets/Resources/ImageLocalization.asset");
            }
            ImageLocalization asset = ScriptableObject.CreateInstance<ImageLocalization>();
            asset.imgReplaces = LocalizationSystem.imageLocalization.imgReplaces;
            AssetDatabase.CreateAsset(asset, "Assets/Resources/ImageLocalization.asset");
        }



        //debug
        //GUILayout.Label("windowpos:" + window.position);
        //GUILayout.Label("scrollH:" + scrollH + " scrollTotalH:" + scrollTotalH);
    }

}
