using Newtonsoft.Json.Utilities;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine;
using System;

public class SSOWebgl : MonoBehaviour
{
    [SerializeField] private int _chainId;
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

    private async void Start()
    {
        Postchain postchain = new Postchain(_baseURL);
        _blockchain = await postchain.Blockchain(_chainId);
        _sso = new SSO(this._blockchain, new SSOWebGLStore());

        var resPending = SSOWebGLStore.ExtractRawTx();

        if (!String.IsNullOrEmpty(resPending))
        {
            await _sso.FinalizeLogin(resPending);
        }

        var resAuto = await _sso.AutoLogin();

        PanelManager.AddOptionsToPanel(resAuto);
    }

    public void Connect()
    {
        if (this._blockchain == null) return;
        _sso.InitiateLogin(_successUrl, _cancelUrl);
    }
}