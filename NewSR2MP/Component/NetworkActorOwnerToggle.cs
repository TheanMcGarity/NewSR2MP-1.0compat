
using NewSR2MP.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using UnityEngine;

namespace NewSR2MP.Component
{
    // Just a toggle thing
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActorOwnerToggle : MonoBehaviour
    {
        public Vector3 savedVelocity;
        
        // === СИСТЕМА ЗОН ПРОГРУЗКИ ===
        /// <summary>
        /// Время следующей проверки расстояния
        /// </summary>
        private float nextCullingCheck = 0f;
        
        /// <summary>
        /// Интервал проверки расстояния (в секундах)
        /// </summary>
        private const float CULLING_CHECK_INTERVAL = 1.0f;
        
        void Start()
        {
            if (GetComponent<NetworkActor>() == null)
            {
                Destroy(this);
                return;
            }

            started = true;
        }
        bool started = false;
        
        void Update()
        {
            // Проверка зоны прогрузки для КАЖДОГО игрока независимо
            if (Time.time >= nextCullingCheck)
            {
                nextCullingCheck = Time.time + CULLING_CHECK_INTERVAL;
                
                // Пропускаем проверку во время смены сцены (переход через портал)
                if (systemContext == null || systemContext.SceneLoader == null || systemContext.SceneLoader.IsSceneLoadInProgress)
                    return;
                    
                UpdateCulling();
            }
        }
        
        void OnEnable()
        {
            OwnActor(OwnershipTransferCause.REGION);
            activeActors.Add(this);
        }

        void OnDisable()
        {
            activeActors.Remove(this);
        }
        void OnDestroy()
        {
            activeActors.Remove(this);
        }
        
        /// <summary>
        /// Обновляет состояние актера в зависимости от расстояния до игрока
        /// </summary>
        private void UpdateCulling()
        {
            try
            {
                // Пропускаем если идет смена сцены (переход через портал)
                if (sceneContext == null || sceneContext.GameModel == null || sceneContext.player == null)
                    return;
                    
                // Не выключаем актеров которыми мы владеем
                var netActor = GetComponent<NetworkActor>();
                if (netActor != null && netActor.IsOwned)
                    return;
                    
                float distance = Vector3.Distance(transform.position, sceneContext.player.transform.position);
                bool isActive = gameObject.activeSelf;
                
                // ВЫГРУЗКА: Актер слишком далеко
                if (distance > Timers.ActorUnloadRadius && isActive)
                {
                    gameObject.SetActive(false);
                    SRMP.Debug($"Actor culled at {distance:F1}m (unload radius: {Timers.ActorUnloadRadius}m)");
                }
                // ЗАГРУЗКА: Актер вернулся в зону прогрузки
                else if (distance <= Timers.ActorLoadRadius && !isActive)
                {
                    gameObject.SetActive(true);
                    SRMP.Debug($"Actor loaded at {distance:F1}m (load radius: {Timers.ActorLoadRadius}m)");
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки во время смены сцены
                SRMP.Debug($"UpdateCulling error (scene transition?): {ex.Message}");
            }
        }
        /// <summary>
        /// Use this to drop the current largo held for this client.
        /// </summary>
        public void LoseGrip()
        {
            VacuumItem vac = sceneContext.PlayerState.VacuumItem;

            if (vac._held == gameObject)
            {
                // SLIGHTLY MODIFIED SR CODE
                Vacuumable vacuumable = vac._held.GetComponent<Vacuumable>();

                if (vacuumable != null)
                {
                    vacuumable.Release();
                }

                vac.LockJoint.connectedBody = null;
                Identifiable ident = vac._held.GetComponent<Identifiable>();
                vac._held = null;
                vac.SetHeldRad(0f);
            }
        }

        public enum OwnershipTransferCause { VAC, REGION, SET_OWNER_PACKET, UNSPECIFIED }
        
        /// <summary>
        /// This is for transfering actor ownership to another player. Recommended for when you want a client to control a feature on the actor. 
        /// </summary>
        public void OwnActor(OwnershipTransferCause cause = OwnershipTransferCause.UNSPECIFIED)
        {
            if (!started)
                return;

            if (cause == OwnershipTransferCause.REGION)
            {
                try
                {
                    // Увеличенная область владения для лучшей синхронизации на расстоянии
                    Vector3 size = new Vector3(150, 200, 150);
                    
                    bool intersects = false;
                    
                    if (sceneContext?.player == null) return;
                    
                    var bounds1 = new Bounds(sceneContext.player.transform.position, size);
                    
                    foreach (var player in players)
                    {
                        if (player.playerID == currentPlayerID)
                            continue;
                        
                        if (player.worldObject == null) continue;
                        
                        var bounds2 = new Bounds(player.worldObject.transform.position, size);
                        
                        if (bounds2.Intersects(bounds1))
                        {
                            intersects = true;
                            break;
                        }
                    }

                    if (intersects) return;
                    
                    // Дополнительная проверка: если актер слишком далеко от ВСЕХ игроков,
                    // ближайший игрок берет владение
                    float minDistanceToAnyPlayer = float.MaxValue;
                    bool isClosestPlayer = false;
                    
                    foreach (var player in players)
                    {
                        if (player.worldObject == null) continue;
                        
                        float distance = Vector3.Distance(transform.position, player.worldObject.transform.position);
                        if (distance < minDistanceToAnyPlayer)
                        {
                            minDistanceToAnyPlayer = distance;
                            isClosestPlayer = (player.playerID == currentPlayerID);
                        }
                    }
                    
                    // Если актер далеко от всех (>150 единиц), ближайший игрок берет владение
                    if (minDistanceToAnyPlayer > 150f && !isClosestPlayer)
                    {
                        return; // Не ближайший игрок - не берем владение
                    }
                }
                catch
                {
                    // Игнорируем ошибки во время смены сцены
                    return;
                }
            }
            NetworkActor net = GetComponent<NetworkActor>();
            
            if (net == null)
            {
                Destroy(this);
                return;
            }
            
            if (net.IsOwned) return;
            
            // Owner change
            net.enabled = true;
            net.IsOwned = true;

            // Inform server of owner change.
            var packet = new ActorUpdateOwnerPacket()
            {
                id = GetComponent<IdentifiableActor>().GetActorId().Value,
                player = currentPlayerID
            };
            MultiplayerManager.NetworkSend(packet);
            
            if (TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = savedVelocity;
        }
    }
}
