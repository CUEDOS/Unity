using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridSpawn))]
public class SpawnScript : MonoBehaviour
{
    public float spawnRate = 0f;
    float waitDuration;
    float counter = 0f;

    GridSpawn grid;

    // Start is called before the first frame update
    void Start()
    {
        grid = GetComponent<GridSpawn>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        waitDuration = (Random.value + 0.5f / 2f) / spawnRate;
        counter += Time.fixedDeltaTime;
        if (counter >= waitDuration)
        {
            grid.SpawnAtNextPoint();
            counter -= waitDuration;
        }
    }
}
