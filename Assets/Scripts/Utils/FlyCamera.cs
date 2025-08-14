// ============================================================================
// Scripts/Utils/FlyCamera.cs — free 3D movement (for build mode)
// ============================================================================
using UnityEngine;
using UnityEngine.InputSystem; // Add this

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSensitivity = 2f;
    public float fastMultiplier = 3f;

    float yaw, pitch;
    Vector2 moveInput, lookInput;
    bool fast, up, down;

    void OnEnable()
    {
        var map = new InputActionMap("FlyCam");

        // Use a composite for WASD movement
        var move = map.AddAction("Move", type: InputActionType.Value, binding: "");
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        var look = map.AddAction("Look", binding: "<Mouse>/delta");
        var fastKey = map.AddAction("Fast", binding: "<Keyboard>/leftShift");
        var upKey = map.AddAction("Up", binding: "<Keyboard>/space");
        var downKey = map.AddAction("Down", binding: "<Keyboard>/leftCtrl");

        move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        move.canceled += ctx => moveInput = Vector2.zero;
        look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        look.canceled += ctx => lookInput = Vector2.zero;
        fastKey.performed += ctx => fast = true;
        fastKey.canceled += ctx => fast = false;
        upKey.performed += ctx => up = true;
        upKey.canceled += ctx => up = false;
        downKey.performed += ctx => down = true;
        downKey.canceled += ctx => down = false;

        map.Enable();
    }

    void Start()
    {
        var e = transform.eulerAngles; yaw = e.y; pitch = e.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        yaw += lookInput.x * lookSensitivity;
        pitch -= lookInput.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        var speed = moveSpeed * (fast ? fastMultiplier : 1f);
        Vector3 dir = new Vector3(
            moveInput.x,
            (up ? 1 : 0) + (down ? -1 : 0),
            moveInput.y);
        transform.position += transform.TransformDirection(dir.normalized) * speed * Time.deltaTime;
    }
}
