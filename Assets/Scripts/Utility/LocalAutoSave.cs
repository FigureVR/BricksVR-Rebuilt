using static Session.SessionType;
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// MetaObjects/LocalAutoSave
/// </summary>
public class LocalAutoSave : MonoBehaviour {
    private UserSettings settings;
    private SessionManager manager;
    private Coroutine routine;
    private int wait = 10;

    public TextMeshProUGUI text;
    
    private void Start()
    {
        manager = SessionManager.GetInstance();
        settings = UserSettings.GetInstance();

        Session.SessionStart += SessionDidStart;
        Session.SessionEnd += SessionDidStart;
    }

    private void SessionDidStart(Session session) {
        if (session.GetSessionType() == Tutorial) return;

        routine = StartCoroutine(AutoSave());
        settings.AutosaveUpdated.AddListener((int number) => {
            wait = number * 60;
            text.text = $"{number} {(number == 1 ? "Minute": "Minutes")}";
        });
    }

    private void SessionDidEnd(Session _) {
        StopCoroutine(routine);
    }

    private IEnumerator AutoSave() {
        while(settings.AutoSave != 0) {
            LocalSessionLoader.SaveRoom(manager.session.saveDirectory);
            yield return new WaitForSeconds(wait);
        }
    }
}
