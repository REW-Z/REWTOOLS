

#if REW_LEGACY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIGuideArrow : MonoBehaviour
{
    public Transform target;

    private CanvasScaler canvasScaler;

    private RectTransform canvas;

    private UIPanelGuideArrows guidesContainer;

    private Transform imgInside;

    private Transform imgOutside;

    void Start()
    {
        if (this.transform.childCount < 2) { Debug.LogAssertion("不存在两个图片！"); return; }

        guidesContainer = this.GetComponentInParent<UIPanelGuideArrows>(true);
        canvasScaler = this.GetComponentInParent<CanvasScaler>(true);
        canvas = canvasScaler.GetComponent<RectTransform>();
        imgInside = this.transform.GetChild(0);
        imgOutside = this.transform.GetChild(1);
    }


    void Update()
    {
        if (target != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

            ProcessScreenPosVec3(ref screenPos);

            var anchorPos = UtilsScreenAndUI.ScreenPointToCanvasPos_MatchWidth(screenPos, canvasScaler);

            var finalPos = new Vector2(Mathf.Clamp(anchorPos.x, 0, canvas.sizeDelta.x), Mathf.Clamp(anchorPos.y, 0, canvas.sizeDelta.y));

            float angle = Mathf.Rad2Deg * Mathf.Atan2(anchorPos.y - (canvas.sizeDelta.y / 2f), anchorPos.x - (canvas.sizeDelta.x / 2f));

            if (anchorPos.x > 0f && anchorPos.x < canvas.sizeDelta.x && anchorPos.y > 0f && anchorPos.y < canvas.sizeDelta.y)
            {
                imgInside.gameObject.SetActive(true);
                imgOutside.gameObject.SetActive(false);
                this.GetComponent<RectTransform>().anchoredPosition = finalPos;
            }
            else
            {
                imgInside.gameObject.SetActive(false);
                imgOutside.gameObject.SetActive(true);
                imgOutside.localEulerAngles = new Vector3(0, 0, angle);
                this.GetComponent<RectTransform>().anchoredPosition = finalPos;
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    private void ProcessScreenPosVec3(ref Vector3 input)
    {
        if (input.z < 0f)
        {
            input *= -1f;
        }

        if (input.z < 1f)
        {
            input *= (1f / input.z);
        }
    }

    private void OnDestroy()
    {
        guidesContainer.OnGuideDestroy(this);
    }
}

#endif