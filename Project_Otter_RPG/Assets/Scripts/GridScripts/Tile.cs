using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] Color enemyColor;
    [SerializeField] Color playerColor;
    private bool characterOn = false;
    private GameObject characterOnTile;
    Image image;

    [SerializeField] private int tileWeight;
    private Vector2Int position;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void init(bool isEnemy)
    {
        image.color = isEnemy ?  enemyColor : playerColor;
    }

    public void SetCharacterOn(bool truthValue)
    {
        characterOn = truthValue;
    }

    public bool GetCharacterOn()
    {
        return characterOn;
    }

    public void SetCharacterOnTile(GameObject character)
    {
        characterOnTile = character;
    }

    public GameObject GetCharacterOnTile()
    {
        return characterOnTile;
    }

    public void SetTileWeight(int amount)
    {
        tileWeight = amount;
    }

    public void AddTileWeight(int addition)
    {
        tileWeight += addition;
    }

    public int GetTileWeight()
    {
        return tileWeight;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector2Int pos)
    {
        position = pos;
    }
}
