using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActionsAsset;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    [Header("Mouse Settings")]
    [Tooltip("Multiplier for mouse sensitivity on the X-axis.")]
    public float MouseSensitivityX = 1.0f;
    
    [Tooltip("Multiplier for mouse sensitivity on the Y-axis.")]
    public float MouseSensitivityY = 1.0f;
    
    [Tooltip("Invert Y-axis for camera movement.")]
    public bool InvertYAxis = false;

    // Exposed properties for the controller script
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Jump { get; private set; }
    public bool Sprint { get; private set; }

    private void Awake()
    {
        // Find the "Player" action map and its actions
        var playerActionMap = inputActionsAsset.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("Player action map not found in the Input Action Asset!");
            return;
        }

        moveAction = playerActionMap.FindAction("Move");
        lookAction = playerActionMap.FindAction("Look");
        jumpAction = playerActionMap.FindAction("Jump");
        sprintAction = playerActionMap.FindAction("Sprint");
        
        // Lock the cursor by default
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        lookAction.performed -= OnLook;
        lookAction.canceled -= OnLook;

        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Move = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        Vector2 rawLook = context.ReadValue<Vector2>();

        // Apply separate sensitivity and invert Y-axis if needed
        Look = new Vector2(
            rawLook.x * MouseSensitivityX,
            (InvertYAxis ? -rawLook.y : rawLook.y) * MouseSensitivityY
        );
    }

    private void Update()
    {
        // Read Jump and Sprint values directly each frame
        Jump = jumpAction.ReadValue<float>() > 0.5f;
        Sprint = sprintAction.ReadValue<float>() > 0.5f;

        // Unlock the mouse when Left Alt is held; otherwise, keep it locked.
        if (Keyboard.current != null && Keyboard.current.leftAltKey.isPressed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
