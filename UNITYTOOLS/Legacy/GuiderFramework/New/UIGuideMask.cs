

#if REW_LEGACY  

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Events;

using DG.Tweening;

public class UIGuideMask : MonoBehaviour
{
    public Sprite defaultSpriteShape;

    private Canvas canvas;
    private Canvas childCanvas;
    private RectTransform maskButton;
    private Image bGBlack;
    private Image finger;

    //masking tmp
    private RectTransform masking = null;

    //Sequence
    private Sequence seq = null;

    //Timer
    private float showTime = 0f;
    //Status
    private bool isMasking = false;



    void Awake()
    {
        canvas = this.GetComponentInParent<Canvas>();
        childCanvas = this.transform.GetChild(0).GetComponent<Canvas>();
        maskButton = this.transform.GetChild(0).Find("MaskButton").GetComponent<RectTransform>();
        bGBlack = this.transform.GetChild(0).Find("BgBlack").GetComponent<Image>();
        finger = this.transform.GetChild(0).Find("Finger").GetComponent<Image>();

        maskButton.GetComponent<Button>().onClick.AddListener(() => {
            Debug.Log("未绑定引导点击");
        });
    }
    
    void Update()
    {
        if (masking != null && maskButton.gameObject.activeInHierarchy)
        {
            //计时器
            if(isMasking)
            {
                showTime += Time.deltaTime;
            }

            //如果目标消失则关闭
            if(!masking.gameObject.activeInHierarchy && showTime > 1f)
            {
                Debug.LogAssertion("目标非活跃，强制引导自动关闭了！！");
                HideMask();
                return;
            }

            //更新位置
            maskButton.transform.position = masking.transform.position;
            maskButton.transform.rotation = masking.transform.rotation;
            maskButton.transform.localScale = new Vector3(masking.lossyScale.x / canvas.transform.localScale.x, masking.lossyScale.y / canvas.transform.localScale.y, masking.lossyScale.z / canvas.transform.localScale.z);

            //finger.transform.position = mask.transform.position;
            //finger.transform.rotation = mask.transform.rotation;
        }

    }

    /// <summary>
    /// 强指引
    /// </summary>
    public void MaskTarget(RectTransform target, bool useDefaultShape = false, float defaultShapeRadius = 100f, bool tapAnyPosition = false, UnityAction callback = null)
    {
        if (isMasking) { Debug.LogAssertion("重复的强制引导事件"); return; }
        isMasking = true;

        //Timer Reset
        showTime = 0f;

        //ACTIVE
        childCanvas.gameObject.SetActive(true);
        
        StartCoroutine(CoMaskTarget(target, useDefaultShape, defaultShapeRadius, tapAnyPosition, callback));
    }
    private IEnumerator CoMaskTarget(RectTransform target, bool useDefaultShape, float defaultShapeRadius, bool tapAnyPosition, UnityAction callback)
    {
        yield return null;

        //Set
        masking = target;

        maskButton.gameObject.SetActive(true);

        maskButton.transform.position = masking.transform.position;
        maskButton.transform.rotation = masking.transform.rotation;

        //tap any position ??  
        maskButton.GetChild(0).gameObject.SetActive(tapAnyPosition);


        //shape
        if (!useDefaultShape)//原图标
        {
            maskButton.GetComponent<Image>().type = masking.GetComponent<Image>().type;
            maskButton.GetComponent<Image>().sprite = masking.GetComponent<Image>().sprite;
            maskButton.GetComponent<Image>().color = masking.GetComponent<Image>().color;

            maskButton.GetComponent<RectTransform>().sizeDelta = masking.sizeDelta;
            maskButton.GetComponent<RectTransform>().pivot = masking.pivot;
        }
        else//默认圆圈
        {
            maskButton.GetComponent<Image>().type = Image.Type.Simple;
            maskButton.GetComponent<Image>().sprite = this.defaultSpriteShape;
            maskButton.GetComponent<Image>().color = Color.white;

            maskButton.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultShapeRadius * 2f, defaultShapeRadius * 2f);
            maskButton.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }



        //blink 
        bGBlack.color = new Color(0, 0, 0, 0);
        bGBlack.DOColor(new Color(0, 0, 0, 0.75f), 1f);


        // mask as button
        Debug.LogWarning("*** Mask Add Lisener!!! *** ");
        maskButton.GetComponent<Button>().onClick.RemoveAllListeners();
        maskButton.GetComponent<Button>().onClick.AddListener(() => {

            Debug.Log("*** Clicked Target !!! *** ");

            if (masking.GetComponentInChildren<Button>() != null)
            {
                Debug.Log("*** Target Invoked !!! *** ");
                masking.GetComponentInChildren<Button>().onClick.Invoke();
            }

            //关闭
            HideMask();

            //回调
            callback?.Invoke();
        });


        // --------------------- FINGER ANIM ---------------------------------

        Image imgFinger = finger.GetComponent<Image>();

        finger.transform.localScale = Vector3.zero;
        finger.transform.localPosition = Vector3.zero;

        yield return new WaitForSeconds(1.5f);
        

        seq.Kill();
        seq = DOTween.Sequence();
        seq.AppendCallback(() => {
            imgFinger.color = new Color(1,1,1,0);
            finger.transform.localScale = Vector3.zero;
            finger.transform.localPosition = Vector3.zero;
            finger.GetComponent<Animation>().Stop();
        });
        seq.AppendCallback(() => {
            //finger anim
            finger.transform.localScale = Vector3.one;
            finger.transform.DOMove(masking.transform.position, 1f).OnComplete(() => { finger.GetComponent<Animation>().Play(); });
            imgFinger.DOColor(Color.white, 0.6f);
        });
        seq.AppendInterval(3f);
        seq.SetLoops(-1);
    }

    /// <summary>
    /// 隐藏强指引  
    /// </summary>
    public void HideMask()
    {
        //DEACTIVE
        childCanvas.gameObject.SetActive(false);

        isMasking = false;
    }

    /// <summary>
    /// 显示指引文字  
    /// </summary>
    public void ShowText(string guideText)
    {
        this.GetComponentInChildren<UITextLocalization>(true).Text = guideText;
    }
}

#endif