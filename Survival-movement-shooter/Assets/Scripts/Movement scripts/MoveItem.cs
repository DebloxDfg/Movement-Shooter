using UnityEngine;

public class MoveItem : MonoBehaviour
{
    public Transform itemPosition;
    void Update()
    {
        transform.position = itemPosition.position;
        transform.rotation = itemPosition.rotation;
    }
}
