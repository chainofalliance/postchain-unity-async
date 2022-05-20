using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using Chromia.Postchain.Fs;
using UnityEngine.UI;
using UnityEngine;
using System.Text;

using Cysharp.Threading.Tasks;

public class FileStorageSample : MonoBehaviour
{
    [SerializeField]
    private InputField input;

    [SerializeField]
    private Text showText;


    private string nodeUrl = "http://localhost:7740";
    private int chainId = 0;

    private FileHub fileHub;
    private User user;
    private Account account;
    private FsFile latestFile;

    private async void Start()
    {
        fileHub = gameObject.AddComponent<FileHub>();
        await fileHub.Establish(nodeUrl, chainId);

        KeyPair keyPair = new KeyPair();
        SingleSignatureAuthDescriptor singleSigAuthDescriptor = new SingleSignatureAuthDescriptor(
            keyPair.PubKey,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray(),
            null
        );
        User user = new User(keyPair, singleSigAuthDescriptor);
        var account = await fileHub.Blockchain.RegisterAccount(user.AuthDescriptor, user);

        this.user = user;
        this.account = account.Content;
    }

    public void SaveWrapper()
    {
#pragma warning disable 4014
        SaveText();
#pragma warning restore 4014
    }

    public async UniTask SaveText()
    {
        if (this.user == null) return;

        var data = Encoding.ASCII.GetBytes(input.text);
        var file = FsFile.FromData(data);

        await fileHub.StoreFile(user, file);

        Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash));

        input.text = "";
        latestFile = file;
    }

    public void LoadWrapper()
    {
#pragma warning disable 4014
        LoadLatestText();
#pragma warning restore 4014
    }

    public async UniTask LoadLatestText()
    {
        if (latestFile == null) return;

        var storedFile = await fileHub.GetFile(latestFile.Hash);

        showText.text = Encoding.ASCII.GetString(storedFile.Content.Data);
    }
}
