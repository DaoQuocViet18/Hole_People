using UnityEngine;

public class PeopleTouch : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == gameObject.tag && 
            collision.collider.gameObject.layer == LayerMask.NameToLayer("Hole"))
            gameObject.SetActive(false);
    }
}
