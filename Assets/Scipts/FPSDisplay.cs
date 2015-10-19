using UnityEngine;

public class FPSDisplay : MonoBehaviour 
{
	float deltaTime;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }
    
    private GUIStyle _style;
    private Rect _rect;

    void Start()
    {
        int w = Screen.width, h = Screen.height;
        _rect = new Rect(0, 0, w, h * 2f / 100);
    }

    void OnGUI()
    {
        if (_style == null)
        {
            var h = Screen.height;
            _style = new GUIStyle
                         {
                             alignment = TextAnchor.UpperLeft,
                             fontSize = h*2/100,
                             normal = {textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f)}
                         };
        }
        
        var msec = deltaTime * 1000.0f;
        var fps = 1.0f / deltaTime;
        var text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        GUI.Label(_rect, text, _style);
    }
}
