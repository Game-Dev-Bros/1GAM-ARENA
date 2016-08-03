using System.Collections;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
	public float spawnRate = 3.0f; // seconds

	public GameObject[] enemyPrefabs;

	private Spawner[] _spawners;
	private GameObject _enemyParent;

	void Awake()
	{
		_spawners = FindObjectsOfType<Spawner>();
		_enemyParent = new GameObject("Enemies");
	}

	void Start()
	{
		StartCoroutine(SpawnCoroutine());
	}

	public GameObject GetEnemyPrefab()
	{
		if (enemyPrefabs.Length == 0)
			return null;

		return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
	}

	public void Spawn(int spawnerIndex = -1)
	{
		if(_spawners.Length <= 0)
			return;

		if (spawnerIndex == -1)
			spawnerIndex = Random.Range(0, _spawners.Length);

		_spawners[spawnerIndex].Spawn();
	}

	private IEnumerator SpawnCoroutine()
	{
		yield return new WaitForSeconds(spawnRate);
		Spawn();
		yield return StartCoroutine(SpawnCoroutine());
	}

	public GameObject GetEnemyParent()
	{
		return _enemyParent;
	}
}