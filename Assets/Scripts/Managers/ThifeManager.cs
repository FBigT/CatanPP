using UnityEngine;
using Assets.Scripts.Utils;

public class ThifeManager : Singleton<ThifeManager>
{
    public BoardGen BoardGen;

    protected override void Awake()
    {
        base.Awake();
    }


    private void Start()
    {
        if (BoardGen == null)
        {
            BoardGen = FindObjectOfType<BoardGen>();
            if (BoardGen == null)
            {
                Debug.LogError("BoardGen not found in the scene. Please ensure it is present.");
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile != null)
                {
                    MoveThief(tile);
                }
            }
        }
    }

    public void MoveThief(HexTile tile)
    {
        //do more garabe here later
        BoardGen.MoveThiefTo(tile);
    }
}
