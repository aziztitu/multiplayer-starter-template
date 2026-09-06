using TMPro;
using UnityEngine;

namespace Azeesoft.Multiplayer
{
    public class PlayerListPlayerItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        public void Set(LobbyPlayerInfo player)
        {
            if (!nameText)
            {
                nameText = GetComponent<TextMeshProUGUI>();
            }

            var label = player.PlayerName;
            if (player.IsHost)
            {
                label += " (Host)";
            }
            if (player.IsLocal)
            {
                label += " (You)";
            }

            nameText.text = label;
        }
    }
}
