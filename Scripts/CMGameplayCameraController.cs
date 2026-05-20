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
        public float rotateSpeedScale = 1f;
        public string zoomAxisName = "Mouse ScrollWheel";
        public float zoomSpeed = 4f;
        public float zoomSpeedScale = 1f;
        public float zoomMin = 2f;
        public float zoomMax = 8f;
        public float zoomDamping = 10f;
        public float cameraSideDamping = 10f;
        public float targetOffsetsDamping = 1f;
        public float offsetDamping = 10f;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public CinemachineThirdPersonFollow FollowComponent { get; protected set; }
        public Camera Camera { get; protected set; }
        public Transform CameraTransform { get; protected set; }
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
        public BasePlayerCharacterController PlayerCharacterController { get; protected set; }

        protected float? _pitchBottomClamp;
        protected float? _pitchTopClamp;
        protected float _pitch;
        protected float _yaw;
        protected float _zoom;
        protected Vector3 _offset;
        protected GameObject _cameraTarget;
        protected Vector3? _targetOffsets;

        public virtual void Init(BasePlayerCharacterController controller)
        {
            PlayerCharacterController = controller;
            Camera = brain.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            if (virtualCamera == null)
                virtualCamera = brain.ActiveVirtualCamera as CinemachineCamera;
            FollowComponent = (CinemachineThirdPersonFollow)virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            _offset = PlayerCharacterController.AssignedCameraTargetOffset = FollowComponent.ShoulderOffset;
            _zoom = PlayerCharacterController.AssignedCameraZoomDistance = FollowComponent.CameraDistance;
            PlayerCharacterController.AssignedCameraFov = CameraFov;
            PlayerCharacterController.AssignedCameraNearClipPlane = CameraNearClipPlane;
            PlayerCharacterController.AssignedCameraFarClipPlane = CameraFarClipPlane;
            PlayerCharacterController.AssignedCameraRotationSpeedScale = rotateSpeedScale;
            PlayerCharacterController.AssignedEnableWallHitSpring = EnableWallHitSpring;
        }

        protected virtual void Update()
        {
            if (PlayerCharacterController.CameraTargetTransform == null)
                return;

            CameraFov = PlayerCharacterController.CameraFov;
            CameraNearClipPlane = PlayerCharacterController.CameraNearClipPlane;
            CameraFarClipPlane = PlayerCharacterController.CameraFarClipPlane;
            EnableWallHitSpring = PlayerCharacterController.EnableWallHitSpring;

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
                _pitch += -pitchInput * pitchRotateSpeed * pitchRotateSpeedScale * (PlayerCharacterController.CameraRotationSpeedScale > 0 ? PlayerCharacterController.CameraRotationSpeedScale : 1f);
            }

            if (UpdateRotation || UpdateRotationY)
            {
                float yawInput = InputManager.GetAxis(yawAxisName, false);
                _yaw += yawInput * yawRotateSpeed * yawRotateSpeedScale * (PlayerCharacterController.CameraRotationSpeedScale > 0 ? PlayerCharacterController.CameraRotationSpeedScale : 1f);
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

            DoUpdateRotation(deltaTime);
            //
            DoUpdateCameraDistance(deltaTime);
            DoUpdateCameraSide(deltaTime);
            DoUpdateOffset(deltaTime);
            _cameraTarget.transform.position = PlayerCharacterController.CameraTargetTransform.position;
        }

        protected virtual void DoUpdateRotation(float deltaTime)
        {
            Quaternion targetRotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
            _cameraTarget.transform.rotation = targetRotation;
        }

        protected virtual void DoUpdateCameraDistance(float deltaTime)
        {
            if (UpdateZoom)
            {
                _zoom += InputManager.GetAxis(zoomAxisName, false) * zoomSpeed * zoomSpeedScale;
                _zoom = Mathf.Clamp(_zoom, zoomMin, zoomMax);
            }
            if (zoomDamping <= 0f)
                FollowComponent.CameraDistance = _zoom;
            else
                FollowComponent.CameraDistance = Mathf.Lerp(FollowComponent.CameraDistance, _zoom, zoomDamping * deltaTime);
        }

        protected virtual void DoUpdateCameraSide(float deltaTime)
        {
            FollowComponent.CameraSide = 1f;
        }

        protected virtual void DoUpdateOffset(float deltaTime)
        {
            Vector3 targetOffsets = PlayerCharacterController.CameraTargetOffset;
            float signedPitch = ToSignedAngle(_pitch);
            if (signedPitch > 0f)
            {
                // Remap pitch 0–70 -> 0–1
                float t = Mathf.InverseLerp(0f, pitchTopClamp, signedPitch);

                // Lerp Z from +3 -> -3
                float z = Mathf.Lerp(PlayerCharacterController.CameraTargetOffset.z, -PlayerCharacterController.CameraTargetOffset.z, t);

                // Final offset
                targetOffsets = new Vector3(PlayerCharacterController.CameraTargetOffset.x, PlayerCharacterController.CameraTargetOffset.y, z);
            }

            if (targetOffsetsDamping <= 0f || !_targetOffsets.HasValue)
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
        }

        public virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
        }
    }
}
