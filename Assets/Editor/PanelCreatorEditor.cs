using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelCreatorEditor : EditorWindow
{
    private string panelName = "NewPanel";
    private Sprite panelImage;
    private float panelWidth = 50f;
    private float panelHeight = 50f;

    private TextAnchor textAlignment = TextAnchor.MiddleRight;

    [MenuItem("Tools/UI Panel Creator")]
    public static void ShowWindow()
    {
        GetWindow<PanelCreatorEditor>("UI Panel Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Create UI Panel", EditorStyles.boldLabel);
        panelName = EditorGUILayout.TextField("Panel Name", panelName);
        panelImage = (Sprite)EditorGUILayout.ObjectField("Panel Image", panelImage, typeof(Sprite), false);

        panelWidth = EditorGUILayout.FloatField("Panel Width", panelWidth);
        panelHeight = EditorGUILayout.FloatField("Panel Height", panelHeight);

        textAlignment = (TextAnchor)EditorGUILayout.EnumPopup("Text Position", textAlignment);

        if (GUILayout.Button("Create Panel"))
        {
            CreatePanel(panelName, panelImage, panelWidth, panelHeight, textAlignment);
        }
    }

    void CreatePanel(string name, Sprite image, float width, float height, TextAnchor alignment)
    {
        string rootName = ToCamelCase(name) + "Panel";
        GameObject panelGO = new GameObject(rootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(width, height);

        // Remove background sprite — keep default Image component without assigning a sprite
        Image img = panelGO.GetComponent<Image>();
        img.sprite = null;
        img.color = new Color(1f, 1f, 1f, 0f); // Fully transparent background

        // Parent to selected object if it's under a Canvas
        GameObject selected = Selection.activeGameObject;
        if (selected != null && selected.GetComponentInParent<Canvas>() != null)
        {
            panelGO.transform.SetParent(selected.transform, false);
        }
        else
        {
            Debug.LogWarning("No valid parent selected with a Canvas. Creating under root.");
            panelGO.transform.SetParent(null, false);
        }

        HorizontalOrVerticalLayoutGroup layout = panelGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = alignment;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 10, 10);

        // Image Container
        GameObject imageGO = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageGO.transform.SetParent(panelGO.transform, false);
        RectTransform imgRT = imageGO.GetComponent<RectTransform>();
        imgRT.sizeDelta = new Vector2(height - 20, height - 20);

        Image imgComponent = imageGO.GetComponent<Image>();
        if (image != null)
        {
            imgComponent.sprite = image;
            imgComponent.preserveAspect = true;
        }

        // Text Object with default "0" text
        string textName = ToCamelCase(name) + "Text";
        GameObject textGO = new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panelGO.transform, false);
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(width / 2f, height - 20);

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "0"; // Default text
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 32;

        Selection.activeGameObject = panelGO;
    }



    string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}
