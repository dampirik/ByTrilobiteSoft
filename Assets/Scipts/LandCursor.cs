using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class LandCursor : MonoBehaviour
{
    public Color ColorBusy = Color.red;
    public Color ColorFree = Color.green;

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
        gameObject.GetComponent<MeshRenderer>().material.color = isBusy ? ColorBusy : ColorFree;
    }

    private bool? _isActive;
    public bool IsActive
    {
        get { return _isActive != null && _isActive.Value; }
        set
        {
            if (_isActive == value)
                return;

            _isActive = value;
            gameObject.SetActive(_isActive.Value);
        }
    }

    void Start()
    {
        UpdateColor(IsBusy);
    }
}
