using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CraftController : MonoBehaviour
{
    public float moveForce = 40f;
    public float verticalForce = 30f;
    public float turnTorque = 60f;
    public float damping = 2f;

    Rigidbody rb;

    InputActionMap map;
    InputAction moveAction;      // Vector2 (x=horizontal, y=forward)
    InputAction ascendAction;    // button / axis (space)
    InputAction descendAction;   // button (ctrl/c)
    InputAction yawAction;       // float (Q/E or left/right arrow)

    public bool enableInput = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupInput();
    }

    void OnEnable() { map?.Enable(); }
    void OnDisable() { map?.Disable(); }

    void SetupInput()
    {
        map = new InputActionMap("Craft");

        moveAction = map.AddAction("Move", InputActionType.Value, binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        ascendAction = map.AddAction("Ascend", InputActionType.Button, "<Keyboard>/space");
        ascendAction.AddBinding("<Gamepad>/rightShoulder");

        descendAction = map.AddAction("Descend", InputActionType.Button);
        descendAction.AddBinding("<Keyboard>/leftCtrl");
        descendAction.AddBinding("<Keyboard>/c");
        descendAction.AddBinding("<Gamepad>/leftShoulder");

        yawAction = map.AddAction("Yaw", InputActionType.Value);
        yawAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/q")
            .With("Positive", "<Keyboard>/e");
        yawAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        yawAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Gamepad>/leftStick/left")
            .With("Positive", "<Gamepad>/leftStick/right");
    }

    void FixedUpdate()
    {
        if (!enableInput || map == null) return;

        Vector2 move2 = moveAction.ReadValue<Vector2>();
        float h = Mathf.Clamp(move2.x, -1f, 1f);
        float v = Mathf.Clamp(move2.y, -1f, 1f);

        float up = 0f;
        if (ascendAction.IsPressed()) up += 1f;
        if (descendAction.IsPressed()) up -= 1f;

        float yaw = Mathf.Clamp(yawAction.ReadValue<float>(), -1f, 1f);

        Vector3 thrust =
            transform.forward * v * moveForce +
            transform.right * h * moveForce +
            transform.up * up * verticalForce;

        rb.AddForce(thrust, ForceMode.Force);
        if (Mathf.Abs(yaw) > 0.0001f)
            rb.AddTorque(Vector3.up * yaw * turnTorque, ForceMode.Force);

        // Damping (use rb.velocity)
        rb.AddForce(-rb.linearVelocity * damping * Time.fixedDeltaTime, ForceMode.Force);
        rb.AddTorque(-rb.angularVelocity * damping * 0.5f * Time.fixedDeltaTime, ForceMode.Force);
    }
}