using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

public class SSOStandalone : MonoBehaviour
{
    [SerializeField] private string _blockchainRID;
    [SerializeField] private string _baseURL;
    [SerializeField] private string _vaultUrl;
    [SerializeField] private string _successUrl;
    [SerializeField] private string _cancelUrl;
    [SerializeField] private string _customProtocolName;

    private Blockchain _blockchain;
    private SSO _sso;

    private void Awake()
    {
        ProtocolHandler.HandleTempTx(_customProtocolName);
        SSO.VaultUrl = _vaultUrl;
    }

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator ConnectToBlockchain()
    {
        Postchain postchain = new Postchain(_baseURL);
        yield return postchain.Blockchain(_blockchainRID, SetBlockchain, DefaultErrorHandler);
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
        _sso = new SSO(this._blockchain);

        yield return _sso.AutoLogin(PanelManager.AddOptionsToPanel, DefaultErrorHandler);
    }


    private IEnumerator SSOS()
    {
        ProtocolHandler.Register(_customProtocolName);
        _sso.InitiateLogin(_successUrl, _cancelUrl);

        while (_sso.Store.TmpTx == null)
        {
            yield return new WaitForSeconds(3);
            _sso.Store.Load();
        }

        var payload = _sso.Store.TmpTx;
        payload = payload.Split("?"[0])[1];
        string raw = payload.Split("="[0])[1];

        yield return _sso.FinalizeLogin(raw, PanelManager.AddOptionToPanel, DefaultErrorHandler);
    }

    public void Connect()
    {
        if (this._blockchain == null) return;
        StartCoroutine(SSOS());
    }
}
