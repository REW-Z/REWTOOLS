//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.Events;

//public class ColorBuff
//{
//    public string name;

//    public Color color;

//    public Color outlineColor;

//    public float outlineFactor;

//    public ColorBuff(string name, Color color, Color outlineColor, float outlineFac)
//    {
//        this.name = name;
//        this.color = color;
//        this.outlineColor = outlineColor;
//        this.outlineFactor = outlineFac;
//    }
//}

//public class OverallColorSystem : SubComponent
//{
//    public OverallColorSystem(MonoBehaviour owner) : base(owner)
//    {
//    }

//    public override void Update(float deltaTime)
//    {//doNothing
//    }

//    //colorbuff list
//    private List<ColorBuff> colorBuffs = new List<ColorBuff>();

//    //Status
//    private Color color = Color.white;            public Color Color => this.color;
//    private Color outlineColor = Color.white;     public Color OutlineColor => this.outlineColor;
//    private float outlineFactor = 0f;                public float OutlineFactor => this.outlineFactor;


//    //Event
//    public UnityEvent onColorChange = new UnityEvent();

//    //Cal
//    private void Recalculate()
//    {
//        this.color = Color.white;
//        this.outlineColor = Color.white;
//        this.outlineFactor = 0f;

//        foreach(var clrBuff in colorBuffs)
//        {
//            this.color *= clrBuff.color;

//            if (clrBuff.outlineFactor > 0f)
//            {
//                this.outlineColor *= clrBuff.outlineColor;
//                this.outlineFactor += clrBuff.outlineFactor;
//            }
//        }

//        //event
//        onColorChange.Invoke();
//    }
    
//    //Add / Remove
//    public void AddColorBuff(string name, Color clr, Color outlineClr = default, float outlineFac = 0f)
//    {
//        if (colorBuffs.Any(c => c.name == name)) return;

//        colorBuffs.Add(new ColorBuff(name, clr, outlineClr, outlineFac));

//        //Recalculate
//        Recalculate();
//    }

//    public void RemoveColorBuff(string name)
//    {
//        var target = colorBuffs.FirstOrDefault(c => c.name == name);

//        if (target == null) return;

//        colorBuffs.Remove(target);

//        //Recalculate
//        Recalculate();
//    }
//}
