using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 3f;
    private Vector3 target;
    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private bool movingToB = true;
    // Start is called before the first frame update
    void Start()
    {
        if (pointA == null || pointB == null)
        {
            enabled = false;
            return;
        }

        pointAPosition = pointA.position;
        pointBPosition = pointB.position;
        target = pointBPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Moving();
    }
    private void Moving()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if((transform.position - target).sqrMagnitude <= 0.0001f)
        {
            movingToB = !movingToB;
            target = movingToB ? pointBPosition : pointAPosition;
        }
    }
    
}
