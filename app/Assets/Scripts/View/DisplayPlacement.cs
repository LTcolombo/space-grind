using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Model;
using Model;
using Service;
using UnityEngine;
using Utils;
using Utils.Injection;

public class DisplayPlacement : InjectableBehaviour
{
    [Inject] private InteractionModel _interaction;
    [Inject] private BuildingsModel _buildings;
    [Inject] private SettlementService _settlement;

    public GameObject gridElement;

    private GameObject _preview;

    public const int CellSize = 2;

    public Material cell;
    public Material blocked;
    public Material available;
    private GameObject[,] _cells;

    private IEnumerator Start()
    {
        _interaction.Updated.Add(OnModeUpdated);

        yield return new WaitForSeconds(1);
    }

    private void OnModeUpdated()
    {
        if (_interaction.Get() == InteractionState.Building)
        {
            if (_cells == null)
            {
                _cells = new GameObject[_buildings.OccupiedData.GetLength(0), _buildings.OccupiedData.GetLength(1)];
            }

            for (var i = 0; i < _buildings.OccupiedData.GetLength(0); i++)
            for (var j = 0; j < _buildings.OccupiedData.GetLength(1); j++)
            {
                if (_cells[i, j] == null)
                {
                    var obj = Instantiate(gridElement, transform, true);
                    obj.transform.localPosition = new Vector3((i + 0.5f) * CellSize, 0.5f, (j + 0.5f) * CellSize);
                    obj.transform.localScale *= CellSize;
                    _cells[i, j] = obj;
                }
                else
                    _cells[i, j].SetActive(true);
            }

            _preview = Instantiate(Resources.Load<GameObject>(_buildings.Current.prefab), transform);
            _preview.gameObject.GetComponentInChildren<ShowInfo>().enabled = false;
            foreach (var mesh in _preview.GetComponentsInChildren<MeshRenderer>())
                mesh.material = cell;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_buildings.Current != null && _interaction.Get() == InteractionState.Building)
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (transform.GetComponent<Collider>().bounds.IntersectRay(mouseRay, out var distance))
            {
                var intersectionPoint = mouseRay.origin + mouseRay.direction * distance;
                var localPoint = transform.InverseTransformPoint(intersectionPoint);
                var snappedPosition = localPoint.Snap(CellSize);


                var state = available;
                var buildingDimensions = new Vector3(_buildings.Current.width, 0, _buildings.Current.height);

                var cellPos = snappedPosition / CellSize - buildingDimensions/2;

                var cells = new List<GameObject>();

                for (var i = (int)cellPos.x; i < (int)cellPos.x + buildingDimensions.x; i++)
                for (var j = (int)cellPos.z; j < (int)cellPos.z + buildingDimensions.z; j++)
                {
                    if (i < 0 || i >= _buildings.OccupiedData.GetLength(0) || j < 0 ||
                        j >= _buildings.OccupiedData.GetLength(1))
                    {
                        state = blocked;
                        break;
                    }

                    cells.Add(_cells[i, j]);

                    if (_buildings.OccupiedData[i, j] != 0)
                    {
                        state = blocked;
                        break;
                    }
                }


                if (Input.GetMouseButtonDown(0) && state == available)
                {
                    PutBuilding(cellPos);
                    return;
                }

                if (_preview == null)
                    return;

                for (var i = 0; i < _cells.GetLength(0); i++)
                for (var j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j].GetComponent<MeshRenderer>().material =
                        cells.Contains(_cells[i, j])
                            ? state
                            : _buildings.OccupiedData[i, j] == 0
                                ? cell
                                : blocked;
                }

                _preview.transform.localPosition = snappedPosition;
            }
        }
    }

    async void PutBuilding(Vector3 cellPos)
    {
        Destroy(_preview);
        if (!await _settlement.PlaceBuilding((byte)cellPos.x, (byte)cellPos.z, (byte)_buildings.CurrentId))
            Debug.LogError("cant place building!");

        for (var i = 0; i < _cells.GetLength(0); i++)
        for (var j = 0; j < _cells.GetLength(1); j++)
        {
            _cells[i, j].SetActive(false);
        }

        _interaction.Set(InteractionState.Walking);
        await Bootstrap.DelayAsync(2);
        await _settlement.ReloadData();
    }

    private void OnDestroy()
    {
        _interaction.Updated.Remove(OnModeUpdated);
    }
    
    
}