using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class PlayerPanelUIManager : MonoBehaviour
{
    public static PlayerPanelUIManager Instance { get; private set; }

    [SerializeField] private List<PlayerPanelUI> playerPanels = new();
    [SerializeField] private ColorPalette colorPalette;

    private int currentIndex = 0;
    private int activePlayerCount = 0;

    private Dictionary<string, Color> playerColorMap = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdatePanelStates();
    }

    public void InitializePlayers(List<string> playerNames)
    {
        if (playerNames.Count > 4)
        {
            Debug.LogError("Maximum of 4 players supported.");
            return;
        }

        colorPalette.ResetPalette();
        activePlayerCount = playerNames.Count;
        playerColorMap.Clear();

        for (int i = 0; i < playerPanels.Count; i++)
        {
            if (i < playerNames.Count)
            {
                string name = playerNames[i];

                playerPanels[i].gameObject.SetActive(true);
                playerPanels[i].SetName(name);

                if (colorPalette.TryGetColor(out Color color))
                {
                    playerPanels[i].SetColor(color);
                    playerColorMap[name] = color;
                }
                else
                {
                    Debug.LogWarning("Not enough colors in palette.");
                }
            }
            else
            {
                playerPanels[i].gameObject.SetActive(false);
            }
        }

        currentIndex = 0;
        UpdatePanelStates();
    }

    public void StepForward()
    {
        currentIndex = (currentIndex + 1) % activePlayerCount;
        UpdatePanelStates();
    }

    public void StepBackward()
    {
        currentIndex = (currentIndex - 1 + activePlayerCount) % activePlayerCount;
        UpdatePanelStates();
    }

    private void UpdatePanelStates()
    {
        for (int i = 0; i < playerPanels.Count; i++)
        {
            if (i < activePlayerCount)
                playerPanels[i].SetActive(i == currentIndex);
        }
    }

    public Color GetCurrentPlayerColor()
    {
        return currentIndex < activePlayerCount ? playerPanels[currentIndex].GetColor() : Color.clear;
    }

    public PlayerPanelUI GetCurrentPanel()
    {
        return currentIndex < activePlayerCount ? playerPanels[currentIndex] : null;
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    public Color GetColorByUsername(string username)
    {
        if (playerColorMap.TryGetValue(username, out var color))
            return color;

        Debug.LogWarning($"No color found for player '{username}'");
        return Color.clear;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerPanelUIManager))]
public class PlayerPanelUIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerPanelUIManager manager = (PlayerPanelUIManager)target;

        GUILayout.Space(10);
        GUILayout.Label("Debug Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Step Forward"))
        {
            manager.StepForward();
        }

        if (GUILayout.Button("Step Backward"))
        {
            manager.StepBackward();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField($"Current Index: {manager.GetCurrentIndex()}");
    }
}
#endif
