using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TesterCityNav : MonoBehaviour
{
    [SerializeField]

    

    void Start()
    {
        // Getting the class object for this agent
        NavMeshAgent navMeshAgent = this.GetComponent<NavMeshAgent>();

        Vector3 targetVector;

        if (this.transform.position.x < 0)
        {
            targetVector = new Vector3(100f, 21f, 100f);
        }
        else
        {
            targetVector = new Vector3(-100f, 21f, 100f);
        }

        // Setting the destination for the agents navmesh to start calcs
        navMeshAgent.SetDestination(targetVector);
    }

}
