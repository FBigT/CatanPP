using UnityEngine;

public interface IDragable
{
    void OnDragStart();
    void OnDrag(Vector3 position);
    void OnDragEnd();
}
