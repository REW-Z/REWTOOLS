using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if DOTWEEN
using DG.Tweening;
#endif
#if LITJSON
using LitJson;
#endif


public class Utils
{
    /// <summary>
    /// 获取变量名称(效率：每次调用0.01毫秒)
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static string GetVarName<T>(System.Linq.Expressions.Expression<System.Func<string, T>> exp)
    {
        return ((System.Linq.Expressions.MemberExpression)exp.Body).Member.Name;
    }
    /// <summary>
    /// 获取变量名称（效率稍低）
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public static string GetVarName(System.Linq.Expressions.Expression<System.Func<string, object>> exp)
    {
        return ((System.Linq.Expressions.MemberExpression)exp.Body).Member.Name;
    }


    ////以下方法不能获取变量名，只能获取"variable"字符串。
    //public static string GetVarName<T>(T variable)
    //{
    //    System.Linq.Expressions.Expression<System.Func<string, T>> exp =   ( _ => variable) ;
    //    return ((System.Linq.Expressions.MemberExpression)exp.Body).Member.Name;
    //}
    //public static string GetVarName(object variable)
    //{
    //    System.Linq.Expressions.Expression<System.Func<string, object>> exp = (_ => variable);
    //    return ((System.Linq.Expressions.MemberExpression)exp.Body).Member.Name;
    //}
    
}


public static class UtilsScene
{
    public static Transform Find(string path)
    {
        string[] segments = path.Split('/');

        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        Transform currentNode = null;
        for (int i = 0; i < segments.Length; ++i)
        {
            if (i == 0)
            {
                currentNode = roots.FirstOrDefault(obj => obj.name == segments[0]).transform;
            }
            else
            {
                currentNode = currentNode.Find(segments[i]);
            }
            if (currentNode == null) break;
        }

        //找到节点
        return currentNode;
    }

    public static T[] GetComponentsInScene<T>(bool includeInactive = false) where T : UnityEngine.Component
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        List<T> list = new List<T>();

        foreach(var root in roots)
        {
            list.AddRange(root.GetComponentsInChildren<T>(includeInactive));
        }

        return list.ToArray();
    }
}

public enum ColliderType
{
    Mesh,
    Box,
    Capsule,
    Sphere,
    Other,
}
public static class UtilsCollider
{
    public static bool ContainPoint(this Collider col, Vector3 point)
    {
        var hits = Physics.RaycastAll(new Ray(point, Vector3.down), 1000f);
        int hitCount = 0;
        foreach(var hit in hits)
        {
            if(hit.collider == col)
            {
                hitCount += 1;
            }
        }

        if (hitCount % 2 == 1)//奇数次碰撞 -- 在内部
            return true;
        else//偶数次碰撞  -- 在外部  
            return false;
    }

    public static ColliderType GetColliderType(this Collider col)
    {
        var name = col.GetType().Name;
        switch(name)
        {
            case "SphereCollider":
                return ColliderType.Sphere;
            case "CapsuleCollider":
                return ColliderType.Capsule;
            case "BoxCollider":
                return ColliderType.Box;
            case "MeshCollider":
                return ColliderType.Mesh;
            default:
                return ColliderType.Other;
        }
    }

    /// <summary>
    /// 简化过的Collider.ContainPoint方法(速度快几倍)    
    /// </summary>
    public static bool SimpleContainPoint(this Collider col, ColliderType type, Vector3 point, float pointThickness = 0f) //point radius 仅对 sphere 和 capsule 有效
    {
        if (!col.bounds.Contains(point) && col.enabled ==true) return false;//AABB不包含则碰撞体不包含    

        //var lossy = (col.transform.lossyScale.x + col.transform.lossyScale.y + col.transform.lossyScale.z) / 3f ;
        //float localThickness = pointThickness / lossy;
        var pointLocal = col.transform.InverseTransformPoint(point);
        var lossyScale = (col.transform.lossyScale.x + col.transform.lossyScale.y) / 2f;
        var thicknessLocal = pointThickness / lossyScale;

        switch (type)
        {
            case ColliderType.Sphere:
                var sphere = (col as SphereCollider);
                return (pointLocal - sphere.center).sqrMagnitude < Mathf.Pow(sphere.radius + thicknessLocal, 2);
            case ColliderType.Capsule:
                var capsule = (col as CapsuleCollider);
                bool isInCircle = (pointLocal.OnOceanPlane() - capsule.center.OnOceanPlane()).sqrMagnitude < Mathf.Pow(capsule.radius + thicknessLocal, 2);
                bool isInHeight = pointLocal.y < (capsule.center.y + capsule.height * 0.5f) && pointLocal.y > (capsule.center.y - capsule.height * 0.5f);
                return isInCircle && isInHeight;
            case ColliderType.Box:
                var box = (col as BoxCollider);
                Bounds localBounds = new Bounds(box.center, box.size);
                return localBounds.Contains(pointLocal);
            default:
                Debug.LogAssertion("未知碰撞体类型！");
                return col.bounds.Contains(point);
        }
    }
}

public static class UtilsCamera
{
    public static bool OverlapPoint(this Camera cam, Vector3 point, float viewportThreshold = 0.05f)
    {
        Vector3 viewPoint = cam.WorldToViewportPoint(point);
        return (viewPoint.x > (-viewportThreshold) && viewPoint.x < (1f + viewportThreshold) && viewPoint.y > (-viewportThreshold) && viewPoint.y < (1f + viewportThreshold));
    }

}

public static class UtilsScreenAndUI
{
    public static Vector3 WorldToScreenPoint(Vector3 worldPoint)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = GameObject.FindObjectOfType<Camera>();

        //if no cam
        if (cam == null) return default;

        //Calculate
        var result1 = RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
        var result2 = cam.WorldToScreenPoint(worldPoint);

        //Same X,Y Axis
        return result2;
    }


    /// <summary>
    /// 屏幕到Canvas的位置缩放（MatchWidth模式）    
    /// </summary>
    /// <param name="scaler"></param>
    /// <returns></returns>
    public static Vector2 GetScreenToCanvasScale(CanvasScaler scaler)
    {
        var screenRes = new Vector2(Screen.width, Screen.height);
        var screenRatio = (float)Screen.height / (float)Screen.width;

        var trueCvsRes = new Vector2(scaler.referenceResolution.x, scaler.referenceResolution.x * screenRatio);

        float multipilerX = (trueCvsRes.x / screenRes.x);
        float multipilerY = (trueCvsRes.y / screenRes.y);

        return new Vector2(multipilerX, multipilerY);
    }

    /// <summary>
    /// 屏幕点映射到Canvas位置（MatchWidth模式）  
    /// </summary>
    /// <param name="screenPoint"></param>
    /// <param name="scaler"></param>
    /// <returns></returns>
    public static Vector2 ScreenPointToCanvasPos_MatchWidth(Vector3 screenPoint, CanvasScaler scaler)
    {
        Vector2 multipiler = GetScreenToCanvasScale(scaler);

        return new Vector2(screenPoint.x * multipiler.x, screenPoint.y * multipiler.y);
    }
    
    public static Vector2 CalTrueSize(RectTransform rectt)
    {
        return default;

        Canvas canvas = rectt.GetComponentInParent<Canvas>();
        RectTransform canvasRectt = canvas.GetComponent<RectTransform>();
        List<RectTransform> parents = new List<RectTransform>();
        RectTransform current = rectt;
        while (current!= canvasRectt)
        {
            current = current.parent.GetComponent<RectTransform>();
            parents.Add(current);
        }


        //Cal True Size  
        List<RectTransform> childPath = parents; parents.Reverse();
        float canvaswidth = childPath[0].GetComponent<CanvasScaler>().referenceResolution.x;
        float canvasheight = canvaswidth * (Screen.height / Screen.width);

        Vector2 currentSize = new Vector2(canvaswidth, canvasheight);
        foreach(var child in childPath)
        {
            //...
        }
    }
}

public static class RectTransformUtility_ZQJExt
{
    public static Bounds CalculateRelativeRectTransformBounds_IgnoreChildsOfChild(RectTransform root, RectTransform child)
    {
        RectTransform current = child;
        return default;
    }
}
public static class UtilsUGUI
{
    public static void ScrollToElementAfter(this ScrollRect scroll, Transform element, bool directChild = false, Vector2 offsetFromTop = default, float delay = 0.1f)
    {
#if DOTWEEN
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.AppendCallback(() =>
        {
            ScrollToElement(scroll, element, directChild, offsetFromTop);
        });
#endif
    }


#if DOTWEEN
    public static void ScrollToElement(this ScrollRect scroll, Transform element, bool directChild = false, Vector2 offsetFromTop = default)
    {
        if(element.GetComponentInParent<ScrollRect>(true) != scroll)
        {
            return;
        }

        //Debug.LogAssertion("ScrollTo..." + scroll.gameObject.name);

        //Kill old  
        //scroll.content.DOKill();

        //ELE  
        Transform lockElement = element;
        if(directChild)
        {
            Transform curr = element;
            while(curr.parent != scroll.content.transform)
            {
                curr = curr.parent;
            }
            lockElement = curr;
        }

        //Canvas  
        var canvas = scroll.GetComponentInParent<Canvas>();

        //包围盒
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, lockElement);



        //框中央位移
        Vector2 viewCenterPos = Vector2.zero;// -scroll.GetComponent<RectTransform>().sizeDelta / 2f; //-scroll.viewport.sizeDelta / 2f;
        //Debug.LogAssertion("框中央位移：" + viewCenterPos);


        //滑动类型
        if (scroll.horizontal && scroll.vertical)
        {
            float verpos = -bounds.center.y - bounds.extents.y + offsetFromTop.y;
            float horpos = -bounds.center.x - bounds.extents.x + offsetFromTop.x;

            var newPos = viewCenterPos + new Vector2(horpos, verpos);

            if((newPos - scroll.content.anchoredPosition).sqrMagnitude > Mathf.Pow(150f, 2))
            {
                scroll.content.DOAnchorPos(newPos, 0.5f);
            }
        }
        else if(scroll.horizontal)
        {
            var newPos = viewCenterPos + new Vector2(-bounds.center.x - bounds.extents.x, scroll.content.anchoredPosition.y);

            if ((newPos - scroll.content.anchoredPosition).sqrMagnitude > Mathf.Pow(150f, 2))
            {
                scroll.content.DOAnchorPos(newPos, 0.5f);
            }
        }
        else if(scroll.vertical)
        {
            float verpos = -bounds.center.y - bounds.extents.y + offsetFromTop.y;

            Vector2 newPos = viewCenterPos + new Vector2(scroll.content.anchoredPosition.x, verpos);

            if ((newPos - scroll.content.anchoredPosition).sqrMagnitude > Mathf.Pow(150f, 2))
            {
                scroll.content.DOAnchorPos(newPos, 0.5f);
            }
        }
    }

#endif



}


public class UtilsCheckin
{
    /// <summary>
    /// 今天是第几天（0 == firest day  6 == 7th day）
    /// </summary>
    /// <param name="checkins1"></param>
    /// <param name="checkins2"></param>
    /// <returns></returns>
    public static int GetTodayIndex(IList<string> checkins1, IList<string> checkins2) 
    {
        if (checkins1[0] == "" && checkins2[0] == "")
        {
            return 0;
        }
        else
        {
            System.DateTime firstDay;
            if (checkins1[0] != "")
            {
                System.DateTime.TryParse(checkins1[0], out firstDay);
            }
            else
            {
                System.DateTime.TryParse(checkins2[0], out firstDay);
            }

            int num = (System.DateTime.Today - firstDay).Days;

            return num;
        }
    }

    /// <summary>
    /// 第n天是否签到了(0 == 未签到 2 == 全部领取)
    /// </summary>
    /// <param name="checkins1"></param>
    /// <param name="checkins2"></param>
    /// <param name="dayIndex"></param>
    /// <returns></returns>
    public static int DayIdChecked(IList<string> checkins1, IList<string> checkins2,int dayIndex)
    {
        int result = 0;
        if (checkins1[dayIndex] != "") result++;
        if (checkins2[dayIndex] != "") result++;
        return result;
    }
    
    /// <summary>
    /// 签到
    /// </summary>
    /// <param name="checkins1"></param>
    /// <param name="checkins2"></param>
    /// <param name="dayId"></param>
    /// <param name="getall"></param>
    public static void CheckIn(ref IList<string>  checkins1, ref IList<string> checkins2, int dayId, bool getall)
    {
        if (dayId > 6) return;

        checkins1[dayId] = System.DateTime.Today.ToShortDateString();

        if (getall)
        {
            checkins2[dayId] = System.DateTime.Today.ToShortDateString();
        }
    }

    /// <summary>
    /// 清除签到信息
    /// </summary>
    /// <param name="checkins1"></param>
    /// <param name="checkins2"></param>
    public static void CleanCheckins(ref IList<string> checkins1, ref IList<string> checkins2)
    {
        for (int i = 0; i < 7; i++)
        {
            checkins1[i] = "";
            checkins2[i] = "";
        }
    }
}


public class UtilsJson
{
    /// <summary>
    /// Json中文反转义
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string UnescapeJson(string input)
    {
        return System.Text.RegularExpressions.Regex.Unescape(input);
    }
}






#if LITJSON
public class UtilsLitJson
{
    /// <summary>
    /// 获取字符串数组
    /// </summary>
    /// <param name="jarr"></param>
    /// <returns></returns>
    public static List<string> GetStringList(JsonData jarr)
    {
        return GetStringArray(jarr).ToList();
    }
    public static string[] GetStringArray(JsonData jarr)
    {
        string[] arr = new string[jarr.Count];

        for(int i = 0; i < jarr.Count; i++)
        {
            arr[i] = (string)jarr[i];
        }

        return arr;
    }

    /// <summary>
    /// 获取整型数组
    /// </summary>
    /// <param name="jarr"></param>
    /// <returns></returns>
    public static List<int> GetInt32List(JsonData jarr)
    {
        return GetInt32Array(jarr).ToList();
    }
    public static int[] GetInt32Array(JsonData jarr)
    {
        int[] arr  = new int[jarr.Count];

        for (int i = 0; i < jarr.Count; i++)
        {
            arr[i] = (int)jarr[i];
        }

        return arr;
    }

    /// <summary>
    /// 获取数组（泛型）(浮点数保存为字符串)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="jarr"></param>
    /// <returns></returns>
    public static List<T> GetList<T>(JsonData jarr)
    {
        return GetArray<T>(jarr).ToList();
    }
    public static T[] GetArray<T>(JsonData jarr)
    {
        T[] arr = new T[jarr.Count];

        //if(typeof(T) == typeof(System.String))
        //{
        //}

        switch (typeof(T).Name)
        {
            case "String":
                {
                    for (int i = 0; i < jarr.Count; i++)
                    {
                        arr[i] = (T)System.Convert.ChangeType((string)jarr[i], typeof(T));// jarr[i];
                    }

                    return arr;
                }
            case "Int32":
                {
                    for (int i = 0; i < jarr.Count; i++)
                    {
                        arr[i] = (T)System.Convert.ChangeType((int)jarr[i], typeof(T));// jarr[i];
                    }

                    return arr;
                }
            case "Single":
                {
                    for (int i = 0; i < jarr.Count; i++)
                    {
                        float result;
                        float.TryParse((string)jarr[i], out result);
                        arr[i] = (T)System.Convert.ChangeType(result, typeof(T));// jarr[i];
                    }

                    return arr;
                }
            default:
                return null;
        }
        
    }

    /// <summary>
    /// 序列化为Json数组（泛型）(浮点数用字符串)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static JsonData ToJsonData<T>(T[] arr)
    {
        JsonData jArr = new JsonData(); jArr.SetJsonType(JsonType.Array);
        
        switch(typeof(T).Name)
        {
            case "String":
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        jArr.Add(i);
                        jArr[i] = (string)(arr[i] as System.String);
                    }

                    return jArr;
                }
            case "Int32":
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        jArr.Add(i);
                        jArr[i] = (System.Convert.ToInt32(arr[i]));
                    }

                    return jArr;
                }
            case "Single":
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        jArr.Add(i);
                        jArr[i] = (System.Convert.ToString( arr[i]));
                    }

                    return jArr;
                }
            default:
                return null;
        }
    }



    
    /// <summary>
    /// 获取Json字段（泛型）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="jdata"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static T GetValue<T>(JsonData jdata, params string[] allPossibleFieldNames) where T: struct
    {
        foreach (var fieldName in allPossibleFieldNames)
        {
            if (jdata.ContainsKey(fieldName))
            {
                object value = jdata[fieldName];
                return (T)value;
            }
        }

        return default(T);
    }
}
#endif


#if DOTWEEN
public static class UtilsDoTween
{

}
#endif









//-------------------------------------------------------------------------------- Extensions ------------------------------------------------------------------------------------



public static class TransformExtension
{
    /// <summary>
    /// 查找指定名称子物体的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="trans"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T Find<T>(this Transform trans, string name) where T : UnityEngine.Component
    {
        //先在第一层子物体查找
        foreach(Transform child in trans)
        {
            if (child.gameObject.name != name) continue;

            T com = child.GetComponent<T>();
            if (com != null)
            {
                return com;
            }
        }

        //在所有子物体查找
        var coms = trans.GetComponentsInChildren<T>(true);
        foreach(var com in coms)
        {
            if(com.gameObject.name == name)
            {
                return com;
            }
        }

        return null;
    }

    /// <summary>
    /// 模糊查找子物体
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="nameContain"></param>
    /// <returns></returns>
    public static Transform FindContain(this Transform trans, string nameContain)
    {
        foreach (Transform child in trans)
        {
            if (child.gameObject.name.Contains(nameContain))
            {
                return child;
            }
        }

        return null;
    }

    /// <summary>
    /// 设置为Active并弹出
    /// </summary>
    /// <param name="gameobj"></param>
    /// <param name="active"></param>
    /// <param name="duration">弹出时间</param>
    public static void PopActive(this GameObject gameobj, bool active, float duration = 0.2f)
    {
        gameobj.SetActive(active);

#if DOTWEEN
        if (active)
        {
            gameobj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            gameobj.transform.DOScale(Vector3.one, duration).SetUpdate(UpdateType.Normal, true).SetEase(Ease.OutBack);
        }
#endif
    }

}


public static class ComponentExtensions
{
#if !UNITY_2020_1_OR_NEWER
    /// <summary>
    /// 获取子节点中的组件（包含了IncludeInactve参数）（Unity后续版本才加入了该参数）
    /// </summary>
    public static T GetComponentInChildren<T>(this Component thisCom, bool includeInactive) where T : Component
    {
        return thisCom.GetComponentsInChildren<T>(includeInactive).FirstOrDefault();
    }
    /// <summary>
    /// 获取父节点中的组件（包含了IncludeInactve参数）（Unity后续版本才加入了该参数）
    /// </summary>
    public static T GetComponentInParent<T>(this Component thisCom, bool includeInactive) where T : Component
    {
        return thisCom.GetComponentsInParent<T>(includeInactive).FirstOrDefault();
    }


    public static T FindComponentThisScene<T>(this UnityEngine.SceneManagement.Scene scene) where T : UnityEngine.Component
    {
        return UnityEngine.Object.FindObjectsOfType<T>().FirstOrDefault(obj => obj.gameObject.scene == scene);
    }
#endif
}


public static class AnimatorExtensions
{
    /// <summary>
    /// 重设所有Trigger
    /// </summary>
    public static void ResetAllTriggers(this Animator animator)
    {
        foreach (var trigger in animator.parameters)
        {
            if (trigger.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(trigger.name);
        }
    }
}


public static class VectorExtension
{
    public static Vector3 OnOceanPlane(this Vector3 v3)
    {
        return new Vector3(v3.x, 0f, v3.z);
    }
}



public static class UGUIExtensions
{
    /// <summary>
    /// 保持高度使用原始长宽比例
    /// </summary>
    public static void SetNativeScaleKeepHeight(this Image img)
    {
        Vector2 sizeDeltaPrevious = img.GetComponent<RectTransform>().sizeDelta;

        img.SetNativeSize();

        Vector2 sizeDeltaNative = img.GetComponent<RectTransform>().sizeDelta;

        float factor = sizeDeltaPrevious.y / sizeDeltaNative.y;

        img.GetComponent<RectTransform>().sizeDelta *= factor;
    }

    /// <summary>
    /// /保持宽度使用原始长宽比例
    /// </summary>
    public static void SetNativeScaleKeepWidth(this Image img)
    {
        Vector2 sizeDeltaPrevious = img.GetComponent<RectTransform>().sizeDelta;

        img.SetNativeSize();

        Vector2 sizeDeltaNative = img.GetComponent<RectTransform>().sizeDelta;

        float factor = sizeDeltaPrevious.x / sizeDeltaNative.x;

        img.GetComponent<RectTransform>().sizeDelta *= factor;
    }


    /// <summary>
    /// 自动缩小使用原始长宽比例
    /// </summary>
    /// <param name="img"></param>
    public static void SetNativeScaleAutoSmall(this Image img)
    {
        var imageSizeExist = imageSizeList.FirstOrDefault(imgsize => imgsize.img == img);
        if(imageSizeExist != null)
        {
            img.GetComponent<RectTransform>().sizeDelta = imageSizeExist.size;
        }
        else
        {
            imageSizeList.Add(new ImageSizeRestore(img, img.GetComponent<RectTransform>().sizeDelta));
        }

        Vector2 sizeDeltaPrevious = img.GetComponent<RectTransform>().sizeDelta;

        img.SetNativeSize();

        Vector2 sizeDeltaNative = img.GetComponent<RectTransform>().sizeDelta;

        float factorx = sizeDeltaPrevious.x / sizeDeltaNative.x;
        float factory = sizeDeltaPrevious.y / sizeDeltaNative.y;

        if (factorx < factory)
        {
            img.GetComponent<RectTransform>().sizeDelta *= factorx;
        }
        else
        {
            img.GetComponent<RectTransform>().sizeDelta *= factory;
        }
    }
    public class ImageSizeRestore
    {
        public Image img;
        public Vector2 size;
        public ImageSizeRestore(Image imgNew, Vector2 sizeNew) { img = imgNew; size = sizeNew; }
    }
    public static List<ImageSizeRestore> imageSizeList = new List<ImageSizeRestore>();
}



// --------------------------------------------------------------------- Custom UEvent ---------------------------------------------------------------------

public class ZUnityEvent<T> : UnityEvent<T> { };
public class ZUnityEvent<T, T2> : UnityEvent<T, T2> { };
public class ZUnityEvent<T, T2, T3> : UnityEvent<T, T2, T3> { };



//-------------------------------------------------------------------------------- Rx ------------------------------------------------------------------------------------



namespace UtilsRx
{
    public class MyEvent<T> : IObservable<T>
    {

        private List<IObserver<T>> observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!this.observers.Contains(observer))
            {
                this.observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
        
        /// <summary>
        /// 测试函数
        /// </summary>
        /// <param name="value"></param>
        public void ChangeValue(T value)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(value);
            }
        }
    }


    public class MyListener<T> : IObserver<T>
    {
        private IDisposable unsubscriber;
        public T currentValue;


        public void Subscribe(IObservable<T> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public void Unsubscribe()
        {
            unsubscriber.Dispose();
        }


        public void OnCompleted()
        {
            this.Unsubscribe();
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            currentValue = value;

            Debug.Log("New Value: " + currentValue.ToString());
        }
    }


    public class SimpleReactiveProperty<T> : IObservable<T>
    {
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<T>> _observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }

        private List<IObserver<T>> observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!this.observers.Contains(observer))
            {
                this.observers.Add(observer);
            }

            return new Unsubscriber(observers, observer);
        }


        private T v;

        public T Value
        {
            get { return v; }
            set
            {
                if (!value.Equals(v))
                {
                    v = value;
                    RaiseOnNext(value);
                }
            }
        }



        private void RaiseOnNext(T newValue)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(newValue);
            }
        }
    }
}