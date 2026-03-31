using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ReuseButtle : MonoBehaviour
{
    private Vector3 spawnPosition; 
    [SerializeField] private float maxDistance = 10f; // Khoảng cách tối đa trước khi destroy

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
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance?.GameOver();
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

