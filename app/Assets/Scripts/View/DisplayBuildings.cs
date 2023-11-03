using Model;
using UnityEngine;
using Utils.Injection;

public class DisplayBuildings : InjectableBehaviour
{
    [Inject] private BuildingsModel _model;

    private void Start()
    {
        _model.Updated.Add(OnModelUpdated);
        OnModelUpdated();
    }

    private void OnModelUpdated()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (var building in _model.Get()) 
        {
            var conf = _model.GetConfig(building.Id);
            var centerPos = new Vector3(building.X + conf.width / 2, 0, building.Y + conf.height / 2)
                            * DisplayPlacement.CellSize;

            var obj = Instantiate(Resources.Load<GameObject>(conf.prefab), transform);
            obj.transform.localPosition = centerPos;
            obj.GetComponent<ShowInfo>().Data = building;
        }
    }

    private void OnDestroy()
    {
        _model.Updated.Remove(OnModelUpdated);
    }
}