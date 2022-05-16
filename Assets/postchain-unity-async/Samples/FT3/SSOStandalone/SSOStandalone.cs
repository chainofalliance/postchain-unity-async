using Chromia.Postchain.Ft3;
using Cysharp.Threading.Tasks;
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

    private async void Start()
    {
        Postchain postchain = new Postchain(_baseURL);
        _blockchain = await postchain.Blockchain(_blockchainRID);
        _sso = new SSO(this._blockchain);

        var aus = await _sso.AutoLogin();
        PanelManager.AddOptionsToPanel(aus);
    }
    private async UniTask SSOS()
    {
        ProtocolHandler.Register(_customProtocolName);
        _sso.InitiateLogin(_successUrl, _cancelUrl);

        while (_sso.Store.TmpTx == null)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);
            _sso.Store.Load();
        }

        var payload = _sso.Store.TmpTx;
        payload = payload.Split("?"[0])[1];
        string raw = payload.Split("="[0])[1];

        var res = await _sso.FinalizeLogin(raw);

        if (!res.Error)
            PanelManager.AddOptionToPanel(res.Content);
    }

    public async void Connect()
    {
        if (this._blockchain == null) return;
        await SSOS();
    }
}
