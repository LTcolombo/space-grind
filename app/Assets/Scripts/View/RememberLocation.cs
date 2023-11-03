using UnityEngine;
using UnityEngine.SceneManagement;

public class RememberLocation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
     
        var posStr = PlayerPrefs.GetString($"Position_{SceneManager.GetActiveScene().name}");
        if (!string.IsNullOrEmpty(posStr))
            transform.position = JsonUtility.FromJson<Vector3>(posStr);
    }

    // Update is called once per frame
    void OnDestroy()
    {
        PlayerPrefs.SetString($"Position_{SceneManager.GetActiveScene().name}", JsonUtility.ToJson(transform.position));
    }
}
