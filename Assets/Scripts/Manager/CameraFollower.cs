using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollower : MonoBehaviour
{
    public static CameraFollower Instance { get; private set; }
    private CinemachineVirtualCamera virtualCamera;
    private Transform Target;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        VirtualCamera();
    }

    void Start()
    {
        VirtualCamera();
        ApplyPendingTarget();
    }

    public void SetFollowTarget(Transform target)
    {
        Target = target;
        VirtualCamera();

        if (virtualCamera != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
    }

    private void VirtualCamera()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
        {
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        }
    }

    private void ApplyPendingTarget()
    {
        if (Target == null)
        {
            return;
        }

        SetFollowTarget(Target);
    }
}
