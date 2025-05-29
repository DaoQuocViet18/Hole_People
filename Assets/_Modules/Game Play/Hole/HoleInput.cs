using UnityEngine;

public class HoleInput : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Chuột trái
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("Người chơi đã nhấn vào goal!");
                }
            }
        }
    }
}
