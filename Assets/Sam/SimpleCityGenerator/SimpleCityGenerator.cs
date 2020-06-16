using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleCityGenerator : MonoBehaviour
{
    // generator public variables

    public float citySize;
    public float BCI;
    public float FAI;
    public float HVariability;
    public float ARVariability;
    public float meanCellSize = 30f;
    public bool useSimManager = true;
    public bool SpawnDistributions = true;

    public GameObject buildingPrefab;
    public GameObject distributionPrefab;

    // generator private variables
    
    private float meanFloorHeight = 2.3f;
    private int gridLength;
    private float cellSize;
    private float meanNumFloors;
    private Vector3 startPos;
    private GameObject Spawner;
    private GameObject DistCentres;

    private void Start()
    {
        if (useSimManager) GetVariables();
    }

    private float Randn(float mean, float var, float min = 0.5f, float max = 1.5f)
    {
        if (max < min)
        {
            Debug.LogWarning("Randn: max bust be higher than min");
        }
        float rand1 = Random.Range(0.0f, 1.0f);
        float rand2 = Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        float generatedNumber = mean + var * n;

        generatedNumber = Mathf.Clamp(generatedNumber, min, max);

        return generatedNumber;
    }

    public void SpawnBuildings()
    {
        gridLength = (int) Mathf.Ceil(citySize / meanCellSize);
        cellSize = citySize / gridLength;

        meanNumFloors = FAI / BCI;

        startPos = new Vector3(-cellSize * gridLength / 2, 0f, -cellSize * gridLength / 2) + transform.position;

        for (int x = 0; x < gridLength; x++)
        {
            for (int y = 0; y < gridLength; y++)
            {
                int floors = (int) Mathf.Ceil(Randn(meanNumFloors, HVariability * Mathf.Pow(meanNumFloors, 1), 0, 5 * meanNumFloors));

                // Height
                float dy = floors * meanFloorHeight;

                //Length and Width
                float dx = cellSize * Randn(BCI, ARVariability, 0.1f, 1);
                float dz = cellSize * Randn(BCI, ARVariability, 0.1f, 1);

                Vector3 scale = new Vector3(dx, dy, dz);

                Vector3 pos = new Vector3((x + Random.Range(0.5f - ((cellSize - dx) / (2 * cellSize)), 0.5f + ((cellSize - dx)/(2 * cellSize)))) * cellSize,
                    dy / 2,
                    (y + Random.Range(0.5f - ((cellSize - dz) / (2 * cellSize)), 0.5f + ((cellSize - dz) / (2 * cellSize)))) * cellSize) + startPos;

                if (floors > 0)
                {
                    Spawner = Instantiate(buildingPrefab, pos, Quaternion.identity, transform);
                    Spawner.transform.localScale = scale;
                }
            }

        }
        
    }

    public void SpawnDistribution()
    {
        DistCentres = new GameObject("DistCentres");
        float distPos = citySize * 0.5f + 50f;
        int[] xs = new int[] { -1, -1, 1, 1 };
        int[] ys = new int[] { -1, 1, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            Vector3 pos = new Vector3(distPos * xs[i], 0, distPos * ys[i]); 
            Spawner = Instantiate(distributionPrefab, pos, Quaternion.identity, DistCentres.transform);
        }

    }

    public void Spawn()
    {
        if (useSimManager)
        {
            GetVariables();
            DestroyAll();
            SpawnBuildings();
        }

        if (SpawnDistributions) SpawnDistribution();
    }

    // Changes the buildings is using editor spawn
    public void EditorSpawn()
    {
        DestroyAll();
        SpawnBuildings();
        if (SpawnDistributions) SpawnDistribution();
    }

    public void DestroyAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

    }

    public void GetVariables()
    {
        citySize = SimManager.expVars["CitySize"];
        BCI = SimManager.expVars["BCI"];
        FAI = SimManager.expVars["FAI"];
        HVariability = SimManager.expVars["HVariability"];
        ARVariability = SimManager.expVars["ARVariability"];
    }

}
