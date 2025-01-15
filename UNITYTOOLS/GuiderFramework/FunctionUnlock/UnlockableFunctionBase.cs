using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockableFunctionBase : MonoBehaviour
{
    public virtual void SetFunctionActive(bool active)
    {
    }

    public virtual bool IsFunctionActive()
    {
        return false;
    }
}
