using UnityEngine;

public class MouseClickManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                var building = hit.collider.GetComponent<BuildingClickHandler>();
                if (building != null)
                {
                    building.OnClicked();
                }
            }
        }
    }
}