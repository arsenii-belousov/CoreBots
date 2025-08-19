// ============================================================================
// Scripts/Utils/PlayerController.cs — free 3D movement (for build mode)
// ============================================================================
using System;
using UnityEngine;
using UnityEngine.InputSystem; // Add this

public class PlayerController : MonoBehaviour
{
    public Camera cam; // Assign in inspector
    public float moveSpeed = 10f;
    public float lookSensitivity = 2f;
    public float fastMultiplier = 3f;

    float yaw, pitch;
    Vector2 moveInput, lookInput;
    bool fast, up, down;

    private BuildControls buildControls;

    private PlacementController placementController; // Reference to the PlacementController

    public BlockRegistry blockRegistry;
    private BlockDef selectedBlock;
    private Quaternion selectedRotation = Quaternion.identity; // Default rotation

    void OnEnable()
    {
        placementController = GetComponentInChildren<PlacementController>();

        selectedBlock = blockRegistry.blocks[0];
        buildControls = new BuildControls();
        buildControls.Build.Enable();

        var move = buildControls.Build.Move2D;
        var look = buildControls.Build.Look;
        var fastKey = buildControls.Build.Fast;
        var upKey = buildControls.Build.Up;
        var downKey = buildControls.Build.Down;
        var place = buildControls.Build.Place;
        var remove = buildControls.Build.Remove;

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
        place.performed += ctx => PlaceBlock();
        remove.performed += ctx => RemoveBlock();
    }

    void OnDisable()
    {
        buildControls.Build.Disable();
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

    Tuple<Vector3Int, Vector3> LookAtBlockOnTheGrid()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int gridPosition = GridUtil.WorldToGrid(hit.point - hit.normal * 0.01f);
            return new Tuple<Vector3Int, Vector3>(gridPosition, hit.point);
        }
        return null;
    }

    void PlaceBlock()
    {
        var (position, normal) = LookAtBlockOnTheGrid();
        placementController.TryPlaceBlockNextToSelected(selectedBlock, position, normal, selectedRotation);
    }

    void RemoveBlock()
    {
        // Implementation for removing a block
    }

}
