using NetCodeGenerator.Serialization;
using SocketIOClient.Transport;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[GenerateNetworkSerialization]
public partial struct TestNestedData
{
    public int Health;
    public float Weight;
    public bool IsAlive;
    public Vector3 Position;
    public Quaternion Rotation;
    public TransportMessageType TransportMessageType;
    public FixedString64Bytes Name;
    public FixedList128Bytes<int> FixedItems1;
    public FixedList128Bytes<Vector3> FixedItems2;
    public FixedList128Bytes<SimpleData> FixedItems3;
}
