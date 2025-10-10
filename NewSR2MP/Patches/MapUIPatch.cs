using Il2CppMonomiPark.SlimeRancher.Map;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppTMPro;
using UnityEngine.UI;

namespace NewSR2MP.Patches;

[HarmonyPatch(typeof(MapUI), nameof(MapUI.Start))]
public class MapUIAwake
{
    static void Postfix(MapUI __instance)
    {
        var playerMarkerPrefab = __instance._markerPrefabMapping._playerMarkerPrefab;
        
        // Players
        foreach (var marker in NetworkPlayerDisplayOnMap.all)
        {
            if (marker.Key == currentPlayerID)
                continue;
            
            SRMP.Log($"🗺️ Setting up player marker {marker.Key} on the map ui.");
            var instance = Object.Instantiate(playerMarkerPrefab);
            instance.transform.SetParent(__instance._mapContainer.transform.parent.FindChild("Markers"));
            
            instance.transform.position = new Vector3(marker.Value.transform.position.x, marker.Value.transform.position.z, 0);
            marker.Value.markerTransform = instance.transform;

            instance.GetComponent<MapFader>()._targetOpacity = 100;
            instance.AddComponent<ForcedOpacityOnMap>().target = 100;

            marker.Value.markerArrow = instance.transform.FindChild("FacingFrame");
            marker.Value.markerArrow.FindChild("FacingArrow").GetComponent<Image>().m_Color = GetPlayerColor((ushort)marker.Key);
            
            var textObj = new GameObject("PlayerName");
            textObj.transform.SetParent(instance.transform);
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.SetText($"<b>{playerUsernamesReverse[marker.Key]}</b>");
            text.alpha = 0.4f;
            text.alignment = TextAlignmentOptions.Center;
            
            textObj.transform.localPosition = new Vector3(0, 45, 0);
            textObj.transform.localScale = Vector3.one * 0.6f;

            instance.transform.localScale = Vector3.one;
        }
        
        // Waypoints
        foreach (var marker in MultiplayerWaypointManager.Instance.playerWaypoints)
        {
            SRMP.Log($"🗺️ Setting up waypoint marker for player {marker.Key} on the map ui.");
            var instance = CreateWaypointIcon(marker.Key, __instance._markerPrefabMapping._navigationMarkerPrefab);
            instance.transform.SetParent(__instance._mapContainer.transform.parent.FindChild("Markers"));
            
            instance.transform.localPosition = new Vector3(marker.Value.position.x, marker.Value.position.z, 0);

            instance.transform.localScale = Vector3.one;
        }
    }
}