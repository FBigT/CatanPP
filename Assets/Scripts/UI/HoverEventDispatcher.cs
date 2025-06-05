using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEventDispatcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private List<IOnHoverHandler> hoverHandlers = new List<IOnHoverHandler>();

    private void Awake()
    {
        GetComponents(hoverHandlers);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var handler in hoverHandlers)
            handler.OnHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var handler in hoverHandlers)
            handler.OnHoverExit();
    }

}
