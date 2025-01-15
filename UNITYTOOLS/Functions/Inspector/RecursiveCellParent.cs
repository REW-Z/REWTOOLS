using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;



public class RecursiveCellParent : MonoBehaviour
{
    private bool _isLayoutDirty = false;


    void Start()
    {
        RefreshRoots();
    }

    private void Update()
    {
        if(_isLayoutDirty)
        {
            ResetRootLayout();
        }
    }


    
    private void RefreshRoots()
    {
        for (int i = 0; i < this.transform.childCount; ++i)
        {
            this.transform.GetChild(i).GetComponent<RecursiveCell>()?.Refresh();
        }
    }

    private void ResetRootLayout()
    {
        this.GetComponent<VerticalLayoutGroup>().enabled = false;
        this.GetComponent<VerticalLayoutGroup>().enabled = true;
    }




    public void SetLayoutDirty()
    {
        _isLayoutDirty = true;
    }
}
