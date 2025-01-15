
#if ZTOOLS_SOFTGUIDE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideEnabler : MonoBehaviour
{
    public SoftGuide target;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }


    private void OnEnable()
    {
        if(target != null && !target.StartedGuide)
        {
            target.gameObject.SetActive(true);
        }
    }
}
#endif