using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flammable : MonoBehaviour
{
    public GameObject firePrefab;       // drag your fire VFX prefab here
    public GameObject smokePrefab;
    public GameObject explosionPrefab;
    public AudioClip fireSFX;
    public AudioClip explosionSFX;
    public float burnDuration = 3f;     // how long it burns before destroying
    public Transform[] spawnPointArray; // assign an empty GameObject as a spawn point

    private bool isBurning = false;

    public void Ignite()
    {
        if (isBurning) return;          // prevent igniting twice
        isBurning = true;
        StartCoroutine(BurnRoutine());
    }

    IEnumerator BurnRoutine()
    {
        isBurning = true;

        // Play SFX
        AudioSource.PlayClipAtPoint(fireSFX, transform.position);

        foreach (Transform pos in spawnPointArray)
        {
            GameObject fire = Instantiate(firePrefab, pos.position, Quaternion.identity);
            //fire.transform.localScale = Vector3.one * 2f;
            Destroy(fire, burnDuration);
        }

        foreach (Transform pos in spawnPointArray)
        {
            GameObject smoke = Instantiate(smokePrefab, pos.position + new Vector3(0.1f, 0, 0.05f), Quaternion.identity);
            smoke.transform.localScale = Vector3.one * 2f;
            Destroy(smoke, burnDuration);
        }

        yield return new WaitForSeconds(burnDuration);

        if (explosionSFX != null)
        {
            AudioManager.Instance.PlaySFX(explosionSFX);
        }

        GameObject fireExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        fireExplosion.transform.localScale = Vector3.one * 2f;

        Destroy(fireExplosion, 1.0f);
        Destroy(gameObject);
    }
}
