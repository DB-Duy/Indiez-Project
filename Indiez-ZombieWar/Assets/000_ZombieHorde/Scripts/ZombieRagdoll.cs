using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum RagdollDeathBehavior
{
    Dissolve = 0,
    BakeOnHitGround = 1,
    BakeOnTimer = 2
}
public class ZombieRagdoll : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private Collider[] _cols;
    [SerializeField, HideInInspector]
    private Rigidbody[] _rbs;
    //public bool CanBake = false;
    [SerializeField, HideInInspector]
    public SkinnedMeshRenderer SkinnedMesh;
    public float RagdollTimeout = 10f;
    private bool _hasTimedOut = false;
    private bool _bakeMinTimePassed = false;
    public float MinTimeForBake = 2f;
    private Material _mat;
    public bool isPlayHighlight = false;
    private Quaternion[] _boneRotations;
    private Vector3[] _bonePos;
    [HideInInspector]
    private int _groundContactCount = 0;
    private Tween delayCalls, playHighlight, disableRagdoll, dissolve;
    [SerializeField]
    public RagdollDeathBehavior OnDeath = RagdollDeathBehavior.Dissolve;

    private void OnValidate()
    {
        _cols = GetComponentsInChildren<Collider>();
        _rbs = GetComponentsInChildren<Rigidbody>();
        SkinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    public bool CanBake()
    {
        if (!_bakeMinTimePassed) { return false; }
        if (_hasTimedOut) { return true; }
        else
        {
            return IsStationary();
        }
    }

    private bool IsStationary()
    {
        bool isStationary = true;
        foreach (Rigidbody rb in _rbs)
        {
            if (rb.velocity.sqrMagnitude > 0.04f || rb.angularVelocity.sqrMagnitude > 0.04f)
            {
                isStationary = false;
                break;
            }
        }
        return isStationary;
    }
    private void Awake()
    {
        _mat = SkinnedMesh.material;
        if (_boneRotations == null)
        {
            _boneRotations = new Quaternion[SkinnedMesh.bones.Length];
        }
        if (_bonePos == null)
        {
            _bonePos = new Vector3[SkinnedMesh.bones.Length];
        }
        for (int i = 0; i < SkinnedMesh.bones.Length; i++)
        {
            _boneRotations[i] = SkinnedMesh.bones[i].localRotation;
            _bonePos[i] = SkinnedMesh.bones[i].localPosition;
        }
        for (int i = 0; i < _rbs.Length; i++)
        {
            _rbs[i].collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
        if (OnDeath == RagdollDeathBehavior.BakeOnHitGround || OnDeath == RagdollDeathBehavior.Dissolve)
        {
            for (int i = 0; i < _cols.Length; i++)
            {
                var col = _cols[i].AddComponent<ZombieRagdollCollider>();
                col.Ragdoll = this;
            }
        }
    }
    public void OnLeaveGround()
    {
        _groundContactCount--;
    }
    public void OnHitGround()
    {
        _groundContactCount++;
        if (_groundContactCount >= 5)
        {
            if (OnDeath == RagdollDeathBehavior.BakeOnHitGround)
            {
                MarkRagdollForBake();
            }
            else if (OnDeath == RagdollDeathBehavior.Dissolve)
            {
                DissolveRagdoll();
            }

        }
    }

    private void DissolveRagdoll()
    {
        if (dissolve == null)
        {
            dissolve = DOTween.Sequence()
                .AppendInterval(0.5f)
                .Append(_mat.DOFloat(1f, "_DissolveAmount", 0.5f))
                .OnComplete(() => gameObject.SetActive(false))
                .SetAutoKill(false)
                .SetRecyclable(true);
        }
        else
        {
            dissolve.Restart();
        }
    }

    private void MarkRagdollForBake()
    {
        if (disableRagdoll == null)
        {
            disableRagdoll = DOTween.Sequence()
                .AppendInterval(0.2f)
                .AppendCallback(() =>
                {
                    DisableRagdoll();
                    _bakeMinTimePassed = true;
                    _hasTimedOut = true;
                })
                .SetAutoKill(false)
                .SetRecyclable(true);
        }
        else
        {
            disableRagdoll.Restart();
        }
    }

    private void OnEnable()
    {
        if (delayCalls == null)
        {
            delayCalls = DOTween.Sequence()
                .Append(DOVirtual.DelayedCall(RagdollTimeout, () => _hasTimedOut = true))
                .Join(DOVirtual.DelayedCall(MinTimeForBake, () => _bakeMinTimePassed = true))
                .SetAutoKill(false)
                .SetRecyclable(true);
        }
        else
        {
            delayCalls.Restart();
        }
        if (isPlayHighlight)
        {
            PlayHighlight();
        }
    }
    private void PlayHighlight()
    {
        if (_mat == null)
        {
            _mat = SkinnedMesh.material;
        }
        if (playHighlight == null)
        {
            playHighlight = DOTween.Sequence()
                .Append(_mat.DOFloat(3f, "_HighlightIntensity", 0.1f))
                .Append(_mat.DOFloat(0f, "_HighlightIntensity", 0.1f))
                .SetAutoKill(false)
                .SetRecyclable(true);
        }
        else
        {
            playHighlight.Restart();
        }
    }
    private void OnDisable()
    {
        for (int i = 0; i < SkinnedMesh.bones.Length; i++)
        {
            SkinnedMesh.bones[i].localRotation = _boneRotations[i];
            SkinnedMesh.bones[i].localPosition = _bonePos[i];
        }
        //playHighlight.Rewind();
        //delayCalls.Rewind();
        //if (disableRagdoll != null)
        //{
        //    disableRagdoll.Rewind();
        //}
        //if (dissolve != null)
        //{
        //    dissolve.Rewind();
        //}
    }
    public void DisableRagdoll()
    {
        for (int i = 0; i < _cols.Length; i++)
        {
            _cols[i].enabled = false;
        }
        for (int i = 0; i < _rbs.Length; i++)
        {
            _rbs[i].isKinematic = true;
        }
    }

    public void ResetRagdoll()
    {
        for (int i = 0; i < _cols.Length; i++)
        {
            _cols[i].enabled = true;
        }
        for (int i = 0; i < _rbs.Length; i++)
        {
            _rbs[i].isKinematic = false;
        }
        for (int i = 0; i < _rbs.Length; i++)
        {
            _rbs[i].velocity = Vector3.zero;
            _rbs[i].angularVelocity = Vector3.zero;
        }
        _hasTimedOut = false;
        _bakeMinTimePassed = false;
        delayCalls.Pause();
        playHighlight.Pause();
        if (disableRagdoll != null)
        {
            disableRagdoll.Pause();
        }
        _mat.SetFloat("_HighlightIntensity", 0);
        _mat.SetFloat("_DissolveAmount", 0);
    }
    public void Explode(Vector3 origin, float radius, float ExplodeForce, float ExplodeUpForce)
    {
        for (int i = 0; i < _rbs.Length; i++)
        {
            _rbs[i].AddExplosionForce(ExplodeForce, origin, radius + 1, ExplodeUpForce, ForceMode.Impulse);
        }
        //rootRb.AddExplosionForce(300f, origin, 5f, 100f, ForceMode.Impulse);
    }
    public void ApplyForce(Vector3 force)
    {
        for (int i = 0; i < _rbs.Length; i++)
        {
            _rbs[i].AddForce(force, ForceMode.Impulse);
        }
    }
}
