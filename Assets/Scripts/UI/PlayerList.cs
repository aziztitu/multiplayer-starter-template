using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Azeesoft.Multiplayer
{
    public class PlayerList : LobbyPlayerList
    {
        [SerializeField] private Transform content;
        [SerializeField] private PlayerListPlayerItem itemPrefab;

        private readonly List<PlayerListPlayerItem> spawnedItems = new();
        private string lastKey;

        public override void SetPlayers(IReadOnlyList<LobbyPlayerInfo> players)
        {
            if (!content || !itemPrefab)
            {
                return;
            }

            var keyBuilder = new StringBuilder();
            foreach (var player in players)
            {
                keyBuilder.Append(player.ClientId).Append(':').Append(player.PlayerName).Append(';');
            }

            var key = keyBuilder.ToString();
            if (key == lastKey)
            {
                return;
            }

            lastKey = key;
            ClearItems();

            foreach (var player in players)
            {
                var item = Instantiate(itemPrefab, content);
                item.Set(player);
                spawnedItems.Add(item);
            }
        }

        public override void Clear()
        {
            ClearItems();
            lastKey = null;
        }

        void ClearItems()
        {
            foreach (var item in spawnedItems)
            {
                if (item)
                {
                    Destroy(item.gameObject);
                }
            }

            spawnedItems.Clear();
        }

        void OnDestroy()
        {
            Clear();
        }
    }
}
