using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Rendering;

public class Inventory : MonoBehaviour
{
    // Class for item data to be used for the inventory
    public class ItemData
    {
        public Item identifier;
        public int amount;

        public void init(Item ident, int startNumber)
        {
            identifier = ident;
            amount = startNumber;
        }
    }

    Dictionary<string, ItemData> inventory = new Dictionary<string, ItemData>();

    /// <summary>
    /// Adds an item to the inventory either by increasing the amount or by adding a new item to the dictionary
    /// </summary>
    /// <param name="obj"></param> The gameobject that contains the item data
    public void AddItem(GameObject obj)
    {
        if (obj.tag == "Item")
        {
            if (obj.TryGetComponent(out ItemDataHolder itemData))
            {
                // Checks if the item is already in the inventory
                if (inventory.ContainsKey(itemData.itemData.itemName) && inventory[itemData.itemData.itemName].amount < 99)
                {
                    // Increases the amount by 1 if the item amount is less that 99
                    inventory.TryGetValue(itemData.itemData.itemName, out ItemData inventoryItem);
                    inventoryItem.amount++;
                }
                else
                {
                    // Adds a new item into the inventory
                    ItemData newItem = new ItemData();
                    newItem.init(obj.GetComponent<ItemDataHolder>().itemData, 1);
                    AddNewItem(newItem);
                }
            }
        }

        Destroy(obj);
    }

    private void AddNewItem(ItemData newItemData)
    {
        int inventoryIndex = 0;
        bool itemAdded = false;
        string newItemName = newItemData.identifier.itemName;
        List<ItemData> healingItems = new List<ItemData>();
        List<ItemData> attackItems = new List<ItemData>();

        // Puts items into respective lists for healing or attack
        foreach (var item in inventory.Values)
        {
            if (item.identifier.itemType == Item.ItemType.HEALING)
            {
                healingItems.Add(item);
            }
            else if (item.identifier.itemType == Item.ItemType.DAMAGING)
            {
                attackItems.Add(item);
            }
        }

        // Checks what type of item the new item is, healing or attack
        if (newItemData.identifier.itemType == Item.ItemType.HEALING)
        {
            foreach (var item in healingItems)
            {
                // Goes through each character in the item name string
                for (int i = 0; i < item.identifier.itemName.Length; i++)
                {
                    if (newItemName.ToLower()[i] == newItemName.ToLower()[newItemName.Length - 1])
                    {
                        // Inserts the new item before the current item in the list if the char is smaller
                        healingItems.Insert(inventoryIndex, newItemData);
                        itemAdded = true;
                        break;
                    }
                    else if (item.identifier.itemName.ToLower()[i] == item.identifier.itemName.ToLower()[item.identifier.itemName.Length - 1])
                    {
                        // moves to the next item
                        break;
                    }
                    else if (newItemName.ToLower()[i] < item.identifier.itemName.ToLower()[i])
                    {
                        // Inserts the item before the current item
                        healingItems.Insert(inventoryIndex, newItemData);
                        itemAdded = true;
                        break;
                    }
                    else if (newItemName.ToLower()[i] > item.identifier.itemName.ToLower()[i])
                    {
                        break;
                    }
                }
                if (itemAdded)
                {
                    break;
                }
                inventoryIndex++;
            }
            if (!itemAdded)
            {
                healingItems.Add(newItemData);
            }
        }
        else if (newItemData.identifier.itemType == Item.ItemType.DAMAGING)
        {
            foreach (var item in attackItems)
            {
                // Goes through each character in the item name string
                for (int i = 0; i < item.identifier.itemName.Length; i++)
                {
                    if (newItemName.ToLower()[i] == newItemName.ToLower()[newItemName.Length - 1])
                    {
                        // Inserts the new item before the current item in the list if the char is smaller
                        attackItems.Insert(inventoryIndex, newItemData);
                        itemAdded = true;
                        break;
                    }
                    else if (item.identifier.itemName.ToLower()[i] == item.identifier.itemName.ToLower()[item.identifier.itemName.Length - 1])
                    {
                        break;
                    }
                    else if (newItemName.ToLower()[i] < item.identifier.itemName.ToLower()[i])
                    {
                        // Inserts the item before the current item
                        attackItems.Insert(inventoryIndex, newItemData);
                        itemAdded = true;
                        break;
                    }
                    else if (newItemName.ToLower()[i] > item.identifier.itemName.ToLower()[i])
                    {
                        break;
                    }
                }
                if (itemAdded)
                {
                    break;
                }
                inventoryIndex++;
            }
            if (!itemAdded)
            {
                attackItems.Add(newItemData);
            }
        }

        // Resets the invenotry
        inventory = new Dictionary<string, ItemData>();

        // Adds the new item order of healing items
        foreach (var item in healingItems)
        {
            inventory.Add(item.identifier.itemName, item);
        }
        // Adds the new item order of attack items
        foreach (var item in attackItems)
        {
            inventory.Add(item.identifier.itemName, item);
        }
    }

    /// <summary>
    /// Called when an item needs to be removed from the inventory
    /// This can be a decrease of the amount or removed from the inventory as a whole
    /// </summary>
    /// <param name="obj"></param> The game object that the player is trying to interact with
    public void RemoveItem(string itemName)
    {
        if (inventory.TryGetValue(itemName, out ItemData itemData))
        {
            // If the item has more than one in the inventory, decrease the amount by 1
            if (itemData.amount > 1)
            {
                itemData.amount--;
            }
            // If the item only has one for the amount in the inventory, remove the item from the inventory
            else if (itemData.amount == 1)
            {
                inventory.Remove(itemName);
            }
        }
    }

    public Dictionary<string, ItemData> GetInventory()
    {
        return inventory;
    }

    public int GetItemAmount(string itemName)
    {
        if (inventory.TryGetValue(itemName, out ItemData itemData))
        {
            return itemData.amount;
        }
        return -1;
    }
}
