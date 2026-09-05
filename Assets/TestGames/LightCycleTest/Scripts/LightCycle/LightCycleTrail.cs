using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;

public class LightCycleTrail : NetworkBehaviour
{
    public NetworkVariable<Color> color = new(Color.red);

    public GameObject trailSegmentPrefab;
    public TrailRenderer trailRenderer;
    public Transform trailSpawnPoint;
    public float segmentSpawnDistance = 2f;
    public float segmentZScaleMultiplier = 1f;
    public float segmentLifetime = 5f;

    private LightCycle lightCycle;
    private Vector3 lastSpawnPosition;
    private LightCycleTrailSegment currentSegment;
    private Queue<LightCycleTrailSegment> segmentPool = new Queue<LightCycleTrailSegment>();

    private void Awake()
    {
        lightCycle = GetComponent<LightCycle>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    void Init()
    {
        if (IsHost)
        {
            color.Value = LightCycleLevelManager.Instance.GetNextColor();
            Debug.Log($"Assigned color: {color.Value}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (LightCycleLevelManager.Instance.IsGameOver.Value)
        {
            return;
        }

        if (!lightCycle.IsAlive.Value)
        {
            if (currentSegment != null)
            {
                UpdateSegment(currentSegment, lastSpawnPosition, trailSpawnPoint.position);
                currentSegment = null;
            }
            return;
        }

        trailRenderer.transform.position = trailSpawnPoint.position;
        trailRenderer.transform.rotation = Quaternion.identity;

        //if (!IsOwner) return;
        
        // Extend current segment to follow the cycle
        if (currentSegment != null)
        {
            UpdateSegment(currentSegment, lastSpawnPosition, trailSpawnPoint.position);
        }

        float distance = Vector3.Distance(trailSpawnPoint.position, lastSpawnPosition);
        if (distance >= segmentSpawnDistance)
        {
            SpawnSegment();
        }
    }

    public override void OnNetworkSpawn()
    {
        //if (IsOwner)
        //{
        //    // Initialize first segment
        //    SpawnSegment();
        //}
    }

    private void SpawnSegment()
    {
        LightCycleTrailSegment segment = GetSegmentFromPoolOrInstantiate();
        segment.gameObject.SetActive(true);
        segment.OnSpawn();
        StartCoroutine(HideAfterTime(segment, segmentLifetime));

        var meshRenderer = segment.GetComponent<MeshRenderer>();
        var mat = meshRenderer.material;
        var col = color.Value;
        col.a = 0.7f;
        mat.color = col;
        mat.SetColor("_EmissionColor", color.Value * 1.5f);

        lastSpawnPosition = trailSpawnPoint.position;

        if (currentSegment != null)
        {
            currentSegment.transform.localScale = new Vector3(
                currentSegment.transform.localScale.x, 
                currentSegment.transform.localScale.y, 
                currentSegment.transform.localScale.z * segmentZScaleMultiplier // I need to set the local scale and position properly
            );
        }

        currentSegment = segment;
        UpdateSegment(currentSegment, lastSpawnPosition, trailSpawnPoint.position);
    }

    private void UpdateSegment(LightCycleTrailSegment segment, Vector3 start, Vector3 end)
    {
        Vector3 center = (start + end) / 2f;
        Vector3 direction = end - start;
        float length = direction.magnitude;

        segment.transform.position = center;
        segment.transform.rotation = Quaternion.LookRotation(direction.normalized);

        // Assuming Z axis forward
        segment.transform.localScale = new Vector3(segment.transform.localScale.x, segment.transform.localScale.y, length);
    }

    IEnumerator HideAfterTime(LightCycleTrailSegment segment, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!LightCycleLevelManager.Instance.IsGameOver.Value)
        {
            segment.gameObject.SetActive(false);
            segmentPool.Enqueue(segment);
        }
    }

    LightCycleTrailSegment GetSegmentFromPoolOrInstantiate()
    {
        if (segmentPool.Count > 0)
        {
            return segmentPool.Dequeue();
        }

        // None available, create new
        GameObject obj = Instantiate(trailSegmentPrefab, trailSpawnPoint.position, trailSpawnPoint.rotation);
        var segment = obj.GetComponent<LightCycleTrailSegment>();
        segment.owner = lightCycle;
        return segment;
    }

    new void OnDestroy()
    {
        base.OnDestroy();
    }
}
