using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public static class UtilityFunctions
{
    /// <summary>
    /// This Function turns a (0° - 360°) into a (-180° - 180°) and then Clamps it between lowerClamp and upperClamp.
    /// (a 5° angle stays 5°, but a 355° will be a -5°)
    /// </summary>
    public static float AngleClamping(float angle, float lowerClamp, float upperClamp)
    {
        if (angle > 180) 
            angle -= 360; // jetzt in -180 bis +180

        return Mathf.Clamp(angle, lowerClamp, upperClamp);
    }

    /// <summary>
    /// This squishes and stretches a value, depending if its closer to the min or max, or gets clamp when outside of range. (exponent: 0.5 => stretch to squish | exponent: 2 => squish to stretch)
    /// </summary>
    public static float CurveValue(float value, float rangeMin, float rangeMax, float exponent)
    {
        Mathf.Clamp(value, rangeMin, rangeMax);
        float between0and1 = Mathf.InverseLerp(rangeMin, rangeMax, value);
        float curved = Mathf.Pow(between0and1, exponent);
        return Mathf.Lerp(rangeMin, rangeMax, curved);
    }

    /// <summary>
    /// Define a range, if the Value is outside it gets clamp, then the value gets refittet (stretched or squished) to a new range. 
    /// </summary>
    public static float RefitRange(float value, float startRange, float endRange, float newMin, float newMax)
    {
        float valueClamped = Mathf.Clamp(value, startRange, endRange);
        float relativeValue = Mathf.InverseLerp(startRange, endRange, valueClamped);
        return Mathf.Lerp(newMin, newMax, relativeValue);
    }






















    public static void BetterWhile(bool condition, Action action, int iterations = 10000, Action target = null, Action fallback = null)
    {
        var count = 0;
        while (count < iterations && condition)
        {
            count++;
            action?.Invoke();
        }

        if (count >= iterations)
        {
            fallback?.Invoke();
            throw new Exception("WHILE LOOP BROKE!");
        }
        else
            target?.Invoke();
    }

    /// <summary>
    /// Returns the Mathf.Sign of a float, but returns 0, if it is zero instead of 1
    /// </summary>
    public static int SignWithZero(float value)
    {
        if (Mathf.Approximately(value, 0))
            return 0;
        else if (value < 0)
            return -1;
        else if (value > 0)
            return 1;

        return 0;
    }

    public static Vector3 GetSpriteWorldCenter(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            //DebugX.LogError("SpriteRenderer is null");
            return Vector3.zero;
        }

        var worldCenter     = spriteRenderer.transform.TransformPoint(spriteRenderer.sprite.bounds.center);

        return worldCenter;
    }

    public static Vector3 GetColliderMinBoxSize(Collider2D collider)
    {
        Vector3 colliderMinBoxSize = Vector3.zero;

        if (collider is PolygonCollider2D)
        {
            float xMin = 100;
            float yMin = 100;
            float xMax = -100;
            float yMax = -100;

            Vector2[] points = (collider as PolygonCollider2D).points;

            foreach (Vector2 point in points)
            {
                if (point.x < xMin) xMin = point.x;
                if (point.y < yMin) yMin = point.y;
                if (point.x > xMax) xMax = point.x;
                if (point.y > yMax) yMax = point.y;
            }

            colliderMinBoxSize = new Vector3(xMax - xMin, yMax - yMin, 0);
        }
        else if (collider is BoxCollider2D)
            colliderMinBoxSize = new Vector3((collider as BoxCollider2D).size.x, (collider as BoxCollider2D).size.y, 0);
        else if (collider is CircleCollider2D)
            colliderMinBoxSize = new Vector3((collider as CircleCollider2D).radius * 2, (collider as CircleCollider2D).radius * 2, 0);
        else if (collider is CapsuleCollider2D)
        {
            colliderMinBoxSize = (collider as CapsuleCollider2D).direction == CapsuleDirection2D.Vertical ?
                new Vector3((collider as CapsuleCollider2D).size.x, Math.Max((collider as CapsuleCollider2D).size.y, (collider as CapsuleCollider2D).size.x), 0)
                : new Vector3(Math.Max((collider as CapsuleCollider2D).size.y, (collider as CapsuleCollider2D).size.x), (collider as CapsuleCollider2D).size.y, 0);
        }

        return colliderMinBoxSize;
    }
    public static Vector3 GetVectorToColliderCenter(Collider2D collider)
    {
        //i do use this, bc the collider.bounds.size is not reliable
        Vector3 vectorToColliderCenter = Vector3.zero;

        if (collider is PolygonCollider2D)
        {
            float xMin = 100;
            float yMin = 100;
            float xMax = -100;
            float yMax = -100;

            Vector2[] points = (collider as PolygonCollider2D).points;

            foreach (Vector2 point in points)
            {
                if (point.x < xMin) xMin = point.x;
                if (point.y < yMin) yMin = point.y;
                if (point.x > xMax) xMax = point.x;
                if (point.y > yMax) yMax = point.y;
            }

            vectorToColliderCenter = new Vector3(((xMax - xMin) / 2) + xMin, ((yMax - yMin) / 2) + yMin, 0) + new Vector3(collider.offset.x, collider.offset.y, 0);
        }
        else if (collider is BoxCollider2D)
            vectorToColliderCenter = collider.offset;
        else if (collider is CircleCollider2D)
            vectorToColliderCenter = collider.offset;
        else if (collider is CapsuleCollider2D)
            vectorToColliderCenter = collider.offset;

        return vectorToColliderCenter;
    }

    public static RaycastHit2D[] ColliderBoxCast(Transform transform, BoxCollider2D collider, LayerMask mask)
    {
        return Physics2D.BoxCastAll(transform.TransformPoint(collider.offset), collider.size, 0f, Vector2.zero, 0f, mask);
    }

    public static LayerMask RemoveLayers(LayerMask original, LayerMask layersToRemove)
    {
        // Use bitwise NOT to invert the bits of layersToRemove
        int invertedLayersToRemove = ~layersToRemove.value;

        // Use bitwise AND to remove layersToRemove from the original LayerMask
        int resultValue = original.value & invertedLayersToRemove;

        // Create a new LayerMask with the result value
        LayerMask resultLayerMask = new LayerMask();
        resultLayerMask.value = resultValue;

        return resultLayerMask;
    }

    public static void PrintLayerNames(LayerMask layerMask /*, DebugX.Team name = DebugX.Team.All*/)
    {
        string debug = $"Layer mask has the following layers:\n";
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask.value & 1 << i) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                debug += $"Layer {i}: {layerName}\n";
            }
        }
        //DebugX.Log(debug, name);
    }
    public static bool HasFlagForEnum<TEnum, TFlags>(TFlags flags, TEnum enumValue) where TEnum : struct, Enum where TFlags : struct, Enum
    {
        TFlags enumFlag     = (TFlags)(object)enumValue;

        var flagValue       = Convert.ToInt32(enumFlag);
        var enumValueInt    = Convert.ToInt32(flags);

        return (enumValueInt & flagValue) != 0;
    }
    public static int CountCheckedFlags<T>(T flags) where T : Enum
    {
        var count = 0;

        foreach (T value in Enum.GetValues(typeof(T)))
        {
            if ((flags as IConvertible).ToUInt64(null) == 0)
                continue;

            if ((flags as IConvertible).ToUInt64(null) != 0 && ((flags as IConvertible).ToUInt64(null) & (value as IConvertible).ToUInt64(null)) == (value as IConvertible).ToUInt64(null))
                count++;
        }

        return count;
    }

    public static bool IsFlippedX(GameObject gameObject)
    {
        var currentTransform    = gameObject.transform;
        var totalScaleX         = 1f;

        while (currentTransform != null)
        {
            totalScaleX     *= currentTransform.localScale.x;
            currentTransform = currentTransform.parent;
        }

        return totalScaleX < 0f;
    }

    public static void ResetAllAnimatorTriggers(Animator animator)
    {
        foreach (var trigger in animator.parameters)
            if (trigger.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(trigger.name);
    }

    public static List<T> ShuffleList<T>(List<T> list)
    {
        var result  = new List<T>(list);
        var count   = result.Count;

        while (count > 1)
        {
            count--;
            var rand        = Random.Range(0, count + 1);
            var value       = result[rand];
            result[rand]    = result[count];
            result[count]   = value;
        }

        return result;
    }

    public static GameObject FindParentWithComponent(GameObject child, Type componentType)
    {
        var currentParent = child.transform.parent;

        while (currentParent != null)
        {
            var component = currentParent.GetComponent(componentType);
            if (component != null)
                return currentParent.gameObject;

            currentParent = currentParent.parent;
        }

        return null;
    }


    public static int[] GetUniqueIndices(int minRange, int maxRange, int count)
    {
        if (count > maxRange - minRange)
            return null;

        var result  = new int[count];
        var temp    = new List<int>();

        for (int i = minRange; i < maxRange; i++)
            temp.Add(i);

        for (int i = 0; i < count; i++)
        {
            var randomIndex = Random.Range(0, temp.Count);

            result[i] = temp[randomIndex];
            temp.RemoveAt(randomIndex);
        }

        return result;
    }

    public static Vector3 Subtract(Vector3 a, Vector3 b)
    {
        Vector3 result = new Vector3();
        result.x = a.x - b.x;
        result.y = a.y - b.y;
        result.z = a.z - b.z;
        return result;
    }

    public static T RandomEnumValue<T>()
    {
        var type    = typeof(T);
        var values  = type.GetEnumValues();
        var index   = Random.Range(0, values.Length);

        return (T)values.GetValue(index);
    }
    public static bool RollChance(int probablity)
    {
        return Random.Range(0, 101) < probablity;
    }
    public static bool RollChance(float probablity)
    {
        return Random.value < probablity;
    }
    public static bool CoinFlip()
    {
        return Random.value > 0.5f;
    }
    public static int CoinFlipInt()
    {
        return CoinFlip() ? 1 : -1;
    }

    public static T GetRandomElement<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    public static T GetRandomElement<T>(Dictionary<int, T> dictionary)
    {
        return dictionary.Values.ElementAt(Random.Range(0, dictionary.Count));
    }

    public static bool TryGetComponentInChildren<T>(GameObject obj, out T  component)
    {
        component = obj.GetComponentInChildren<T>();

        if (component == null)  return false;
        else                    return true;
    }

    public static bool TryGetComponentInParent<T>(GameObject obj, out T component)
    {
        component = obj.GetComponentInParent<T>();

        if (component == null)  return false;
        else                    return true;
    }

    public static float GetDistance(Vector2 pos1, Vector2 pos2)
    {
        float x2 = Mathf.Pow(pos1.x - pos2.x, 2);
        float y2 = Mathf.Pow(pos1.y - pos2.y, 2);

        return Mathf.Sqrt(x2 + y2);
    }

    public static IEnumerator Wait(float duration, Action targetAction = null)
    {
        var elapsed = 0f;
        while (elapsed <= duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        targetAction?.Invoke();
    }

    public static IEnumerator WaitFrames(int frames, Action targetAction = null)
    {
        var counter = 0;
        while (++counter <= frames)
        {
            yield return null;
        }
        targetAction?.Invoke();
    }

    /*
    /// <summary>
    /// Waits until a statement becomes true. If it is true already, it doesn't wait at all
    /// </summary>
    /// <param name="until">statement that needs to become true</param>
    /// <param name="targetAction">the action of what happens, when it is true</param>
    /// <param name="coroutineHolder">the mono behaviour the coroutine should be running on, relevant, if the game manager is null</param>
    /// <param name="maxWaitTime">if the wait until is not invoking, the coroutine is canceled after this time (in seconds)</param>
    /// <param name="invokeAfterWait">if true the target action is invoked after the maximum wait time and the wait until is canceled</param>
    /// <param name="invokeDirectCheck">if check is true no coroutine is started but the target acion is invoked, can be used as a null check</param>
    public static Coroutine WaitUntil(Func<bool> until, Action targetAction = null, MonoBehaviour coroutineHolder = null, float maxWaitTime = 0, bool invokeAfterWait = false, bool invokeDirectCheck = false)
    {
        //TODO: when setting the routine to null/stopping it, the max wait routine should also stop
        Coroutine routine = null;

        if (coroutineHolder == null)
            coroutineHolder = GameManager.Instance;

        if (invokeDirectCheck || until())
            targetAction?.Invoke();
        else if ((coroutineHolder == GameManager.Instance && (GameManager.Instance?.Initialized ?? false)) || coroutineHolder != GameManager.Instance)
        {
            targetAction += () => routine = null;
            routine = coroutineHolder.StartCoroutine(WaitUntilRoutine(until, targetAction));
        }
        else if (coroutineHolder == GameManager.Instance)
            DebugX.LogError("starting a coroutine on the GameManager, while it is inactive");
        else
            DebugX.LogError("something went wrong starting the wait until coroutine");

        if (maxWaitTime > 0 && coroutineHolder != null && routine != null)
        {
            coroutineHolder.StartCoroutine(Wait(maxWaitTime, () =>
            {
                if (routine == null) return;

                coroutineHolder.StopCoroutine(routine);
                if (invokeAfterWait)
                    targetAction?.Invoke();
            }));
        }

        return routine;
    }
    */

    public static IEnumerator WaitUntilRoutine(Func<bool> until, Action targetAction = null, float maxWaitTime = 0, bool invokeAfterWait = false, bool invokeDirectCheck = false)
    {
        if (invokeDirectCheck || until())
        {
            targetAction?.Invoke();
            yield break;
        }

        float timer = 0f;
        while (!until() && (maxWaitTime <= 0 || timer < maxWaitTime))
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (invokeAfterWait || until())
            targetAction?.Invoke();
    }

    /// <summary>
    /// Assign a renderer sorting layer
    /// </summary>
    /// <param name="renderer">the renderers affected by this change</param>
    /// <param name="sortingLayer">the sorting layer the renderers should be rendered on</param>
    /// <param name="sortingOrder">the sorting order of the renderers</param>
    public static void AssignSortingLayer(Renderer renderer, string sortingLayer, int sortingOrder)
    {
        if (renderer != null)
        {
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// Assign a renderer sorting layer
    /// </summary>
    /// <param name="sortingGroup">the sorting groups affected by this change</param>
    /// <param name="sortingLayer">the sorting layer the renderers should be rendered on</param>
    /// <param name="sortingOrder">the sorting order of the renderers</param>
    public static void AssignSortingLayer(SortingGroup sortingGroup, string sortingLayer, int sortingOrder)
    {
        if (sortingGroup != null)
        {
            sortingGroup.sortingLayerName = sortingLayer;
            sortingGroup.sortingOrder = sortingOrder;
        }
    }

    /*
    public static void AssignSortingLayer(PickupItem item, string sortingLayer = "", int sortingOrder = int.MinValue)
    {
        if (item.SortingGroup != null)
            AssignSortingLayer(item.SortingGroup, string.IsNullOrEmpty(sortingLayer) ? item.RenderLayer : sortingLayer, sortingOrder == int.MinValue ? item.RenderOrder : sortingOrder);
        else
            AssignSortingLayer(item.Renderer, string.IsNullOrEmpty(sortingLayer) ? item.RenderLayer : sortingLayer, sortingOrder == int.MinValue ? item.RenderOrder : sortingOrder);
    }
    */

    public static IEnumerator DistanceEvent(Transform objectA, Transform objectB, float threshold, Action targetAction, bool smaller)
    {
        var distance = 0f;
        if (smaller)
        {
            while (distance < threshold)
            {
                distance = GetDistance(objectA.position, objectB.position);
                yield return null;
            }
        }
        else
        {
            while (distance > threshold)
            {
                distance = GetDistance(objectA.position, objectB.position);
                yield return null;
            }
        }

        targetAction?.Invoke();
    }

    public static bool LayerInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }


    /// <summary>
    /// Sends a raycast that returns either a hit for a certain mask or a number of tags. Allows all hits that don't have the tag but are within the mask.
    /// </summary>
    /// <param name="origin">raycast origin</param>
    /// <param name="direction">raycast direction</param>
    /// <param name="distance">raycast distance</param>
    /// <param name="mask">raycast layer mask</param>
    /// <param name="additionalLayersForTag">any additional layers the raycast should check for colliders with the given tags</param>
    /// <param name="tags">tags to hit</param>
    /// <returns>a tuple consisting of the hit status (bool) and the raycast hit (to check what was hit instead)</returns>
    public static Tuple<bool, bool, RaycastHit2D> TagCast(Vector2 origin, Vector2 direction, float distance, LayerMask mask, LayerMask additionalLayersForTag, params string[] tags)
    {
        var hit = Physics2D.Raycast(origin, direction, distance, mask);
        var valid = false;
        var tagHit = false;

        if (hit)
            valid = true;
        else
        {
            hit = Physics2D.Raycast(origin, direction, distance, mask | additionalLayersForTag);
            if (hit && tags.Length > 0 && tags.Any(t => hit.collider.CompareTag(t)))
            {
                valid = true;
                tagHit = true;
            }
        }

        //if (hit)
        //    DebugX.Log($"{hit.collider.name} is in layer {LayerInMask(mask, hit.collider.gameObject.layer)}, has tag {(tags.Length > 0 ? tags.Any(t => hit.collider.CompareTag(t)) : true)}", name: DebugX.Team.Josua, color: Color.gray, obj: hit.collider);

        return Tuple.Create(valid, tagHit, hit);
    }

    public static string FormatTimeStamp(float totalSeconds)
    {
        // Calculate hours, minutes, and seconds
        int hours = Mathf.FloorToInt(totalSeconds / 3600);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);

        // Format the string with leading zeros if necessary
        return string.Format("{0}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }

    public static string FormatTimeToHours(float totalSeconds)
    {
        float hours = totalSeconds / 3600;

        return string.Format("{0:F1}", hours);
    }

    public static T CopyValues<T>(T comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType())
            return null;

        BindingFlags    flags   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[]  pinfos  = type.GetProperties(flags);

        foreach (var pinfo in pinfos)
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { }
            }

        FieldInfo[] finfos = type.GetFields(flags);

        foreach (var finfo in finfos)
            finfo.SetValue(comp, finfo.GetValue(other));

        return comp as T;
    }

    private static IEnumerator WaitUntilRoutine(Func<bool> until, Action targetAction = null)
    {
        yield return new WaitUntil(until);
        targetAction?.Invoke();
    }

    public static Color GetSpriteCenterColor( Sprite sprite)
    {
        Color itemcolor = Color.gray;
        if (sprite != null && sprite.texture.isReadable)
        {
            var spriteOrigin = new Vector2((int)sprite.textureRect.x, (int)sprite.textureRect.y); // get the origin point of the sprite in the sprite sheet, usually sits in the lower left corner of the sprite
            var spriteSize = sprite.bounds.size;
            float pixelsPerUnit = sprite.pixelsPerUnit;
            spriteSize.y *= pixelsPerUnit; // get the actual size of the sprite
            spriteSize.x *= pixelsPerUnit;

            var spritePivot = new Vector2((int)spriteOrigin.x + (int)spriteSize.x / 2, (int)spriteOrigin.y + (int)spriteSize.y / 2); // get the pivot point of sprite in a sprite sheet
            itemcolor = sprite.texture.GetPixel((int)spritePivot.x - 3, (int)spritePivot.y); //pick color
        }
        return itemcolor;
    }

    public static Color GetSpriteColor(Sprite sprite)
    {
        if (sprite == null || !sprite.texture.isReadable)
            return Color.grey;

        int precision = 9; // the square of precision is the number of pixels testet

        var spriteSize = sprite.bounds.size;
        float pixelsPerUnit = sprite.pixelsPerUnit;
        spriteSize.y *= pixelsPerUnit; // get the actual size of the sprite
        spriteSize.x *= pixelsPerUnit;
        var spriteOrigin = new Vector2((int)sprite.textureRect.x, (int)sprite.textureRect.y); // get the origin point of the sprite in the sprite sheet, usually sits in the lower left corner of the sprite

        int colorCount = 0;
        Vector3 colors = new Vector3(0, 0, 0);
        Color pixelColor = new();

        for (float i = 0.5f; i < precision; i += 1)
        {
            for (float j = 0.5f; j < precision; j += 1)
            {
                pixelColor = sprite.texture.GetPixel((int)(spriteOrigin.x + (spriteSize.x / precision) * i), (int)(spriteOrigin.y + (spriteSize.y / precision) * j));

                if (pixelColor.a > 0.6f && !(pixelColor.r < 0.001f && pixelColor.g < 0.001f && pixelColor.b < 0.001f))
                {
                    colors += new Vector3(pixelColor.r, pixelColor.g, pixelColor.b);
                    colorCount++;
                }

            }
        }
        colors /= colorCount;
        if (colorCount != 0)
            return new Color(colors.x, colors.y, colors.z);
        else
            return Color.grey;

    }

    public static Color GetMostUsedColor(Sprite sprite)
    {
        Texture2D texture = sprite.texture;
        Color[] pixels = texture.GetPixels();
        Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

        // Count occurrences of each color
        foreach (Color color in pixels)
        {
            if (color.a < 0.1f) continue; // Optionally ignore nearly transparent pixels
            if (colorCount.ContainsKey(color))
                colorCount[color]++;
            else
                colorCount[color] = 1;
        }

        // Get the most used color
        Color mostUsedColor = colorCount.OrderByDescending(c => c.Value).FirstOrDefault().Key;
        return mostUsedColor;
    }
}