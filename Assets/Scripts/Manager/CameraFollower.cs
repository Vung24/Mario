using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollower : MonoBehaviour
{
    public static CameraFollower Instance { get; private set; }
    private CinemachineVirtualCamera virtualCamera;
    private Transform pendingTarget;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureVirtualCamera();
    }

    void Start()
    {
        EnsureVirtualCamera();
        ApplyPendingTarget();
    }

    public void SetFollowTarget(Transform target)
    {
        pendingTarget = target;
        EnsureVirtualCamera();

        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
    }

    private void EnsureVirtualCamera()
    {
        if (virtualCamera != null)
        {
            return;
        }

        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        }
    }

    private void ApplyPendingTarget()
    {
        if (pendingTarget == null)
        {
            return;
        }

        SetFollowTarget(pendingTarget);
    }
}
