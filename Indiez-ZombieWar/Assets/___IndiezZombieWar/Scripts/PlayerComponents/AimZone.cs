using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimZone : MonoBehaviour
{
    private Collider[] overlapColliders = new Collider[100];
    public bool IsGettingTargets = true;
    [SerializeField]
    private Transform _sphere1, _sphere2;
    [SerializeField]
    private float Radius;
    [SerializeField, HideInInspector]
    public bool gizmo = false;
    public LayerMask _zombieMask;
    [SerializeField, HideInInspector]
    private ActionManager _actionManager;
    public int targetsInZone { get; private set; }

    private void OnValidate()
    {
        _actionManager = FindObjectOfType<ActionManager>();
    }
    private void OnDrawGizmos()
    {
        if (!gizmo) { return; }
        Gizmos.DrawSphere(_sphere1.position, Radius);
        Gizmos.DrawSphere(_sphere2.position, Radius);
    }
    [ContextMenu("ToggleGizmo")]
    private void ToggleGizmo()
    {
        gizmo = !gizmo;
    }

    private void Awake()
    {
        _actionManager.OnRequestAcquireTarget += GetTargets;
        _actionManager.OnPlayerDead += StopGettingTargets;
    }
    private void StopGettingTargets()
    {
        CancelInvoke(nameof(GetTargets));
    }
    private void Start()
    {
        InvokeRepeating(nameof(GetTargets), 0, 0.1f);

    }
    private void OnDestroy()
    {
        _actionManager.OnRequestAcquireTarget -= GetTargets;
        _actionManager.OnPlayerDead -= StopGettingTargets;
    }
    public void GetTargets()
    {
        if (!IsGettingTargets) { return; }
        targetsInZone = Physics.OverlapCapsuleNonAlloc(_sphere1.position, _sphere2.position, Radius, overlapColliders, _zombieMask, QueryTriggerInteraction.Collide);
        if (targetsInZone > 0)
        {
            float closest = Mathf.Infinity;
            int closestIdx = 0;
            for (int i = 0; i < targetsInZone; i++)
            {
                float dist = Vector3.SqrMagnitude(transform.position - overlapColliders[i].transform.position);
                if (dist < closest)
                {
                    closestIdx = i;
                    closest = dist;
                }
            }
            _actionManager.AcquireTarget(overlapColliders[closestIdx].transform);
        }
        else
        {
            _actionManager.AcquireTarget(null);
        }
    }
}
