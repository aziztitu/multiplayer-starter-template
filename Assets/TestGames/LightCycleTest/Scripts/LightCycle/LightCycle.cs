using Azeesoft.Multiplayer;
using AZUtils;
using Cysharp.Threading.Tasks;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightCycle : NetworkInstancesBehavior<LightCycle>
{
    public enum PowerUpType
    {
        Jump,
    }

    // Owner-controlled network variables
    public NetworkVariable<int> Lives = new(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> IsAlive = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> HasPowerUp_Jump = new(false);

    public Transform avatarRoot;
    public GameObject explosionPrefab;
    public float InvincibilitySeconds = 3f;
    public float RespawnSeconds = 3f;

    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public StarterAssetsInputs inputs;
    [HideInInspector] public LightCycleMovement lightCycleMovement;
    [HideInInspector] public ThirdPersonCinemachineCameraRotation cameraRotation;
    [HideInInspector] public Cinemachine.CinemachineVirtualCamera playerFollowCamera;

    // Used on owner to skip collisions. Used on all clients for Cosmetics.
    [HideInInspector] public bool CanDie = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inputs = GetComponent<StarterAssetsInputs>();
        lightCycleMovement = GetComponentInChildren<LightCycleMovement>(true);
        cameraRotation = GetComponentInChildren<ThirdPersonCinemachineCameraRotation>(true);
        playerFollowCamera = GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>(true);


        playerInput.enabled = false;
        playerFollowCamera.enabled = false;
        cameraRotation.enabled = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected new void Start()
    {
        base.Start();
        Init();
    }

    void Init()
    {
        if (IsOwner)
        {
            playerFollowCamera.enabled = true;
            playerInput.enabled = true;
            cameraRotation.enabled = true;
        }

        StartInvincibility();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called on the Owner
    public void Die()
    {
        if (IsOwner)
        {
            IsAlive.Value = false;
            if (Lives.Value > 0)
            {
                Lives.Value--;
            }
            OnDeathRpc();

            if (Lives.Value > 0) {
                StartCoroutine(WaitAndRespawn(RespawnSeconds));
            } 
            else
            {
                playerFollowCamera.enabled = false;
                LightCycleLevelManager.Instance.StartSpectating();
            }
        }
    }

    // Called by Owner
    [Rpc(SendTo.Everyone)]
    private void OnDeathRpc()
    {
        var explosion = Instantiate(explosionPrefab, lightCycleMovement.transform.position, lightCycleMovement.transform.rotation);
        Destroy(explosion, 3f);

        avatarRoot.gameObject.SetActive(false);

        if (IsHost)
        {
            HasPowerUp_Jump.Value = false;
        }
    }

    // Called on Owner
    IEnumerator WaitAndRespawn(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _ = Respawn();
    }

    // Called on Owner
    async UniTask Respawn()
    {
        StartInvincibility();
        avatarRoot.gameObject.SetActive(true);

        var spawnPoint = PlayerCharacterSpawner.Instance.GetNextSpawnGroup().GetNextSpawnPoint();
        await lightCycleMovement.Teleport(spawnPoint.Position, spawnPoint.Rotation);

        IsAlive.Value = true;
        OnRespawnRpc();
    }

    // Called by Owner
    [Rpc(SendTo.NotOwner)]
    private void OnRespawnRpc()
    {
        StartInvincibility();
        avatarRoot.gameObject.SetActive(true);
    }

    void StartInvincibility()
    {
        CanDie = false;
        StartCoroutine(WaitAndLoseInvincibility(InvincibilitySeconds));
    }

    IEnumerator WaitAndLoseInvincibility(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        CanDie = true;
    }

    // Called on the Host
    public bool PickUpPowerUp(PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUpType.Jump:
                if (HasPowerUp_Jump.Value)
                {
                    return false;
                }

                HasPowerUp_Jump.Value = true;
                break;
            default:
                return false;
        }

        return true;
    }

    [Rpc(SendTo.Server)]
    public void ConsumePowerUpRpc(PowerUpType powerUpType)
    {
        switch(powerUpType)
        {
            case PowerUpType.Jump:
                HasPowerUp_Jump.Value = false;
                break;
        }
    }
}
