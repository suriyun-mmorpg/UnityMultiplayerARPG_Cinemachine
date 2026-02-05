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
        public float pitchClampDamping = 10f;
        public string yawAxisName = "Mouse X";
        public float yawRotateSpeed = 4f;
        public float yawRotateSpeedScale = 1f;
        public string zoomAxisName = "Mouse ScrollWheel";
        public float zoomSpeed = 4f;
        public float zoomSpeedScale = 1f;
        public float zoomDamping = 10f;
        public float cameraSideDamping = 10f;
        public float targetOffsetsDamping = 1f;
        public float offsetDamping = 10f;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public CinemachineThirdPersonFollow FollowComponent { get; protected set; }
        public Camera Camera { get; protected set; }
        public Transform CameraTransform { get; protected set; }
        public Transform FollowingEntityTransform { get; set; }
        public Vector3 TargetOffset { get; set; }
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
                return FollowComponent.AvoidObstacles.Enabled;
            }
            set
            {
                FollowComponent.AvoidObstacles.Enabled = value;
            }
        }
        public bool UpdateRotation { get; set; }
        public bool UpdateRotationX { get; set; }
        public bool UpdateRotationY { get; set; }
        public bool UpdateZoom { get; set; }
        public float CameraRotationSpeedScale { get; set; }
        public bool IsLeftViewSide { get; set; }
        public bool IsZoomAimming { get; set; }

        protected float? _pitchBottomClamp;
        protected float? _pitchTopClamp;
        protected float _pitch;
        protected float _yaw;
        protected float _zoom;
        protected Vector3 _offset;
        protected GameObject _cameraTarget;
        protected float _currentCameraSide = 1f;
        protected Vector3? _targetOffsets;

        public virtual void Init()
        {

        }

        protected virtual void Update()
        {
            if (FollowingEntityTransform == null)
                return;

            if (_cameraTarget == null)
                _cameraTarget = new GameObject("__CMCameraTarget");

            float deltaTime = Time.deltaTime;
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

            if (!_pitchBottomClamp.HasValue)
                _pitchBottomClamp = pitchBottomClamp;
            else
                _pitchBottomClamp = Mathf.Lerp(_pitchBottomClamp.Value, pitchBottomClamp, pitchClampDamping * deltaTime);

            if (!_pitchTopClamp.HasValue)
                _pitchTopClamp = pitchTopClamp;
            else
                _pitchTopClamp = Mathf.Lerp(_pitchTopClamp.Value, pitchTopClamp, pitchClampDamping * deltaTime);

            _yaw = ClampAngle(_yaw, float.MinValue, float.MaxValue);
            _pitch = ClampAngle(_pitch, _pitchBottomClamp.Value, _pitchTopClamp.Value);

            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
            _cameraTarget.transform.rotation = targetRotation;

            _cameraTarget.transform.position = FollowingEntityTransform.position;

            if (zoomDamping <= 0f)
                _zoom = CurrentZoomDistance;
            else
                _zoom = Mathf.Lerp(_zoom, CurrentZoomDistance, zoomDamping * Time.deltaTime);

            FollowComponent.CameraDistance = _zoom;

            if (IsLeftViewSide)
            {
                _currentCameraSide = Mathf.Lerp(_currentCameraSide, 0, cameraSideDamping * deltaTime);
            }
            else
            {
                _currentCameraSide = Mathf.Lerp(_currentCameraSide, 1, cameraSideDamping * deltaTime);
            }
            FollowComponent.CameraSide = _currentCameraSide;

            Vector3 targetOffsets = TargetOffset;
            float signedPitch = ToSignedAngle(_pitch);
            if (signedPitch > 0f)
            {
                // Remap pitch 0–70 -> 0–1
                float t = Mathf.InverseLerp(0f, pitchTopClamp, signedPitch);

                // Lerp Z from +3 -> -3
                float z = Mathf.Lerp(TargetOffset.z, -TargetOffset.z, t);

                // Final offset
                targetOffsets = new Vector3(TargetOffset.x, TargetOffset.y, z);
            }

            if (!_targetOffsets.HasValue)
                _targetOffsets = targetOffsets;
            else
                _targetOffsets = Vector3.Lerp(_targetOffsets.Value, targetOffsets, targetOffsetsDamping * deltaTime);

            if (offsetDamping <= 0)
                _offset = _targetOffsets.Value;
            else
                _offset = Vector3.Lerp(_offset, _targetOffsets.Value, offsetDamping * deltaTime);

            FollowComponent.ShoulderOffset = _offset;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float start = (min + max) * 0.5f - 180;
            float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
            return Mathf.Clamp(angle, min + floor, max + floor);
        }

        private float ToSignedAngle(float angle)
        {
            return (angle > 180f) ? angle - 360f : angle;
        }

        public virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = characterEntity;
            Camera = brain.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            FollowComponent = (CinemachineThirdPersonFollow)virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        }

        public virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
            FollowingEntityTransform = null;
        }
    }
}
