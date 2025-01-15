using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImageLocalization : ScriptableObject
{
    public ImageReplace[] imgReplaces;

    public ImageLocalization():base()
    {
        imgReplaces = new ImageReplace[0];
    }
}

