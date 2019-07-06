using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int Width;
    public int Height;

    public Camera Camera;
    public GameObject Quad;
    public BoxCollider2D Collider;

    private Vector2 _borderSize;
    private Vector2 _rectSize;
    
    private void Start()
    {
        Quad.transform.localScale = new Vector3(Width, Height, 1);
        Collider.size = new Vector2(Width, Height);

        var screenPos = Camera.WorldToScreenPoint(new Vector3(Width / 2, Height / 2, 0));
        _borderSize = new Vector2(Screen.width - screenPos.x, Screen.height - screenPos.y);
        _rectSize = new Vector2(Screen.width - _borderSize.x * 2, Screen.height - _borderSize.y * 2);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var position = ScreenToCell(eventData.position); 
        Debug.Log(position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var position = ScreenToCell(eventData.position); 
        Debug.Log(position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var position = ScreenToCell(eventData.position); 
        Debug.Log(position);
    }

    private Vector2Int ScreenToCell(Vector2 position)
    {
        position.x -= _borderSize.x;
        position.y -= _borderSize.y;

        var verticalCellSize = _rectSize.y / Height;
        var horizontalCellSize = _rectSize.x / Width;

        return new Vector2Int((int)(position.x / horizontalCellSize), (int)(position.y / verticalCellSize));
    }
}
