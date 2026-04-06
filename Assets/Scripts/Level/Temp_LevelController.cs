using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp_LevelController : MonoBehaviour
{
    public Transform startPos;

    public Transform ResolveStartPos()
    {
        if (startPos != null && startPos.IsChildOf(transform))
        {
            return startPos;
        }

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.name == "Start")
            {
                startPos = child;
                return startPos;
            }
        }

        foreach (Transform child in allChildren)
        {
            if (child.name == "CheckPoint" || child.name == "Checkpoint")
            {
                startPos = child;
                return startPos;
            }
        }

        startPos = null;
        return null;
    }

    private void OnEnable()
    {
        Transform start = ResolveStartPos();

        if (start != null)
        {
            GameManager.Instance.SetStartCheckpoint(start);
        }
    }
}

