using Assets.Scipts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUILogic : MonoBehaviour
{
    private bool _isShow;

    private GameObject _menuCreate;
    private GameObject _menuInfoBuilding;

    private Building _activeBuilding;

    void Start()
    {
        _menuCreate = GameObject.Find("MenuCreateCube");
        _menuCreate.SetActive(false);

        _menuInfoBuilding = GameObject.Find("InfoMenu");
        _menuInfoBuilding.SetActive(false);

        var input = GameObject.Find("Size").GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(OnSize);
        input.onEndEdit = se;
        
        input = GameObject.Find("StartBuilding").GetComponent<InputField>();
        se = new InputField.SubmitEvent();
        se.AddListener(OnStartBuilding);
        input.onEndEdit = se;
    }

    private void OnSize(string arg)
    {
        var mainLogic = GameObject.Find("SceneLogic").GetComponent<MainLogic>();
        mainLogic.SetSize(arg);
    }
    
    private void OnStartBuilding(string arg)
    {
        var mainLogic = GameObject.Find("SceneLogic").GetComponent<MainLogic>();
        mainLogic.SetStartBuilding(arg);
    }

    public void OnSelectCubeSize()
    {
        MainLogic.CurrentBuildingSize = BuildingType.None;

        if (!_isShow)
        {
            ShowSelectCubeSize();
        }
        else
        {
            HideSelectCubeSize();
        }
    }

    private void ShowSelectCubeSize()
    {
        if (_isShow)
            return;
        
        _isShow = true;
        _menuCreate.SetActive(true);
    }

    private void HideSelectCubeSize()
    {
        if (!_isShow)
            return;
        
        _isShow = false;
        _menuCreate.SetActive(false);
    }

    public void OnCreate(int size)
    {
        HideSelectCubeSize();
        MainLogic.CurrentBuildingSize = (BuildingType) size;
    }

    public void OnBuildingInfo()
    {
        if (_activeBuilding != null)
        {
            var text = GameObject.Find("BuildingInfo").GetComponent<Text>();
            text.text = "Building size ";

            switch (_activeBuilding.Size)
            {
                case BuildingType.Size1X1:
                    text.text += "1x1";
                    break;
                case BuildingType.Size2X2:
                    text.text += "2x2";
                    break;
                case BuildingType.Size3X3:
                    text.text += "3x3";
                    break;
                default:
                    text.text += "unknown";
                    break;
            }
        }
    }

    public void OnBuildingDelete()
    {
        if (_activeBuilding != null)
        {
            var mainLogic = GameObject.Find("SceneLogic").GetComponent<MainLogic>();
            mainLogic.BuildingDelete(_activeBuilding);

            _activeBuilding = null;
            _menuInfoBuilding.SetActive(false);
        }
    }

    public void SetActiveBuilding(Building building)
    {
        _activeBuilding = building;
        _menuInfoBuilding.SetActive(_activeBuilding != null);

        if (_activeBuilding != null)
        {
            _menuInfoBuilding.SetActive(true);
            var text = GameObject.Find("BuildingInfo").GetComponent<Text>();
            text.text = "";
        }
        else if (_menuInfoBuilding.activeInHierarchy)
        {
            var text = GameObject.Find("BuildingInfo").GetComponent<Text>();
            text.text = "";
            _menuInfoBuilding.SetActive(false);
        }

        if (_isShow)
        {
            HideSelectCubeSize();
            MainLogic.CurrentBuildingSize = BuildingType.None;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainLogic.CurrentBuildingSize = BuildingType.None;

            if (_activeBuilding != null)
                SetActiveBuilding(null);
        }

        if (!_isShow)
            return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            //click вне зоны меню
            HideSelectCubeSize();

            MainLogic.CurrentBuildingSize = BuildingType.None;
        }
    }
}
