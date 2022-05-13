using System.Collections;
using System.Threading.Tasks;
using Chromia.Postchain.Ft3;
using Chromia.Postchain.Fs;
using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;
using System;

using Cysharp.Threading.Tasks;

// FileHub has to be initialised
public class StoreFile
{
    private const string FILEHUB_NODE = "http://127.0.0.1:7740";
    private const string FILEHUB_BRID = "ED5C6FF9862E0E545C472E3FB033A776CD7FAB28AFE28124ABF6245A26CA579D";

    private static System.Random random = new System.Random();

    private FileHub fileHub;
    private User user;
    private Account account;

    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private byte[] GenerateData(int length)
    {
        var data = GenerateRandomString(length);
        UnityEngine.Debug.Log(data);
        return Encoding.ASCII.GetBytes(data);
    }

    private async UniTask SetupFileHub()
    {
        FileHub fileHub = new GameObject().AddComponent<FileHub>();
        await fileHub.Establish(FILEHUB_NODE, FILEHUB_BRID);
        this.fileHub = fileHub;
    }

    private async UniTask SetupAccount()
    {
        User user = TestUser.SingleSig();
        var account = await fileHub.Blockchain.RegisterAccount(user.AuthDescriptor, user);

        if (account.Error)
            throw new Exception(account.ErrorMessage);

        this.user = user;
        this.account = account.Content;
    }

    // Create file
    [Test]
    public void StoreFileTest1()
    {
        var data = GenerateData(11);
        var file = FsFile.FromData(data);

        Assert.AreEqual(file.NumberOfChunks(), 1);
        Assert.AreEqual(Util.ByteArrayToString(file.Data), Util.ByteArrayToString(data));
    }

    // Store file
    [Test]
    public async Task StoreFileTest2()
    {
        var data = GenerateData(36);
        var file = FsFile.FromData(data);

        await SetupFileHub();
        await SetupAccount();

        await fileHub.StoreFile(user, file);

        Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash));

        var storedFile = await fileHub.GetFile(file.Hash);

        Assert.False(storedFile.Error);
        Assert.AreEqual(Util.ByteArrayToString(storedFile.Content.Data), Util.ByteArrayToString(file.Data));
    }

    // Store actual file
    [Test]
    public async Task StoreFileTest3()
    {
        var path = Path.Combine(UnityEngine.Application.dataPath, "postchain-unity-async/Tests/FileStorage/files/small.txt");
        var file = FsFile.FromLocalFile(path);

        await SetupFileHub();
        await SetupAccount();

        await fileHub.StoreFile(user, file);

        Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash));

        var storedFile = await fileHub.GetFile(file.Hash);

        Assert.False(storedFile.Error);
        Assert.AreEqual(Util.ByteArrayToString(storedFile.Content.Data), Util.ByteArrayToString(file.Data));
    }

    // Store actual file, large
    [Test]
    public async Task StoreFileTest4()
    {
        var path = Path.Combine(UnityEngine.Application.dataPath, "postchain-unity-async/Tests/FileStorage/files/large.txt");
        var file = FsFile.FromLocalFile(path);

        Debug.Log(file.NumberOfChunks());

        await SetupFileHub();
        await SetupAccount();

        await fileHub.StoreFile(user, file);

        Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash));

        var storedFile = await fileHub.GetFile(file.Hash);

        Assert.False(storedFile.Error);
        Assert.AreEqual(Util.ByteArrayToString(storedFile.Content.Data), Util.ByteArrayToString(file.Data));
    }

    // Store file, large file split into multiple chunks
    [Test]
    public async Task StoreFileTest5()
    {
        var dataSize = 1024 * 1024 * 2;
        var data = GenerateData(dataSize);
        var file = FsFile.FromData(data);

        await SetupFileHub();
        await SetupAccount();

        await fileHub.StoreFile(user, file);

        Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash));

        var storedFile = await fileHub.GetFile(file.Hash);

        Assert.False(storedFile.Error);
        Assert.AreEqual(storedFile.Content.NumberOfChunks(), 21);
        Assert.AreEqual(storedFile.Content.NumberOfChunks(), file.NumberOfChunks());
    }
}
