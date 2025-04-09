using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    Transform template;
    Transform entries;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
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

            entry.Find("saveName").GetComponent<TMP_Text>().text = saves.ElementAt(i).SaveName;
            entry.Find("turnNumber").GetComponent<TMP_Text>().text = saves.ElementAt(i).TurnNumber.ToString();
            entry.Find("date").GetComponent<TMP_Text>().text = saves.ElementAt(i).DateTime.ToString();
            //buttonHolder.Find("btnLoad").GetComponent<Button>().onClick.AddListener(() => LoadGameEntry(entry));
            entry.gameObject.SetActive(true);
        }
    }

    private void LoadGameEntry(Transform transform)
    {
        //Loads save
    }
}
