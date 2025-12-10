using UnityEngine;

public class TeleportBeacon : MonoBehaviour
{
    public bool isTaken;
    public GameObject inhabitant;

    private void Update()
    {
        if(inhabitant != null)
            inhabitant.transform.parent = transform;
    }
}
