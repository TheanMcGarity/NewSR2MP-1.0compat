using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class TransformSmoother : MonoBehaviour
    {
        public void SetRigidbodyState(bool enabled)
        {
            if (GetComponent<Rigidbody>() != null)
                GetComponent<Rigidbody>().constraints =
                    enabled 
                        ? RigidbodyConstraints.None 
                        : RigidbodyConstraints.FreezeAll;
        }

        
        void Start()
        {
            if (GetComponent<NetworkPlayer>() != null)
            {
                thisPlayer = GetComponent<NetworkPlayer>();
            }
            if (GetComponent<NetworkActor>() != null)
            {
                thisActor = GetComponent<NetworkActor>();
            }
            
            lastReceivedPos = transform.position;
            lastReceivedTime = Time.time;
            receivedVelocity = Vector3.zero;
        }

        public NetworkPlayer thisPlayer;
        public NetworkActor thisActor;
        
        /// <summary>
        /// Next rotation. The future rotation, this is the rotation the transform is smoothing to.
        /// </summary>
        public Vector3 nextRot;

        /// <summary>
        /// Next position. The future position, this is the position the transform is smoothing to.
        /// </summary>
        public Vector3 nextPos;

        /// <summary>
        /// Interpolation Period. the speed at which the transform is smoothed.
        /// </summary>
        public float interpolPeriod = PlayerTimer;

        private Vector3 lastReceivedPos;
        
        private float lastReceivedTime;
        
        private Vector3 receivedVelocity;
        
        private bool usePhysicsExtrapolation = true;
        
        private Vector3 gravity = new Vector3(0, -9.81f, 0);
        
        private const float MAX_EXTRAPOLATION_DISTANCE = 50f;
        
        private float extrapolationSmoothing = 0.15f;

        public Vector3 currPos => transform.position;
        private float positionTime;

        public Vector3 currRot => transform.eulerAngles;

        /// <summary>
        /// Sets a new target position and speed from a network packet.
        /// </summary>
        public void SetNetworkTarget(Vector3 position, Vector3 rotation, Vector3 velocity)
        {
            lastReceivedPos = position;
            lastReceivedTime = Time.time;
            receivedVelocity = velocity;
            
            nextPos = position;
            nextRot = rotation;
            
            usePhysicsExtrapolation = velocity.sqrMagnitude > 0.1f;
        }
        
        public void Update()
        {
            if (thisActor) SetRigidbodyState(thisActor.IsOwned);
            
            if (thisActor && thisActor.IsOwned) return;
            
            float timeSinceLastUpdate = Time.time - lastReceivedTime;
            
            Vector3 extrapolatedPos = nextPos;
            
            if (usePhysicsExtrapolation && receivedVelocity.sqrMagnitude > 0.01f)
            {
                bool hasGravity = false;
                if (TryGetComponent<Rigidbody>(out var rb))
                {
                    hasGravity = rb.useGravity;
                }
                
                extrapolatedPos = lastReceivedPos + receivedVelocity * timeSinceLastUpdate;
                
                if (hasGravity)
                {
                    extrapolatedPos += gravity * (0.5f * timeSinceLastUpdate * timeSinceLastUpdate);
                }
                
                // Ограничиваем экстраполяцию чтобы объекты не улетали слишком далеко
                float extrapolationDistance = Vector3.Distance(lastReceivedPos, extrapolatedPos);
                if (extrapolationDistance > MAX_EXTRAPOLATION_DISTANCE)
                {
                    // Если экстраполяция завела слишком далеко, используем последнюю известную позицию
                    extrapolatedPos = lastReceivedPos;
                }
            }
            
            float t = 1.0f - ((positionTime - Time.unscaledTime) / interpolPeriod);
            t = Mathf.Clamp01(t);
            
            Vector3 targetPosition = Vector3.Lerp(nextPos, extrapolatedPos, extrapolationSmoothing);
            
            transform.position = Vector3.Lerp(currPos, targetPosition, t);
            
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(currRot), Quaternion.Euler(nextRot), t);

            positionTime = Time.unscaledTime + interpolPeriod;
        }

        void OnDisable()
        {
            if (TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = GetComponent<NetworkActorOwnerToggle>().savedVelocity;
        }
    }
}