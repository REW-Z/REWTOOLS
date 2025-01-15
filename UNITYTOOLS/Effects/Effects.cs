using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if DOTWEEN
using DG.Tweening;
#endif

public class Effects
{
    /// <summary>
    /// UI图片特效
    /// </summary>
    public static void ImageBoom(string spritePath, float radius = 200f, int imgCount = 5, RectTransform flyto = null, UnityEngine.Events.UnityAction callback = null)
    {
#if DOTWEEN

        GameObject original = new GameObject("_FX");
        original.AddComponent<RectTransform>();
        original.AddComponent<CanvasRenderer>();
        original.AddComponent<UnityEngine.UI.Image>();
        original.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>(spritePath);
        original.SetActive(false);


        Canvas canvas = GameObject.FindObjectsOfType<Canvas>().FirstOrDefault(c => c.renderMode != RenderMode.WorldSpace);
        if (flyto != null) canvas = flyto.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        for (int i = 0; i < imgCount; i++)
        {
            GameObject newo = GameObject.Instantiate(original, original.transform.position, original.transform.rotation, canvas.transform);
            newo.SetActive(true);
            RectTransform rectt = newo.GetComponent<RectTransform>();

            rectt.transform.localRotation = Quaternion.identity;
            rectt.anchorMin = new Vector2(0.5f, 0.5f);
            rectt.anchorMax = new Vector2(0.5f, 0.5f);
            rectt.anchoredPosition3D = Vector3.zero;
            rectt.DOAnchorPos(rectt.anchoredPosition + new Vector2(Random.Range(-radius, radius), Random.Range(-radius, radius)), 0.5f).OnComplete(() =>
            {
                if(flyto != null)
                {
                    rectt.DOMove(flyto.position, 0.5f).OnComplete(() =>
                    {
                        GameObject.Destroy(rectt.gameObject);
                    });
                }
                else
                {
                    rectt.DOAnchorPos(Vector2.zero, 0.5f).OnStart(() =>
                    {
                        newo.GetComponent<UnityEngine.UI.Image>().DOFade(0f, 0.5f);
                    })
                    .OnComplete(() => 
                    {
                        GameObject.Destroy(rectt.gameObject);
                    });
                }
            });

        }

        GameObject.Destroy(original);

        //Callback
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(1f).AppendCallback(() => { callback(); });
#endif
    }

    private IEnumerator CoCallback(float delay, UnityEngine.Events.UnityAction callback = null)
    {
        yield return new WaitForSeconds(delay);

        callback();
    }

    public static void ImageFlip(RectTransform imgRectTransform, float durition = 1f, int flipCount = 5)
    {
#if DOTWEEN

        imgRectTransform.DOLocalRotate(imgRectTransform.localEulerAngles + new Vector3(0, 360f * flipCount, 0), durition, RotateMode.LocalAxisAdd);
#endif
    }



    /// <summary>
    /// 文字特效
    /// </summary>
    public static void TextTypewrite(UnityEngine.UI.Text txtCom, float duration, string targetText, int blinkFrameRate = 2)
    {
#if DOTWEEN
        txtCom.DOText(targetText, duration);
        txtCom.StartCoroutine(CoTextWhileDotween(txtCom, duration, blinkFrameRate));
#else
        
#endif
    }
    private static IEnumerator CoTextWhileDotween(UnityEngine.UI.Text txtCom, float duration, int frameRate)
    {
        float timeSum = 0f;
        int safeNum = 9999;
        bool added = false;
        while (timeSum < duration && safeNum > 0)
        {
            //Time
            for( int i = 0; i < frameRate; i++)
            {
                yield return null;
                safeNum--;
                timeSum += Time.deltaTime;
            }

            //Set
            if (added == false)
            {
                if (!txtCom.text.Contains ('|'))
                {
                    txtCom.text += "|";
                }
                added = true;
            }
            else
            {
                if (txtCom.text.Contains('|'))
                {
                    txtCom.text = txtCom.text.Remove(txtCom.text.LastIndexOf('|'));
                }
                added = false;
            }
        }

        //Remove | At Last
        if (txtCom.text.Contains('|'))
        {
            txtCom.text = txtCom.text.Remove(txtCom.text.LastIndexOf('|'));
        }

    }




    /// <summary>
    /// 模型特效
    /// </summary>
    public static void MeshBlink(GameObject gameObj, Color emColor, float duration = 1f)
    {
#if DOTWEEN
        SkinnedMeshRenderer[] smrs = gameObj.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var smr in smrs)
        {
            Material[] materialsNew = smr.materials;
            foreach (Material mat in materialsNew)
            {
                Color colorOld = mat.GetColor("_EmissionColor");
                bool emEnabledOld = mat.IsKeywordEnabled("_EmissionColor");

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emColor);
                mat.DOColor(colorOld, "_EmissionColor", duration).OnComplete(() =>
                {
                    if (!emEnabledOld) mat.DisableKeyword("_EMISSION");
                });

            }
            smr.materials = materialsNew;
        }

        MeshRenderer[] mrs = gameObj.GetComponentsInChildren<MeshRenderer>();

        foreach (var mr in mrs)
        {
            Material[] materialsNew = mr.materials;
            foreach (Material mat in materialsNew)
            {
                Color colorOld = mat.GetColor("_EmissionColor");
                bool emEnabledOld = mat.IsKeywordEnabled("_EmissionColor");

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emColor);
                mat.DOColor(colorOld, "_EmissionColor", duration).OnComplete(() =>
                {
                    if (!emEnabledOld) mat.DisableKeyword("_EMISSION");
                });
            }

            mr.materials = materialsNew;
        }
#endif

    }
}


public class BeseMeshTest : UnityEngine.UI.BaseMeshEffect
{
    // UGUI 网格自定义（包含TextMeshPro）  
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        for (int index = 0; index < verts.Count; index++)
        {
            //Operation...  
        }
        vh.AddUIVertexTriangleStream(verts);
    }
}