using UnityEngine;

public class LandCube : MonoBehaviour
{
    public Color ColorBusy = Color.red;
    public Color ColorFree = Color.green;

    private bool _isActive;
    private bool _isBusy;

    public bool IsBusy
    {
        get { return _isBusy; }
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            UpdateColor(_isBusy);
        }
    }

    private void UpdateColor(bool isBusy)
    {
        if (_isActive)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = isBusy ? ColorBusy : ColorFree;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material.color = _startColor;
        }
    }

    public Vector2 Position;

    private Color _startColor;

    // Use this for initialization
	void Start ()
	{
        _startColor = gameObject.GetComponent<MeshRenderer>().material.color;
	}

    public void Deactivate()
    {
        _isActive = false;
        UpdateColor(_isBusy);
    }

    public void Activate(bool tmpBusy = false)
    {
        _isActive = true;
        UpdateColor(tmpBusy || _isBusy);
    }
}
