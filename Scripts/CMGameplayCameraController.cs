using Cinemachine;
using UnityEngine;


namespace MultiplayerARPG.Cinemachine
{
    public class CMGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public CinemachineBrain brain;
        public CinemachineVirtualCamera virtualCamera;
        public string pitchAxisName;
        public float pitchRotateSpeed;
        public float pitchRotateSpeedScale = 1f;
        public float pitchBottomClamp = -30f;
        public float pitchTopClamp = 70f;
        public string yawAxisName;
        public float yawRotateSpeed;
        public float yawRotateSpeedScale = 1f;
        public string zoomAxisName;
        public float zoomSpeed;
        public float zoomSpeedScale = 1f;
        public float zoomMin = 1f;
        public float zoomMax = 4;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public Cinemachine3rdPersonFollow FollowComponent { get; protected set; }
        public Camera Camera { get; protected set; }
        public Transform CameraTransform { get; protected set; }

        public Transform FollowingEntityTransform { get; set; }
        public Vector3 TargetOffset
        {
            get
            {
                return FollowComponent.ShoulderOffset;
            }
            set
            {
                FollowComponent.ShoulderOffset = value;
            }
        }
        public float MinZoomDistance { get; set; }
        public float MaxZoomDistance { get; set; }
        public float CurrentZoomDistance
        {
            get
            {
                return FollowComponent.CameraDistance;
            }
            set
            {
                FollowComponent.CameraDistance = value;
            }
        }
        public bool EnableWallHitSpring
        {
            get
            {
                return FollowComponent.CameraCollisionFilter != 0;
            }
            set
            {
                FollowComponent.CameraCollisionFilter = value ? defaultCameraCollisionFilter : 0;
            }
        }
        public bool UpdateRotation { get; set; }
        public bool UpdateRotationX { get; set; }
        public bool UpdateRotationY { get; set; }
        public bool UpdateZoom { get; set; }

        private float pitch;
        private float yaw;
        private float zoom;
        private GameObject cameraTarget;
        private int defaultCameraCollisionFilter;

        protected virtual void Start()
        {
            Camera = brain.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            FollowComponent = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            defaultCameraCollisionFilter = FollowComponent.CameraCollisionFilter;
        }

        protected virtual void Update()
        {
            if (cameraTarget == null)
                cameraTarget = new GameObject("__CMCameraTarget");

            virtualCamera.Follow = FollowingEntityTransform;
            if (UpdateRotation || UpdateRotationX)
            {
                pitch += InputManager.GetAxis(pitchAxisName, false) * pitchRotateSpeed * pitchRotateSpeedScale;
            }

            if (UpdateRotation || UpdateRotationY)
            {
                yaw += InputManager.GetAxis(yawAxisName, false) * yawRotateSpeed * yawRotateSpeedScale;
            }

            yaw = ClampAngle(yaw, float.MinValue, float.MaxValue);
            pitch = ClampAngle(pitch, pitchBottomClamp, pitchTopClamp);
            cameraTarget.transform.position = FollowingEntityTransform.position;
            cameraTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);

            if (UpdateZoom)
            {
                zoom += InputManager.GetAxis(zoomAxisName, false) * zoomSpeed * zoomSpeedScale;
            }

            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
            FollowComponent.CameraDistance = zoom;
        }

        private float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
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
