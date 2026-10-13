using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class RadialSelection : MonoBehaviour
{
    [SerializeField] private int numberOfRadialSlices = 4;
    [SerializeField] private GameObject radialPart;
    [SerializeField] private Transform radialMenuCanvas;
    [SerializeField] private float angleBetweenPart = 10.0f;
    [SerializeField] private float rotationAmount;
    private Color color1 = Color.blue;
    private Color color2 = Color.yellow;
    private Color color3 = Color.orange;
    private Color color4 = Color.red;


    public List<GameObject> radialMenuParts = new List<GameObject>();


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
            Vector3 newAngle = currentAngle - rotationalAngle;
            if (newAngle.z < 0) newAngle.z += 360;

            radialMenuCanvas.transform.localEulerAngles = newAngle;
        }
    }
    public void SpawnRaidialPart()
    {
        rotationAmount = 360f / numberOfRadialSlices;
        for (int i = 0; i < numberOfRadialSlices; i++)
        {
            float angle = i * 350f / numberOfRadialSlices - angleBetweenPart/2;
            Vector3 radialPartEulerAngle = new Vector3(0, 0, angle);

            GameObject spawnedRadialPart = Instantiate(radialPart, radialMenuCanvas);
            Canvas attackCanvas = GameObject.Find("Attack_Canvas").GetComponent<Canvas>();
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(attackCanvas.worldCamera, radialMenuCanvas.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(attackCanvas.transform as RectTransform, screenPos, attackCanvas.worldCamera, out Vector2 localPoint);

            spawnedRadialPart.GetComponent<RectTransform>().anchoredPosition = localPoint;
            spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle;

            spawnedRadialPart.GetComponent<Image>().fillAmount = 1.0f / (float)numberOfRadialSlices - (angleBetweenPart/360);

            if (radialMenuParts.Count == 0)
            {
                spawnedRadialPart.GetComponent<Image>().color = color1;
            }
            if (radialMenuParts.Count == 1)
            {
                spawnedRadialPart.GetComponent<Image>().color = color2;
            }
            if (radialMenuParts.Count == 2)
            {
                spawnedRadialPart.GetComponent<Image>().color = color3;
            }
            if (radialMenuParts.Count == 3)
            {
                spawnedRadialPart.GetComponent<Image>().color = color4;
            }

            radialMenuParts.Add(spawnedRadialPart);
        }

        radialMenuCanvas.transform.localEulerAngles = new Vector3(0.0f, 0.0f, -135.0f);
    }

    public void MoveRadialMenu()
    {
        foreach(var part in radialMenuParts)
        {
            Canvas attackCanvas = GameObject.Find("Attack_Canvas").GetComponent<Canvas>();
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(attackCanvas.worldCamera, radialMenuCanvas.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(attackCanvas.transform as RectTransform, screenPos, attackCanvas.worldCamera, out Vector2 localPoint);

            part.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }
    }
}
