using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject[] Targets;
    public Vector3 brickPosition = new Vector3(10, 0, 0);
    private Vector3 _MiddlePoint;

    private void Start()
    {

        Targets = GameObject.FindGameObjectsWithTag("UAV");
    }

    void Update()
    {
        _MiddlePoint = GetAveragePos(Targets);
        FollowTargets();
    }

    public virtual void FollowTargets()
    {
        Vector3 dir = (Camera.main.transform.position - _MiddlePoint).normalized;
        Vector3 averagePos = GetAveragePos(Targets);
        transform.position = averagePos + brickPosition;
    }

    protected virtual Vector3 GetAveragePos(GameObject[] targets)
    {
        Vector3 averagePos = Vector3.zero;
        Vector3[] positions = new Vector3[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            positions[i] = targets[i].transform.position;
            averagePos += positions[i];
        }
        return averagePos / positions.Length;
    }
}
