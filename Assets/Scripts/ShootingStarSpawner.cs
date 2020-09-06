using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingStarSpawner : MonoBehaviour
{
    public float m_starSpawnRate;
    public float m_starSpeed;
    public Vector3 m_spawnPosition;
    public Vector3 m_goalPosition;
    public GameObject m_shootingStar;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnShootingStars());
    }

    private IEnumerator SpawnShootingStars()
    {
        float spawnRate;
        GameObject shootingStar;
        Vector3 goalPosition;
        while (true)
        {
            spawnRate = Random.Range(m_starSpawnRate - 20, m_starSpawnRate + 20);
            shootingStar = Instantiate(m_shootingStar, gameObject.transform);
            shootingStar.transform.position = new Vector3(Random.Range(m_spawnPosition.x - 2, m_spawnPosition.x + 2), Random.Range(m_spawnPosition.y - 2, m_spawnPosition.y + 2), shootingStar.transform.position.z);
            goalPosition = new Vector3(Random.Range(m_goalPosition.x - 2, m_goalPosition.x + 2), Random.Range(m_goalPosition.y - 2, m_goalPosition.y + 2), m_goalPosition.z);
            StartCoroutine(MoveShootingStar(shootingStar.transform, goalPosition));
            yield return new WaitForSeconds(m_starSpawnRate);
        }
    }

    private IEnumerator MoveShootingStar(Transform l_shootingStar, Vector3 l_goalPosition)
    {
        while (l_shootingStar.position != m_goalPosition)
        {
            l_shootingStar.position = Vector3.MoveTowards(l_shootingStar.position, m_goalPosition, m_starSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(l_shootingStar.gameObject);
    }
}
