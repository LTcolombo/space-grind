using System.Collections;
using System.Threading.Tasks;
using Service;
using Unity.VisualScripting;
using UnityEngine;
using Utils.Injection;

public class RequestTurnEnd : InjectableBehaviour
{
    [Inject] private SettlementService _service;

    public async void SubmitTurn()
    {
        await _service.SubmitTurn();

        StartCoroutine(ReloadDataWithDelay());
    }

    IEnumerator ReloadDataWithDelay()
    {
        yield return new WaitForSeconds(1);
        _ = _service.ReloadData();
    }
}
