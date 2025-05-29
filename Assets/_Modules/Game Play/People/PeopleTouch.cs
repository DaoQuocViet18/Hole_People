using UnityEngine;

public class PeopleTouch : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == gameObject.tag)
            gameObject.SetActive(false);
    }
}
