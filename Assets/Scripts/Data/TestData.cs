using NetCodeGenerator.Serialization;
using SocketIOClient.Transport;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
[GenerateNetworkSerialization]
public partial struct TestData
{
    public int Health;
    public float Weight;
    public bool IsAlive;
    public bool IsGrounded;
    public Vector3 Position;
    public Quaternion Rotation;
    public TransportMessageType TransportMessageType;
    public FixedString64Bytes Name;
    public FixedList128Bytes<int> FixedItems1;
    public List<float> Items1;
    public FixedList128Bytes<Vector3> FixedItems2;
    public List<Quaternion> Items2;
    public FixedList128Bytes<TestNestedData> FixedItems3;
    public List<TestNestedData> Items3;
}
