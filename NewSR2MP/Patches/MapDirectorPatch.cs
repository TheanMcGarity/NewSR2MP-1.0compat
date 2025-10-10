using Il2CppMonomiPark.SlimeRancher.Map;
namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(MapDirector), nameof(MapDirector.SetPlayerNavigationMarker))]
    internal class MapDirectorSetPlayerNavigationMarker
    {
        public static bool Prefix(MapDirector __instance, Vector3 position, MapDefinition onMap,
            float minimumDistanceToPlace)
        {
            if (handlingNavPacket) return true;

            var existingWaypoint = MultiplayerWaypointManager.Instance.GetWaypoint((ushort)currentPlayerID);
            if (existingWaypoint != null && existingWaypoint.isActive)
            {
                float distance = Vector3.Distance(position, existingWaypoint.position);
                
                if (distance < 15f)
                {
                    SRMP.Debug($"Removing existing waypoint (clicked nearby, distance: {distance:F1}m)");
                    __instance.ClearPlayerNavigationMarker();
                    return false; 
                }
            }

            return true;
        }

        public static void Postfix(MapDirector __instance, Vector3 position, MapDefinition onMap, float minimumDistanceToPlace)
        {
            if (handlingNavPacket) return;
            
            MapType packetMapType;
            switch (onMap.name)
            {
                case "LabyrinthMap":
                    packetMapType = MapType.Labyrinth;
                    break;
                default:
                    packetMapType = MapType.RainbowIsland;
                    break;
            }

            var packet = new PlaceNavMarkerPacket()
            {
                map = packetMapType,
                position = position,
                playerID = (ushort)currentPlayerID, // Добавляем ID текущего игрока
            };
            
            MultiplayerManager.NetworkSend(packet);
        }
    }  
    
    [HarmonyPatch(typeof(MapDirector), nameof(MapDirector.ClearPlayerNavigationMarker))]
    internal class MapDirectorClearPlayerNavigationMarker
    {
        public static void Postfix(MapDirector __instance)
        {
            if (handlingNavPacket) return;

            var packet = new RemoveNavMarkerPacket()
            {
                playerID = (ushort)currentPlayerID, // Добавляем ID текущего игрока
            };
            
            MultiplayerManager.NetworkSend(packet);
        }
    }
}
