using UnityEngine;

public static class PlacedBrickCreator
{
    public static GameObject CreateFromBrickObject(BrickData.LocalBrickData brick, bool recalculateMesh = true, Session session = null) {
        return CreateFromAttributes(brick.type, BrickData.CustomVec3.To(brick.pos), BrickData.CustomQuaternion.To(brick.rot), brick.color, recalculateMesh, null, session);
    }

    private static GameObject CreateFromAttributes(string type, Vector3 pos, Quaternion rot, int color, bool recalculateMesh = true, string headClientId = null, Session session = null) {
        if(!type.Contains(" - Placed")) type += " - Placed";
        session = session ?? Session.GetInstance();
        string uuid = BrickId.FetchNewBrickID();
        GameObject brickObject;

        headClientId = headClientId ?? session.ClientID;

        if (headClientId == session.ClientID) {
            brickObject = GameObject.Instantiate(BrickPrefabCache.GetInstance().Get(type), pos, rot);
        } else {
            AvatarManager avatarManager = AvatarManager.GetInstance();
            Transform headTransform = avatarManager.LocalAvatar.head.transform;

            brickObject = GameObject.Instantiate(BrickPrefabCache.GetInstance().Get(type), headTransform);
            brickObject.transform.localPosition = pos;
            brickObject.transform.localRotation = rot;
        }

        BrickAttach newBrickAttach = brickObject.GetComponent<BrickAttach>();

        newBrickAttach.Color = ColorInt.IntToColor32(color);

        BrickUuid brickUuid = brickObject.GetComponent<BrickUuid>();
        brickUuid.uuid = uuid;

        BrickStore.GetInstance().Put(uuid, brickObject);

        BrickMeshRecalculator.GetInstance().AddAttach(newBrickAttach);

        BrickSounds sounds = BrickSounds.GetInstance();

        if (session.GetClientState() != Session.ClientState.Loading && UserSettings.GetInstance().BrickClickSoundsEnabled) {
            if (newBrickAttach.IsOnCarpet()) {
                sounds.PlayBrickCarpetSound(pos);
            } else {
                sounds.PlayBrickSnapSound(pos);
            }
        }

        if(recalculateMesh)
            newBrickAttach.NotifyNearbyBricksToRecalculateMesh();

        return brickObject;
    }

    public static void DestroyBrickObject(GameObject gameObject, Session session = null) {
        BrickDestroyer destroyer = BrickDestroyer.GetInstance();
        session = session ?? Session.GetInstance();

        if (session.GetSessionType() == Session.SessionType.SinglePlayer)
            destroyer.DelayedDestroy(gameObject);
    }
}
