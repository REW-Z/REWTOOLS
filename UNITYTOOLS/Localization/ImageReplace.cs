using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ImageReplace
{
    public string imgName;
    
    public Sprite[] sprites;
    
    public ImageReplace() : base()
    {
        imgName = default(string);
        sprites = new Sprite[System.Enum.GetNames(typeof(LocalizationSystem.Language)).Length];
        for (int i = 0; i < sprites.Length; i++) { sprites[i] = null; }
    }
}