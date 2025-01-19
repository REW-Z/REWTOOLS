
#if REW_LEGACY
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SubComponent
{
    public MonoBehaviour owner = null;
    public SubComponent(MonoBehaviour owner)
    {
        this.owner = owner;
    }
    public abstract void Update(float deltaTime);

}
#endif