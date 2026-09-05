using AZUtils;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Azeesoft.Multiplayer
{
    public class PlayerNetworkIdentity : NetworkInstancesBehavior<PlayerNetworkIdentity>, INetworkPlayerIdentity
    {
        public NetworkVariable<FixedString64Bytes> PlayerName = new("Anonymous", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public ulong ClientId => NetworkObject.OwnerClientId;

        string INetworkPlayerIdentity.PlayerName
        {
            get => PlayerName.Value.ToString();
            set => PlayerName.Value = value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkPlayerIdentities.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            NetworkPlayerIdentities.Unregister(OwnerClientId);
            base.OnNetworkDespawn();
        }

        protected new void Start()
        {
            base.Start();
            Debug.Log($"Player Ready: {NetworkObject.OwnerClientId}");
        }
    }
}
