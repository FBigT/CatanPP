using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.MainMenu;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    Transform template;
    Transform entries;

    SessionManager sessionService;
    
    void Awake()
    {
        sessionService = this.AddComponent<SessionManager>();
        template = transform.Find("tableEntryTemplate");
        entries = transform.Find("entries");
        template.gameObject.SetActive(false);

        PrintEntries(new List<SessionSave>() { new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now), new("Primjer", 1, System.DateTime.Now) });
    }

    public void PrintEntries(IEnumerable<SessionSave> saves) {

        for (int i = 0; i < saves.Count(); i++)
        {
            Transform entry = Instantiate(template, entries);
            Transform buttonHolder = entry.Find("ButtonHolder");

            var currentEntry = entry;

            entry.Find("saveName").GetComponent<TMP_Text>().text = saves.ElementAt(i).SaveName;
            entry.Find("turnNumber").GetComponent<TMP_Text>().text = saves.ElementAt(i).TurnNumber.ToString();
            entry.Find("date").GetComponent<TMP_Text>().text = saves.ElementAt(i).DateTime.ToString();

            buttonHolder.Find("btnLoad").GetComponent<Button>().onClick.AddListener(() => LoadGameSave(currentEntry));
            buttonHolder.Find("btnDelete").GetComponent<Button>().onClick.AddListener(() => DeleteGameSave(currentEntry));

            currentEntry.gameObject.SetActive(true);
        }
    }

    private void LoadGameSave(Transform transform)
    {
        //Loads save
    }

    private void DeleteGameSave(Transform transform)
    {
        long.TryParse(transform.Find("id").GetComponent<TMP_Text>().text, out long id);
        sessionService.DeleteSessionSave(id);

        Transform buttonHolder = transform.Find("ButtonHolder");

        var btnLoad = buttonHolder.Find("btnLoad").GetComponent<Button>();
        var btnDelete = buttonHolder.Find("btnDelete").GetComponent<Button>();
        btnLoad.interactable = false;
        btnDelete.interactable = false;

        btnLoad.onClick.RemoveAllListeners();
        btnDelete.onClick.RemoveAllListeners();

        transform.gameObject.SetActive(false);

        Destroy(transform.gameObject, 0.1f);
    }
}
