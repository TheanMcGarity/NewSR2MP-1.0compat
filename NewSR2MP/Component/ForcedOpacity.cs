using Il2CppMonomiPark.SlimeRancher.UI.Map;

namespace NewSR2MP.Component;

[RegisterTypeInIl2Cpp(false)]
public class ForcedOpacityOnMap : MonoBehaviour
{
    MapFader fader;
    
    public float target;
    
    void Awake()
    {
        fader = GetComponent<MapFader>();
    }

    void FixedUpdate()
    {
        fader._targetOpacity = target;
    }
}