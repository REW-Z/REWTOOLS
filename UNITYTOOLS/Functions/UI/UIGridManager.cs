using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGridManager : MonoBehaviour
{
    public enum GridType
    {
        FixedRow,
        FixedColumn,
    }
    public enum GridDirection
    {
        RightDown,
    }
    //Init Infos
    public GridType gridType;
    public GridDirection gridDir;
    public int rowCount = 1;
    public int columnCount = 1;


    //settings
    [Header("是否自动排版")]
    public bool autoLayout = false;

    //status
    private int currentGridCount = 0;  public int CurrentGridCount => this.currentGridCount;




    // ------------- mono behaviours ------------- 
    public void Awake()
    {
        currentGridCount = 0;
        for (int i = 0; i < this.transform.childCount; ++i)
        {
            if (this.transform.GetChild(i).gameObject.activeSelf)
                currentGridCount += 1;
        }
    }


    // ------------- methods -------------
    public void SetGrid(int count)
    {
        if (currentGridCount == count) return;

        //Enable / Instantiate Cells
        GameObject original = this.transform.GetChild(0).gameObject;
        RectTransform recttOriginal = original.GetComponent<RectTransform>();

        for (int i = 0; i < count; i++)
        {
            if(i < this.transform.childCount)
            {
                GameObject cell = this.transform.GetChild(i).gameObject;
                cell.SetActive(true);
            }
            else
            {
                GameObject newcell = Instantiate(original, original.transform.position, original.transform.rotation, original.transform.parent);
                var rectt = newcell.GetComponent<RectTransform>();

                //自动排版 (必须开启Autolayout)
                if (autoLayout)
                {
                    if (gridDir == GridDirection.RightDown)
                    {
                        if (gridType == GridType.FixedColumn)
                        {
                            rectt.anchoredPosition = recttOriginal.anchoredPosition + new Vector2((i % columnCount) * rectt.sizeDelta.x, (i / columnCount) * -rectt.sizeDelta.y);
                        }
                        else if (gridType == GridType.FixedRow)
                        {
                            rectt.anchoredPosition = recttOriginal.anchoredPosition + new Vector2((i / rowCount) * rectt.sizeDelta.x, (i % rowCount) * -rectt.sizeDelta.y);
                        }
                    }
                }
                
            }
        }

        //Disable Cell
        for (int i = count; i < this.transform.childCount; i++)
        {
            GameObject cell = this.transform.GetChild(i).gameObject;
            cell.SetActive(false);
        }


        //Reset Content Size(Scroll Rect) (必须开启Autolayout)
        if(autoLayout)
        {
            RectTransform contentThis = this.GetComponent<RectTransform>();
            RectTransform firstChild = this.transform.GetChild(0).GetComponent<RectTransform>();

            if (gridDir == GridDirection.RightDown)
            {
                if (gridType == GridType.FixedColumn)
                {
                    contentThis.sizeDelta = new Vector2(columnCount * firstChild.sizeDelta.x, Mathf.CeilToInt((float)count / (float)columnCount) * firstChild.sizeDelta.y);
                }
                else if (gridType == GridType.FixedRow)
                {
                    contentThis.sizeDelta = new Vector2(Mathf.CeilToInt((float)count / (float)rowCount) * firstChild.sizeDelta.x, rowCount * firstChild.sizeDelta.y);
                }
            }
        }


        //Set Current
        currentGridCount = count;
    }
    
    public void CleanGrids()
    {
        //Destory OBJS
        for (int i = this.transform.childCount - 1; i > 0; i--)
        {
            DestroyImmediate(this.transform.GetChild(i).gameObject);
        }
        this.transform.GetChild(0).gameObject.SetActive(false);
        
        //Reset
        SetGrid(0);
    }
    
    public Transform GetCell(int idx)
    {
        //if idx >= current count  ==> Reset Grid
        if(idx >= currentGridCount)
            SetGrid(idx);

        //return
        return this.transform.GetChild(idx);
    }
}






#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(UIGridManager))]
public class CustomUIGridManager: UnityEditor.Editor
{
    private string textCount;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("+"))
        {
            (target as UIGridManager).SetGrid((target as UIGridManager).CurrentGridCount + 1);
        }
        if (GUILayout.Button("-"))
        {
            (target as UIGridManager).SetGrid(Mathf.Clamp((target as UIGridManager).CurrentGridCount - 1, 0, 999));
        }

        this.textCount = GUILayout.TextField(textCount);
        int count = 1;
        int.TryParse(this.textCount, out count);

        if (GUILayout.Button("SetGrid"))
        {
            (target as UIGridManager).SetGrid(count);
        }

        if (GUILayout.Button("CleanGrid"))
        {
            (target as UIGridManager).CleanGrids();
        }
    }
}
#endif