//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//using DG.Tweening;



//#if UNITY_EDITOR
//using UnityEditor;
//#endif






//public class CustomScrollEvent<T> : UnityEngine.Events.UnityEvent<T> { };

//public class UIScrollGrid : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
//{
//    private RectTransform _viewport = null;
//    private RectTransform _content = null;
//    private RectTransform thisRectt = null;

//    public RectTransform viewport
//    {
//        get
//        {
//            if (this._viewport == null)
//            {
//                this._viewport = this.transform.Find("Viewport").GetComponent<RectTransform>();
//            }
//            return _viewport;
//        }
//    }
//    public RectTransform content
//    {
//        get
//        {
//            if(_content == null)
//            {
//                this._content = this.transform.Find("Viewport").Find("Content").GetComponent<RectTransform>();
//            }
//            return this._content;
//        }
//    }


    
//    public CustomScrollEvent<float> onValueChanged = new CustomScrollEvent<float>();
//    public CustomScrollEvent<int> onStartSelect = new CustomScrollEvent<int>();
//    public CustomScrollEvent<int> onEndSelect = new CustomScrollEvent<int>();



//    //constant  
//    private const float sensitive = 1.25f;

//    //serializable
//    [Header("Grid Count")]
//    public int gridCount = 1;

//    private Vector2 targetPos = default;
//    private bool posReached = true;
//    private bool isDraging = false;




//    void Awake()
//    {
//        thisRectt = this.GetComponent<RectTransform>();
//    }
//    void Start()
//    {
//        //限制锚点
//        content.anchorMin = new Vector2(0, 1);
//        content.anchorMax = new Vector2(0, 1);
//    }

//    void Update()
//    {
//        if (posReached) return;
//        if (isDraging) return;

//        if((targetPos - content.anchoredPosition).sqrMagnitude > 1f)
//        {
//            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPos, Time.deltaTime * 5f);
//            onValueChanged.Invoke(GetHorizontalValue());
//        }
//        else
//        {
//            content.anchoredPosition = targetPos;
//            posReached = true;
//            onValueChanged.Invoke(GetHorizontalValue());
//        }
//    }




//    // *************************** UGUI EVT **********************************

//    private Vector2 startPointerPos;
//    private Vector2 startContentPos;

//    private float startTime;
//    private int startIdx;

//    public void OnBeginDrag(PointerEventData eventData)
//    {
//        isDraging = true;

//        startPointerPos = eventData.position;
//        startContentPos = content.anchoredPosition;
//        startIdx = GetValueIndex();
//        startTime = Time.time;

//        onStartSelect.Invoke(GetValueIndex());
//    }

//    public void OnDrag(PointerEventData eventData)
//    {
//        //movement
//        float movementX = eventData.position.x - startPointerPos.x;
//        float movementY = eventData.position.y - startPointerPos.y;

//        movementX *= sensitive;
//        if (true) movementY = 0;

//        //clamp  
//        float maxX = content.sizeDelta.x - ViewportWidth(); //+ viewport.sizeDelta.x 
//        float maxY = content.sizeDelta.y - ViewportHeight(); // + viewport.sizeDelta.y

//        //move content in viewport
//        content.anchoredPosition = new Vector2(Mathf.Clamp(  startContentPos.x + movementX, -maxX, 0), Mathf.Clamp(startContentPos.y + movementY, -maxY, 0));

//        //event
//        onValueChanged.Invoke(GetHorizontalValue());
//    }

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        float endTime = Time.time;
//        bool isFastSlide = (Time.time - startTime) < 0.2f;

//        int leftIdx = (int)(GetHorizontalValue() / (1f / gridCount));
//        int rightIdx = Mathf.Clamp(leftIdx + 1, 0, gridCount - 1);

//        float lDistance = GetHorizontalValue() - (leftIdx * (1f / gridCount));
//        float rDistance = (rightIdx * (1f / gridCount)) - GetHorizontalValue();

//        // 最近的位置  
//        int selectidx = lDistance < rDistance ? leftIdx : rightIdx;
        
//        // 最终的位置  
//        int finalIdx = startIdx;
//        if(isFastSlide)
//        {
//            if(eventData.position.x > startPointerPos.x)
//                finalIdx = ClampIdx(startIdx - 1);
//            else
//                finalIdx = ClampIdx(startIdx + 1);
//        }
//        else if (selectidx - startIdx > 0)
//        {
//            finalIdx = ClampIdx(startIdx + 1);
//        }
//        else if (selectidx - startIdx < 0)
//        {
//            finalIdx = ClampIdx(startIdx - 1);
//        }

//        // final scroll to select    
//        targetPos = new Vector2(-(finalIdx * ViewportWidth()), content.anchoredPosition.y);
//        posReached = false;

//        //event 
//        onEndSelect.Invoke(finalIdx);
        
//        //status
//        isDraging = false;
//    }



//    public float ViewportWidth()
//    {
//        return (thisRectt.sizeDelta.x + viewport.sizeDelta.x);
//    }
//    public float ViewportHeight()
//    {
//        return (thisRectt.sizeDelta.y + viewport.sizeDelta.y);
//    }

//    /// <summary>
//    /// 设置格子数量
//    /// </summary>
//    /// <param name="gridCount"></param>
//    public void SetGrid(int gridCount)
//    {
//        this.gridCount = gridCount;
//        content.sizeDelta = new Vector2(ViewportWidth() * gridCount, content.sizeDelta.y);
//    }

//    /// <summary>
//    /// 自动滑动到
//    /// </summary>
//    public void ScrollTo(int idx)
//    {
//        int clampedIdx = ClampIdx(idx);

//        //isDraging = true;
//        //onStartSelect.Invoke(clampedIdx);

//        targetPos = new Vector2(-(clampedIdx * ViewportWidth()), content.anchoredPosition.y);
//        posReached = false;

//        onEndSelect.Invoke(clampedIdx);

//        isDraging = false;
//    }

//    /// <summary>
//    /// 0 - 0.9  (gridcount==10)
//    /// </summary>
//    /// <returns></returns>
//    public float GetHorizontalValue()
//    {
//        return (-content.anchoredPosition.x) / (content.sizeDelta.x);
//    }

//    public int GetValueIndex()
//    {
//        return (Mathf.RoundToInt((GetHorizontalValue() / (1f / this.gridCount))));
//    }

//    public int ClampIdx(int input)
//    {
//        return Mathf.Clamp(input, 0, this.gridCount - 1);
//    }
//}




//#if UNITY_EDITOR
//[CustomEditor(typeof(UIScrollGrid))]
//public class UIScrollGridEdtior : Editor
//{
//    private static GUIStyle styleComment = null;

//    private static void InitStyle()
//    {
//        styleComment = new GUIStyle();
//        styleComment.normal = new GUIStyleState();
//        styleComment.normal.textColor = Color.green;
//    }

//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        if(styleComment == null)
//        {
//            InitStyle();
//        }

//        GUILayout.Label("需要ScrollRect组件的节点结构（可以右键创建预设的Scroll然后替换组件）", styleComment);
//        GUILayout.Label("必须准确设置content的sizeDelta", styleComment);
//    }
//}
//#endif