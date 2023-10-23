using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class RaycastUtilities
{
    public static bool UIRaycast(Vector2 position, List<RaycastResult> results)
    {
        var pointerData = ScreenPosToPointerData(position);
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }

    static PointerEventData ScreenPosToPointerData(Vector2 screenPos)
       => new(EventSystem.current) { position = screenPos };
}