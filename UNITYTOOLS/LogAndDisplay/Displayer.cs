
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if DOTWEEN
using DG.Tweening;
#endif

public static class Displayer
{
    public static void Display(string txt)
    {
        Display(txt, Color.red, 24);
    }

    public static void Display(string txt, Color color, int fontSize, Canvas canvasTarget = null, bool force = false)
    {

        //寻找画布
        Canvas canvas = canvasTarget;
        if (canvas == null)
            canvas = GameObject.FindObjectsOfType<Canvas>().FirstOrDefault(c => c.renderMode == RenderMode.ScreenSpaceOverlay);
        if (canvas == null)
            canvas = GameObject.FindObjectsOfType<Canvas>().FirstOrDefault(c => (c.renderMode == RenderMode.ScreenSpaceCamera));

        //无画布 => 返回
        if (canvas == null)
        {
            Debug.LogAssertion("No Canvas");
            return;
        }

        //创建文本
        GameObject newo = new GameObject("Notice");
        newo.transform.parent = canvas.transform;

        RectTransform rectt = newo.AddComponent<RectTransform>();
        rectt.anchorMin = new Vector2(0.5f, 0.5f);
        rectt.anchorMax = new Vector2(0.5f, 0.5f);
        rectt.anchoredPosition3D = Vector3.zero;
        rectt.sizeDelta = new Vector2(500f, 100f);
        rectt.localScale = Vector3.one;
        rectt.localEulerAngles = Vector3.zero;

        UnityEngine.UI.Text uiText = rectt.gameObject.AddComponent<UnityEngine.UI.Text>();

        uiText.raycastTarget = false;

        uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.fontSize = fontSize;
        uiText.color = color;
        uiText.text = txt;

#if DOTWEEN
        rectt.DOAnchorPos(rectt.anchoredPosition + new Vector2(0, 100f), 1.8f);
        uiText.DOFade(0f, 1.8f);
#endif

        GameObject.Destroy(newo, 2f);
    }
}
