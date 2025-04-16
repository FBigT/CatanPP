using System.Collections;
using TMPro;
using UnityEngine;

public class SimpleTextLoading : MonoBehaviour
{
    public TMP_Text loadingText;
    public string baseText = "Finding game";
    public float interval = 0.5f;

    private void Start()
    {
        StartCoroutine(AnimateLoadingText());
    }

    IEnumerator AnimateLoadingText()
    {
        int dotCount = 0;

        while (true)
        {
            loadingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // cycles 0 -> 1 -> 2 -> 3 -> 0
            yield return new WaitForSeconds(interval);
        }
    }
}
