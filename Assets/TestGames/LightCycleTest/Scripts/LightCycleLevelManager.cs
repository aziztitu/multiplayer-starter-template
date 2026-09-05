using Azeesoft.Multiplayer;
using AZUtils;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class LightCycleLevelManager : BaseNetworkLevelManager<LightCycleLevelManager>
{
    public NetworkVariable<long> WinnerClientId = new();
    public NetworkVariable<bool> IsGameOver = new();

    public Color[] colors;
    public int nextColor = 0;

    [Header("Spectator Mode")]
    public GameObject spectatorUI;
    public TextMeshProUGUI spectatingPlayerText;
    [HideInInspector] public LightCycle spectatingLightCycle;
    public bool IsSpectating => spectatingLightCycle != null;

    [Header("End Screen UI")]
    public GameObject endScreenUI;
    public TextMeshProUGUI winnerText;
    public GameObject serverOptions;
    public GameObject clientOptions;

    bool canCheckWinCondition = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        base.Start();
        endScreenUI.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        IsGameOver.OnValueChanged += OnIsGameOverChanged;

        if (IsHost)
        {
            _ = WaitAndEnableWinConditionChecks();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        IsGameOver.OnValueChanged -= OnIsGameOverChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (endScreenUI.activeInHierarchy)
        {
            if (WinnerClientId.Value >= 0 && PlayerNetworkIdentity.Instances.TryGetValue((ulong) WinnerClientId.Value, out var playerNetworkIdentity))
            {
                winnerText.text = $"{playerNetworkIdentity.PlayerName.Value.ToString()} wins!";
            }
            else
            {
                winnerText.text = $"Game Over";
            }
            serverOptions.SetActive(IsHost);
            clientOptions.SetActive(!IsHost);
        }

        spectatorUI.SetActive(IsSpectating);
        if (IsSpectating)
        {
            if (PlayerNetworkIdentity.Instances.TryGetValue(spectatingLightCycle.OwnerClientId, out var playerNetworkIdentity))
            {
                spectatingPlayerText.text = playerNetworkIdentity.PlayerName.Value.ToString();
            }
            else
            {
                spectatingPlayerText.text = "Unknown";
            }

            if (spectatingLightCycle.Lives.Value <= 0)
            {
                SelectNextPlayerToSpectate();
            }
        }

        if (IsHost && canCheckWinCondition &&!IsGameOver.Value)
        {
            CheckWinCondition();
        }
    }

    async UniTask WaitAndEnableWinConditionChecks()
    {
        await UniTask.Delay(5000);
        canCheckWinCondition = true;
    }

    public Color GetNextColor()
    {
        if (colors.Length == 0)
        {
            return Color.red;
        }

        var color = colors[nextColor];
        nextColor++;
        if (nextColor >= colors.Length)
        {
            nextColor = 0;
        }
        return color;
    }

    // Called on Host
    void CheckWinCondition()
    {
        var aliveLightCycles = LightCycle.Instances.Values.Where(lightCycle => lightCycle.Lives.Value > 0).ToList();
        if (aliveLightCycles.Count == LightCycle.Instances.Count)
        {
            return;
        }

        if (aliveLightCycles.Count() == 0)
        {
            WinnerClientId.Value = -1;
            IsGameOver.Value = true;
            return;
        }
        if (aliveLightCycles.Count() == 1)
        {
            WinnerClientId.Value = (long) aliveLightCycles[0].OwnerClientId;
            IsGameOver.Value = true;
            return;
        }
    }

    void OnIsGameOverChanged(bool wasGameOver, bool isGameOver)
    {
        if (!isGameOver)
        {
            return;
        }

        // Game Ended
        if (LightCycle.LocalClientInstance)
        {
            LightCycle.LocalClientInstance.playerInput.enabled = false;
        }

        endScreenUI.SetActive(true);
        HelperUtilities.UpdateCursorLock(false);
    }

    public void StartSpectating()
    {
        SelectNextPlayerToSpectate();
        if (!spectatingLightCycle)
        {
            return;
        }

        HelperUtilities.UpdateCursorLock(false);
    }

    public void SelectNextPlayerToSpectate()
    {
        SelectPlayerToSpectateByDelta(1);
    }

    public void SelectPrevPlayerToSpectate()
    {
        SelectPlayerToSpectateByDelta(-1);
    }

    public void SelectPlayerToSpectateByDelta(int delta)
    {
        var aliveLightCycles = LightCycle.Instances.Values.Where(lightCycle => lightCycle.Lives.Value > 0).ToList();
        if (aliveLightCycles.Count == 0)
        {
            return;
        }

        var index = aliveLightCycles.FindIndex(lc => lc == spectatingLightCycle);
        index += delta;
        index %= aliveLightCycles.Count;

        if (spectatingLightCycle)
        {
            spectatingLightCycle.playerFollowCamera.enabled = false;
        }

        spectatingLightCycle = aliveLightCycles[index];
        spectatingLightCycle.playerFollowCamera.enabled = true;
    }
}
