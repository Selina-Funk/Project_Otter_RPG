using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> attackButtons = new List<GameObject>();
    [SerializeField] private GameObject actionMenu;

    // Adds move action to the actionTypes List in BattleManager
    public void MoveAction()
    {
        BattleManager.GetInstance().SetPlayerAction(BattleManager.ActionTypes.MOVE);
        ResetActiveButton();
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count == BattleManager.GetInstance().GetPlayerActions())
        {
            ShowUIMenu(false);
            BattleManager.GetInstance().SetCanPerformActions(true);
        }
    }

    // Adds attack action to the actionTypes List in BattleManager
    public void AttackAction()
    {
        PlayerCombat playerCombat = GameObject.Find("Player_UI").GetComponent<PlayerCombat>();
        gameObject.transform.Find("Action_Menu").gameObject.SetActive(false);
        gameObject.transform.Find("Moves_Menu").gameObject.SetActive(true);
        for (int i = 0; i < attackButtons.Count; i++)
        {
            if (i > (playerCombat.GetMoves().Count - 1))
            {
                break;
            }
            attackButtons[i].gameObject.SetActive(true);
            attackButtons[i].transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = playerCombat.GetMoves()[i].moveName;
        }
        EventSystem.current.SetSelectedGameObject(attackButtons[0]);
    }

    public void MoveOne()
    {
        PlayerCombat playerCombat = GameObject.Find("Player_UI").GetComponent<PlayerCombat>();
        string moveName = attackButtons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        playerCombat.SetChosenMoveData(playerCombat.ChosenMove(moveName));
        gameObject.transform.Find("Moves_Menu").gameObject.SetActive(false);
        foreach (var button in attackButtons)
        {
            if (button.activeInHierarchy)
            {
                button.SetActive(false);
            }
            else
            {
                break;
            }
        }
        gameObject.transform.Find("Action_Menu").gameObject.SetActive(true);
        BattleManager.GetInstance().SetPlayerAction(BattleManager.ActionTypes.ATTACK);
        ResetActiveButton();
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count == BattleManager.GetInstance().GetPlayerActions())
        {
            ShowUIMenu(false);
            BattleManager.GetInstance().SetCanPerformActions(true);
        }
    }

    public void MoveTwo()
    {
        PlayerCombat playerCombat = GameObject.Find("Player_UI").GetComponent<PlayerCombat>();
        string moveName = attackButtons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        playerCombat.SetChosenMoveData(playerCombat.ChosenMove(moveName));
        gameObject.transform.Find("Moves_Menu").gameObject.SetActive(false);
        foreach (var button in attackButtons)
        {
            if (button.activeInHierarchy)
            {
                button.SetActive(false);
            }
            else
            {
                break;
            }
        }
        gameObject.transform.Find("Action_Menu").gameObject.SetActive(true);
        BattleManager.GetInstance().SetPlayerAction(BattleManager.ActionTypes.ATTACK);
        ResetActiveButton();
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count == BattleManager.GetInstance().GetPlayerActions())
        {
            ShowUIMenu(false);
            BattleManager.GetInstance().SetCanPerformActions(true);
        }
    }

    public void MoveThree()
    {
        PlayerCombat playerCombat = GameObject.Find("Player_UI").GetComponent<PlayerCombat>();
        string moveName = attackButtons[2].gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        playerCombat.SetChosenMoveData(playerCombat.ChosenMove(moveName));
        gameObject.transform.Find("Moves_Menu").gameObject.SetActive(false);
        foreach (var button in attackButtons)
        {
            if (button.activeInHierarchy)
            {
                button.SetActive(false);
            }
            else
            {
                break;
            }
        }
        gameObject.transform.Find("Action_Menu").gameObject.SetActive(true);
        BattleManager.GetInstance().SetPlayerAction(BattleManager.ActionTypes.ATTACK);
        ResetActiveButton();
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count == BattleManager.GetInstance().GetPlayerActions())
        {
            ShowUIMenu(false);
            BattleManager.GetInstance().SetCanPerformActions(true);
        }
    }

    public void MoveFour()
    {
        PlayerCombat playerCombat = GameObject.Find("Player_UI").GetComponent<PlayerCombat>();
        string moveName = attackButtons[3].gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        playerCombat.SetChosenMoveData(playerCombat.ChosenMove(moveName));
        gameObject.transform.Find("Moves_Menu").gameObject.SetActive(false);
        foreach (var button in attackButtons)
        {
            if (button.activeInHierarchy)
            {
                button.SetActive(false);
            }
            else
            {
                break;
            }
        }
        gameObject.transform.Find("Action_Menu").gameObject.SetActive(true);
        BattleManager.GetInstance().SetPlayerAction(BattleManager.ActionTypes.ATTACK);
        ResetActiveButton();
        if (BattleManager.GetInstance().GetPlayerActionTypesList().Count == BattleManager.GetInstance().GetPlayerActions())
        {
            ShowUIMenu(false);
            BattleManager.GetInstance().SetCanPerformActions(true);
        }
    }

    public void ShowUIMenu(bool truthValue)
    {
        actionMenu.SetActive(truthValue);
    }

    private void ResetActiveButton()
    {
        EventSystem.current.SetSelectedGameObject(GameObject.Find("Move_Button"));
    }

    public void StartGame()
    {
        SceneManager.LoadScene("PlayerCombatMovement");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
