using AZUtils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class LightCyclePowerUpPickup : NetworkBehaviour
{
    public GameObject Avatar;
    public GameObject IconHolder;
    public Collider Collider;
    public LightCycle.PowerUpType PowerUpType;
    public RangeFloat ReappearDelay = new(15f, 30f);
    public float IconRotationDuration = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DORotate(
            new Vector3(0, 360, 0),
            IconRotationDuration,
            RotateMode.FastBeyond360
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost)
        {
            return;
        }

        var lightCycle = other.GetComponentInParent<LightCycle>();
        if (lightCycle != null)
        {
            if (lightCycle.PickUpPowerUp(PowerUpType))
            {
                HideAndReappearLater(ReappearDelay.GetRandom()).Forget();
            }
        }
    }

    // Called on the Host
    async UniTask HideAndReappearLater(float seconds)
    {
        UpdateVisibilityRpc(false);

        await UniTask.Delay((int)(seconds * 1000));

        UpdateVisibilityRpc(true);
    }


    [Rpc(SendTo.Everyone)]
    void UpdateVisibilityRpc(bool visible)
    {
        Avatar.SetActive(visible);
        Collider.enabled = visible;
    }
}
