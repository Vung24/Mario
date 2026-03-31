using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantEnemy : MonoBehaviour
{
    [SerializeField] private Bullet bulletManager;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private string attackAnimationTrigger = "Attack"; // Tên trigger animation

    private float fireTimer = 0f;
    private GameObject player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (bulletManager == null)
        {
            bulletManager = GetComponent<Bullet>();
        }
        fireTimer = fireRate;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null && IsPlayerInRange())
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                ShootBullet();
                fireTimer = 1f / fireRate;
            }
        }
        else
        {
            fireTimer = 1f / fireRate;
        }
    }

    private bool IsPlayerInRange()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            return false;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool inDistance = distance <= detectionRange;
        
        // Chỉ bắn nếu player ở BÊN TRÁI (player.x < plant.x)
        bool playerOnLeft = player.transform.position.x < transform.position.x;

        return inDistance && playerOnLeft;
    }

    private void ShootBullet()
    {
        if (bulletManager != null)
        {
            Vector3 shootDirection = Vector3.left; 
            
            bulletManager.GetBullet(
                shootPoint != null ? shootPoint.position : transform.position, 
                shootDirection
            );
            
            UpdateAnimation();
        }
    }
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(attackAnimationTrigger);
        }
    }
}

