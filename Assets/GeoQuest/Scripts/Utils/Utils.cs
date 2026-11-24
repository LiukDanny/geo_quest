using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils 
{
    public static GameObject GetUIUnderPointer(Vector2 position)//remove to help func
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0 ? results[0].gameObject : null;
    }
    
    public static bool IsPointerOverUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
#endif
    }

    public static Vector2 GetPointInRange(Vector2 characterPos, Vector2 objectPos, float range)
    {
        Vector2 direction = (characterPos - objectPos).normalized;
        return objectPos + direction * range;
    }
}
