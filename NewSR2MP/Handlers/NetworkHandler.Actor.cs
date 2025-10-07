using System.Collections;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.World;
using NewSR2MP.Attributes;

namespace NewSR2MP;

public partial class NetworkHandler
{
    
    [PacketResponse]
    private static void HandleActorSpawn(NetPlayerState netPlayer, ActorSpawnPacket packet, byte channel)
    {
        try
        {
            // Пропускаем если идет смена сцены (переход через портал)
            if (systemContext == null || systemContext.SceneLoader == null || systemContext.SceneLoader.IsSceneLoadInProgress)
            {
                SRMP.Debug("Skipping actor spawn packet - scene transition in progress");
                return;
            }
            
            if (sceneContext == null || sceneContext.GameModel == null)
            {
                SRMP.Debug("Skipping actor spawn packet - sceneContext or GameModel is null (portal transition)");
                return;
            }
            
            var sg = sceneGroups[packet.scene];

            if (actors.TryGetValue(packet.id, out var actor))
                actors.Remove(packet.id);

            Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
            var ident = identifiableTypes[packet.ident];
            var identObj = ident.prefab;


            SRMP.Debug($"[{systemContext._SceneLoader_k__BackingField.CurrentSceneGroup.name} | {sg.name}]");


            

            handlingPacket = true;
            var obj = RegisterActor(new ActorId(packet.id), ident, packet.position, Quaternion.Euler(packet.rotation), sg);
            handlingPacket = false;
            
            // Если RegisterActor вернул null (смена сцены), пропускаем этот пакет
            if (obj == null)
            {
                SRMP.Debug($"Skipped actor spawn packet for {ident.name} - scene transition in progress");
                return;
            }
            
            obj.AddComponent<NetworkActor>();
            obj.AddComponent<NetworkActorOwnerToggle>();
            obj.AddComponent<TransformSmoother>();
            
            if (obj.TryGetComponent<NetworkActor>(out var netComp))
                if (!actors.TryAdd(packet.id, netComp))
                    actors[packet.id] = netComp;
            
            if (obj && !ident.TryCast<GadgetDefinition>())
            {
                obj.AddComponent<NetworkResource>(); // Try add resource network component. Will remove if it's not a resource so please do not change

                if (!actors.ContainsKey(obj.GetComponent<Identifiable>().GetActorId().Value))
                {
                    actors.Add(obj.GetComponent<Identifiable>().GetActorId().Value,
                        obj.GetComponent<NetworkActor>());
                    obj.GetComponent<TransformSmoother>().interpolPeriod = ActorTimer;
                    if (obj.TryGetComponent<Vacuumable>(out var vac))
                        vac._launched = true;
                }
                else
                {
                    if (!obj.TryGetComponent<Gadget>(out _))
                        obj.GetComponent<TransformSmoother>().enabled = false;
                    obj.GetComponent<TransformSmoother>().interpolPeriod = ActorTimer;
                    if (obj.TryGetComponent<Vacuumable>(out var vac))
                        vac._launched = true;
                }

                obj.GetComponent<NetworkActor>().IsOwned = false;
                obj.GetComponent<TransformSmoother>().SetNetworkTarget(packet.position, packet.rotation, packet.velocity);

                // Мгновенно применяем velocity для плавного движения брошенных объектов
                if (packet.velocity.sqrMagnitude > 0.01f && obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.velocity = packet.velocity;
                    // Не даем Rigidbody "заснуть" пока объект движется
                    rb.WakeUp();
                    rb.sleepThreshold = 0.1f; // Уменьшаем порог засыпания
                }

                obj.GetComponent<NetworkActorOwnerToggle>().savedVelocity = packet.velocity;
                
                if (packet.player == currentPlayerID)
                {
                    obj.GetComponent<NetworkActor>().IsOwned = true;
                    obj.GetComponent<NetworkActor>().enabled = true;
                    obj.GetComponent<TransformSmoother>().enabled = false;
                    MelonCoroutines.Start(ActorVelocityApplicator(packet.velocity, obj));
                }
                obj.GetComponent<TransformSmoother>().nextPos = packet.position;
                obj.transform.position = packet.position;
            }

        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
        }
    }

    private static IEnumerator ActorVelocityApplicator(Vector3 vel, GameObject actor)
    {
        actor.AddComponent<DontPushPlayer>();
        yield return null;
        actor.GetComponent<NetworkActor>().IsOwned = true;
        actor.GetComponent<TransformSmoother>().enabled = false;
        yield return null;
        actor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        yield return null;
        actor.GetComponent<Rigidbody>().velocity = vel;
    }
    
    [PacketResponse]
    private static void HandleClientActorSpawn(NetPlayerState netPlayer, ActorSpawnClientPacket packet, byte channel)
    {
        try
        {
            // Проверка входных данных
            if (netPlayer == null)
            {
                SRMP.Error("HandleClientActorSpawn: netPlayer is null");
                return;
            }
            
            // Пропускаем если идет смена сцены (переход через портал)
            if (sceneContext == null || sceneContext.GameModel == null)
            {
                SRMP.Debug("Skipping client actor spawn - scene transition in progress");
                return;
            }
            
            var sg = sceneGroups[packet.scene];
            Quaternion rot = Quaternion.Euler(packet.rotation);
            var ident = identifiableTypes[packet.ident];
            var identObj = ident.prefab;


            var nextID = NextMultiplayerActorID;

            var obj = RegisterActor(new ActorId(nextID), ident, packet.position, rot, sg);

            // Если RegisterActor вернул null (смена сцены), пропускаем этот пакет
            if (obj == null)
            {
                SRMP.Debug($"Skipped client actor spawn packet for {ident.name} - scene transition in progress");
                return;
            }

            obj.AddComponent<NetworkActor>();
            obj.AddComponent<NetworkActorOwnerToggle>();
            obj.AddComponent<TransformSmoother>();

            if (obj && !ident.TryCast<GadgetDefinition>())
            {
                obj.AddComponent<NetworkResource>();
                
                var transformSmoother = obj.GetComponent<TransformSmoother>();
                if (transformSmoother != null)
                {
                    transformSmoother.enabled = false;
                    transformSmoother.interpolPeriod = ActorTimer;
                }
                
                // Мгновенно применяем velocity для плавного броска
                if (obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.velocity = packet.velocity;
                    // Не даем Rigidbody "заснуть" пока объект движется
                    rb.WakeUp();
                    rb.sleepThreshold = 0.1f; // Уменьшаем порог засыпания
                    SRMP.Debug($"Client received thrown actor with velocity {packet.velocity.magnitude:F2} m/s");
                }
                
                var vacuumable = obj.GetComponent<Vacuumable>();
                if (vacuumable != null)
                    vacuumable.Launch(Vacuumable.LaunchSource.PLAYER);
            }

            if (obj.TryGetComponent<NetworkActor>(out var netComp)
               )
                if (!actors.TryAdd(nextID, netComp))
                    actors[nextID] = netComp;

            var forwardPacket = new ActorSpawnPacket()
            {
                id = nextID,
                ident = packet.ident,
                position = packet.position,
                rotation = packet.rotation,
                velocity = packet.velocity,
                scene = packet.scene,
                player = packet.player,
            };

            long actorID = -1;

            if (obj.TryGetComponent<IdentifiableActor>(out var identifiableActor))
            {
                if (identifiableActor._model != null && identifiableActor._model.actorId != null)
                    actorID = identifiableActor._model.actorId.Value;
            }
            else if (obj.TryGetComponent<Gadget>(out var gadget))
            {
                if (gadget._model != null && gadget._model.actorId != null)
                    actorID = gadget._model.actorId.Value;
            }
            
            // Проверяем что actorID валиден
            if (actorID == -1)
            {
                SRMP.Error($"HandleClientActorSpawn: Could not get actorID for {ident.name}");
                return;
            }
            
            // Отправляем ActorSpawnPacket ВСЕМ игрокам (включая того кто бросил)
            // Его локальный актер был уничтожен, нужно пересоздать с правильным ID
            MultiplayerManager.NetworkSend(forwardPacket);
            
            var ownerToggle = obj.GetComponent<NetworkActorOwnerToggle>();
            if (ownerToggle != null)
                ownerToggle.savedVelocity = packet.velocity;
            obj.GetComponent<NetworkActor>().IsOwned = false;
            obj.GetComponent<NetworkActor>().enabled = false;
            obj.GetComponent<TransformSmoother>().enabled = true;
            obj.GetComponent<TransformSmoother>().nextPos = packet.position;
            obj.transform.position = packet.position;
        }
        catch (Exception e)
        {
            SRMP.Error($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
        }

    }

    
    [PacketResponse]
    private static void HandleActorOwner(NetPlayerState netPlayer, ActorUpdateOwnerPacket packet, byte channel)
    {
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;


            actor.IsOwned = false;
            actor.GetComponent<TransformSmoother>().enabled = true;
            actor.GetComponent<TransformSmoother>().SetNetworkTarget(actor.transform.position, actor.transform.eulerAngles, Vector3.zero);
            actor.enabled = false;

            actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();
        }
        catch (Exception e)
        {
            SRMP.Error($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
        }
        
        
    }

    
    [PacketResponse]
    private static void HandleDestroyActor(NetPlayerState netPlayer, ActorDestroyGlobalPacket packet, byte channel)
    {
        try
        {
            if (actors.TryGetValue(packet.id, out var actor))
            {
                DeregisterActor(new ActorId(packet.id));

                Object.Destroy(actor.gameObject);
                actors.Remove(packet.id);
            }
            else if (gadgets.TryGetValue(packet.id, out var gadget))
            {
                DeregisterActor(new ActorId(packet.id));

                Object.Destroy(gadget.gameObject);
                actors.Remove(packet.id);
            }
            
        }
        catch (Exception e)
        {
            SRMP.Error($"Exception in destroying actor({packet.id})! Stack Trace:\n{e}");
        }
    }

    [PacketResponse]
    private static void HandleActorVelocity(NetPlayerState netPlayer, ActorVelocityPacket packet, byte channel)
    {
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;
            
            actor.GetComponent<Rigidbody>().velocity = packet.velocity;
            
            if (packet.bounce)
                if (!actor.IsOwned)
                    MultiplayerManager.NetworkSend(new ActorVelocityPacket
                    {
                        id = packet.id,
                        bounce = false,
                        velocity = actor.GetComponent<Rigidbody>().velocity
                    });
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in setting actor({packet.id}) velocity! Stack Trace:\n{e}");
        }
    }

    [PacketResponse]
    private static void HandleActorSetOwner(NetPlayerState netPlayer, ActorSetOwnerPacket packet, byte channel)
    {
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;

            if (actor.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = packet.velocity;
            
            actor.GetComponent<NetworkActorOwnerToggle>().OwnActor(NetworkActorOwnerToggle.OwnershipTransferCause.SET_OWNER_PACKET);
        }
        catch (Exception e)
        {
            SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
        }
    }


    [PacketResponse]
    private static void HandleActor(NetPlayerState netPlayer, ActorUpdatePacket packet, byte channel)
    {

        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;
            var t = actor.GetComponent<TransformSmoother>();
            
            // Используем новый метод для установки целевой позиции с экстраполяцией
            t.SetNetworkTarget(packet.position, packet.rotation, packet.velocity);

            // Будим Rigidbody если объект движется
            if (packet.velocity.sqrMagnitude > 0.01f && actor.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.WakeUp();
            }

            if (actor.TryGetComponent<SlimeEmotions>(out var emotions))
                emotions.SetFromNetwork(packet.slimeEmotions);

            if (actor.TryGetComponent<NetworkActorOwnerToggle>(out var ownerToggle))
                ownerToggle.savedVelocity = packet.velocity;
        }
        catch (Exception e)
        {
            SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
        }


    }
    [PacketResponse]
    private static void HandleResourceState(NetPlayerState netPlayer, ResourceStatePacket packet, byte channel)
    {
        try
        {
            if (!actors.TryGetValue(packet.id, out var nres)) return;

            var res = nres.GetComponent<ResourceCycle>();
            Rigidbody rigidbody = res._body;

            switch (packet.state)
            {
                case ResourceCycle.State.ROTTEN:
                    if (res._model.state == ResourceCycle.State.ROTTEN) break;
                    res.Rot();
                    res.SetRotten(true);
                    break;
                case ResourceCycle.State.RIPE:
                    if (res._model.state == ResourceCycle.State.RIPE) break;
                    res.Ripen();
                    if (res.VacuumableWhenRipe)
                    {
                        res._vacuumable.enabled = true;
                    }

                    if (res.gameObject.transform.localScale.x < res._defaultScale.x * 0.33f)
                    {
                        res.gameObject.transform.localScale = res._defaultScale * 0.33f;
                    }

                    TweenUtil.ScaleTo(res.gameObject, res._defaultScale, 4f);
                    break;
                case ResourceCycle.State.UNRIPE:
                    if (res._model.state == ResourceCycle.State.UNRIPE) break;
                    res._model.state = ResourceCycle.State.UNRIPE;
                    res.transform.localScale = res._defaultScale * 0.33f;
                    break;
                case ResourceCycle.State.EDIBLE:
                    if (res._model.state == ResourceCycle.State.EDIBLE) break;
                    res.MakeEdible();
                    res._additionalRipenessDelegate = null;
                    rigidbody.isKinematic = false;
                    if (res._preparingToRelease)
                    {
                        res._preparingToRelease = false;
                        res._releaseAt = 0f;
                        res.ToShake.localPosition = res._toShakeDefaultPos;
                        if (res.ReleaseCue != null)
                        {
                            SECTR_PointSource component = res.GetComponent<SECTR_PointSource>();
                            component.Cue = res.ReleaseCue;
                            component.Play();
                        }
                    }

                    break;
            }
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling state for resource({packet.id})! Stack Trace:\n{e}");
        }


    }

}