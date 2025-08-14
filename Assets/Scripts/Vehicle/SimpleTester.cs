// ============================================================================
// Scripts/Vehicle/SimpleHitTester.cs — test "shooter" for damage checking
// ============================================================================
using UnityEngine;

public class SimpleHitTester : MonoBehaviour
{
    public Camera cam;
    public int damagePerShot = 25;
    public float fireRate = 10f; // shots/sec
    float cd;

    void Update()
    {
        cd -= Time.deltaTime;
        if (Input.GetMouseButton(0) && cd <= 0f)
        {
            cd = 1f / fireRate;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                var dmg = hit.collider.GetComponentInParent<DamageableBlock>();
                if (dmg != null) dmg.ApplyDamage(damagePerShot);
            }
        }
    }
}
