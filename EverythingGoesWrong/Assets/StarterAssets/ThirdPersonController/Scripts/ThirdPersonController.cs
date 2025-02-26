using UnityEngine;

namespace StarterAssets { [RequireComponent(typeof(CharacterController))] public class CustomThirdPersonController : MonoBehaviour { 
    [Header("Player")] 
    public float MoveSpeed = 2.0f; 
    public float SprintSpeed = 5.335f; 
    
    [Range(0.0f, 0.3f)] 
    public float RotationSmoothTime = 0.12f; 
    public float SpeedChangeRate = 10.0f; 
    public AudioClip LandingAudioClip; 
    public AudioClip[] FootstepAudioClips; 
    [Range(0, 1)] 
    public float FootstepAudioVolume = 0.5f; 
    [Space(10)] 
    public float JumpHeight = 1.2f; 
    public float Gravity = -15.0f; 
    [Space(10)] 
    public float JumpTimeout = 0.50f; 
    public float FallTimeout = 0.15f;
    [Header("Player Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

   [Header("Cinemachine")]
public GameObject CameraRoot;        // This will handle horizontal rotation
public GameObject CameraPivot;       // This will handle vertical rotation    
public float TopClamp = 70.0f;
public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;

      // Private variables for camera
        private float _horizontalRotation;
        private float _verticalRotation;
        private float _rotationVelocity;
        private const float _threshold = 0.01f;

    // Private variables
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Animator _animator;
    private CharacterController _controller;
    private MyPlayerInput _input; // Our custom input component
    private GameObject _mainCamera;
    private bool _hasAnimator;

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Start()
    { _horizontalRotation = CameraRoot.transform.rotation.eulerAngles.y;
            _verticalRotation = CameraPivot.transform.rotation.eulerAngles.x;
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<MyPlayerInput>();

        AssignAnimationIDs();

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        if (_hasAnimator)
            _animator.SetBool(_animIDGrounded, Grounded);
    }

    private void CameraRotation()
        {
            if (_input.Look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = 1.0f;

                // Update horizontal rotation (handled by CameraRoot)
                _horizontalRotation += _input.Look.x * deltaTimeMultiplier;
                
                // Update vertical rotation (handled by CameraPivot)
                _verticalRotation -= _input.Look.y * deltaTimeMultiplier; // Negative for intuitive up/down
_verticalRotation = ClampAngle(_verticalRotation, BottomClamp, TopClamp);
            }

            // Apply rotations separately
            CameraRoot.transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
            CameraPivot.transform.rotation = Quaternion.Euler(_verticalRotation + CameraAngleOverride, _horizontalRotation, 0f);
        }

    private void Move()
        {
            float targetSpeed = _input.Sprint ? SprintSpeed : MoveSpeed;
            if (_input.Move == Vector2.zero)
                targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.Move.magnitude;

            // Adjust speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f)
                _animationBlend = 0f;

            // Get input direction relative to camera
            Vector3 inputDirection = new Vector3(_input.Move.x, 0.0f, _input.Move.y).normalized;

            // Transform input direction based on camera's orientation
            Vector3 transformedDirection = Quaternion.Euler(0, _mainCamera.transform.eulerAngles.y, 0) * inputDirection;

            // Move the player without rotating
            _controller.Move(transformedDirection.normalized * (_speed * Time.deltaTime) + 
                           new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // Update animation parameters if needed
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }
            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

            if (_input.Jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                if (_hasAnimator)
                    _animator.SetBool(_animIDJump, true);
            }
            if (_jumpTimeoutDelta >= 0.0f)
                _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0.0f)
                _fallTimeoutDelta -= Time.deltaTime;
            else if (_hasAnimator)
                _animator.SetBool(_animIDFreeFall, true);
        }

        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += Gravity * Time.deltaTime;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        Gizmos.color = Grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && FootstepAudioClips.Length > 0)
        {
            int index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
    }
}
}