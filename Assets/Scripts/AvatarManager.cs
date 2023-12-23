using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// MetaObjects/Session
/// </summary>
public class AvatarManager : MonoBehaviour {
    private static AvatarManager _instance;

    public GameObject prefab;
    public Session session;

    public static AvatarManager GetInstance() {
        return _instance = _instance != null ? _instance : FindObjectOfType<AvatarManager>();
    }

    public PlayerAvatar LocalAvatar {
        get {
            return Avatars[session.ClientID];
        }
    }

    private readonly Dictionary<string, PlayerAvatar> Avatars = new Dictionary<string, PlayerAvatar>();

    public event Action<PlayerAvatar> AvatarCreated;
    public event Action<PlayerAvatar> AvatarDestroyed;

    public void Awake() {
        CreateAvatar(session.ClientID);
        Avatars.Clear();
    }

    public PlayerAvatar CreateAvatar(string id) {
        PlayerAvatar avatar = Instantiate(prefab).GetComponent<PlayerAvatar>();
        Avatars.Add(id, avatar);

        return avatar;
    }

    public PlayerAvatar GetAvatar(string id) {
        bool exists = Avatars.TryGetValue(id, out PlayerAvatar avatar);
        if (!exists) return null;

        return avatar;
    }

    public Dictionary<string, PlayerAvatar>.ValueCollection GetAvatars() {
        return Avatars.Values;
    }
}
