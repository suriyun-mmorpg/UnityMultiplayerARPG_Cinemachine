using Insthync.CameraAndInput;
using Unity.Cinemachine;
using UnityEngine;

namespace MultiplayerARPG.Cinemachine
{
    public class CMGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public CinemachineBrain brain;
        public CinemachineCamera virtualCamera;
        public string pitchAxisName = "Mouse Y";
        public float pitchRotateSpeed = 4f;
        public float pitchRotateSpeedScale = 1f;
        public float pitchBottomClamp = -30f;
        public float pitchTopClamp = 70f;
        public float pitchBottomClampForCrouch = -30f;
        public float pitchTopClampForCrouch = 70f;
        public float pitchBottomClampForCrawl = -30f;
        public float pitchTopClampForCrawl = 70f;
        public float pitchBottomClampForSwim = -30f;
        public float pitchTopClampForSwim = 70f;
        public string yawAxisName = "Mouse X";
        public float yawRotateSpeed = 4f;
        public float yawRotateSpeedScale = 1f;
        public string zoomAxisName = "Mouse ScrollWheel";
        public float zoomSpeed = 4f;
        public float zoomSpeedScale = 1f;
        public float zoomDamping = 10f;
        public float cameraSideDamping = 10f;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public CinemachineThirdPersonFollow FollowComponent { get; protected set; }
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
        public float CameraFov
        {
            get
            {
                return virtualCamera.Lens.FieldOfView;
            }
            set
            {
                var lens = virtualCamera.Lens;
                lens.FieldOfView = value;
                virtualCamera.Lens = lens;
            }
        }
        public float CameraNearClipPlane
        {
            get
            {
                return virtualCamera.Lens.NearClipPlane;
            }
            set
            {
                var lens = virtualCamera.Lens;
                lens.NearClipPlane = value;
                virtualCamera.Lens = lens;
            }
        }
        public float CameraFarClipPlane
        {
            get
            {
                return virtualCamera.Lens.FarClipPlane;
            }
            set
            {
                var lens = virtualCamera.Lens;
                lens.FarClipPlane = value;
                virtualCamera.Lens = lens;
            }
        }
        public float MinZoomDistance
        {
            get
            {
                return FollowComponent.CameraDistance;
            }
            set
            {
            }
        }
        public float MaxZoomDistance
        {
            get
            {
                return FollowComponent.CameraDistance;
            }
            set
            {
            }
        }
        public float CurrentZoomDistance { get; set; }
        public bool EnableWallHitSpring
        {
            get
            {
                return FollowComponent.AvoidObstacles.CollisionFilter != 0;
            }
            set
            {
                FollowComponent.AvoidObstacles.CollisionFilter = value ? _defaultCameraCollisionFilter : 0;
            }
        }
        public bool UpdateRotation { get; set; }
        public bool UpdateRotationX { get; set; }
        public bool UpdateRotationY { get; set; }
        public bool UpdateZoom { get; set; }
        public float CameraRotationSpeedScale { get; set; }
        public bool IsLeftViewSide { get; set; }
        public bool IsZoomAimming { get; set; }

        protected float _pitch;
        protected float _yaw;
        protected float _zoom;
        protected GameObject _cameraTarget;
        protected int _defaultCameraCollisionFilter;
        protected float _currentCameraSide = 1f;

        public virtual void Init()
        {

        }

        protected virtual void Update()
        {
            if (FollowingEntityTransform == null)
                return;

            if (_cameraTarget == null)
                _cameraTarget = new GameObject("__CMCameraTarget");

            virtualCamera.Follow = _cameraTarget.transform;

            float pitchBottomClamp = this.pitchBottomClamp;
            float pitchTopClamp = this.pitchTopClamp;

            if (GameInstance.PlayingCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
            {
                pitchBottomClamp = pitchBottomClampForSwim;
                pitchTopClamp = pitchTopClampForSwim;
            }
            else if (GameInstance.PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
            {
                pitchBottomClamp = pitchBottomClampForCrouch;
                pitchTopClamp = pitchTopClampForCrouch;
            }
            else if (GameInstance.PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
            {
                pitchBottomClamp = pitchBottomClampForCrawl;
                pitchTopClamp = pitchTopClampForCrawl;
            }

            if (UpdateRotation || UpdateRotationX)
            {
                float pitchInput = InputManager.GetAxis(pitchAxisName, false);
                _pitch += -pitchInput * pitchRotateSpeed * pitchRotateSpeedScale * (CameraRotationSpeedScale > 0 ? CameraRotationSpeedScale : 1f);
            }

            if (UpdateRotation || UpdateRotationY)
            {
                float yawInput = InputManager.GetAxis(yawAxisName, false);
                _yaw += yawInput * yawRotateSpeed * yawRotateSpeedScale * (CameraRotationSpeedScale > 0 ? CameraRotationSpeedScale : 1f);
            }

            _yaw = ClampAngle(_yaw, float.MinValue, float.MaxValue);
            _pitch = ClampAngle(_pitch, pitchBottomClamp, pitchTopClamp);

            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
            _cameraTarget.transform.rotation = targetRotation;

            _cameraTarget.transform.position = FollowingEntityTransform.position;

            _zoom = Mathf.Lerp(_zoom, CurrentZoomDistance, zoomDamping * Time.deltaTime);
            FollowComponent.CameraDistance = _zoom;

            if (IsLeftViewSide)
            {
                _currentCameraSide = Mathf.Lerp(_currentCameraSide, 0, cameraSideDamping * Time.deltaTime);
            }
            else
            {
                _currentCameraSide = Mathf.Lerp(_currentCameraSide, 1, cameraSideDamping * Time.deltaTime);
            }
            FollowComponent.CameraSide = _currentCameraSide;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float start = (min + max) * 0.5f - 180;
            float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
            return Mathf.Clamp(angle, min + floor, max + floor);
        }

        public virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = characterEntity;
            Camera = brain.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            FollowComponent = (CinemachineThirdPersonFollow)virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            _defaultCameraCollisionFilter = FollowComponent.AvoidObstacles.CollisionFilter;
        }

        public virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
            FollowingEntityTransform = null;
        }
    }
}
