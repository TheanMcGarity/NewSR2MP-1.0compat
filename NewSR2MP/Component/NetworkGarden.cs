namespace NewSR2MP.Component;

[RegisterTypeInIl2Cpp(false)]
public class NetworkGarden : MonoBehaviour
{
    private SpawnResource garden;

    public static Dictionary<string, NetworkGarden> all = new(); 

    public bool isOwned = false;
    public double cachedTime = 0;
    
    private float syncTimer = 0;
    void Awake()
    { 
        garden = GetComponent<SpawnResource>();
        all.Add(garden._id, this);
    }

    void Update()
    {
        syncTimer += Time.deltaTime;
        if (syncTimer <= PlanterTimer)
            return;
        syncTimer = 0;
        
        if ((ServerActive() || ClientActive()) && isOwned)
        {
            MultiplayerManager.NetworkSend(new GardenUpdatePacket
            {
                id = garden._id,
                time = garden._model.nextSpawnTime,
            });
        }
    }


    private void OnDestroy()
    {
        all.Remove(garden._id);
    }

    void OnEnable()
    {
        MultiplayerManager.NetworkSend(new GardenOwnershipPacket
        {
            id = garden._id
        });
        isOwned = true;
        
        garden._model.nextSpawnTime = cachedTime;
    }

    void OnDisable()
    {
        isOwned = false;
    }
}