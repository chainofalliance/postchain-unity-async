using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

public class DemoScript : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] private InputField _registerUser;
    [SerializeField] private Button _registerUserButton;
    [SerializeField] private InputField _checkUser;
    [SerializeField] private Button _checkUserButton;
    [SerializeField] private Button _testButton;
    [SerializeField] private Text _infoText;
    [SerializeField] private BlockchainWrapper _blockchain;
#pragma warning restore CS0649

    void Start()
    {
        _registerUserButton.onClick.AddListener(OnRegisterUser);
        _checkUserButton.onClick.AddListener(OnCheckUser);
        _testButton.onClick.AddListener(OnTest);
    }

    public async void OnRegisterUser()
    {
        string username = _registerUser.text;

        _registerUserButton.interactable = false;

        var res = await _blockchain.Operation("register_user", username);

        if (!res.Error)
        {
            _infoText.text = "Successfully registered user with name " + _registerUser.text;
            _registerUserButton.interactable = true;
        }
    }

    public async void OnCheckUser()
    {
        string username = _checkUser.text;

        _checkUserButton.interactable = false;
        var res = await _blockchain.Query<bool>("check_user", ("name", username));

        if (!res.Error)
            OnCheckUserSuccess(res.Content);
    }

    public async void OnTest()
    {
        string username = _registerUser.text;

        _testButton.interactable = false;

        var res = await _blockchain.Operation("fun1", new Dictionary<string, int>() {
            {"333", 123},
            {"444", 345}
        });

        if (!res.Error)
        {
            _infoText.text = "Successfully tested! " + _registerUser.text;
            _testButton.interactable = true;
        }
    }

    private void OnCheckUserSuccess(bool doesUserExist)
    {
        if (doesUserExist)
        {
            _infoText.text = "A user with name " + _checkUser.text + " already exists";
        }
        else
        {
            _infoText.text = "No user with name " + _checkUser.text + " exists";
        }
        _checkUserButton.interactable = true;
    }
}
