using System.Collections.Generic;
using UnityEngine;

namespace Azeesoft.Multiplayer
{
    public readonly struct LobbyPlayerInfo
    {
        public LobbyPlayerInfo(ulong clientId, string playerName, bool isLocal, bool isHost)
        {
            ClientId = clientId;
            PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Anonymous" : playerName;
            IsLocal = isLocal;
            IsHost = isHost;
        }

        public ulong ClientId { get; }
        public string PlayerName { get; }
        public bool IsLocal { get; }
        public bool IsHost { get; }
    }

    public abstract class LobbyPlayerList : MonoBehaviour
    {
        public abstract void SetPlayers(IReadOnlyList<LobbyPlayerInfo> players);
        public abstract void Clear();
    }
}
