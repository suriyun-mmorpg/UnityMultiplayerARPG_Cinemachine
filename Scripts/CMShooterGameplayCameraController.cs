using UnityEngine;

namespace MultiplayerARPG.Cinemachine
{
    public class CMShooterGameplayCameraController : CMGameplayCameraController, IShooterGameplayCameraController
    {
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
        public float CameraRotationSpeedScale { get; set; }

        [Header("Recoil")]
        public float recoilReturnSpeed = 2f;
        public float recoilSmoothing = 6f;

        protected Vector3 _targetRecoilRotation;
        protected Vector3 _currentRecoilRotation;

        protected override void Update()
        {
            base.Update();
            float deltaTime = Time.deltaTime;
            // Update recoiling
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, deltaTime * recoilReturnSpeed);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, Time.fixedDeltaTime * recoilSmoothing);
            _cameraTarget.transform.eulerAngles += _currentRecoilRotation;
        }

        public virtual void Recoil(float x, float y, float z)
        {
            _targetRecoilRotation += new Vector3(x, y, z);
        }
    }
}
