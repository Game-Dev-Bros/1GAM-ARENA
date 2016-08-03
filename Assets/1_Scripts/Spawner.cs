using UnityEngine;

public class Spawner : MonoBehaviour
{
	private SpawnerManager _spawnerManager;

	void Awake()
	{
		_spawnerManager = FindObjectOfType<SpawnerManager>();
	}

	public void OnDrawGizmos()
	{
		DebugExtension.DrawCircle(transform.position, transform.forward, Color.red, 0.5f);
	}

	public void Spawn()
	{
		GameObject enemyPrefab = _spawnerManager.GetEnemyPrefab();

		if (enemyPrefab == null)
			return;

		GameObject newEnemy = Instantiate(enemyPrefab);
		newEnemy.transform.position = transform.position;
		newEnemy.transform.parent = _spawnerManager.GetEnemyParent().transform;
	}
}
