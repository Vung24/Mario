using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static Bullet Instance { get; private set; }

    [SerializeField] private GameObject bulletPrefab;
    private Queue<GameObject> pool;
    private int poolSize = 10;
    private float bulletSpeed = 5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        pool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    public void GetBullet(Vector3 position, Vector3 direction)
    {
        if (pool.Count > 0)
        {
            GameObject bullet = pool.Dequeue();
            bullet.SetActive(true);
            bullet.transform.position = position;
            
            ReuseBullet bulletScript = bullet.GetComponent<ReuseBullet>();
            if (bulletScript != null)
            {
                bulletScript.OnBulletSpawned(position);
            }
            
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                bullet.SetActive(false);
                pool.Enqueue(bullet);
                return;
            }
            
            rb.velocity = direction * bulletSpeed;
        }
    }
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        pool.Enqueue(bullet);
    }


}
