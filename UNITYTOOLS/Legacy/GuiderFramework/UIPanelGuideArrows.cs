

#if REW_LEGACY

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPanelGuideArrows : MonoBehaviour
{
    public GameObject prefab = null;
    public List<UIGuideArrow> guides = new List<UIGuideArrow>();


    void Start()
    {
    }


    void Update()
    {
    }

    public void RegisterGuide(Transform target)
    {
        if (target == null) return;
        if (prefab == null) return;
        if (guides.Any(g => g.target == target)) return;

        GameObject newGuideObj = Instantiate(prefab, this.transform.position, this.transform.rotation, this.transform);

        newGuideObj.SetActive(true);
        UIGuideArrow guide = newGuideObj.GetComponent<UIGuideArrow>();

        guide.target = target;
        guides.Add(guide);
    }


    public void UnregisterGuide(Transform target)
    {
        UIGuideArrow targetGuide = guides.FirstOrDefault(g => g.target = target);
        if (targetGuide != null)
        {
            UnregisterGuide(targetGuide);
        }
    }

    public void UnregisterGuide(UIGuideArrow guide)
    {
        if (this.guides.Contains(guide))
        {
            this.guides.Remove(guide);
            //Debug.Log("Unregister");
        }
    }




    public void OnGuideDestroy(UIGuideArrow guide)
    {
        UnregisterGuide(guide);
    }
}


#endif