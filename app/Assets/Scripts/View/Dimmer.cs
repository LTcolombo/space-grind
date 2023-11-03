using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Dimmer : MonoBehaviour
{
    public static bool Visible;
    private Image _img;

    void Start()
    {
        _img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        var color = _img.color;

        color.a += ((Visible ? 1 : 0) - color.a) * 5 * Time.deltaTime; 
        
        _img.color = color;
    }
}
