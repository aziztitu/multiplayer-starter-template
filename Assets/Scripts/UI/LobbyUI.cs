using AZUtils;
using Netcode.Transports.WebRTC;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamworksIntegration.API;
using Netcode.Transports;
#endif

namespace Azeesoft.Multiplayer
{
    public class LobbyUI : SingletonMonoBehaviour<LobbyUI>
    {
        public enum TransportType
        {
            Unity,
            Steam,
            WebRTC
        }

        [Header("Connect Screen")]
        [SerializeField] private GameObject ConnectScreen;
        [SerializeField] private TMP_InputField CodeInput;
        [SerializeField] private TMP_Dropdown TransportSelectDropdown;

        [Header("Lobby Screen")]
        [SerializeField] private NetworkObject LobbyManagerPrefab;
        [SerializeField] private GameObject LobbyScreen;
        [SerializeField] private TMP_InputField LobbyCodeText;
        [SerializeField] private GameObject StartButton;
        [SerializeField] private GameObject WaitingMessage;
        [SerializeField] private TextMeshProUGUI TotalPlayersText;
        [SerializeField] private Button StartGameBtn;
        [SerializeField] private TMP_InputField PlayerNameInput;
        [SerializeField] [Tooltip("PlayerList, or any LobbyPlayerList you swap in.")]
        private LobbyPlayerList playerList;

        [Header("Transport Selection")]
        [SerializeField] private TransportType defaultTransportTypeInEditor = TransportType.Steam;
        [SerializeField] private TransportType defaultTransportTypeInDesktop = TransportType.Steam;
        [SerializeField] private TransportType defaultTransportTypeInMobile = TransportType.WebRTC;

        [Header("Misc")]
        [FormerlySerializedAs("testGameSceneNames")]
        [SerializeField] private string[] gameSceneNames;

        public string SelectedGameSceneName
        {
            get
            {
                if (LobbyManager.Instance)
                {
                    return LobbyManager.Instance.SelectedGameSceneName.Value.ToString();
                }
                return "";
            }
            private set
            {
                if (LobbyManager.Instance)
                {
                    LobbyManager.Instance.SelectedGameSceneName.Value = value;
                }
            }
        }

        private TransportType DefaultTransportType
        {
            get
            {
                if (Application.isEditor)
                {
                    return defaultTransportTypeInEditor;
                }

                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    return TransportType.WebRTC;
                }

                if (Application.platform == RuntimePlatform.WindowsPlayer
                    || Application.platform == RuntimePlatform.OSXPlayer
                    || Application.platform == RuntimePlatform.LinuxPlayer)
                {
                    return defaultTransportTypeInDesktop;
                }

                if (Application.platform == RuntimePlatform.Android
                    || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return defaultTransportTypeInMobile;
                }

                return TransportType.Unity;
            }
        }

        private TransportType UseTransportType => (TransportType)TransportSelectDropdown.value;

        new void Awake()
        {
            base.Awake();

            InitTransformSelectDropdown();
        }

        void Start()
        {
            Time.timeScale = 1.0f;
            HelperUtilities.UpdateCursorLock(false);

            if (AZNetworkManager.Instance.CurrentNetworkTransport is WebRTCTransport)
            {
                var webRTCTransport = AZNetworkManager.Instance.GetTransport<WebRTCTransport>();
                CodeInput.text = webRTCTransport.roomId;
            }

            NetworkManager.Singleton.OnServerStarted += SpawnLobbyManager;
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoaded;
            }

            if (NetworkPlayerIdentities.TryGet(NetworkManager.Singleton.LocalClientId, out var playerNetworkIdentity))
            {
                PlayerNameInput.text = playerNetworkIdentity.PlayerName;
            }

            PlayerNameInput.onValueChanged.AddListener((playerName) =>
            {
                if (!NetworkManager.Singleton)
                {
                    return;
                }

                if (NetworkPlayerIdentities.TryGet(NetworkManager.Singleton.LocalClientId, out var identity))
                {
                    identity.PlayerName = playerName;
                }
            });
        }

        void Update()
        {
            if (!NetworkManager.Singleton)
            {
                return;
            }

            var isInLobby = NetworkManager.Singleton.IsConnectedClient;
            ConnectScreen.SetActive(!isInLobby);
            LobbyScreen.SetActive(isInLobby);

            if (isInLobby)
            {
                var isHost = NetworkManager.Singleton.IsHost;
                StartButton.SetActive(isHost);
                WaitingMessage.SetActive(!isHost);

                var showLobbyCode = UseTransportType != TransportType.Unity;
                LobbyCodeText.gameObject.SetActive(showLobbyCode);

                if (showLobbyCode)
                {
                    var transport = AZNetworkManager.Instance.CurrentNetworkTransport;
#if !DISABLESTEAMWORKS
                    if (transport is SteamNetworkingSocketsTransport steamTransport)
                    {
                        var serverSteamId = isHost ? User.Client.Id.SteamId : steamTransport.ConnectToSteamID;
                        LobbyCodeText.text = $"{serverSteamId}";
                    }
#endif
                    if (transport is WebRTCTransport webRTCTransport)
                    {
                        LobbyCodeText.text = $"{webRTCTransport.roomId}";
                    }
                }

                StartGameBtn.interactable = SelectedGameSceneName != "";
                RefreshPlayerList();
            }
            else
            {
                var showCodeInput = UseTransportType != TransportType.Unity;
                CodeInput.gameObject.SetActive(showCodeInput);
                playerList?.Clear();
            }
        }

        private void HandleSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                SpawnLobbyManager();
            }
        }

        public void Host()
        {
            SetupNetworkTransport();
            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("Could not start the Host");
            }
        }

        public void Join()
        {
            SetupNetworkTransport();
            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Could not connect to the Host");
            }
        }

        void InitTransformSelectDropdown()
        {
            List<TMP_Dropdown.OptionData> options = Enum.GetNames(typeof(TransportType)).Select(name =>
            {
                return new TMP_Dropdown.OptionData()
                {
                    text = name,
                };
            }).ToList();

            TransportSelectDropdown.ClearOptions();
            TransportSelectDropdown.AddOptions(options);

            if (AZNetworkManager.Instance.IsConnectedClient)
            {
                TransportType currentTransportType = TransportType.Unity;
                if (AZNetworkManager.Instance.CurrentNetworkTransport is UnityTransport)
                {
                    currentTransportType = TransportType.Unity;
                }
#if !DISABLESTEAMWORKS
                else if (AZNetworkManager.Instance.CurrentNetworkTransport is SteamNetworkingSocketsTransport)
                {
                    currentTransportType = TransportType.Steam;
                }
#endif
                else if (AZNetworkManager.Instance.CurrentNetworkTransport is WebRTCTransport)
                {
                    currentTransportType = TransportType.WebRTC;
                }
                TransportSelectDropdown.value = (int)currentTransportType;
            }
            else
            {
                TransportSelectDropdown.value = (int)DefaultTransportType;
            }
        }

        void SpawnLobbyManager()
        {
            if (!NetworkManager.Singleton.IsHost || LobbyManager.Instance != null)
            {
                return;
            }

            var lobbyManager = Instantiate(LobbyManagerPrefab);
            lobbyManager.Spawn(true);

            if (gameSceneNames != null && gameSceneNames.Length > 0)
            {
                SelectedGameSceneName = gameSceneNames[0];
            }
        }

        void SetupNetworkTransport()
        {
            if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }

            switch (UseTransportType)
            {
                case TransportType.Unity:
                    AZNetworkManager.Instance.UseTransport<UnityTransport>();
                    break;
                case TransportType.Steam:
#if !DISABLESTEAMWORKS
                    var steamTransport = AZNetworkManager.Instance.UseTransport<SteamNetworkingSocketsTransport>();
                    if (ulong.TryParse(CodeInput.text, out var code))
                    {
                        steamTransport.ConnectToSteamID = code;
                    }
                    break;
#else
                    Debug.LogWarning("Trying to use Steam, but Steamworks is disabled");
                    break;
#endif
                case TransportType.WebRTC:
                    var webRTCTransport = AZNetworkManager.Instance.UseTransport<WebRTCTransport>();
                    webRTCTransport.roomId = CodeInput.text;
                    break;
            }
        }

        public void LeaveLobby()
        {
            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.LogError("You are not in a Lobby");
                return;
            }

            NetworkManager.Singleton.Shutdown();
        }

        public void StartGame()
        {
            if (SelectedGameSceneName == "")
            {
                return;
            }

            var status = NetworkManager.Singleton.SceneManager.LoadScene(SelectedGameSceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {SelectedGameSceneName} " +
                      $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }

        void RefreshPlayerList()
        {
            var networkManager = NetworkManager.Singleton;
            var clientIds = new SortedSet<ulong>();
            foreach (var clientId in networkManager.ConnectedClientsIds)
            {
                clientIds.Add(clientId);
            }

            foreach (var identity in NetworkPlayerIdentities.All)
            {
                clientIds.Add(identity.ClientId);
            }

            if (TotalPlayersText)
            {
                TotalPlayersText.text = $"Total Players: {clientIds.Count}";
            }

            var players = new List<LobbyPlayerInfo>(clientIds.Count);
            foreach (var clientId in clientIds)
            {
                var playerName = "Anonymous";
                if (NetworkPlayerIdentities.TryGet(clientId, out var identity) && !string.IsNullOrWhiteSpace(identity.PlayerName))
                {
                    playerName = identity.PlayerName;
                }

                players.Add(new LobbyPlayerInfo(
                    clientId,
                    playerName,
                    clientId == networkManager.LocalClientId,
                    clientId == NetworkManager.ServerClientId));
            }

            playerList?.SetPlayers(players);
        }

        void OnDestroy()
        {
            playerList?.Clear();
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleSceneLoaded;
            }
            NetworkManager.Singleton.OnServerStarted -= SpawnLobbyManager;
        }

        public void Exit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
