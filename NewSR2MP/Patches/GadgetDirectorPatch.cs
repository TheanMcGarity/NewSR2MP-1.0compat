using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.World;

namespace NewSR2MP.Patches;
[HarmonyPatch(typeof(GadgetDirector), nameof(GadgetDirector.InstantiateGadget))]
public class GadgetDirectorInstantiateGadget
{
    static void Postfix(GadgetDirector __instance, GameObject __result, GameObject original, SceneGroup sceneGroup, Vector3 position, Quaternion rotation, bool spawnImmediate)
    {
        if (!handlingPacket)
        {
            if (ClientActive())
            {
                // Пропускаем если идет смена сцены (переход через портал)
                if (systemContext == null || systemContext.SceneLoader == null || systemContext.SceneLoader.IsSceneLoadInProgress)
                {
                    SRMP.Debug("Skipping gadget spawn - scene transition in progress");
                    return;
                }
                
                if (sceneContext == null || sceneContext.GameModel == null)
                {
                    SRMP.Debug("Skipping gadget spawn - sceneContext or GameModel is null (portal transition)");
                    return;
                }
                
                var isGadget = __result.TryGetComponent<Gadget>(out var gadget);

                // Проверяем что текущая сцена известна
                if (systemContext.SceneLoader.CurrentSceneGroup == null)
                    return;
                    
                string currentSceneName = systemContext.SceneLoader.CurrentSceneGroup.name;
                if (!sceneGroupsReverse.ContainsKey(currentSceneName))
                {
                    SRMP.Debug($"Cannot spawn gadget - unknown scene: {currentSceneName}");
                    return;
                }

                var packet = new ActorSpawnClientPacket()
                {
                    ident = GetIdentID(gadget.identType),
                    position = position,
                    rotation = __result.transform.eulerAngles,
                    player = currentPlayerID,
                    scene = sceneGroupsReverse[currentSceneName]
                };
                MultiplayerManager.NetworkSend(packet);
                SRMP.Debug($"Client Spawn - {gadget.identType.name}");
                handlingPacket = true; 
                if (isGadget)
                    DestroyGadget(__result, "SR2MP.SpawnGadgetClient");
                handlingPacket = false;
            }
            else if (ServerActive())
            {
                // Пропускаем если идет смена сцены (переход через портал)
                if (sceneContext == null || sceneContext.GameModel == null)
                {
                    SRMP.Debug("Skipping gadget spawn - scene transition in progress");
                    return;
                }
                
                var gadgetComp = __result.GetComponent<Gadget>();
                if (gadgetComp == null || gadgetComp.GetActorId() == null)
                {
                    SRMP.Debug("Gadget component or ActorId is null");
                    return;
                }
                
                var id = gadgetComp.GetActorId().Value;

                gadgets.TryAdd(id, gadgetComp);
                
                // Проверяем что сцена известна
                if (sceneGroup == null || !sceneGroupsReverse.ContainsKey(sceneGroup.name))
                {
                    SRMP.Debug($"Cannot spawn gadget - unknown scene: {sceneGroup?.name}");
                    return;
                }
                
                MultiplayerManager.NetworkSend(new ActorSpawnPacket
                {
                    rotation = __result.transform.eulerAngles,
                    position = position,
                    id = id,
                    ident = GetIdentID(__result.GetComponent<Gadget>().identType),
                    scene = sceneGroupsReverse[sceneGroup.name]
                });
            }
        }
    }
}