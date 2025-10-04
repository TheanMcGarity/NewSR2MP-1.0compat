
using NewSR2MP.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.World;
using MelonLoader;
using UnityEngine;

namespace NewSR2MP.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActor : MonoBehaviour
    {


        private bool isOwned = true;
        
        /// <summary>
        /// Is currently owned by the client. Recommended to use ownership system for this.
        /// </summary>
        public bool IsOwned
        {
            get
            {
                return isOwned; 
            }
            internal set
            {
                isOwned = value;
            }
        }

        private Identifiable identComp;
        private Rigidbody rigidbody;
        void Awake()
        {
            try
            {
                identComp = GetComponent<Identifiable>();
                rigidbody = GetComponent<Rigidbody>();
            }
            catch { }
        }

        private float transformTimer;
        public float vacTimer;

        internal int startingOwner = 0;

        public long trueID = -1;

        void Start()
        {
            if (GetComponent<ResourceCycle>() != null)
            {
                gameObject.AddComponent<NetworkResource>();
            }
            
            
            if (ClientActive() && !ServerActive())
                isOwned = false;
        }
        uint frame;
        public void Update()
        {
            // Пропускаем если идет смена сцены (переход через портал)
            if (systemContext == null || systemContext.SceneLoader == null || systemContext.SceneLoader.IsSceneLoadInProgress)
                return;
            
            if (sceneContext == null || sceneContext.GameModel == null)
                return;
            
            if (gameObject.TryGetComponent(out Gadget gadget))
            {
                gameObject.RemoveComponent<TransformSmoother>();
                gameObject.RemoveComponent<NetworkActorOwnerToggle>();
                Destroy(this);
            }

            if (!IsOwned)
            {
                var ts = GetComponent<TransformSmoother>();
                if (ts != null)
                    ts.enabled = true;
                enabled = false;
                return;
            }
            
            // Проверка расстояния до ближайшего игрока
            float minDistanceToPlayer = float.MaxValue;
            try
            {
                foreach (var player in players)
                {
                    if (player.worldObject == null) continue;
                    float distance = Vector3.Distance(transform.position, player.worldObject.transform.position);
                    if (distance < minDistanceToPlayer)
                        minDistanceToPlayer = distance;
                }

            }
            catch
            {
                // Игнорируем ошибки во время смены сцены
                minDistanceToPlayer = 0f; // Считаем что игрок рядом
            }
            
            // Если актер слишком далеко (>200 единиц), синхронизируем реже
            float syncMultiplier = 1f;
            if (minDistanceToPlayer > 200f)
            {
                syncMultiplier = 3f; // В 3 раза реже
            }
            else if (minDistanceToPlayer > 100f)
            {
                syncMultiplier = 2f; // В 2 раза реже
            }
            
            transformTimer -= Time.unscaledDeltaTime;
            if (transformTimer <= 0)
            {
                transformTimer = ActorTimer * syncMultiplier;
                try
                {
                    var packet = new ActorUpdatePacket()
                    {
                        id = identComp.GetActorId().Value,
                        position = transform.position,
                        rotation = transform.eulerAngles,
                        velocity = rigidbody.velocity,
                    };
                    
                    if (TryGetComponent<SlimeEmotions>(out var emotions))
                    {
                        packet.slimeEmotions = new NetworkEmotions(
                            emotions._emotions.x,
                            emotions._emotions.y,
                            emotions._emotions.z,
                            emotions._emotions.w);
                    }
                    
                    MultiplayerManager.NetworkSend(packet);
                }
                catch { }
            }
            frame++;
        }
        public void OnDisable()
        {
            GetComponent<TransformSmoother>().enabled = true;
        }
        void OnDestroy()
        {
            actors.Remove(identComp.GetActorId().Value);
        }
    }
}

