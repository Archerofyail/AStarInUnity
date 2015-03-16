using UnityEngine;
using System.Collections;

public class ChaserSpawner : MonoBehaviour
{
	private float minSpawnTime = 1f;
	private float maxSpawnTime = 3f;
	private GameObject prefab;
	// Use this for initialization
	void Start()
	{
		prefab = Resources.Load<GameObject>("Chaser");
		StartCoroutine(SpawnCoroutine());
	}


	IEnumerator SpawnCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
			Instantiate(prefab);
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}

}
