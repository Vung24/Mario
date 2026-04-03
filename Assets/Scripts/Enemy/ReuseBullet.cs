using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ReuseBullet : MonoBehaviour
{
    private Vector3 spawnPosition; 
    private float maxDistance = 15f; 

    public void OnBulletSpawned(Vector3 position)
    {
        spawnPosition = position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, spawnPosition);
        if (distance > maxDistance)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool hitPlayer = collision.CompareTag("Player") || collision.GetComponentInParent<PlayerCollision>() != null;
        if (hitPlayer)
        {
            PlayerSkill skill = collision.GetComponentInParent<PlayerSkill>();
            if (skill == null || !skill.GuyImmortal())
            {
                GameManager.Instance?.GameOver();
            }

            ReturnToPool();
        }
        else
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        FindObjectOfType<Bullet>().ReturnBullet(gameObject);
    }
}

