# Postchain Client Unity Async

**Compatible with Postchain 3.3.3 / Rell 0.10.5**

This Unity package offers an API for the Chromia Blockchain. It includes normal transaction serialization, an API for Chromia's [FT3 library](https://rell.chromia.com/en/master/advanced-topics/ft3.html) as well as their [SSO](https://rell.chromia.com/en/master/advanced-topics/ft3/ft3-single-sign-on.html).

Currently tested with Standalone (Windows and Linux) as well as WebGL builds.

Sister project to [this Postchain API](https://github.com/Cherden/postchain-unity), but with async/await pattern instead of coroutines.

## Examples

### Postchain
```C#
private void Start()
{
    // Create BlockchainClient
    GameObject goConnection = new GameObject();
    BlockchainClient connection = goConnection.AddComponent<BlockchainClient>();

    connection.Setup(
        "5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205",
        "http://localhost:7740"
    );

    // Create keypair
    var keyPair = PostchainUtil.MakeKeyPair();

    // Create and execute Operation
    var request = connection.NewTransaction(new byte[][] { keyPair["pubKey"] }, ErrorHandler);
    request.AddOperation("test_operation_create_user", "Bob");

    request.Sign(keyPair["privKey"], keyPair["pubKey"]);
    StartCoroutine(request.PostAndWait(() => Debug.Log("Successfully registered Bob")));

    // Query
    StartCoroutine(connection.Query<string>("find_user", new (string, object)[] { ("username", "Bob") },
        (string found) => Debug.Log("Found: " + found), ErrorHandler));
}

#region Helper
private void ErrorHandler(string error)
{
    throw new System.Exception(error);
}
#endregion
```

### FT3
```C#
public Blockchain Blockchain;

private void Start()
{
    StartCoroutine(ExampleFlow());
}

private IEnumerator ExampleFlow()
{
    // Connect to Blockchain
    Postchain postchain = new Postchain("http://localhost:7740");
    yield return postchain.Blockchain("5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205",
         SetBlockchain, ErrorHandler);

    // Register Ft3 Account
    KeyPair keyPair = new KeyPair();
    SingleSignatureAuthDescriptor singleSigAuthDescriptor = new SingleSignatureAuthDescriptor(
        keyPair.PubKey,
        new FlagsType[] { FlagsType.Account, FlagsType.Transfer }
    );

    // Create user who holds the keypair and the authdescriptor
    User user = new User(keyPair, singleSigAuthDescriptor);
    Account account = null;
    yield return this.Blockchain.RegisterAccount(user.AuthDescriptor, user,
        (Account _account) => account = _account, ErrorHandler);

    // Register Asset
    Asset asset = null;
    yield return Asset.Register("TestAsset", "5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205",
        Blockchain, (Asset _asset) => asset = _asset, ErrorHandler);

    // Give Balance
    yield return AssetBalance.GiveBalance(account.Id, asset.Id, 1000, Blockchain, () => { }, ErrorHandler);

    // Sync Account and print asset amount
    yield return account.Sync(() => Debug.Log(account.Assets[0].Amount), ErrorHandler);

    // Burn
    yield return account.BurnTokens(asset.Id, 500, () => Debug.Log("Successfully burned"), ErrorHandler);

    // Self Transfer
    yield return account.Transfer(account.Id, asset.Id, 250, () => Debug.Log("Selftransfer completed"), ErrorHandler);

    // Custom Operation
    yield return Blockchain.TransactionBuilder()
        .Add(Operation.Op("test_operation_create_user", "Bob"))
        .Build(user.AuthDescriptor.Signers.ToArray(), ErrorHandler)
        .Sign(user.KeyPair)
        .Post();

    // Custom Query
    yield return Blockchain.Query<string>("find_user", new (string, object)[] { ("username", "Bob") },
        (string found) => Debug.Log("Found: " + found), ErrorHandler);
}


#region Helper
private void SetBlockchain(Blockchain blockchain)
{
    Blockchain = blockchain;
}

private void ErrorHandler(string error)
{
    throw new System.Exception(error);
}
#endregion
```

### SSO Standalone
```C#
private const string CUSTOM_SCHEME = "sso";

private Blockchain _blockchain;
private SSO _sso;

private void Awake()
{
    ProtocolHandler.HandleTempTx(CUSTOM_SCHEME);
    SSO.VaultUrl = "https://vault-testnet.chromia.com";
}

private void Start()
{
    StartCoroutine(ExampleFlow());
}

private IEnumerator ExampleFlow()
{
    Postchain postchain = new Postchain("http://localhost:7740");
    yield return postchain.Blockchain("5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205",
        SetBlockchain, ErrorHandler);

    _sso = new SSO(this._blockchain);


    List<(Account, User)> found = null;
    yield return _sso.AutoLogin((List<(Account, User)> aus) => found = aus, ErrorHandler);

    if (found.Count == 0)
    {
        // Trigger login
        yield return SSOS();
    }
}

private IEnumerator SSOS()
{
    ProtocolHandler.Register(CUSTOM_SCHEME);
    // Use registered scheme here
    _sso.InitiateLogin("sso://success", "sso://cancel");

    while (_sso.Store.TmpTx == null)
    {
        yield return new WaitForSeconds(3);
        _sso.Store.Load();
    }
    string payload = _sso.Store.TmpTx;

    try
    {
        payload = payload.Split("?"[0])[1];
        payload = payload.Split("="[0])[1];
    }
    catch (System.Exception)
    {
        throw;
    }

    yield return _sso.FinalizeLogin(payload,
        ((Account, User) au) => Debug.Log("Successfully registered via sso"), ErrorHandler);
}

#region Helper
private void SetBlockchain(Blockchain blockchain)
{
    this._blockchain = blockchain;
}

private void ErrorHandler(string error)
{
    throw new Exception(error);
}
#endregion
```

See `Samples/FT3/SSO*` for for further demo scenes and webgl.

## Test

Run the rell code defined in `Samples/FT3/rell` and use Unity's internal test suite.

## Support

Use the [Chromia Dev Telegram Chat](https://t.me/ChromiaDev)
