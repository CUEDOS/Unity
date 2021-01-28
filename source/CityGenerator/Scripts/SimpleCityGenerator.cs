using System;
using UnityEngine;
using Random = UnityEngine.Random;
namespace SCG
{
    public class SimpleCityGenerator : MonoBehaviour
    {
        // generator public variables

        #region Public Variables
        public float tileSize;
        public enum Type { LowRiseResidential, HighRiseResidential, Commercial, Industrial, Rural };
        public Type type;
        
        public GameObject buildingPrefab;
        //public GameObject distributionPrefab;

        #endregion


        #region Private Variables
        
        private float _bci;
        private float _fai;
        private float _hVariability;
        private float _arVariability;
        private float _meanCellSize = 30f;


        private const float MeanFloorHeight = 2.3f;
        private int _gridLength;
        private float _cellSize;
        private float _meanNumFloors;
        private Vector3 _startPos;
        private GameObject _spawner;
        private GameObject _distCentres;
        
        #endregion


        private void Start()
        {
            switch (type)
            {
                case Type.LowRiseResidential: 
                    _bci = 0.3f;
                    _fai = 0.3f;
                    _hVariability = 1f;
                    _arVariability = 0.3f;
                    _meanCellSize = 20f;
                    break;
                
                case Type.HighRiseResidential:
                    _bci = 0.7f;
                    _fai = 3;
                    _hVariability = 3f;
                    _arVariability = 0.2f;
                    _meanCellSize = 30f;
                    break;
                
                case Type.Commercial:
                    _bci = 0.7f;
                    _fai = 11f;
                    _hVariability = 3f;
                    _arVariability =0.2f;
                    _meanCellSize = 30f;
                    break;
                
                case Type.Industrial:
                    _bci = 0.9f;
                    _fai = 1f;
                    _hVariability = 0.5f;
                    _arVariability = 1f;
                    _meanCellSize = 40f;
                    break;
                
                case Type.Rural:
                    _bci = 0.1f;
                    _fai = 0.05f;
                    _hVariability = 0.1f;
                    _arVariability = 0.1f;
                    _meanCellSize = 60f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            _gridLength = (int)Mathf.Ceil(tileSize / _meanCellSize);
            _cellSize = tileSize / _gridLength;

            _meanNumFloors = _fai / _bci;

            _startPos = new Vector3(-_cellSize * _gridLength / 2, 0f, -_cellSize * _gridLength / 2) + transform.position;

            for (int x = 0; x < _gridLength; x++)
            {

                for (int y = 0; y < _gridLength; y++)
                {
                    int floors = (int)Mathf.Ceil(Randn(_meanNumFloors, _hVariability * Mathf.Pow(_meanNumFloors, 1), 0, 5 * _meanNumFloors));

                    // Height
                    float dy = floors * MeanFloorHeight;

                    //Length and Width
                    float dx = _cellSize * Randn(_bci, _arVariability, 0.1f, 1);
                    float dz = _cellSize * Randn(_bci, _arVariability, 0.1f, 1);

                    Vector3 scale = new Vector3(dx, dy, dz);

                    Vector3 pos = new Vector3((x + Random.Range(0.5f - ((_cellSize - dx) / (2 * _cellSize)), 0.5f + ((_cellSize - dx) / (2 * _cellSize)))) * _cellSize,
                        dy / 2,
                        (y + Random.Range(0.5f - ((_cellSize - dz) / (2 * _cellSize)), 0.5f + ((_cellSize - dz) / (2 * _cellSize)))) * _cellSize) + _startPos;

                    if (floors > 0)
                    {
                        _spawner = Instantiate(buildingPrefab, pos, Quaternion.identity, transform);
                        _spawner.transform.localScale = scale;
                    }
                }

            }

        }

        // public void SpawnDistribution()
        // {
        //     _distCentres = new GameObject("DistCentres");
        //     float distPos = tileSize * 0.5f + 50f;
        //     int[] xs = new int[] { -1, -1, 1, 1 };
        //     int[] ys = new int[] { -1, 1, -1, 1 };
        //
        //     for (int i = 0; i < 4; i++)
        //     {
        //         Vector3 pos = new Vector3(distPos * xs[i], 0, distPos * ys[i]);
        //         _spawner = Instantiate(distributionPrefab, pos, Quaternion.identity, _distCentres.transform);
        //     }
        //
        // }

        public void Spawn()
        {
            Start();
            DestroyAll();
            SpawnBuildings();
        }

        public void DestroyAll()
        {
            var count = transform.childCount;
            for (var i = 0; i < count; i++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

        }

        
    }
}