using System;
using Model;
using UnityEngine;
using Utils.Injection;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Collider))]
public class ShowInfo : InjectableBehaviour
{
    [Inject] private BuildingsModel _buildings;

    private Collider _selectionCollider;
    private Camera _camera;

    [SerializeField] private BuildingInfo _info;
    [SerializeField] private BuildingInfo _infoInstance;
    public Settlement.Types.Building Data;

    void Start()
    {
        _camera = Camera.main;

        _infoInstance = Instantiate(_info, GameObject.Find("HUD").transform);

        _selectionCollider = GetComponent<Collider>();
    }


    void Update()
    {
        var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
        if (_selectionCollider != null)
        {
            _infoInstance.gameObject.SetActive(_selectionCollider.bounds.IntersectRay(mouseRay, out _));
            _infoInstance.SetData(Data, _buildings.GetConfig(Data.Id));
            _infoInstance.transform.position = Input.mousePosition;
        }
    }

    private void OnDestroy()
    {
        if (_infoInstance != null)
            Destroy(_infoInstance.gameObject);
    }
}