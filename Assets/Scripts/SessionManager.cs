using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager {

    public List<PlayerController> playerCtrList = new List<PlayerController>();

    public void registerPlayerController(PlayerController controller)
    {
        lock (((System.Collections.ICollection)playerCtrList).SyncRoot)
        {
            if (!playerCtrList.Contains(controller))
            {
                playerCtrList.Add(controller);
            }
        }

    }

    public void removePlayerController(PlayerController controller)
    {
        lock (((System.Collections.ICollection)playerCtrList).SyncRoot)
        {
            if (playerCtrList.Contains(controller))
            {
                playerCtrList.Remove(controller);
            }
        }
    }
}
