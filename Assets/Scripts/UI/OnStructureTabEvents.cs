using UnityEngine;
using UnityEngine.UIElements;

public class OnStructureTabEvents : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button houseButton;
    private Button roadButton;
    private Button tallHouseButton;

    public GameObject housePrefab;
    public GameObject roadPrefab;
    public GameObject tallHousePrefab;

    private GameObject selectedPrefab;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        houseButton = uiDocument.rootVisualElement.Q<Button>("HouseButton");
        roadButton = uiDocument.rootVisualElement.Q<Button>("RoadButton");
        tallHouseButton = uiDocument.rootVisualElement.Q<Button>("TallHouseButton");

        houseButton.clicked += () => SelectStructure(housePrefab);
        roadButton.clicked += () => SelectStructure(roadPrefab);
        tallHouseButton.clicked += () => SelectStructure(tallHousePrefab);
    }

    private void OnDisable()
    {
        houseButton.clicked -= () => SelectStructure(housePrefab);
        roadButton.clicked -= () => SelectStructure(roadPrefab);
        tallHouseButton.clicked -= () => SelectStructure(tallHousePrefab);
    }

    private void SelectStructure(GameObject structurePrefab)
    {
        selectedPrefab = structurePrefab;
        Debug.Log($"Selected: {selectedPrefab.name}");
    }

    public GameObject GetSelectedStructure()
    {
        return selectedPrefab;
    }
}
