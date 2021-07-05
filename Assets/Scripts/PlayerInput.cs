using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput current;
    private void Awake()
    {
        current = this;
    }

    [SerializeField]
    private LayerMask FigureLayer;

    [SerializeField]
    private LayerMask TileLayer;

    [SerializeField]
    private GameObject game_manager_object;

    public event Action OnClick;
    public void Click()
    {
        OnClick?.Invoke();
    }

    public event Action OnClickNoWhere;
    public void ClickNoWhere()
    {
        OnClickNoWhere?.Invoke();
    }

    public delegate void ColliderDelegate (Collider collider);

    public event ColliderDelegate OnClickTile;
    public void ClickTile(Collider collider)
    {
        OnClickTile?.Invoke(collider);
    }

    public event ColliderDelegate OnClickFigure;
    public void ClickFigure(Collider collider)
    {
        OnClickFigure?.Invoke(collider);
    }

    public event Action OnCanselButtonDown;
    public void CanselButtonDown()
    {
        OnCanselButtonDown?.Invoke();
    }

    public event Action OnContinueButtonDown;
    public void ContinueButtonDown()
    {
        OnContinueButtonDown?.Invoke();
    }

    public delegate void AxisDelegate2D(float Horizontal, float Vertical);
    public event AxisDelegate2D OnRotateAround;
    public void RotateAround(float horizontal, float vertical)
    {
        OnRotateAround?.Invoke(horizontal, vertical);
    }

    public delegate void AxisDelegate(float axis);
    public event AxisDelegate OnZoom;
    public void Zoom(float scrollWheel)
    {
        OnZoom?.Invoke(scrollWheel);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Click();
            RaycastHit rayHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
                                out rayHit, Mathf.Infinity, FigureLayer))
            {
                ClickFigure(rayHit.collider);
            }
            else if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
                                out rayHit, Mathf.Infinity, TileLayer))
            {
                ClickTile(rayHit.collider);
            }
            else
            {
                ClickNoWhere();
            }
        }
        if (Input.GetButtonDown("Cancel"))
        {
            CanselButtonDown();
        }
        if (Input.GetButton("Fire2"))
        {
            RotateAround(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            Zoom(scrollWheel);
        }
    }
}
