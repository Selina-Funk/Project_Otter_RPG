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

        if (playerInventory.GetItemAmount(itemName) > 1)
        {
            playerInventory.RemoveItem(itemName);
            button.transform.Find("Amount_Text").GetComponent<TextMeshProUGUI>().text = playerInventory.GetItemAmount(itemName).ToString();
        }
        else
        {
            int itemIterator = 0;
            
            foreach (var item in playerInventory.GetInventory())
            {
                Debug.Log("Inventory item NAME: " + item.Value.identifier.name);
                Debug.Log("DESIRED ITEM NAME: " + itemName);
                Debug.Log("DO THE ITEM AND BUTTON NAME MATCH? " + item.Value.identifier.name == itemName);
                if (item.Value.identifier.name == itemName)
                {
                    Debug.Log("ITEM NAME: " + item.Value.identifier.name + " | BUTTON NAME: " + itemName);
                    Debug.Log("DO THE ITEM AND BUTTON NAME MATCH? " + item.Value.identifier.name == itemName);
                    itemIterator++;
                    Debug.Log("ITEM ITERATOR = " + itemIterator);
                    break;
                }
                itemIterator++;
            }
            Debug.Log("ITEM ITERATOR FOR INV MENU: " + itemIterator);
            if (itemIterator == 0)
            {
                EventSystem.current.SetSelectedGameObject(buttonContainer.transform.GetChild(itemIterator + 1).gameObject);
            }
            else if (itemIterator == buttonContainer.transform.childCount)
            {
                EventSystem.current.SetSelectedGameObject(buttonContainer.transform.GetChild(buttonContainer.transform.childCount - 1).gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(buttonContainer.transform.GetChild(itemIterator - 1).gameObject);
            }
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
