using Il2CppMonomiPark.SlimeRancher.Map;
using Il2CppMonomiPark.SlimeRancher.Regions;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.World;
using MelonLoader;
using UnityEngine;

namespace NewSR2MP.Component
{
    /// <summary>
    /// Component that displays a network player on the map and compass
    /// </summary>
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkPlayerDisplayOnMap : DisplayOnMap
    {
        public static Dictionary<int, NetworkPlayerDisplayOnMap> all = new();
        public int playerID;

        //public MapDirector.Marker marker;
        
        public Transform markerTransform;
        public Transform markerArrow;
        
        void Start()
        {
            if (!all.TryAdd(playerID, this))
            {
                SRMP.Error("Failed to register marker!");
                Destroy(this);
                return;
            }
            //marker = new MapDirector.Marker(new PlayerMapMarkerSource().Cast<IMapMarkerSource>());
            //sceneContext.mapDirector.RegisterMarker($"netPlayer_{playerID}", marker);
        }

        private void OnDestroy()
        {
            all.Remove(playerID);
            sceneContext.mapDirector.DeregisterMarker($"netPlayer_{playerID}");
        }

        public override SceneGroup GetSceneGroup()
        {
            return sceneGroups[players[playerID].worldObject.sceneGroup];
        }

        private void Update()
        {
            if (markerTransform)
            {
                markerTransform.localPosition = new Vector3(transform.position.x, transform.position.z, 0);
                
                markerArrow.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.y);
            }
        }
    }
}

