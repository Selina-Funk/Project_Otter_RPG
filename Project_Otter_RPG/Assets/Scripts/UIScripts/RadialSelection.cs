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

    public List<GameObject> radialMenuParts = new List<GameObject>();


    private void Update()
    {

        Canvas attackCanvas = GameObject.Find("Attack_Canvas").GetComponent<Canvas>();
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(attackCanvas.worldCamera, GameObject.Find("Player_UI").transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(attackCanvas.transform as RectTransform, screenPos, attackCanvas.worldCamera, out Vector2 localPoint);

        //foreach (var part in radialMenuParts)
        //{
        //    part.GetComponent<RectTransform>().anchoredPosition = localPoint;
        //}

        radialMenuCanvas.GetComponent<Transform>().position = GameObject.Find("Player_UI").transform.position;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3 rationalAngle = new Vector3(0, 0, rotationAmount);
            Vector3 currentAngle = radialMenuParts[0].transform.localEulerAngles;
            Vector3 newAngle = currentAngle + rationalAngle;

            Vector3.Lerp(radialMenuParts[0].transform.localEulerAngles, newAngle, 0.5f);
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

            radialMenuParts.Add(spawnedRadialPart);
        }
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
