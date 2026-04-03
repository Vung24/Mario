using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomController : MonoBehaviour
{
    [SerializeField] private string mushroomTag = "Mushroom";
    [SerializeField] private float buffDuration = 5f;
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float jumpMultiplier = 1.35f;
    [SerializeField] private float scaleMultiplier = 1.4f;
    [SerializeField] private float collectDelayAfterSpawn = 0.2f;

    private bool collected;
    private bool canCollect;

    private void OnEnable()
    {
        collected = false;
        canCollect = false;
        StartCoroutine(EnableCollectAfterDelay());
    }

    private IEnumerator EnableCollectAfterDelay()
    {
        if (collectDelayAfterSpawn > 0f)
        {
            yield return new WaitForSeconds(collectDelayAfterSpawn);
        }

        canCollect = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    private void TryCollect(GameObject target)
    {
        bool isMushroomTagged = CompareTag(mushroomTag) ||
                               (transform.parent != null && transform.parent.CompareTag(mushroomTag));
        if (!isMushroomTagged)
        {
            return;
        }

        if (collected || !canCollect)
        {
            return;
        }

        PlayerController player = target.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }

        collected = true;
        player.ApplyMushroomBuff(buffDuration, speedMultiplier, jumpMultiplier, scaleMultiplier);
        Destroy(gameObject);
    }
}
