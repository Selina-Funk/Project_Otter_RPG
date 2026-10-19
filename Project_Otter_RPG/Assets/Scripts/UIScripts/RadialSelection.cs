using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialSelection : MonoBehaviour
{
    [Header("Radial button Information")]
    [SerializeField] private int numberOfRadialSlices = 4;
    [SerializeField] private GameObject radialPart;
    [SerializeField] private Transform radialMenuCanvas;
    [SerializeField] private float angleBetweenPart = 10.0f;
    [SerializeField] private float rotationAmount;

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


    private void Update()
    {

        Canvas attackCanvas = GameObject.Find("Attack_Canvas").GetComponent<Canvas>();
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(attackCanvas.worldCamera, GameObject.Find("Player_UI").transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(attackCanvas.transform as RectTransform, screenPos, attackCanvas.worldCamera, out Vector2 localPoint);

        radialMenuCanvas.GetComponent<Transform>().position = GameObject.Find("Player_UI").transform.position;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector3 rationalAngle = new Vector3(0, 0, rotationAmount);
            Vector3 currentAngle = radialMenuCanvas.transform.localEulerAngles;
            Vector3 newAngle = currentAngle + rationalAngle;
            if (newAngle.z > 360) newAngle.z -= 360;

            radialMenuCanvas.transform.localEulerAngles = newAngle;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector3 rotationalAngle = new Vector3(0, 0, -rotationAmount);
            Vector3 currentAngle = radialMenuCanvas.transform.localEulerAngles;
            Vector3 newAngle = currentAngle + rotationalAngle;
            if (newAngle.z < 0) newAngle.z += 360;

            radialMenuCanvas.transform.localEulerAngles = newAngle;
        }
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
            radialButton.colors = radialColorBlock;
            radialMenuParts.Add(spawnedRadialPart);
        }
        SetButtonNavigation();
        GameObject buttonObj = radialMenuParts[0].transform.Find("Button").gameObject;
        EventSystem.current.SetSelectedGameObject(buttonObj);

        radialMenuCanvas.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -135.0f);
    }

    private void SetButtonNavigation()
    {
        int i;
        for (i = 0; i < radialMenuParts.Count; i++)
        {
            GameObject buttonObj = radialMenuParts[i].transform.Find("Button").gameObject;
            Button radialButton = buttonObj.GetComponent<Button>();
            Navigation buttonNav = radialButton.navigation;
            if (i == 0)
            {
                buttonNav.selectOnRight = radialMenuParts[(radialMenuParts.Count - 1)].transform.Find("Button").GetComponent<Button>();
            }
            else
            {
                buttonNav.selectOnRight = radialMenuParts[i-1].transform.Find("Button").GetComponent<Button>();
            }

            if (i == (radialMenuParts.Count - 1))
            {
                buttonNav.selectOnLeft = radialMenuParts[0].transform.Find("Button").GetComponent<Button>();
            }
            else
            {
                buttonNav.selectOnLeft = radialMenuParts[i+1].transform.Find("Button").GetComponent<Button>();
            }
            radialButton.navigation = buttonNav;
        }
    }
}
