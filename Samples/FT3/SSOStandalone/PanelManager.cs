using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    public Button ConnectButton;
    public Dropdown AccountsDropdown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void AddOptionsToPanel(List<(Account, User)> aus)
    {
        var options = aus.Select((elem) => new Dropdown.OptionData(elem.Item1.Id));
        PanelManager.Instance.AccountsDropdown.AddOptions(options.ToList());
    }

    public static void AddOptionToPanel((Account, User) au)
    {
        var option = new Dropdown.OptionData(au.Item1.Id);
        PanelManager.Instance.AccountsDropdown.AddOptions(new Dropdown.OptionData[] { option }.ToList());
    }
}
