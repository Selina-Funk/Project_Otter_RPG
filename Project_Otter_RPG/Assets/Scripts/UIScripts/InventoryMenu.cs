using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    Inventory playersInventory;

    [SerializeField] GameObject buttonObj;
    [SerializeField] GameObject buttonContainer;
    private bool canSee = true;

    private void Awake()
    {
        playersInventory = GameObject.Find("OverworldPlayer").GetComponent<Inventory>();

        ChangeDisplayMode();
    }

    public void ChangeDisplayMode()
    {
        canSee = !canSee;
        this.gameObject.SetActive(canSee);
    }

    private void AddButtons()
    {
        foreach (var item in playersInventory.GetInventory().Values)
        {
            GameObject newButton = Instantiate(buttonObj, buttonContainer.transform);
            TextMeshProUGUI nameText = newButton.transform.Find("Name_Text").GetComponent<TextMeshProUGUI>();
            nameText.text = item.identifier.itemName.ToString();
            TextMeshProUGUI amountText = newButton.transform.Find("Amount_Text").GetComponent<TextMeshProUGUI>();
            amountText.text = item.amount.ToString();
            newButton.transform.SetParent(buttonContainer.transform);
        }

        if (buttonContainer.transform.childCount > 0)
        {
            EventSystem.current.SetSelectedGameObject(buttonContainer.transform.GetChild(0).gameObject);
        }
    }

    public void GetButtonGameObject()
    {
        Inventory playerInventory = GameObject.Find("OverworldPlayer").GetComponent<Inventory>();
        var button = EventSystem.current.currentSelectedGameObject;

        string itemName = button.transform.Find("Name_Text").GetComponent<TextMeshProUGUI>().text;

        playerInventory.RemoveItem(itemName);

        if (playerInventory.GetItemAmount(itemName) > 0)
        {
            button.transform.Find("Amount_Text").GetComponent<TextMeshProUGUI>().text = playerInventory.GetItemAmount(itemName).ToString();
        }
        else
        {
            Destroy(button);
        }
    }

    public GameObject GetButtonContainer()
    {
        return buttonContainer;
    }

    public bool GetCanSee()
    {
        return canSee;
    }

    private void OnEnable()
    {
        AddButtons();
    }

    private void OnDisable()
    {
        foreach (Transform child in buttonContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
