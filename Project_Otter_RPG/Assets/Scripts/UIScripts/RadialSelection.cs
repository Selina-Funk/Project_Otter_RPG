using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RadialSelection : MonoBehaviour
{
    [Header("Radial button Information")]
    private int numberOfRadialSlices;
    [SerializeField] private GameObject radialPart;
    [SerializeField] private Transform radialMenuCanvas;
    [SerializeField] private float angleBetweenPart = 10.0f;
    [SerializeField] private float rotationAmount;
    [SerializeField] private PlayerCombat playerCombat;
    GameObject entireRadialMenu;

    [Header("Button Colors")]
    [SerializeField] private Color color1;
    [SerializeField] private Color color1Highlight;
    [SerializeField] private Color color2;
    [SerializeField] private Color color2Highlight;
    [SerializeField] private Color color3;
    [SerializeField] private Color color3Highlight;
    [SerializeField] private Color color4;
    [SerializeField] private Color color4Highlight;
    private List<GameObject> radialMenuParts = new List<GameObject>();
    private List<MoveData> attackData = new List<MoveData>();
    private Dictionary<GameObject, MoveData> buttonToAttack = new Dictionary<GameObject, MoveData>();

    private PlayerActions playerActions;

    private void Awake()
    {
        playerActions = new PlayerActions();
        playerActions.Combat.RotateRight.performed += RotateWheelRight;
        playerActions.Combat.RotateLeft.performed += RotateWheelLeft;
        numberOfRadialSlices = playerCombat.GetMoves().Count;
        attackData = playerCombat.GetMoves();
    }

    private void Update()
    {

        Canvas attackCanvas = GameObject.Find("Attack_Canvas").GetComponent<Canvas>();
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(attackCanvas.worldCamera, GameObject.Find("Player_UI").transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(attackCanvas.transform as RectTransform, screenPos, attackCanvas.worldCamera, out Vector2 localPoint);

        radialMenuCanvas.GetComponent<Transform>().position = GameObject.Find("Player_UI").transform.position;
    }
    public void SpawnRaidialPart()
    {
        GameObject spawnedRadialPart = null;
        rotationAmount = 360f / numberOfRadialSlices;
        for (int i = 0; i < numberOfRadialSlices; i++)
        {
            float angle = i * 350f / numberOfRadialSlices - angleBetweenPart/2;
            Vector3 radialPartEulerAngle = new Vector3(0, 0, angle);

            spawnedRadialPart = Instantiate(radialPart, radialMenuCanvas);
            Canvas attackCanvas = GameObject.Find("Attack_Canvas").GetComponent<Canvas>();
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(attackCanvas.worldCamera, radialMenuCanvas.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(attackCanvas.transform as RectTransform, screenPos, attackCanvas.worldCamera, out Vector2 localPoint);

            spawnedRadialPart.GetComponent<RectTransform>().anchoredPosition = localPoint;
            spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle;

            spawnedRadialPart.GetComponent<Image>().fillAmount = 1.0f / (float)numberOfRadialSlices - (angleBetweenPart/360);
            Button radialButton = spawnedRadialPart.transform.Find("Button").GetComponent<Button>();
            ColorBlock radialColorBlock = radialButton.colors;

            if (radialMenuParts.Count == 0)
            {
                radialColorBlock.normalColor = color1;
                radialColorBlock.selectedColor = color1Highlight;
            }
            if (radialMenuParts.Count == 1)
            {
                radialColorBlock.normalColor = color2;
                radialColorBlock.selectedColor = color2Highlight;
            }
            if (radialMenuParts.Count == 2)
            {
                radialColorBlock.normalColor = color3;
                radialColorBlock.selectedColor = color3Highlight;
            }
            if (radialMenuParts.Count == 3)
            {
                radialColorBlock.normalColor = color4;
                radialColorBlock.selectedColor = color4Highlight;
            }
            buttonToAttack[spawnedRadialPart] = attackData[i];
            //attackData.RemoveAt(i);
            spawnedRadialPart.transform.Find("Button").GetComponent<Button>().onClick.AddListener(Attack);

            radialButton.colors = radialColorBlock;
            radialMenuParts.Add(spawnedRadialPart);
        }

        GameObject buttonObj = radialMenuParts[0].transform.Find("Button").gameObject;
        EventSystem.current.SetSelectedGameObject(buttonObj);

        radialMenuCanvas.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -135.0f);
    }

    private void RotateWheelLeft(InputAction.CallbackContext context)
    {
        ShiftLeft();
        Vector3 rationalAngle = new Vector3(0, 0, rotationAmount);
        Vector3 currentAngle = radialMenuCanvas.transform.localEulerAngles;
        Vector3 newAngle = currentAngle + rationalAngle;
        if (newAngle.z > 360) newAngle.z -= 360;
        radialMenuCanvas.transform.localEulerAngles = newAngle;
    }

    private void RotateWheelRight(InputAction.CallbackContext context)
    {
        ShiftRight();
        Vector3 rotationalAngle = new Vector3(0, 0, -rotationAmount);
        Vector3 currentAngle = radialMenuCanvas.transform.localEulerAngles;
        Vector3 newAngle = currentAngle + rotationalAngle;
        if (newAngle.z < 0) newAngle.z += 360;
        radialMenuCanvas.transform.localEulerAngles = newAngle;
    }

    private void ShiftLeft()
    {
        GameObject currentButtonObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        int i;

        for (i = 0; i < radialMenuParts.Count-1; i++)
        {
            if (radialMenuParts[i] == currentButtonObj) break;
        }

        if (i == 0)
        {
            GameObject desiredButton = radialMenuParts[radialMenuParts.Count - 1];
            EventSystem.current.SetSelectedGameObject(desiredButton.transform.Find("Button").gameObject);
        }
        else
        {
            GameObject desiredButton = radialMenuParts[i - 1];
            EventSystem.current.SetSelectedGameObject(desiredButton.transform.Find("Button").gameObject);
        }
    }

    private void ShiftRight()
    {
        GameObject currentButtonObj = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        int i;

        for (i = 0; i < radialMenuParts.Count - 1; i++)
        {
            if (radialMenuParts[i] == currentButtonObj) break;
        }

        if (i == radialMenuParts.Count - 1)
        {
            GameObject desiredButton = radialMenuParts[0];
            EventSystem.current.SetSelectedGameObject(desiredButton.transform.Find("Button").gameObject);
        }
        else
        {
            GameObject desiredButton = radialMenuParts[i + 1];
            EventSystem.current.SetSelectedGameObject(desiredButton.transform.Find("Button").gameObject);
        }
    }

    private void Attack()
    {
        entireRadialMenu = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        playerCombat.SetChosenMoveData(buttonToAttack[EventSystem.current.currentSelectedGameObject.transform.parent.gameObject]);
        playerCombat.isAttacking = true;
        DisableRotation();
        entireRadialMenu.transform.parent.gameObject.SetActive(false);
    }

    public void BringBackMenu()
    {
        EnableRotation();
        GameObject parentTest = entireRadialMenu.transform.parent.gameObject;
        parentTest.SetActive(true);
    }

    public void DisableRotation()
    {
        playerActions.Combat.RotateRight.Disable();
        playerActions.Combat.RotateLeft.Disable();
        foreach (var button in radialMenuParts)
        {
            button.transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void EnableRotation()
    {
        playerActions.Combat.RotateRight.Enable();
        playerActions.Combat.RotateLeft.Enable();

        StartCoroutine(ReenableButtonListeners(0.5f));
    }

    private IEnumerator ReenableButtonListeners(float duration)
    {
        yield return new WaitForSeconds(duration);

        foreach (var button in radialMenuParts)
        {
            button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(Attack);
        }
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        //playerActions.Disable();
    }
}
