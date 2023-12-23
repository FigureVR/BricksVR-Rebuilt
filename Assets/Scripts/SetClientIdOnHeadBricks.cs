using UnityEngine;

public class SetClientIdOnHeadBricks : MonoBehaviour {
    // Add multiplayer support later.
    void Start() {
        Session session = Session.GetInstance();

        if (session.GetSessionType() == Session.SessionType.MultiPlayer) throw new System.Exception("Multi-player not supported");

        string clientId = Session.GetInstance().ClientID;

        BrickAttach[] attaches = GetComponentsInChildren<BrickAttach>();
        foreach (BrickAttach attach in attaches)
            attach.headClientId = clientId;
    }
}
