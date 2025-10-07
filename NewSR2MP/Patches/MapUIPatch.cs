using Il2CppMonomiPark.SlimeRancher.Map;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppTMPro;

namespace NewSR2MP.Patches;

[HarmonyPatch(typeof(MapUI), nameof(MapUI.Start))]
public class MapUIAwake
{
    static void Postfix(MapUI __instance)
    {
        var playerMarkerPrefab = __instance._markerPrefabMapping._playerMarkerPrefab;
        
        foreach (var marker in NetworkPlayerDisplayOnMap.all)
        {
            if (marker.Key == currentPlayerID)
                continue;
            
            SRMP.Log($"🗺️ Setting up marker {marker.Key} on the map ui.");
            var instance = Object.Instantiate(playerMarkerPrefab);
            instance.transform.SetParent(__instance._mapContainer.transform.parent.FindChild("Markers"));
            
            instance.transform.position = new Vector3(marker.Value.transform.position.x, marker.Value.transform.position.z, 0);
            marker.Value.markerTransform = instance.transform;

            instance.GetComponent<MapFader>()._targetOpacity = 100;
            instance.AddComponent<ForcedOpacityOnMap>().target = 100;

            marker.Value.markerArrow = instance.transform.FindChild("FacingFrame");
            
            var textObj = new GameObject("PlayerName");
            textObj.transform.SetParent(instance.transform);
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.SetText($"<b>{playerUsernamesReverse[marker.Key]}</b>");
            text.alignment = TextAlignmentOptions.Center;
            
            textObj.transform.localPosition = new Vector3(0, 45, 0);
            textObj.transform.localScale = Vector3.one * 0.6f;

            instance.transform.localScale = Vector3.one;
        }
    }
}