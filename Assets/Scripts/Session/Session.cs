using SharpRakNet.Network;

using System.Collections;
using System;

using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// MetaObjects/Session
/// </summary>
public class Session : MonoBehaviour {
    public static event Action<Session> SessionStart;
    public static event Action<Session> SessionEnd;

    public ChunkedRenderer chunkedRenderer;
    public TutorialManager tutorialManager;
    public OfflineManager offlineManager;
    public LoadingScreen loadingScreen;
    public OnlineManager onlineManager;

    private SessionInfo info;
    public SessionInfo Info { get => info; }

    // Single player properties
    public string saveDirectory;
    public Toggle toggle;

    // Multi-player properties
    public RaknetClient client;

    private static Session instance;
    public static Session GetInstance() {
        return instance = instance != null ? instance : FindObjectOfType<Session>();
    }

    private string clientID;
    public string ClientID {
        get {
            if (clientID == null)
                clientID = Guid.NewGuid().ToString();

            return clientID;
        }
    }

    private ClientState clientState = ClientState.Disconnected;
    private SessionType sessionType;

    public ClientState GetClientState() { return clientState; }
    public SessionType GetSessionType() { return sessionType; }

    public enum SessionType {
        SinglePlayer,
        MultiPlayer,
        Tutorial
    }

    public enum ClientState {
        Playing,
        Loading,
        Downloading,
        Connecting,
        Disconnected
    }

    public bool CanPlace {
        get {
            switch (sessionType) {
                case SessionType.SinglePlayer:
                    return true;
                case SessionType.MultiPlayer:
                    return true;
                case SessionType.Tutorial:
                    return true;
                default:
                    return false;
            }
        }
    }

    public void Connect(string ip, int port) {
        throw new NotImplementedException();
    }

    public IEnumerator LoadSave(string file) {
        BrickPrefabCache prefabCache = BrickPrefabCache.GetInstance();
        SessionManager manager = SessionManager.GetInstance();
        sessionType = SessionType.SinglePlayer;
        clientState = ClientState.Loading;

        loadingScreen.loadingText.text = $"Status: Generating brick cache...";
        prefabCache.GenerateCache();
        loadingScreen.loadingText.text = $"Status: Reading bricks from file...";

        BrickData.LocalBrickData[] brickData = LocalSessionLoader.ReadSave(file);
        BrickAttach[] createdBricks = new BrickAttach[brickData.Length];

        chunkedRenderer.enabled = false;
        for (int i = 0; i < brickData.Length; i++) {
            try {
                createdBricks[i] = PlacedBrickCreator.CreateFromBrickObject(brickData[i], false)
                    .GetComponent<BrickAttach>();
            } catch (Exception e) {
                Debug.LogError($"Failed To load a brick {brickData[i].type}");
                Debug.LogException(e);
            }

            if ((createdBricks[i] != null) && (i % 8) == 0) {
                yield return null;
                loadingScreen.loadingText.text = $"Status: Loaded {i + 1}/{brickData.Length} bricks...";
            }
        }

        for (int i = 0; i < brickData.Length; i++) {
            if (createdBricks[i] == null)
                continue;

            createdBricks[i].RecalculateEnabledConnectors();
            createdBricks[i].RecalculateRenderedGeometry();

            yield return null;
            loadingScreen.loadingText.text = $"Status: Optimized {i + 1}/{brickData.Length} bricks...";
        }

        chunkedRenderer.enabled = true;

        yield return StartCoroutine(ScreenFadeProvider.Fade(manager.ambientMusic));

        manager.musicPlayer.Pause();

        manager.WarmOtherCaches();
        yield return manager.brickPickerMenu.WarmMenu();
        BrickColorMap.WarmColorDictionary();
        //manager.WarmSpawnerCaches();

        manager.buttonInput.DisableMenuControls();
        
        manager.menuEnvironment.SetActive(false);
        manager.mainEnvironment.SetActive(true);

        manager.movementVignette.WithVignetteDisabled(() => {
            manager.playerControllers.transform.position = manager.gameSpawnPoint.position;
            manager.playerControllers.transform.rotation = manager.gameSpawnPoint.rotation;

            if (!Application.isEditor) return;

            Vector3 pos = manager.playerControllers.transform.position;
            pos.y -= 0.3f;
            manager.playerControllers.transform.position = pos;
        });

        manager.menuBoard.SetActive(false);

        manager.menuLeftHand.SetActive(false);
        manager.menuRightHand.SetActive(false);

        // Some time for things to settle
        yield return new WaitForSeconds(0.25f);

        manager.musicPlayer.Resume();
        manager.joystickLocomotion.enabled = true;

        Settings settings = LocalSessionLoader.ReadSettings(file);
        toggle.isOn = settings.lowGravity;

        yield return StartCoroutine(ScreenFadeProvider.Unfade(manager.ambientMusic, manager._ambientMusicMaxVolume));

        saveDirectory = file;

        clientState = ClientState.Playing;

        SessionStart.Invoke(this);
    }

    public void StartTutorialSession() {
        SessionManager manager = SessionManager.GetInstance();
        manager.joystickLocomotion.enabled = true;
        sessionType = SessionType.Tutorial;
        clientState = ClientState.Playing;
        
        manager.menuLeftHand.SetActive(false);
        manager.menuRightHand.SetActive(false);

        SessionStart.Invoke(this);
    }

    public void EndSession() {
        clientState = ClientState.Disconnected;
        SessionEnd.Invoke(this);
    }

    public static void ClearStartListeners() {
        foreach(Delegate @delegate in SessionStart.GetInvocationList()) {
            SessionStart -= (Action<Session>)@delegate;
        }
    }

    public static void ClearEndListeners() {
        foreach (Delegate @delegate in SessionEnd.GetInvocationList()) {
            SessionStart -= (Action<Session>)@delegate;
        }
    }
}
