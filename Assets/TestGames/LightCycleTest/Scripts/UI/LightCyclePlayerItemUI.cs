using Azeesoft.Multiplayer;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LightCyclePlayerItemUI : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI livesText;

    ulong clientId;
    bool isLocalPlayer => clientId == NetworkManager.Singleton.LocalClientId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RefreshPlayerName();
        RefreshLives();
    }

    public void Init(ulong clientId)
    {
        this.clientId = clientId;

        if (isLocalPlayer)
        {
            playerNameText.fontStyle |= FontStyles.Bold;
            livesText.fontStyle |= FontStyles.Bold;
        }
    }

    void RefreshPlayerName()
    {
        if (PlayerNetworkIdentity.Instances.TryGetValue(clientId, out var playerIdentity))
        {
            playerNameText.text = playerIdentity.PlayerName.Value.ToString();
        }
    }

    void RefreshLives()
    {
        if (LightCycle.Instances.TryGetValue(clientId, out var lightCycle))
        {
            livesText.text = lightCycle.Lives.Value.ToString();
        }
    }
}
