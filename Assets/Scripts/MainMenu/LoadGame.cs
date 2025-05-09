using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.MainMenu;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;  // for SessionCodeDto
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    [Header("Saved Games")]
    public GameObject template;
    public GameObject entries;

    [Header("Join by Code")]
    public TMP_InputField sessionCodeInput;
    public Button btnJoinSession;
    public TMP_Text joinErrorMessage;

    [Header("Delete Confirmation")]
    public GameObject confirmDialog;
    public Button confirmDelete;

    private SessionManager sessionService;
    private Transform currentDeleteSelection;

    void Awake()
    {
        sessionService = this.AddComponent<SessionManager>();

        // initialize UI
        template.SetActive(false);
        confirmDialog.SetActive(false);
        joinErrorMessage.text = string.Empty;

        // join button listener
        btnJoinSession.onClick.AddListener(() => {
            joinErrorMessage.text = "";
            var code = sessionCodeInput.text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                joinErrorMessage.text = "Please enter a session code.";
                return;
            }
            sessionService.JoinSession(code, OnJoinSuccess, OnJoinError);
        });

        confirmDelete.onClick.AddListener(() => DeleteSave(currentDeleteSelection));

        // load existing saves from backend
        sessionService.GetAllSessionSaves(PrintEntries, error => Debug.LogError("Failed to fetch saves: " + error));
    }

    public void PrintEntries(IEnumerable<SessionSave> saves)
    {
        // clear old entries
        foreach (Transform child in entries.transform)
            if (child != template.transform)
                Destroy(child.gameObject);

        int index = 0;
        foreach (var save in saves)
        {
            var entry = Instantiate(template, entries.transform);
            entry.transform.Find("saveName").GetComponent<TMP_Text>().text = save.SaveName;
            entry.transform.Find("turnNumber").GetComponent<TMP_Text>().text = save.TurnNumber.ToString();
            entry.transform.Find("date").GetComponent<TMP_Text>().text = save.DateTime.ToString();
            entry.transform.Find("id").GetComponent<TMP_Text>().text = save.Id.ToString();

            var buttonHolder = entry.transform.Find("ButtonHolder");
            int idx = index; // capture for closure
            buttonHolder.Find("btnLoad").GetComponent<Button>()
                .onClick.AddListener(() => LoadGameSave(entry.transform));
            buttonHolder.Find("btnDelete").GetComponent<Button>()
                .onClick.AddListener(() => PrepareDeleteSave(entry.transform));

            entry.SetActive(true);
            index++;
        }
    }

    private void OnJoinSuccess(SessionCodeDto dto)
    {
        // persist session info
        LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
        LocalStorageService.SetVariable("session-code", dto.code);

        // load the game scene
        SceneManager.LoadScene("GameModeCampaign");
    }

    private void OnJoinError(string error)
    {
        joinErrorMessage.text = error;
    }

    private void LoadGameSave(Transform entry)
    {
        // loads a saved session by its ID (not covered here)
    }

    private void PrepareDeleteSave(Transform entry)
    {
        currentDeleteSelection = entry;
        confirmDialog.SetActive(true);
    }

    private void DeleteSave(Transform entry)
    {
        if (long.TryParse(entry.Find("id").GetComponent<TMP_Text>().text, out long id))
        {
            sessionService.DeleteSessionSave(id);
        }

        // remove UI entry
        entry.gameObject.SetActive(false);
        Destroy(entry.gameObject, 0.1f);
        confirmDialog.SetActive(false);
    }
}
