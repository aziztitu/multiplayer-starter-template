using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LightCyclePlayersUI : MonoBehaviour
{
    public LightCyclePlayerItemUI playerItemPrefab;
    public Transform playerItemsContainer;

    public Dictionary<ulong, LightCyclePlayerItemUI> playerItems = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnExistingPlayers();
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // Update is called once per frame
    void Update()
    {
        SpawnExistingPlayers();
    }

    void SpawnExistingPlayers()
    {
        if (!NetworkManager.Singleton)
        {
            return;
        }

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            AddPlayerItem(clientId);
        }
    }

    void OnClientStarted()
    {
        SpawnExistingPlayers();
    }

    void OnClientConnected(ulong clientId)
    {
        AddPlayerItem(clientId);
    }

    void OnClientDisconnected(ulong clientId)
    {
        RemovePlayerItem(clientId);
    }

    void AddPlayerItem(ulong clientId)
    {
        if (playerItems.ContainsKey(clientId))
        {
            return;
        }
        
        var playerItem = Instantiate(playerItemPrefab, playerItemsContainer);
        playerItem.Init(clientId);
        playerItems.Add(clientId, playerItem);
    }

    void RemovePlayerItem(ulong clientId)
    {
        if (playerItems.ContainsKey(clientId))
        {
            Destroy(playerItems[clientId]);
            playerItems.Remove(clientId);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
