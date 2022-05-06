using UnityEngine;
using UnityEngine.UI;

public class DemoScript : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] private InputField _registerUser;
    [SerializeField] private Button _registerUserButton;
    [SerializeField] private InputField _checkUser;
    [SerializeField] private Button _checkUserButton;
    [SerializeField] private Text _infoText;
    [SerializeField] private BlockchainWrapper _blockchain;
#pragma warning restore CS0649

    void Start()
    {
        _registerUserButton.onClick.AddListener(OnRegisterUser);
        _checkUserButton.onClick.AddListener(OnCheckUser);
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
