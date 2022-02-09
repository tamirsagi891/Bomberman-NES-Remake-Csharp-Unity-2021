using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;

    // follows the player accordingly
    private void FixedUpdate()
    {
        if (target == null) return;

        if (!(target.transform.position.x > 0.5f) || !(target.transform.position.x < 15.5f)) return;
        offset.x = target.transform.position.x;
        transform.position = offset;
    }
}