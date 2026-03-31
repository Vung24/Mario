using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnemy : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speedSlime = 3f;
    private bool faceMovementDirection = true;
    private bool invertFacing = false;
    private SpriteRenderer spriteRenderer;

    private Vector3 target;
    private Vector3 pointAPosition;
    private Vector3 pointBPosition;
    private bool movingToB = true;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            enabled = false;
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        pointAPosition = pointA.position;
        pointBPosition = pointB.position;
        target = pointBPosition;
        UpdateFacing();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speedSlime * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude <= 0.0001f)
        {
            movingToB = !movingToB;
            target = movingToB ? pointBPosition : pointAPosition;
        }

        if (faceMovementDirection)
        {
            UpdateFacing();
        }
    }

    private void UpdateFacing()
    {
        float directionX = target.x - transform.position.x;
        if (Mathf.Abs(directionX) < 0.0001f)
        {
            return;
        }

        bool movingRight = directionX < 0f;
        bool shouldFaceRight = invertFacing ? !movingRight : movingRight;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !shouldFaceRight;
            return;
        }

        Vector3 scale = transform.localScale;
        float absX = Mathf.Abs(scale.x);
        scale.x = shouldFaceRight ? absX : -absX;
        transform.localScale = scale;
    }
}
