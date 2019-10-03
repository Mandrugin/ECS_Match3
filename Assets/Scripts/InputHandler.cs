﻿using ECS.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private int Width;
    private int Height;

    public Camera Camera;
    public GameObject Quad;
    public BoxCollider2D Collider;

    private Vector2 _borderSize;
    private Vector2 _rectSize;

    public SettingsConverter SettingsConverter;
    
    private void Start()
    {
        Width = SettingsConverter.Width;
        Height = SettingsConverter.Height;
        
        Quad.transform.localScale = new Vector3(Width, Height, 1);
        Collider.size = new Vector2(Width, Height);

        Camera.orthographicSize = Height / 2.0f + 1;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        var position = ScreenToCell(eventData.position);
        var entityManager = World.Active.EntityManager;
        var entity = entityManager.CreateEntity(typeof(ClickedComponent));
        entityManager.SetComponentData(entity, new ClickedComponent{x = position.x, y = position.y});
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
