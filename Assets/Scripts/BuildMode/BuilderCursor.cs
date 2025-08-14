// ============================================================================
// Scripts/BuildMode/BuildCursor.cs — aiming/preview/placement/removal
// ============================================================================
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCursor : MonoBehaviour
{
    public Camera cam;
    public BlockRegistry registry;
    public VehicleAssembler assembler;
    public Material previewMat;

    public bool snapToGrid = true;
    public int selectedIndex = 0;

    GameObject preview;
    BlockDef selected => registry.GetByIndex(selectedIndex);

    Quaternion rot = Quaternion.identity;

    InputActionMap map;
    InputAction placeAction, removeAction, snapToggleAction;
    InputAction[] selectActions = new InputAction[9];
    InputAction rotateLeftAction, rotateRightAction, rotateZAction;

    void OnEnable()
    {
        map = new InputActionMap("BuildCursor");

        placeAction = map.AddAction("Place", binding: "<Mouse>/leftButton");
        removeAction = map.AddAction("Remove", binding: "<Mouse>/rightButton");
        snapToggleAction = map.AddAction("SnapToggle", binding: "<Keyboard>/f");

        for (int i = 0; i < 9; i++)
        {
            selectActions[i] = map.AddAction($"Select{i + 1}", binding: $"<Keyboard>/alpha{(i + 1)}");
            int idx = i;
            selectActions[i].performed += ctx =>
            {
                selectedIndex = Mathf.Clamp(idx, 0, registry.Count - 1);
                CreatePreview();
            };
        }

        rotateLeftAction = map.AddAction("RotateLeft", binding: "<Keyboard>/q");
        rotateRightAction = map.AddAction("RotateRight", binding: "<Keyboard>/e");
        rotateZAction = map.AddAction("RotateZ", binding: "<Keyboard>/r");

        rotateLeftAction.performed += ctx => rot *= Quaternion.Euler(0, -90, 0);
        rotateRightAction.performed += ctx => rot *= Quaternion.Euler(0, 90, 0);
        rotateZAction.performed += ctx => rot *= Quaternion.Euler(0, 0, 90);
        snapToggleAction.performed += ctx => snapToGrid = !snapToGrid;

        placeAction.performed += ctx => TryPlace();
        removeAction.performed += ctx => TryRemove();

        map.Enable();
    }

    void OnDisable()
    {
        map?.Disable();
    }

    void Start()
    {
        if (registry != null) registry.Init();
        CreatePreview();
    }

    void Update()
    {
        UpdatePreviewTransform();
    }

    void CreatePreview()
    {
        if (preview) Destroy(preview);
        var def = selected; if (def == null || def.prefab == null) return;
        preview = Instantiate(def.prefab);
        foreach (var r in preview.GetComponentsInChildren<Renderer>())
        {
            r.material = previewMat;
        }
        var cols = preview.GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false; // preview does not participate in physics
    }

    void UpdatePreviewTransform()
    {
        if (!preview) return;
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 500f))
        {
            var pos = hit.point;
            if (snapToGrid) pos = GridUtil.Snap(pos + hit.normal*0.5f); // Raise by half block height
            preview.transform.position = pos ; // Raise by half block height
            preview.transform.rotation = rot;
        }
    }

    void TryPlace()
    {
        var def = selected; if (def == null) return;
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 500f))
        {
            var dir = hit.normal;
            var pos = hit.point;
            if (snapToGrid) pos = GridUtil.Snap(pos + dir*0.5f); // Raise by half block height

            // Validity check: must be adjacent to existing block if required
            if (def.mustAttachToStructure && assembler.runtimeBlocks.Count > 0)
            {
                bool adjacent = false;
                foreach (var go in assembler.runtimeBlocks)
                {
                    if (!go) continue;
                    float d = Vector3.Distance(go.transform.position, pos);
                    if (d <= GridUtil.CellSize + 0.01f) { adjacent = true; break; }
                }
                if (!adjacent) return;
            }

            // Instance in scene
            var goVar = Instantiate(def.prefab, assembler.transform);
            goVar.transform.position = pos; // Raise by half block height
            goVar.transform.rotation = rot;
            goVar.transform.localScale = Vector3.one;
            var rb = goVar.GetComponent<Rigidbody>(); if (rb) Destroy(rb);
            var dmg = goVar.GetComponent<DamageableBlock>(); if (!dmg) dmg = goVar.AddComponent<DamageableBlock>();
            dmg.Initialize(assembler, def, def.blockHP);

            assembler.runtimeBlocks.Add(goVar);
            assembler.RecalculateAggregates();
        }
    }

    void TryRemove()
    {
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 500f))
        {
            var dmg = hit.collider.GetComponentInParent<DamageableBlock>();
            if (dmg != null && assembler.runtimeBlocks.Contains(dmg.gameObject))
            {
                assembler.OnBlockDestroyed(dmg);
            }
        }
    }
}