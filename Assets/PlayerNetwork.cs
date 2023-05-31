using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<CustomData> customData = new NetworkVariable<CustomData>(new CustomData{_int = 1, _bool = false, message = "Starting"}, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct CustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        // Serializing the custom struct
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn() {
        randomNumber.OnValueChanged += (int previousValue, int newValue) => {
            Debug.Log(OwnerClientId + "; randomNumber: " + randomNumber.Value);
        };

        customData.OnValueChanged += (CustomData previousValue, CustomData newValue) => {
            Debug.Log(OwnerClientId + "; " + customData.Value._int + "; " + customData.Value._bool + "; " + customData.Value.message);
        };
    }

    void Update()
    {
        // Giving control only for your player
        if(!IsOwner) return;

        if(Input.GetKeyDown(KeyCode.T)){
            randomNumber.Value = Random.Range(0, 100);
            customData.Value = new CustomData {_int = Random.Range(0, 10), _bool = true, message = "Randomizing"};

            TestServerRpc("sample test server rpc message", new ServerRpcParams());
            TestClientRpc(new ClientRpcParams {Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> {1}}});
        }

        Vector3 moveDir = new Vector3(0,0,0);

        if(Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if(Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if(Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if(Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 25f;
        transform.position += moveDir*moveSpeed*Time.deltaTime;
    }

    // This menthod will only run on host machine, even if triggered from the client machine, the Id would be the client from where the triggered was started
    [ServerRpc]
    private void TestServerRpc(string message, ServerRpcParams serverRpcParams) {
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    // This method is to propagate the changes in the client machines and can only be accessed from server side
    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams) {
        Debug.Log("TestClientRpc ");
    }
}
