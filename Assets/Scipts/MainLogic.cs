using System;
using System.Linq;
using Assets.Scipts;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainLogic : MonoBehaviour
{
    public static BuildingType CurrentBuildingSize;

    public GameObject LandCube;

    public GameObject[] Buildings = new GameObject[3];
    
    private LandCube[][] _cubes;

    private GameObject _mainCamera;
    private GameObject _gameMenu;
    private GameObject _loadingMenu;

    private LandCube[] _activeCubes;

    private Vector3 _oldMousePosition;

    private int _size;

    private int _startBuilding;

    private Vector3 _startCamera;

    private readonly List<Building> _buildings;

    private BuildingType _oldBuildingSize;

    private bool _isCreate;

    public MainLogic()
    {
        _oldMousePosition = Vector2.zero;
        _size = _startBuilding = 1;
        _buildings = new List<Building>();
    }

    // Use this for initialization
	void Start ()
	{
        _mainCamera = GameObject.Find("Main Camera");

        _gameMenu = GameObject.Find("GameMenu");
        _gameMenu.SetActive(false);

        _loadingMenu = GameObject.Find("LoadingMenu");

        _startCamera = _mainCamera.transform.position;
	}

    public void Reset()
    {
        _loadingMenu.SetActive(true);
        _gameMenu.SetActive(false);

        foreach (var building in _buildings)
        {
            building.Release();
            Destroy(building.gameObject);
        }
        _buildings.Clear();

        if (_cubes != null)
        {
            for (var x = 0; x < _cubes.Length; x++)
            {
                for (var y = 0; y < _cubes[x].Length; y++)
                {
                    var cube = _cubes[x][y];
                    Destroy(cube.gameObject);
                    _cubes[x][y] = null;
                }
                _cubes[x] = null;
            }
            _cubes = null;
        }

        CurrentBuildingSize = BuildingType.None;
        _oldBuildingSize = BuildingType.None;
    }

    public void SetSize(string value)
    {
        int.TryParse(value, out _size);
    }

    public void SetStartBuilding(string value)
    {
        int.TryParse(value, out _startBuilding);
    }

    public void Create()
    {
        Debug.Log(string.Format("Create sizeX: {0} sizeY: {1} startBuilding: {2}", _size, _size, _startBuilding));
        Create(_size, _size, _startBuilding);
    }

    private void Create(int sizeX, int sizeY, int startBuilding)
    {
        if (sizeX < 1 || sizeX > 300 ||
            sizeY < 1 || sizeY > 300 ||
            startBuilding < 0 || startBuilding > 100)
            return;

        _loadingMenu.SetActive(false);
        
        _cubes = new LandCube[sizeX][];

        for (var x = 0; x < sizeX; x++)
        {
            _cubes[x] = new LandCube[sizeY];
        }

        StartCoroutine(CreateDefaultState(sizeX, sizeY, startBuilding));
    }

    private enum SpiralDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private void FillSpiralStep(ref int x, ref int y, ref SpiralDirection direction, ref int minX, ref int minY, ref int maxX, ref int maxY)
    {
        switch (direction)
        {
            case SpiralDirection.Left:
                x -= 1;  // движение влево
                if (x < minX)
                { // проверка выхода за заполненную центральную часть слева
                    direction = SpiralDirection.Up; // меняем направление
                    minX = x; // увеличиваем заполненную часть влево
                }
                break;
            case SpiralDirection.Up:  // движение вверх проверка сверху        
                y -= 1;
                if (y < minY)
                {
                    direction = SpiralDirection.Right;
                    minY = y;
                }
                break;
            case SpiralDirection.Right:  // движение вправо проверка справа
                x += 1;
                if (x > maxX)
                {
                    direction = SpiralDirection.Down;
                    maxX = x;
                }
                break;
            case SpiralDirection.Down:  // движение вниз проверка снизу
                y += 1;
                if (y > maxY)
                {
                    direction = SpiralDirection.Left;
                    maxY = y;
                }
                break;
        }
    }

    IEnumerator CreateDefaultState(int nX, int nY, int startBuilding)
    {
        _isCreate = true;
        //создаем блоки по спирали от центра

        // центр
        var x = nX / 2;
        var y = nY / 2;
        
        //устанавливываем камеру в центр
        _mainCamera.transform.position = new Vector3(_startCamera.x + x, _startCamera.y, _startCamera.z + y);

        // задаем границы движения 
        var minX = x; var maxX = x; // влево вправо
        var minY = y; var maxY = y; // вверх вниз
        var direction = SpiralDirection.Left; // сначала пойдем влево

        var isBlack = false;
        for (var i = 0; i < nX * nY; i++)
        {
            var cube = (GameObject)Instantiate(LandCube, new Vector3(x, 0, y), Quaternion.identity);
            cube.GetComponent<MeshRenderer>().material.color = isBlack ? Color.black : Color.white;
            isBlack = !isBlack;

            var script = cube.GetComponent<LandCube>();
            script.Position = new Vector2(x, y);
            _cubes[x][y] = script;

            FillSpiralStep(ref x, ref y, ref direction, ref minX, ref minY, ref maxX, ref maxY);
        }

        yield return null;

        //заполняем StartBuilding %
        var countcubes = (int)Math.Round((startBuilding * nX * nY / 100f), 0, MidpointRounding.AwayFromZero);

        var currentCountcubes = 0;
	    while (currentCountcubes < countcubes)
	    {
            var sizeBuilding = UnityEngine.Random.Range(1, 4);
            var size = sizeBuilding * sizeBuilding;
            
	        if (currentCountcubes + size > countcubes)
	            continue;

            var rndX = UnityEngine.Random.Range(0, nX);
            var rndY = UnityEngine.Random.Range(0, nY);
	        var ray = new Vector3(rndX, 0, rndY);

	        LandCube[] cubes;
            var result = CheckBuilding(ray, (BuildingType)sizeBuilding, out cubes);
            
	        if (!result)
	            continue;

            SetBuilding(cubes);

	        currentCountcubes += size;

            if (currentCountcubes == countcubes)
                break;
	    }

        _gameMenu.SetActive(true);

        _isCreate = false;
    }

    public bool CheckFree(int x, int y, out LandCube result)
    {
        if (x < 0 || x >= _cubes.Length)
        {
            result = null;
            return false;
        }

        if (y < 0 || y >= _cubes[x].Length)
        {
            result = null;
            return false;
        }

        result = _cubes[x][y];

        if (result == null)
        {
            return false;
        }

        return !_cubes[x][y].IsBusy;
    }

    public bool CheckBuilding(Vector3 ray, BuildingType buildingType, out LandCube[] cubes)
    {
        //проверяем блоки по спирали от центра
        var x = (int)Math.Round(ray.x, 0, MidpointRounding.AwayFromZero);
        var y = (int)Math.Round(ray.z, 0, MidpointRounding.AwayFromZero);
        
        // задаем границы движения 
        var minX = x; var maxX = x; // влево вправо
        var minY = y; var maxY = y; // вверх вниз
        var direction = SpiralDirection.Up; // сначала пойдем вверх

        var n = (int) buildingType;

        cubes = new LandCube[n*n];

        for (var i = 0; i < n * n; i++)
        {
            LandCube cube;
            CheckFree(x, y, out cube);
            cubes[i] = cube;

            FillSpiralStep(ref x, ref y, ref direction, ref minX, ref minY, ref maxX, ref maxY);
        }

        return cubes.All(c => c != null && !c.IsBusy);
    }

    public void SetBuilding(LandCube[] cubes)
    {
        if (cubes == null || cubes.Length<1)
        {
            throw new ArgumentException();
        }

        if(cubes.Any(c => c == null || c.IsBusy))
        {
            throw new ArgumentException();
        }

        var size = (int)Math.Sqrt(cubes.Length);
        if (size <= 0)
        {
            throw new ArgumentException();
        }

        Vector3 position;

        if ((BuildingType) size == BuildingType.Size2X2)
        {
            position = new Vector3(cubes[0].Position.x + 0.5f, 1, cubes[0].Position.y - 0.5f);
        }
        else
        {
            position = new Vector3(cubes[0].Position.x, 1, cubes[0].Position.y);
        }

        var buildingObject = (GameObject)Instantiate(Buildings[size - 1], position, Quaternion.identity);
        var building = buildingObject.GetComponent<Building>();
        building.SetLandCubes(cubes);

        if (CurrentBuildingSize != BuildingType.None)
            building.SetShadowMode();

        _buildings.Add(building);
    }

    private void ClearActiveCubes()
    {
        if (_activeCubes != null)
        {
            foreach (var cube in _activeCubes.Where(c => c != null))
            {
                cube.Deactivate();
            }
            _activeCubes = null;
        }   
    }
    
	// Update is called once per frame
	void Update ()
    {
        if (_isCreate)
            return;

        if (CurrentBuildingSize != _oldBuildingSize)
        {
            ClearActiveCubes();
            
            //changeState
            foreach (var building in _buildings)
            {
                if (CurrentBuildingSize == BuildingType.None)
                {
                    building.SetInfoMode();
                }
                else
                {
                    building.SetShadowMode();
                }
            }
        }

        if (CurrentBuildingSize == BuildingType.None)
        {
            UpdateStateInfo();
	    }
        else
        {
            UpdateStateCreate();
        }

	    _oldBuildingSize = CurrentBuildingSize;
    }

    public void BuildingDelete(Building building)
    {
        building.Release();
        Destroy(building.gameObject);
        _buildings.Remove(building);
    }

    private void UpdateStateInfo()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Building selectBuilding = null;
            if (Physics.Raycast(ray, out hit))
            {
                selectBuilding = hit.transform.gameObject.GetComponent<Building>();
            }

            GameObject.Find("GUI").GetComponent<GUILogic>().SetActiveBuilding(selectBuilding);
        }
    }

    private void UpdateStateCreate()
    {
        var mousePosition = Input.mousePosition;
        var delta = Math.Abs(_oldMousePosition.x - mousePosition.x) + Math.Abs(_oldMousePosition.y - mousePosition.y);

        if (delta > 10)
        {
            ClearActiveCubes();

            _oldMousePosition = mousePosition;

            var ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                CheckBuilding(hit.point, CurrentBuildingSize, out _activeCubes);

                var anyCubes = _activeCubes.Any(c => c == null);

                foreach (var cube in _activeCubes.Where(c => c != null))
                {
                    cube.Activate(anyCubes);
                }
            }
        }

        if (Input.GetMouseButtonDown(0) &&
            _activeCubes != null && _activeCubes.All(c => c != null && !c.IsBusy) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            SetBuilding(_activeCubes);
        }
    }
}
