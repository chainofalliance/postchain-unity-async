using System;
using System.Collections.Generic;
using UnityEngine;

using Chromia.Postchain.Client;

public class BlockchainWrapper : MonoBehaviour
{
    #region PRIVATE_VARIABLES    
    private BlockchainClient _client;
    private Dictionary<string, byte[]> _keyPair;
    private Dictionary<string, List<Tuple<string, object[]>>> _trackItems;
    private System.Random _random = new System.Random();
	#endregion

	#region UNITY_CALLBACKS
    void Awake()
    {
        _trackItems = new Dictionary<string, List<Tuple<string, object[]>>>();
        _client = gameObject.GetComponent<BlockchainClient>();
        SetKeyPair(CreateKeyPair());
    }
	#endregion

	#region PUBLIC_METHODS

    // Creates a new private/public keypair.
    public Dictionary<string, byte[]> CreateKeyPair()
    {
        return PostchainUtil.MakeKeyPair();
    }

    // Sets a keypair. Usefull when the keypair is stored in local storage.
    public void SetKeyPair(Dictionary<string, byte[]> keyPair)
    {
        _keyPair = keyPair;
    }

    // Returns the keypair. Usefull when the keypair should be stored in local storage
    public Dictionary<string, byte[]> GetKeyPair()
    {
        return _keyPair;
    }

    /*
        Caches an operation to be sent later in a transaction. A topic can be specified
        to save operations in seperate transaction tracks. This allows caching of
        low priority operations to be sent later.
    */
    public void AddTrackOperation(string topic, string name, params object[] item)
    {
        if (!_trackItems.ContainsKey(topic))
        {
            _trackItems.Add(topic, new List<Tuple<string, object[]>>());
        }

        _trackItems[topic].Add(Tuple.Create(name, item));
    }

    // Submits all track operations saved in a specific topic. Can be configured to use a new keypair.
    public void SubmitTrackOperations(Action onSuccess, Action<string> onError, string topic, bool newKey = false)
    {
        if (!_trackItems.ContainsKey(topic))
        {
            return;
        }

        var keyPair = _keyPair;

        if(newKey)
            keyPair = CreateKeyPair();

        var request = _client.NewTransaction(new byte[][] {keyPair["pubKey"]}, onError);

        foreach(var entry in _trackItems[topic])
        {
            request.AddOperation(entry.Item1, entry.Item2);
        }

        /*
            The blockchain hashes the operations to generate a transaction id (txid) which has to be unique.
            In order to prevent this colision, a "nop" operation can be attached which will be filtered by
            the blockchain. 
        */
        request.AddOperation("nop", _random.Next(System.Int32.MinValue, System.Int32.MaxValue).ToString());
        
        request.Sign(keyPair["privKey"], keyPair["pubKey"]);

        _trackItems[topic].Clear();
        StartCoroutine(request.PostAndWait(onSuccess));
    }

    // Sends an operation immediately as a transaction.
    public void Operation(Action onSuccess, Action<string> onError, string name, params object[] item)
    {
        var request = _client.NewTransaction(new byte[][] {_keyPair["pubKey"]}, onError);
        request.AddOperation(name, item);

        /*
            The blockchain hashes the operations to generate a transaction id (txid) which has to be unique.
            In order to prevent this colision, a "nop" operation can be attached which will be filtered by
            the blockchain. 
        */
        request.AddOperation("nop", _random.Next(System.Int32.MinValue, System.Int32.MaxValue).ToString());

        request.Sign(_keyPair["privKey"], _keyPair["pubKey"]);
        StartCoroutine(request.PostAndWait(onSuccess));
    }

    // Queries data from the blockchain. 
    public void Query<T>(Action<T> onSuccess, Action<string> onError, string type, params (string name, object content)[] queryObject)
    {
        StartCoroutine(_client.Query<T>(type, queryObject, onSuccess, onError));
    }

    // Returns the pubkey of the local keypair.
    public byte[] GetPubkey()
    {
        return _keyPair["pubKey"];
    }
	#endregion
}
