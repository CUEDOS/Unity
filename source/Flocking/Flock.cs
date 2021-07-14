using System;
using System.Collections.Generic;
using Samspace;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class Flock : MonoBehaviour
{
    #region Private Variables
    private Vector3 _totalForce;
    private Vector3 _separationForce;
    private Vector3 _migrationForce;
    private Vector3 _interForce;
    private Vector3 _laneFormForce;
    private Vector3 _selAlignForce;
    private Vector3 _orthSepForce;

    private Rigidbody _rb;
    private CollisionManager _cm;
    private Collider _collider;
    private NDExp _nd;

    private Vector3 _velocityIdeal;
    private Vector3 _migrationVector;
        
    private Vortex[] _vortexList;
    private SourceSink[] _sourceSink;

    #endregion
 
    #region Inspector
    [Header("Force Magnitudes")]
    [SerializeField]
    private float migrationForceMag;
    [SerializeField]
    private float interForceMag;
    [SerializeField]
    private float totalForceMag;
    [SerializeField]
    private float separationForceMag;
    [SerializeField]
    private float laneFormForceMag;
    [SerializeField]
    private float selAlignForceMag;
    [SerializeField]
    private float orthSepForceMag;
            
    [Header("Force Gains")]
    [Range(0, 2)]
    public float migrationGain;
    [Range(0, 2)]
    public float interGain;
    [Range(0, 20)]
    public float laneFormationGain;
    [Range(0, 2)]
    public float separationGain;
    [Range(0, 100)]
    public float selAlignGain;
    [Range(0f, 10f)]
    public float orthSepGain;

    [Header("Force Std")] 
    public float lfStd;
    public float sepStd;
    public float selAlStd;
    public float orthSepStd;

    [FormerlySerializedAs("ND")]
    [Header("Rule Variables")]
    [Tooltip("Non-dimensionalise based on ND Exp Script")]
    public bool nd;
    public float speed;
    public float soiRadius;
    public float flockingInterval;

    [Header("Other Variables")]
    [Tooltip("Destroys agent when arrives at destination")]
    public bool destroyOnArrival;
    [Tooltip("Distance from destination when agent is destroyed")]
    public float destroyDist = 5f;
    [Tooltip("If checked, agents destroy when collide with other agents/obstacles")]
    public bool destroyOnCollision;
    [Tooltip("Delay for when an agent is destroyed")]
    public float destroyDelay;
    [Tooltip("Vector3 for destination, generally useful for debugging to see if correct destination is pre-set")]
    public Vector3 destination = Vector3.zero;

    [Header("Debug - Show Force Gizmos")]
    public bool migration;
    public bool intersection;
    public bool idealVelocity;
    public bool laneFormation;
    public bool orthoganolSeparation;
    public bool noReturn;
    #endregion

    #region Misc Variables
    private bool _returning;
    private Vector3 _startLocation;
    public int spawnerID;
    private const int MAXColiders = 200; // Max colliders in overlap sphere
    private LayerMask _droneMask;
    private const float Sqrt2PI = 2.50662827463f;
        
    #endregion

    private void Awake()
    {
        _collider = GetComponent<Collider>();

    }
    public void Start()
    {
        if (nd)
        {
            _nd = FindObjectOfType<NDExp>();
            speed = _nd.velocity;
            var xs = _nd.Xc;
            soiRadius = xs * 3;
            var ts = _nd.Tc;
            flockingInterval = ts / (2 * Mathf.PI * 20); // 20 flocking calculations per radius
        }

        _startLocation = transform.position;

        _rb = GetComponent<Rigidbody>();
        _cm = FindObjectOfType<CollisionManager>();

        _droneMask = LayerMask.GetMask("Agent");

        _vortexList = FindObjectsOfType<Vortex>();
        _sourceSink = FindObjectsOfType<SourceSink>();
            
        InvokeRepeating(nameof(Flocking), 0, flockingInterval );
    }

    public void SetDestination(Vector3 dest)
    {
        destination = dest;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_cm) _cm.NewConflict(gameObject, collision.gameObject, collision.relativeVelocity);
        if(destroyOnCollision) DestroyAgent();
    }

    private void FixedUpdate()
    {
        // For collision
        _migrationVector = (destination - transform.position).normalized;
        _migrationForce = _migrationVector * speed;
        migrationForceMag = _migrationForce.magnitude;

        if (destination == Vector3.zero)
        {
            // Destroy when far away from origin
            if (transform.position.magnitude > destroyDist && destroyOnArrival)
            {
                DestroyAgent();
            }
        }
        else
        {
            if (!_returning && Vector3.Distance(transform.position, destination) < destroyDist)
            {
                if (noReturn)
                {
                    DestroyAgent();
 
                }
                destination = _startLocation;
                _migrationVector = (destination - transform.position).normalized;
                _migrationForce = _migrationVector * speed;
                _migrationForce *= migrationGain;
                migrationForceMag = _migrationForce.magnitude;
                _returning = true;
            }

            if (_returning && Vector3.Distance(transform.position, destination) < destroyDist)
            {
                DestroyAgent();
            }
        }


        _velocityIdeal = (_interForce + _migrationForce + _laneFormForce + _selAlignForce + _separationForce + _orthSepForce).normalized * speed;
        _rb.velocity = _velocityIdeal;
    }
        
    private float Randn(float mean, float var)
    {
        var rand1 = Random.Range(0.0f, 1.0f);
        var rand2 = Random.Range(0.0f, 1.0f);

        var n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        var generatedNumber = mean + var * n;

        return generatedNumber;
    }

    private void Flocking()
    {
            
        var neighbourCol = new Collider[MAXColiders];
        var size = Physics.OverlapSphereNonAlloc(transform.position, soiRadius, neighbourCol, _droneMask);

        // Find neighbours which are agents
        var neighbours = new List<GameObject>();
        for (var i = 0; i < size; i++)
        {
            if (neighbourCol[i].transform != transform && neighbourCol[i].CompareTag("UAV")) neighbours.Add(neighbourCol[i].gameObject);
        }

        // Flocking Calculations
        if (neighbours.Count > 0)
        {
            var v = _rb.velocity;
                
            // Lane Formaiton
            _laneFormForce = Vector3.zero;

            // SelectiveAlignment
            _selAlignForce = Vector3.zero;
                
            // Orthoganal separation
            _orthSepForce = Vector3.zero;

            foreach (var neighbour in neighbours)
            {
                // Data
                var positionN = neighbour.transform.position;
                var positionT = transform.position;
                var ab = positionN - positionT;
                var dist = ab.magnitude;
                var neightbourVel = neighbour.GetComponent<Rigidbody>().velocity;
                var neighbourMig = neighbour.GetComponent<Flock>()._migrationForce;
                    
                // ==========LaneFormation===============
                var diff = Vector3.Dot(neighbourMig, _migrationForce) /
                           (_migrationForce.magnitude * neighbourMig.magnitude); // Find the difference in migration vectors
                var workingVec = (-Vector3.Dot(_migrationForce, ab) * _migrationForce + _migrationForce.magnitude * _migrationForce.magnitude
                    * ab).normalized * (diff * Norm(dist, lfStd)); // Lane formation force
                _laneFormForce += (!float.IsNaN(workingVec.magnitude)) ? workingVec : Vector3.zero;

                //===========SelectiveAlignment==========
                var cv = (v - neightbourVel).normalized;
                var mag = Mathf.Clamp01(Vector3.Dot(cv, ab.normalized));
                _selAlignForce += Norm(dist,selAlStd) * mag * neightbourVel.normalized;
                    
                // ========Orthoganol Separation=========
                _orthSepForce += Norm(dist, orthSepStd) * mag * Vector3.Cross(_rb.velocity, neightbourVel).normalized;

            }
                
            //===========Lane Formation===============
            _laneFormForce *= laneFormationGain;
            laneFormForceMag = _laneFormForce.magnitude;
                
            //===========SelectiveAlignment==========
            _selAlignForce *= selAlignGain;
            selAlignForceMag = _selAlignForce.magnitude;
                
            // ========Orthoganol Separation=========
            _orthSepForce *= orthSepGain;
            orthSepForceMag = _orthSepForce.magnitude;

        }
        else
        {
            _laneFormForce = Vector3.zero;
            _selAlignForce = Vector3.zero;
            _orthSepForce = Vector3.zero;
        }
            
        //=============Separation=================
        _separationForce = Vector3.zero;

        foreach (var neighbour in neighbours)
        {
            var position = transform.position;
            var sepVec = neighbour.transform.position - position;
            var dist = sepVec.magnitude;
            var repDir = -sepVec.normalized;

            _separationForce += Norm(dist,sepStd) * repDir;

        }

        _separationForce *= separationGain;
        separationForceMag = _separationForce.magnitude;
            
        // =============Intersection===============
        _interForce = Vector3.zero;
        foreach (var vortex in _vortexList)
        {
            var pos = transform.position;
            var r = Vector3.Cross(vortex.transform.position - pos, vortex.axis).magnitude;

            switch (vortex.type)
            {
                case Vortex.Type.Potential:
                    _interForce += Vector3.Cross(vortex.axis, vortex.transform.position - pos).normalized * vortex.strengthV / (2 * Mathf.PI * r);
                    break;
                case Vortex.Type.Rigidbody:
                    _interForce += Vector3.Cross(vortex.axis, vortex.transform.position - pos).normalized * vortex.strengthV;
                    break;
                case Vortex.Type.Scully:
                    _interForce += Vector3.Cross(vortex.axis, vortex.transform.position - pos).normalized * vortex.strengthV / (2 * Mathf.PI) *
                                   (r / (Mathf.Pow(vortex.radius, 2) + Mathf.Pow(r, 2)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
        }

        foreach (var ss in _sourceSink)
        {
            var pos = transform.position;
            var r = Vector3.Cross(ss.transform.position - pos, ss.axis).magnitude;
            switch (ss.type)
            {
                case SourceSink.Type.Source:
                    _interForce += -(ss.transform.position - pos).normalized * ss.strengthS / (2 * Mathf.PI * r); // Direction: From pos to sink
                    break;
                case SourceSink.Type.Sink:
                    _interForce += (ss.transform.position - pos).normalized * ss.strengthS / (2 * Mathf.PI * r); // Direction: From pos to sink
                    break;
                case SourceSink.Type.Doublet:
                    var position = ss.transform.position;
                    var relPos = (position - pos).normalized; // Only works for 2D
                    var cosTheta = Vector3.Dot(_migrationVector, relPos);
                    var sinTheta = Vector3.Cross(relPos, _migrationVector).magnitude;
                    var vr = - speed * cosTheta * ss.Rsquared / (r * r);
                    var vt = - speed * sinTheta * ss.Rsquared / (r * r);

                    _interForce += (position - pos).normalized * vr;
                    _interForce += Vector3.Cross(ss.axis, position - pos).normalized * vt;
                    break;
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _interForce *= interGain;
        interForceMag = _interForce.magnitude;
    }

    void DestroyAgent()
    {
        _rb.velocity = Vector3.zero; // Stop moving
        _collider.enabled = false; // make invisible to other agents
        Destroy(gameObject, destroyDelay); // destroy in time
    }

    private float Norm(float value, float std)
    {
        return 1 / (std * Sqrt2PI) * Mathf.Exp(-0.5f * Mathf.Pow((value / std), 2f));
    }

    private void OnDrawGizmosSelected()
    {
        var position = transform.position;

        if (migration)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(position, position + _migrationForce);
        }
        if (intersection)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(position, position + _interForce);
        }

        if (laneFormation)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(position, position + _laneFormForce);
        }
       
        if (orthoganolSeparation)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position+ _orthSepForce);
        }
        if (idealVelocity)
        {
            Gizmos.color = Color.red;
            if (_rb) Gizmos.DrawLine(position, position + _rb.velocity);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(position, position + _velocityIdeal);
        }

    }
}