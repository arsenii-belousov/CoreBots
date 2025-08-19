using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 pivotOffset = new Vector3(0, 1.2f, 0);
    public float distance = 6f;
    public float minDistance = 2f;
    public float maxDistance = 12f;
    public float yaw = 0f;
    public float pitch = 15f;
    public float pitchMin = -20f;
    public float pitchMax = 70f;
    public float rotateSensitivity = 120f;
    public float zoomSensitivity = 2f;
    public float followSmooth = 12f;

    InputActionMap map;
    InputAction lookAction;
    InputAction scrollAction;
    bool active = false;

    public void SetActive(bool on)
    {
        active = on;
        if (on) map?.Enable(); else map?.Disable();
    }

    void Awake()
    {
        map = new InputActionMap("TPCam");
        lookAction = map.AddAction("Look", InputActionType.Value, "<Mouse>/delta");
        scrollAction = map.AddAction("Zoom", InputActionType.Value, "<Mouse>/scroll");
    }

    void LateUpdate()
    {
        if (!active || target == null) return;

        Vector2 look = lookAction.ReadValue<Vector2>();
        Vector2 scroll = scrollAction.ReadValue<Vector2>();

        yaw += look.x * rotateSensitivity * Time.deltaTime;
        pitch -= look.y * rotateSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        if (Mathf.Abs(scroll.y) > 0.01f)
        {
            distance -= scroll.y * zoomSensitivity * Time.deltaTime * 10f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desired = target.position + pivotOffset - rot * Vector3.forward * distance;

        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = rot;
    }
}