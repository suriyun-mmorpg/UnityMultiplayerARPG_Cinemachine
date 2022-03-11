using Cinemachine;
using UnityEngine;


namespace MultiplayerARPG.Cinemachine
{
    public class CMGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public CinemachineBrain brain;
        public CinemachineVirtualCamera virtualCamera;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public Cinemachine3rdPersonFollow FollowComponent { get; protected set; }
        public Camera Camera { get; protected set; }
        public Transform CameraTransform { get; protected set; }

        public Transform FollowingEntityTransform { get; set; }
        public Vector3 TargetOffset { get { return FollowComponent.ShoulderOffset; } set { FollowComponent.ShoulderOffset = value; } }
        public float MinZoomDistance { get; set; }
        public float MaxZoomDistance { get; set; }
        public float CurrentZoomDistance { get { return FollowComponent.CameraDistance; } set { FollowComponent.CameraDistance = value; } }
        public bool EnableWallHitSpring { get; set; }
        public bool UpdateRotation { get; set; }
        public bool UpdateRotationX { get; set; }
        public bool UpdateRotationY { get; set; }
        public bool UpdateZoom { get; set; }

        protected virtual void Start()
        {
            Camera = brain.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            FollowComponent = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        }

        protected virtual void Update()
        {
            virtualCamera.Follow = FollowingEntityTransform;
            if (UpdateRotation || UpdateRotationX)
            {

            }

            if (UpdateRotation || UpdateRotationY)
            {

            }

            if (UpdateZoom)
            {

            }

            if (EnableWallHitSpring)
            {

            }
        }

        public void Setup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = characterEntity;
        }

        public void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
            FollowingEntityTransform = null;
        }
    }
}
