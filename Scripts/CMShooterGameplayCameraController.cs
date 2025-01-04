using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.Cinemachine
{
    public class CMShooterGameplayCameraController : CMGameplayCameraController, IShooterGameplayCameraController
    {
        /// <summary>
        /// Sort ASC by distance from origin to impact point
        /// </summary>
        public struct RaycastHitComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        public bool EnableAimAssist { get; set; }
        public bool EnableAimAssistX { get; set; }
        public bool EnableAimAssistY { get; set; }
        public bool AimAssistPlayer { get; set; }
        public bool AimAssistMonster { get; set; }
        public bool AimAssistBuilding { get; set; }
        public bool AimAssistHarvestable { get; set; }
        public float AimAssistRadius { get; set; }
        public float AimAssistXSpeed { get; set; }
        public float AimAssistYSpeed { get; set; }
        public float AimAssistMaxAngleFromFollowingTarget { get; set; }

        [Header("Aim Assist")]
        public float aimAssistMinDistanceFromFollowingTarget = 3f;
        public float aimAssistDistance = 10f;
        [Tooltip("Set only obstacle layers, it will be used to check hitting object layer is an obstacle or not. If it is, it won't perform aim assisting")]
        public LayerMask aimAssistObstacleLayerMask = Physics.DefaultRaycastLayers;

        [Header("Recoil")]
        public float recoilReturnSpeed = 2f;
        public float recoilSmoothing = 6f;

        protected Vector3 _targetRecoilRotation;
        protected Vector3 _currentRecoilRotation;
        protected RaycastHit _aimAssistCastHit;

        protected virtual int GetAimAssistLayerMask()
        {
            int layerMask = 0;
            if (AimAssistPlayer)
                layerMask = layerMask | GameInstance.Singleton.playerLayer.Mask;
            if (AimAssistMonster)
                layerMask = layerMask | GameInstance.Singleton.monsterLayer.Mask;
            if (AimAssistBuilding)
                layerMask = layerMask | GameInstance.Singleton.buildingLayer.Mask;
            if (AimAssistHarvestable)
                layerMask = layerMask | GameInstance.Singleton.harvestableLayer.Mask;
            return layerMask;
        }

        protected virtual bool AvoidAimAssist(RaycastHit hitInfo)
        {
            IGameEntity entity = hitInfo.collider.GetComponent<IGameEntity>();
            if (!entity.IsNull() && entity.Entity != PlayerCharacterEntity)
            {
                DamageableEntity damageableEntity = entity.Entity as DamageableEntity;
                return damageableEntity == null || damageableEntity.IsDead() || !damageableEntity.CanReceiveDamageFrom(PlayerCharacterEntity.GetInfo());
            }
            return true;
        }

        protected override void Update()
        {
            float deltaTime = Time.deltaTime;
            UpdateAimAssist(deltaTime);
            base.Update();
            // Update recoiling
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, deltaTime * recoilReturnSpeed);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, Time.fixedDeltaTime * recoilSmoothing);
            _cameraTarget.transform.eulerAngles += _currentRecoilRotation;
        }

        private void LateUpdate()
        {
            // TODO: Make it configuarable
            if (GameInstance.PlayingCharacterEntity != null && GameInstance.PlayingCharacterEntity.ActiveMovement != null && _cameraTarget != null)
                GameInstance.PlayingCharacterEntity.ActiveMovement.SetLookRotation(Quaternion.LookRotation(_cameraTarget.transform.forward), true);
        }

        protected void UpdateAimAssist(float deltaTime)
        {
            if (!EnableAimAssist)
                return;
            RaycastHit[] hits = Physics.SphereCastAll(CameraTransform.position, AimAssistRadius, CameraTransform.forward, aimAssistDistance, GetAimAssistLayerMask());
            System.Array.Sort(hits, 0, hits.Length, new RaycastHitComparer());
            RaycastHit tempHit;
            RaycastHit? hitTarget = null;
            Vector3 cameraDir = CameraTransform.forward;
            Vector3 targetDir;
            for (int i = 0; i < hits.Length; ++i)
            {
                tempHit = hits[i];
                if (aimAssistObstacleLayerMask.value == (aimAssistObstacleLayerMask.value | (1 << tempHit.transform.gameObject.layer)))
                    return;
                if (AvoidAimAssist(tempHit))
                    continue;
                if (Vector3.Distance(PlayerCharacterEntity.EntityTransform.position, tempHit.point) <= aimAssistMinDistanceFromFollowingTarget)
                    continue;
                targetDir = (tempHit.point - PlayerCharacterEntity.EntityTransform.position).normalized;
                if (Vector3.Angle(cameraDir, targetDir) > AimAssistMaxAngleFromFollowingTarget)
                    continue;
                hitTarget = tempHit;
                break;
            }
            if (!hitTarget.HasValue)
                return;
            // Set `xRotation`, `yRotation` by hit object's position
            _aimAssistCastHit = hitTarget.Value;
            Vector3 targetCenter = _aimAssistCastHit.collider.bounds.center;
            Vector3 directionToTarget = (targetCenter - CameraTransform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            if (EnableAimAssistX)
                _pitch = Mathf.MoveTowardsAngle(_pitch, lookRotation.eulerAngles.x, AimAssistXSpeed * deltaTime);
            if (EnableAimAssistY)
                _yaw = Mathf.MoveTowardsAngle(_yaw, lookRotation.eulerAngles.y, AimAssistYSpeed * deltaTime);
        }

        public virtual void Recoil(float x, float y, float z)
        {
            _targetRecoilRotation += new Vector3(x, y, z);
        }
    }
}
