using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assertion
{
    public static void Assert(System.Object obj, string log = "")
    {
        if(!(obj != null))
        {
            Debug.LogAssertion(log);
            throw new UnityException("Assertion Exception.");
        }
    }
    public static void Assert(bool value, string log = "")
    {
        if (value == false)
        {
            Debug.LogAssertion(log);
            throw new UnityException("Assertion Exception.");
        }
    }
}
