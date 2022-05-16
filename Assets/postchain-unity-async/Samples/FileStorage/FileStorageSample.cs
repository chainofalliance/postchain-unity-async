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
    private string nodeUrl;

    [SerializeField]
    private FileHub fileHub;

    [SerializeField]
    private InputField input;

    [SerializeField]
    private Text showText;

    private User user;
    private Account account;
    private FsFile latestFile;

    private async void Start()
    {
        await fileHub.Establish(nodeUrl, 1);

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

    private async UniTask SaveText()
    {
        if (this.user == null) return;

        var data = Encoding.ASCII.GetBytes(input.text);
        var file = FsFile.FromData(data);

        await fileHub.StoreFile(user, file);

        Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash));

        input.text = "";
        latestFile = file;
    }

    private async UniTask LoadLatestText()
    {
        if (latestFile == null) return;

        var storedFile = await fileHub.GetFile(latestFile.Hash);

        showText.text = Encoding.ASCII.GetString(storedFile.Content.Data);
    }
}
