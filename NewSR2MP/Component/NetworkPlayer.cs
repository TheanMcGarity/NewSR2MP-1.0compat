
using NewSR2MP.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Il2CppTMPro;
using UnityEngine;

namespace NewSR2MP.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkPlayer : MonoBehaviour
    {
        public ProductUserId epicID { get; private set; }

        public void SetUsername(string username)
        {
            usernamePanel = transform.GetChild(1).GetComponent<TextMesh>();
            usernamePanel.text = username;
            usernamePanel.characterSize = 0.2f;
            usernamePanel.anchor = TextAnchor.MiddleCenter;
            usernamePanel.fontSize = 24;
        }
        
        internal void Intialize(ProductUserId epicID)
        {
            this.epicID = epicID;
        }
        
        void Awake()
        {
            if (transform.GetComponents<NetworkPlayer>().Length > 1)
            {
                Destroy(this);
            }
        }

        public TextMesh usernamePanel;
        
        public int id;
        float transformTimer = PlayerTimer;

        public void Update()
        {
            transformTimer -= Time.unscaledDeltaTime;
            if (transformTimer < 0)
            {
                transformTimer = PlayerTimer;
                
                // Пропускаем если идет смена сцены (переход через портал)
                if (systemContext == null || systemContext.SceneLoader == null || systemContext.SceneLoader.IsSceneLoadInProgress)
                    return;
                
                if (sceneContext == null || sceneContext.GameModel == null)
                    return;
                
                var anim = GetComponent<Animator>();
                
                // Проверяем что текущая сцена есть в словаре (пропускаем служебные сцены типа ErrorHandling)
                if (systemContext.SceneLoader.CurrentSceneGroup == null)
                    return;
                    
                string currentSceneName = systemContext.SceneLoader.CurrentSceneGroup.name;
                if (!sceneGroupsReverse.ContainsKey(currentSceneName))
                {
                    // Не отправляем пакет для неизвестных сцен
                    return;
                }
                
                var packet = new PlayerUpdatePacket()
                {
                    id = id,
                    scene = (byte)sceneGroupsReverse[currentSceneName],
                    pos = transform.position,
                    rot = transform.rotation,
                    horizontalMovement = anim.GetFloat("HorizontalMovement"),
                    forwardMovement = anim.GetFloat("ForwardMovement"),
                    yaw = anim.GetFloat("Yaw"),
                    airborneState = anim.GetInteger("AirborneState"),
                    moving = anim.GetBool("Moving"),
                    horizontalSpeed = anim.GetFloat("HorizontalSpeed"),
                    forwardSpeed = anim.GetFloat("ForwardSpeed"),
                    sprinting = anim.GetBool("Sprinting"),
                };
                MultiplayerManager.NetworkSend(packet);

            }
        }
    }
}