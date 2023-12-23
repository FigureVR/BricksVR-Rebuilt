using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMenuManager : MonoBehaviour
{
    public AvatarManager avatarManager;
    public GameObject playerEntryPrefab;

    public List<PlayerAvatar> users = new List<PlayerAvatar>();

    public GameObject listParent;
    private Transform _listParentTransform;

    // Start is called before the first frame update
    void Awake()
    {
        _listParentTransform = listParent.transform;
    }

    private void OnEnable()
    {
        avatarManager.AvatarCreated += UserJoined;
        avatarManager.AvatarDestroyed += UserQuit;

        RefreshPlayerList();
    }

    private void OnDisable()
    {
        avatarManager.AvatarCreated -= UserJoined;
        avatarManager.AvatarDestroyed -= UserQuit;
    }

    private void RefreshPlayerList()
    {
        RepopulateUserList();
        RebuildUI();
    }

    private void RepopulateUserList()
    {
        users = avatarManager.GetAvatars().OrderBy(avatar => !avatar.isLocal).ToList();
    }

    private void RebuildUI()
    {
        foreach (Transform t in _listParentTransform)
            Destroy(t.gameObject);

        foreach (PlayerAvatar avatar in users) {
            GameObject newPlayerEntry = Instantiate(playerEntryPrefab, _listParentTransform);
            PlayerListItem playerListItem = newPlayerEntry.GetComponent<PlayerListItem>();
            playerListItem.Initialize(avatar);
        }
    }

    private void UserJoined(PlayerAvatar avatar)
    {
        RefreshPlayerList();
    }

    private void UserQuit(PlayerAvatar avatar)
    {
        RefreshPlayerList();
    }
}
