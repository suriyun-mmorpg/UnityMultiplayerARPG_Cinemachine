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

        public  virtual void Recoil(float x, float y)
        {

        }
    }
}
