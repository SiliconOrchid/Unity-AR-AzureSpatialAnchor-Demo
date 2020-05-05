using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public static class TouchExtension
{
    public static bool IsTouchUIObject(this Vector2 touchPosition)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = new Vector2(touchPosition.x, touchPosition.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        return results.Count > 0;
    }
}