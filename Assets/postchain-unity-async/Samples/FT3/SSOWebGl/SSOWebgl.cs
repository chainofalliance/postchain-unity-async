using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

public class SSOWebgl : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;

    private Blockchain _blockchain;
    private SSO _sso;

    private void Awake()
    {
        SSO.VaultUrl = _vaultUrl;

#if UNITY_WEBGL
        AotHelper.EnsureList<AuthDescriptorFactory.AuthDescriptorQuery>();
        AotHelper.EnsureList<Asset>();
#endif
    }

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private void SetBlockchain(Blockchain blockchain)
    {
        this._blockchain = blockchain;
    }

    private void DefaultErrorHandler(string error)
    {
        throw new Exception(error);
    }

    private IEnumerator StartRoutine()
    {
        yield return ConnectToBlockchain();
        _sso = new SSO(this._blockchain, new SSOStoreLocalStorage());

        yield return _sso.PendingSSO(PanelManager.AddOptionToPanel, DefaultErrorHandler);
        yield return _sso.AutoLogin(PanelManager.AddOptionsToPanel, DefaultErrorHandler);
    }

    private IEnumerator ConnectToBlockchain()
    {
        Postchain postchain = new Postchain(_baseURL);
        yield return postchain.Blockchain(_blockchainRID, SetBlockchain, DefaultErrorHandler);
    }

    public void Connect()
    {
        if (this._blockchain == null) return;
        _sso.InitiateLogin(_successUrl, _cancelUrl);
    }
}