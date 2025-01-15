using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RecursiveCell : MonoBehaviour
{
    public int depth;


    //public property  
    public RectTransform CellThis => this.transform.Find("This").GetComponent<RectTransform>();

    public RectTransform ChildsContainer => this.transform.Find("Childs").GetComponent<RectTransform>();


    //SIZE  
    private float _sizey = 0f;
    private bool _isSizeDirty = true;
    public float SizeY
    {
        get
        {
            if(_isSizeDirty == true)
            {
                RecalculateSize();
                _isSizeDirty = false;
            }

            return _sizey;
        }
    }
    private void RecalculateSize()
    {
        if (this._unfold == true)
        {
            float height = 0f;

            height += this.CellThis.sizeDelta.y;

            for (int i = 0; i < activeChildCount; ++i)
            {
                height += this.ChildsContainer.GetChild(i).GetComponent<RecursiveCell>().SizeY;
            }
            _sizey = height;
        }
        else
        {
            _sizey = CellThis.sizeDelta.y;
        }
    }


    //status  
    private bool _unfold = false;
    private int activeChildCount = 0;


    //methods  

    void Start()
    {
        this.CellThis.Find("ButtonFold").GetComponent<Button>().onClick.AddListener(ToggleFold);
    }

    public void SetChildCount(int count)
    {
        //如果没有第一个子节点就创建一个  
        if(count > 0 && ChildsContainer.childCount == 0)
        {
            EnableChild();
        }

        //设置当前子节点数字段  
        this.activeChildCount = count;
        this.ChildsContainer.GetComponent<UIGridManager>().SetGrid(count);

        //子节点刷新
        for (int i = 0; i < this.ChildsContainer.childCount; ++i)
        {
            this.ChildsContainer.GetChild(i).GetComponent<RecursiveCell>().Refresh(includeParents:false);
        }


        //刷新本节点（如果是展开状态还需要刷新父节点）    
        if(_unfold == true)
        {
            for (int i = 0; i < this.ChildsContainer.childCount; ++i)
            {
                this.ChildsContainer.GetChild(i).GetComponent<RecursiveCell>().SetThisAndParentsSizeDirty();
            }
            Refresh(includeParents: true);
        }
        else
        {
            Refresh(includeParents: false);
        }
        
    }


    public void ToggleFold()
    {
        //ToggleFoldStatus  
        this._unfold = !this._unfold;

        //Size标记为Dirty  
        SetThisAndParentsSizeDirty();

        //刷新本节点和所有父节点的显示  
        Refresh(includeParents: true);

        //根节点尺寸变化 -> 需要重新激活VerticalLayout
        this.GetComponentInParent<RecursiveCellParent>().SetLayoutDirty();
    }

    public void Fold()
    {
        if(_unfold)
        {
            ToggleFold();
        }
    }

    public void SetThisAndParentsSizeDirty()
    {
        RecursiveCell currentNode = this;
        while (currentNode != null)
        {
            currentNode._isSizeDirty = true;

            currentNode = currentNode.transform.parent.parent.GetComponent<RecursiveCell>();
        }
    }

    public void Refresh(bool includeParents = false)
    {
        //展开按钮是否显示  
        Transform btnFold = this.CellThis.Find("ButtonFold");
        if (this.activeChildCount == 0)
        {
            btnFold.gameObject.SetActive(false);
        }
        else
        {
            btnFold.gameObject.SetActive(true);
            if (this._unfold == true)
            {
                btnFold.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, -90);
            }
            else
            {
                btnFold.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
            }
        }

        //本节点显示状态  
        if (this._unfold == true)
        {
            float sizeY = this.SizeY;

            var rectt = this.GetComponent<RectTransform>();

            rectt.sizeDelta = new Vector2(rectt.sizeDelta.x, sizeY);

            this.ChildsContainer.sizeDelta = new Vector2(this.ChildsContainer.sizeDelta.x, sizeY - this.CellThis.sizeDelta.y);

            this.ChildsContainer.localScale = Vector3.one;

            this.ChildsContainer.gameObject.SetActive(true);
        }
        else
        {
            float sizeY = this.SizeY;

            var rectt = this.GetComponent<RectTransform>();

            rectt.sizeDelta = new Vector2(rectt.sizeDelta.x, sizeY);

            this.ChildsContainer.sizeDelta = new Vector2(this.ChildsContainer.sizeDelta.x, sizeY - this.CellThis.sizeDelta.y);

            this.ChildsContainer.localScale = Vector3.zero;

            this.ChildsContainer.gameObject.SetActive(false);
        }


        var parent = this.transform.parent.parent.GetComponent<RecursiveCell>();
        if (parent != null) parent.Refresh(includeParents:true);
    }

    private void EnableChild()
    {
        if (this.ChildsContainer.childCount != 0) return;//无需创建第一个子节点  

        if(this.depth > 50)
        {
            Debug.LogAssertion("层级已经超过50！");
            return;
        }

        var firstChild = GameObject.Instantiate(this.gameObject, this.ChildsContainer.position, Quaternion.identity, this.ChildsContainer);
        firstChild.gameObject.SetActive(false);

        firstChild.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        firstChild.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);

        firstChild.GetComponent<RecursiveCell>().depth = this.depth + 1;
        firstChild.GetComponent<RecursiveCell>()._isSizeDirty = true;

        //颜色更深  
        var thisColor = this.CellThis.GetComponent<Image>().color;
        firstChild.GetComponent<RecursiveCell>().CellThis.GetComponent<Image>().color = new Color(thisColor.r * 0.8f, thisColor.g * 0.8f, thisColor.b * 0.8f, thisColor.a);
    }
}
