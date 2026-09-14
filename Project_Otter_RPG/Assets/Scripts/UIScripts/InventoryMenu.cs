using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

        // Checks to see if the item amount is greater than one
        if (playerInventory.GetItemAmount(itemName) > 1)
        {
            playerInventory.RemoveItem(itemName);
            button.transform.Find("Amount_Text").GetComponent<TextMeshProUGUI>().text = playerInventory.GetItemAmount(itemName).ToString();
        }
        else
        {
            int itemIterator = 0;
            
            // goes through each item in the players inventory and checks to see if the item used can be found
            foreach (var item in playerInventory.GetInventory())
            {
                if (item.Value.identifier.name == itemName)
                {
                    break;
                }
                else
                {
                    itemIterator++;
                }
            }
            // Selects the next button depending on where the item is located in the menu
            if (playerInventory.GetInventory().Count - 1 > 0)
            {
                if (itemIterator == 0)
                {
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("My_Content").transform.GetChild(itemIterator + 1).gameObject);
                }
                else if (itemIterator == (GameObject.Find("My_Content").transform.childCount - 1))
                {
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("My_Content").transform.GetChild(itemIterator - 1).gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("My_Content").transform.GetChild(itemIterator).gameObject);
                }
            }
            
            // Removes the item and button
            playerInventory.RemoveItem(itemName);
            Destroy(button);
        }
    }

    public void AccessInventory()
    {
        OverworldPlayerMovement OverPlayerMov = GameObject.Find("OverworldPlayer").GetComponent<OverworldPlayerMovement>();
        
        ChangeDisplayMode();

        if (GetCanSee())
        {
            OverPlayerMov.DisableMovement();

            GameObject buttonCont = GetButtonContainer();

            PopulateButtons(buttonCont);

            LoopTopAndBottom(buttonCont);
        }
        else
        {
            OverPlayerMov.EnableMovement();
        }
    }

    private void PopulateButtons(GameObject buttonContainer)
    {
        int buttonIterator = 0;
        UnityEngine.UI.Button targetButton = null;

        foreach (Transform child in buttonContainer.transform)
        {
            Navigation navigation = new Navigation();
            if (buttonIterator > 0)
            {
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnDown = child.GetComponent<UnityEngine.UI.Button>();
                targetButton.GetComponent<UnityEngine.UI.Button>().navigation = navigation;

                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = targetButton.GetComponent<UnityEngine.UI.Button>();
                child.GetComponent<UnityEngine.UI.Button>().navigation = navigation;
            }
            targetButton = child.GetComponent<UnityEngine.UI.Button>();
            buttonIterator++;
        }
    }

    private void LoopTopAndBottom(GameObject buttonCont)
    {
        if (buttonCont.transform.childCount >= 2)
        {
            Navigation navigation = buttonCont.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().navigation;
            navigation.selectOnUp = buttonCont.transform.GetChild(buttonCont.transform.childCount - 1).GetComponent<UnityEngine.UI.Button>();
            buttonCont.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>().navigation = navigation;

            navigation = buttonCont.transform.GetChild(buttonCont.transform.childCount - 1).GetComponent<UnityEngine.UI.Button>().navigation;
            navigation.selectOnDown = buttonCont.transform.GetChild(0).GetComponent<UnityEngine.UI.Button>();
            buttonCont.transform.GetChild(buttonCont.transform.childCount - 1).GetComponent<UnityEngine.UI.Button>().navigation = navigation;
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
