using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.UI;
using System.Reflection;

public static class Extension
{

    public static bool IsValidEmailAddress(this string emailaddress)
    {
        Regex rx = new Regex(
         @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
        return rx.IsMatch(emailaddress);
    }

    public static string Repeat(this string st, int number)
    {
        string result = "";
        for (int i = 0; i < number; i++)
            result += st;
        return result;
    }
    public static string FormatNumber(this int num)
    {
        string tk = num.ToString();
        List<string> lst = new List<string>();
        for (int i = tk.Length - 1; i >= 2; i -= 3)
        {
            lst.Add(tk.Substring(i - 2, 3));
        }
        int prefix = tk.Length % 3;
        if (prefix != 0)
        {
            lst.Add(tk.Substring(0, prefix));
        }
        string result = "";
        for (int i = lst.Count - 1; i >= 0; i--)
        {
            result += lst[i];
            if (i != 0)
                result += ",";
        }
        return result;
    }

    public static Rect GetRect(this SpriteRenderer map)
    {
        float width = map.transform.localScale.x * map.sprite.texture.width / map.sprite.pixelsPerUnit;
        float height = map.transform.localScale.y * map.sprite.texture.height / map.sprite.pixelsPerUnit;
        Vector2 anchor = map.transform.position.ToV2() - new Vector2(width / 2, height / 2);
        return new Rect(anchor, new Vector2(width, height));
    }

    public static Transform GetClosest(this Vector2 input, List<Transform> vertices)
    {
        Transform result = null;
        float min = 99999;
        for (int i = 0; i < vertices.Count; i++)
        {
            float dist = Vector2.Distance(vertices[i].position2D(), input);
            if (dist < min)
            {
                min = dist;
                result = vertices[i];
            }
        }
        return result;
    }

    public static Vector2 World2Geo(this Vector2 pos, Rect rect)
    {
        Vector2 edge_a = ((pos - rect.min) / new Vector2(rect.width, rect.height) - new Vector2(0.5f, 0.5f)) * new Vector2(360, 180);
        return new Vector2((float)Math.Round(edge_a.x, 1), (float)Math.Round(edge_a.y, 1));
    }

    public static Vector2 World2UV(this Vector2 pos, Rect rect)
    {
        Vector2 edge_a = ((pos - rect.min) / new Vector2(rect.width, rect.height));
        return new Vector2((float)Math.Round(edge_a.x, 1), (float)Math.Round(edge_a.y, 1));
    }

    public static Vector2 Geo2World(this Vector2 geo, Rect rect)
    {
        //y first
        geo.x /= 360;
        geo.y /= 180;
        geo += new Vector2(0.5f, 0.5f);
        geo.x *= rect.width;
        geo.y *= rect.height;
        geo += rect.min;
        return geo;
    }

    public static void SetColor(this SpriteRenderer sp, Color col)
    {
        sp.color = new Color(col.r, col.g, col.b, sp.color.a);
    }
    public static void SetAlpha(this SpriteRenderer sp, float a)
    {
        sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, a);
    }

    //Data type 
    public static int ToInt(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 0;
        }
        return int.Parse(str);
    }
    public static float ToFloat(this string str)
    {
        if (str.Contains("f")) str = str.Replace("f", "");
        return float.Parse(str);
    }

    public static float ToPercentageFloat(this string str)
    {
        if (str.Contains("f")) str = str.Replace("f", "");
        if (str.Contains("%"))
        {
            str = str.Replace("%", "");
            return float.Parse(str) / 100;
        }
        else
        {
            return float.Parse(str);
        }
    }

    public static Vector3 ToV3(this Vector2 v, float z = 0)
    {
        return new Vector3(v.x, v.y, z);
    }
    public static Vector2 ToV2(this Vector3 v, float z = 0)
    {
        return new Vector2(v.x, v.y);
    }

    public static void ClearChildFromEnd(this Transform t, int start_index = 0)
    {
        bool isPlaying = Application.isPlaying;
        if (t.childCount <= start_index)
            return;
        while (t.childCount != start_index)
        {
            Transform child = t.GetChild(t.childCount - 1);

            if (isPlaying)
            {
                child.SetParent(null);
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }
    //Transform
    public static void ClearChild(this Transform t, int start_index = 0)
    {
        bool isPlaying = Application.isPlaying;
        if (t.childCount <= start_index)
            return;
        while (t.childCount != start_index)
        {
            Transform child = t.GetChild(0);

            if (isPlaying)
            {
                child.SetParent(null);
                UnityEngine.Object.Destroy(child.gameObject);
            }
            else UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }
    //Transform
    public static List<Transform> GetAllChild(this Transform t)
    {
        bool isPlaying = Application.isPlaying;
        int count = t.childCount;
        List<Transform> childs = new List<Transform>();
        for (int i = 0; i < count; i++)
            childs.Add(t.GetChild(i));
        return childs;
    }
    public static Vector2 position2D(this Transform t)
    {
        return t.position.ToV2();
    }

    public static bool IsVisible(this Transform t, Camera cam)
    {
        Vector2 pos = cam.WorldToViewportPoint(t.position);
        if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1)
            return false;
        return true;
    }
    public static bool IsVisible(this Vector3 t, Camera cam)
    {
        Vector2 pos = cam.WorldToViewportPoint(t);
        if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1)
            return false;
        return true;
    }
    public static bool IsVisible(this Vector2 t, Camera cam)
    {
        Vector2 pos = cam.WorldToViewportPoint(t);
        if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1)
            return false;
        return true;
    }
    public static void SetPosX(this Transform t, float x)
    {
        t.position = new Vector3(x, t.position.y, t.position.z);
    }
    public static void SetPosY(this Transform t, float y)
    {
        t.position = new Vector3(t.position.x, y, t.position.z);
    }
    public static void SetPosZ(this Transform t, float z)
    {
        t.position = new Vector3(t.position.x, t.position.y, z);
    }
    public static void SetLocalX(this Transform t, float x)
    {
        t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
    }
    public static void SetLocalY(this Transform t, float y)
    {
        t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
    }
    public static void SetLocalZ(this Transform t, float z)
    {
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
    }
    public static void SetRotX(this Transform t, float x)
    {
        t.rotation = Quaternion.Euler(x, t.eulerAngles.y, t.eulerAngles.z);
    }
    public static void SetRotY(this Transform t, float y)
    {
        t.rotation = Quaternion.Euler(t.eulerAngles.x, y, t.eulerAngles.z);
    }
    public static void SetRotZ(this Transform t, float z)
    {
        t.rotation = Quaternion.Euler(t.eulerAngles.x, t.eulerAngles.y, z);
    }

    //Scale
    public static void SetScaleX(this Transform t, float x)
    {
        t.localScale = new Vector3(x, t.localScale.y, t.localScale.z);
    }

    public static void SetScaleY(this Transform t, float y)
    {
        t.localScale = new Vector3(t.localScale.x, y, t.localScale.z);
    }


    //Rigbody2D
    public static void SetVX(this Rigidbody2D r, float x)
    {
        if (!float.IsInfinity(x))
            r.velocity = new Vector2(x, r.velocity.y);
    }
    public static void SetVY(this Rigidbody2D r, float y)
    {
        if (!float.IsInfinity(y))
            r.velocity = new Vector2(r.velocity.x, y);
    }
    //List&[]
    public static float GetRotFrom(this Vector2 ori, Vector2 target)
    {
        if (!Camera.main)
        {
            return 0f;
        }

        Vector2 dif = target - ori;
        dif.Normalize();
        return (Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg);
    }

    public static void TargetRotateTo(this Transform ori, Vector2 target)
    {
        ori.rotation = Quaternion.Euler(new Vector3(0, 0, ori.position2D().GetRotFrom(target)));
    }

    public static Vector2 GetMousePos()
    {
        if (!Camera.main)
        {
            return Vector2.zero;
        }
        return (Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    public static Transform GetMinDis(this Transform ori, List<Transform> trans)
    {
        float[] dis = new float[trans.Count];
        for (int i = 0; i < trans.Count; i++)
        {
            dis[i] = (GetDis(ori, trans[i]));
        }
        float min = Mathf.Min(dis);

        for (int i = 0; i < dis.Length; i++)
        {
            if (min == dis[i])
            {
                return trans[i];
            }
        }
        return null;
    }

    public static bool InRectangle(this Transform t, Vector2 pos)
    {
        float min_x = t.position.x - t.localScale.x * 0.5f;
        float max_x = t.position.x + t.localScale.x * 0.5f;
        float min_y = t.position.y - t.localScale.y * 0.5f;
        float max_y = t.position.y + t.localScale.y * 0.5f;

        return !(pos.x < min_x || pos.x > max_x || pos.y < min_y || pos.y > max_y);
    }


    public static float GetDis(Transform ori, Transform target)
    {
        if (target != null)
        {

            return (target.transform.position2D() - ori.transform.position2D()).magnitude;
        }
        else
        {
            return 0f;
        }
    }

    public static int GetDirByFour(Vector2 ori, Vector2 target)
    {
        Vector2 s = target - ori;
        float d = Mathf.Atan2(s.y, s.x) * Mathf.Rad2Deg;
        if ((d > 0 && d <= 45) || (d <= 0 && d > -45))
        {
            return 1;
        }
        else if ((d > 45 && d <= 135))
        {
            return 2;
        }
        else if ((d > 135 && d <= 180) || (d < -135 && d >= -180))
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }

    public static Transform GetNearByTag(this Transform ori, Vector2 offset, float r, string tag, LayerMask layer)
    {
        List<Transform> resList = new List<Transform>();
        Collider2D[] colls = Physics2D.OverlapCircleAll(ori.position + new Vector3(offset.x, offset.y, 0), r, layer);
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].tag == tag)
            {
                resList.Add(colls[i].transform);
            }
        }
        float min = 9999;
        int index = -1;
        for (int i = 0; i < resList.Count; i++)
        {
            float res = (resList[i].position - ori.position).magnitude;
            if (res < min)
            {
                min = res;
                index = i;
            }
        }

        if (index != -1)
        {
            return resList[index];
        }
        return null;
    }

    public static void SlowByDamping(this Rigidbody2D r, bool slowX, bool slowY, float damping)
    {
        if (Time.timeScale == 0)
            return;
        float rad = Mathf.Atan2(r.velocity.y, r.velocity.x);
        float xPer = Mathf.Abs(Mathf.Cos(rad));
        float yPer = Mathf.Abs(Mathf.Sin(rad));
        if (slowX)
        {
            if (r.velocity.x > 0.01f)
            {
                float result = r.velocity.x - (damping * Time.deltaTime * xPer);
                r.velocity = new Vector2(result < 0 ? 0 : result, r.velocity.y);
            }
            else if (r.velocity.x < -0.01f)
            {
                float result = r.velocity.x + (damping * Time.deltaTime * xPer);
                r.velocity = new Vector2(result > 0 ? 0 : result, r.velocity.y);
            }
            else
                r.velocity = new Vector2(0, r.velocity.y);
        }
        if (slowY)
        {
            if (r.velocity.y > 0.01f)
            {
                float result = r.velocity.y - (damping * Time.deltaTime * yPer);
                r.velocity = new Vector2(r.velocity.x, result < 0 ? 0 : result);
            }
            else if (r.velocity.y < -0.01f)
            {
                float result = r.velocity.y + (damping * Time.deltaTime * yPer);
                r.velocity = new Vector2(r.velocity.x, result > 0 ? 0 : result);
            }
            else
                r.velocity = new Vector2(r.velocity.x, 0);
        }
    }
    public static Vector2 SlowByDamping(this Vector2 r, bool slowX, bool slowY, float damping)
    {
        if (Time.timeScale == 0)
            return r;
        //float rad = Mathf.Atan2(r.y, r.x);
        //float xPer = Mathf.Abs(Mathf.Cos(rad));
        //float yPer = Mathf.Abs(Mathf.Sin(rad));

        if (slowX)
        {
            if (r.x > 0.01f)
            {
                float result = r.x - (damping * Time.deltaTime);
                r.x = (result < 0 ? 0 : result);
            }
            else if (r.x < -0.01f)
            {
                float result = r.x + (damping * Time.deltaTime);
                r.x = (result > 0 ? 0 : result);
            }
            else
                r.x = (0);
        }
        if (slowY)
        {
            if (r.y > 0.01f)
            {
                float result = r.y - (damping * Time.deltaTime);
                r.y = (result < 0 ? 0 : result);
            }
            else if (r.y < -0.01f)
            {
                float result = r.y + (damping * Time.deltaTime);
                r.y = (result > 0 ? 0 : result);
            }
            else
                r.y = (0);
        }
        return r;
    }
    public static bool IsFreeze(this Rigidbody2D r)
    {
        return r.velocity.magnitude < 0.1f;
    }

    public static bool Close(this Vector3 r, Vector3 target, float distance)
    {
        return Vector2.Distance(r, target) < distance;
    }
    public static bool CloseX(this Vector3 r, Vector3 target, float distance)
    {
        return Mathf.Abs(target.x - r.x) < distance;
    }
    public static bool CloseY(this Vector3 r, Vector3 target, float distance)
    {
        return Mathf.Abs(target.y - r.y) < distance;
    }
    public static string ConcatBy(this string[] list, string add)
    {
        string over = "";
        for (int i = 0; i < list.Length; i++)
            over += list[i] + (i == list.Length - 1 ? "" : add);
        return over;
    }

    public static bool CanDisplay(this string st)
    {
        if (st != null && st != "" && st != "0")
            return true;
        else
            return false;
    }
    //List

    private static System.Random rng = new System.Random();
    // public static void Shuffle<T>(this IList<T> list)
    // {
    //     int n = list.Count;
    //     while (n > 1)
    //     {
    //         n--;
    //         int k = rng.Next(n + 1);
    //         T value = list[k];
    //         list[k] = list[n];
    //         list[n] = value;
    //     }
    // }
    public static string Color(this string st, string code)
    {
        if (code == "")
            return st;
        return $"<color=#{code}>{st}</color>";
    }

    public static string RemoveColor(this string st)
    {
        return Regex.Replace(st, @" ?\<.*?\>", string.Empty);
    }

    public static string Color(this string st, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{st}</color>";
    }

    public static string GetColorText(this Color col)
    {
        return $"{ColorUtility.ToHtmlStringRGBA(col)}";
    }
    public static string ColorNumber(this float st, string code)
    {
        if (st == 0)
            return "";
        string sign = "";
        if (st > 0)
            sign = "+";
        //if (st<0)
        //    sign = "-";
        return $"<color=#{code}>{sign}{st}</color>";
    }
    public static string ColorNumber(this float st, Color color)
    {
        if (st == 0)
            return "";
        string sign = "";
        if (st > 0)
            sign = "+";
        //if (st<0)
        //    sign = "-";
        return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{sign}{st}</color>";
    }
    public static string ColorNumberStatus(this float st)
    {
        string code = "2F9F4B";
        if (st < 0)
        {
            code = "D42339";
        }

        if (st == 0)
            return "";
        string sign = "";
        if (st > 0)
            sign = "+";
        //if (st<0)
        //    sign = "-";
        return $"<color=#{code}>{sign}{st}</color>";
    }
    public static string ColorNumberStatus(this int st)
    {
        string code = "2F9F4B";
        if (st < 0)
        {
            code = "D42339";
        }

        if (st == 0)
            return "";
        string sign = "";
        if (st > 0)
            sign = "+";
        //if (st<0)
        //    sign = "-";
        return $"<color=#{code}>{sign}{st}</color>";
    }

    public static string ColorNumberStatusCombine(this int st, int value)
    {
        if (value == 0)
        {
            return st.ToString();
        }
        string code = "2F9F4B";
        if (value < 0)
            code = "D42339";

        if (st == 0)
            return "";
        return $"<color=#{code}>{st}</color>";
    }

    public static void SwapItems<T>(this List<T> list, int idxX, int idxY)
    {
        if (idxX != idxY)
        {
            T tmp = list[idxX];
            list[idxX] = list[idxY];
            list[idxY] = tmp;
        }
    }
    public static void MoveIndex<T>(this List<T> list, int srcIdx, int destIdx)
    {
        if (srcIdx != destIdx)
        {
            list.Insert(destIdx, list[srcIdx]);
            list.RemoveAt(destIdx < srcIdx ? srcIdx + 1 : srcIdx);
        }
    }

    public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform == null) return;

        Vector2 size = rectTransform.rect.size;
        Vector2 deltaPivot = rectTransform.pivot - pivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        rectTransform.pivot = pivot;
        rectTransform.localPosition -= deltaPosition;
    }

    public static bool StringListSame(this List<string> a, List<string> b)
    {
        if (a == null || b == null)
        {
            return false;
        }
        if (a.Count != b.Count)
        {
            return false;
        }
        for (int i = 0; i < a.Count; i++)
        {
            if (b[i].Equals(a[i]) == false)
            {
                return false;
            }
        }
        return true;
    }

    public static void Active(this Transform transform, int index)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == index)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public static Vector2 Rect2World(this RectTransform rect)
    {
        return Camera.main.ScreenToWorldPoint(rect.position);
    }
    public static Vector2 World2Rect(this Vector2 pos)
    {
        return RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
    }
    public static Vector2 World2RectLocal(this Vector2 pos)
    {
        Vector2 canvas_scale = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().sizeDelta;
        float sc = canvas_scale.x / Screen.width;
        pos = pos.World2Rect() - new Vector2(Screen.width / 2, Screen.height / 2);
        pos *= sc;
        return pos;
    }

    public static void Invoke(this string str)
    {
        string[] parse = str.Split(' ');
        System.Reflection.MethodInfo method = Type.GetType(parse[0]).GetMethod(parse[1], System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            method.Invoke(null, null);
        }
    }

    public static string Int2Time(this int sec)
    {
        return $"{sec / 60} : {sec % 60}";
    }

    public static void RemoveNull<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null)
            {
                list.RemoveAt(i);
            }
        }
    }

    public static IEnumerator MoveOverSpeed(this GameObject objectToMove, Vector3 end, float speed)
    {
        // speed should be 1 unit per second
        while (objectToMove.transform.position != end)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator LocalMoveOverSpeed(this GameObject objectToMove, Vector3 end, float speed)
    {
        // speed should be 1 unit per second
        while (objectToMove.transform.localPosition != end)
        {
            objectToMove.transform.localPosition = Vector3.MoveTowards(objectToMove.transform.localPosition, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }
    public static IEnumerator LocalMoveOverSeconds(this GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.localPosition;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.localPosition = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.localPosition = end;
    }

    public static List<GameObject> CheckGameObjectListForGroupsOfN(this List<GameObject> gameObjects, string name, int match_num)
    {
        List<GameObject> groupsOfN = new List<GameObject>();

        groupsOfN = gameObjects.FindAll(c => c.name == name);
        if (groupsOfN.Count < match_num)
        {
            groupsOfN.Clear();
        }
        else
        {
            gameObjects.RemoveAll(c => c.name == name);
        }

        return groupsOfN;
    }

    public static void CallInstasnce(this string function, string instance)
    {
        if (string.IsNullOrEmpty(function))
        {
            return;
        }
        string[] parse = function.Split(' ');
        string type = parse[0];
        string func = parse[1];

        Type t = Type.GetType(type);
        MethodInfo method = t.GetMethod(func, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        List<object> args = new List<object>();
        for (int i = 0; i < parse.Length - 2; i++)
            args.Add(parse[i + 2]);

        object result = null;
        if (string.IsNullOrEmpty(instance) == false)
        {
            var all = GameObject.FindObjectsOfType(t);
            for (int i = 0; i < all.Length; i++)
            {
                MonoBehaviour ins = all[i] as MonoBehaviour;
                if (ins.gameObject.name == instance)
                {
                    result = ins;
                    break;
                }
            }
        }
        if (result == null) result = GameObject.FindObjectOfType(t);
        method.Invoke(result, args.ToArray());
    }

    public static void CallStatic(this string function)
    {
        if (string.IsNullOrEmpty(function))
        {
            return;
        }
        string[] parse = function.Split(' ');
        string type = parse[0];
        string func = parse[1];

        Type t = Type.GetType(type);
        MethodInfo method = t.GetMethod(func, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        List<object> args = new List<object>();
        for (int i = 0; i < parse.Length - 2; i++)
            args.Add(parse[i + 2]);
        method.Invoke(null, args.ToArray());
    }

}