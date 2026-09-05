using Azeesoft.Multiplayer;
using AZUtils;
using BasicTools.ButtonInspector;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : BaseNetworkLevelManager<LevelManager>
{
    public NetworkObject ballPrefab;

    [Button("Spawn Ball", "SpawnBall")]
    public bool spawnBallButton;

    [Button("Spawn Players", "SpawnPlayers")]
    public bool spawnPlayers;

    [Header("Misc")]
    public NetworkVariable<TestData> data;
    public InputAction logAction;
    public InputAction incAction;
    public InputAction decAction;

    new void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        logAction.Enable();
        incAction.Enable();
        decAction.Enable();
    }

    void Update()
    {
        ProcessDebugInputs();
    }

    public void SpawnBall()
    {
        var ball = Instantiate(ballPrefab, transform.position, transform.rotation);
        ball.Spawn(true);
    }

    public void SpawnPlayers()
    {
        SimplePlayerCharacterSpawner.Instance.SpawnPlayersForAllClients();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            var testData = new TestData();
            //testData.FixedItems1.Clear();
            //testData.FixedItems1.Add(10);

            //testData.FixedItems2.Clear();
            //testData.FixedItems2.Add(new Vector3(10, 20, 30));

            //testData.FixedItems3.Clear();
            //testData.FixedItems3.Add(new TestNestedData()
            //{
            //    Health = 30,
            //});

            testData.Items1 = new();
            testData.Items2 = new();
            testData.Items3 = new();

            data.Value = testData;
        }
    }

    void ProcessDebugInputs()
    {
        if (logAction.WasPressedThisFrame())
        {
            var testData = data.Value;
            Debug.Log($"{testData.Name}, {testData.Health}, {testData.IsAlive}");
            Debug.Log($"{testData.Position}, {testData.Rotation}");
            Debug.Log($"{testData.TransportMessageType}, {testData.Rotation}");
            Debug.Log($"Items1: {testData.Items1.Count}");
            Debug.Log($"Items2: {testData.Items2.Count}");
            Debug.Log($"Items3: {testData.Items3.Count}");
        }

        if (IsHost)
        {
            int sign = 0;
            if (incAction.WasPressedThisFrame())
            {
                sign = 1;

                //data.Value.FixedItems1.Add(sign);
                data.Value.Items1.Add(sign);

                //data.Value.FixedItems2.Add(new Vector3(10, 20, 30));
                data.Value.Items2.Add(Quaternion.Euler(new Vector3(10, 20, 30)));

                //data.Value.FixedItems3.Add(new TestNestedData()
                //{
                //    Health = 30,
                //});
                data.Value.Items3.Add(new TestNestedData()
                {
                    Health = 60,
                });
            }
            if (decAction.WasPressedThisFrame())
            {
                sign = -1;

                //data.Value.FixedItems1.RemoveAt(0);
                data.Value.Items1.RemoveAt(0);

                //data.Value.FixedItems2.RemoveAt(0);
                data.Value.Items2.RemoveAt(0);

                //data.Value.FixedItems3.RemoveAt(0);
                data.Value.Items3.RemoveAt(0);
            }

            if (sign != 0)
            {
                data.Value = new()
                {
                    Name = data.Value.Name + "; " + sign,
                    Health = data.Value.Health + sign,
                    Position = data.Value.Position + new Vector3(0, 10 * sign, 0),
                    Rotation = Quaternion.Euler(data.Value.Rotation.eulerAngles + new Vector3(0, 10 * sign, 0)),
                    TransportMessageType = sign > 0 ? SocketIOClient.Transport.TransportMessageType.Text : SocketIOClient.Transport.TransportMessageType.Close,
                    IsAlive = sign > 0,
                    Items1 = data.Value.Items1,
                    Items2 = data.Value.Items2,
                    Items3 = data.Value.Items3,
                };
            }
        }
    }
}
