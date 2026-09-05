using AZUtils;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacter : NetworkBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera playerFollowCamera;
    private PlayerInput playerInput;

    private void Awake()
    {
        playerFollowCamera.enabled = false;

        playerInput = GetComponentInChildren<PlayerInput>(true);
        playerInput.enabled = false;
    }

    void Init()
    {
        if (IsOwner)
        {
            playerFollowCamera.enabled = true;
            playerInput.enabled = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        Init();
        base.OnNetworkSpawn();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
