using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCycleTrailSegment : MonoBehaviour
{
    [HideInInspector] public LightCycle owner;
    Collider _collider;

    bool allowCollisionWithOwner = false;
    public float stationaryDeathSeconds = 2f;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSpawn()
    {
        allowCollisionWithOwner = false;
        StartCoroutine(WaitAndAllowOwnerCollision(stationaryDeathSeconds));
    }

    IEnumerator WaitAndAllowOwnerCollision(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        allowCollisionWithOwner = true;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    var lightCycle = other.gameObject.GetComponentInParent<LightCycle>();
    //    if (!lightCycle)
    //    {
    //        return;
    //    }

    //    if (lightCycle == owner && !allowCollisionWithOwner)
    //    {
    //        return;
    //    }

    //    Debug.Log("You Died");
    //}

    private void OnTriggerStay(Collider other)
    {
        if (LightCycleLevelManager.Instance.IsGameOver.Value)
        {
            return;
        }

        var lightCycle = other.gameObject.GetComponentInParent<LightCycle>();
        if (!lightCycle)
        {
            return;
        }

        if (!lightCycle.IsOwner)
        {
            return;
        }

        if (!lightCycle.CanDie || !lightCycle.IsAlive.Value)
        {
            return;
        }

        if (lightCycle == owner && !allowCollisionWithOwner)
        {
            return;
        }

        lightCycle.Die();
    }
}
