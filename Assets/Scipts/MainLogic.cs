using System;
using System.Linq;
using Assets.Scipts;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MainLogic : MonoBehaviour
{
    [Range(1,300)] public int Size;
    
    [Range(0, 100)] public int StartBuilding;

    public BuildingType CurrentBuildingSize;

    public GameObject LandCursorPrefab;

    public GameObject[] Buildings = new GameObject[3];

    private readonly LandCursor[] _cursor;

    private Vector3 _oldMousePosition;

    private readonly List<Building> _buildings;

    private BuildingType _oldBuildingSize;

    private CellsController _cellsController;

    private MeshCollider _land;

    public MainLogic()
    {
        _oldMousePosition = Vector2.zero;
        _buildings = new List<Building>();
        _cursor = new LandCursor[9];
    }
    
    // Use this for initialization
	void Start()
    {
        var land = GameObject.Find("Land");
        land.transform.localScale = new Vector3(Size, Size, 1);
        land.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(Size / 2f, Size / 2f);
        land.transform.position = Size % 2 == 0 ? new Vector3(0.5f, 0, 0.5f) : new Vector3(0, 0, 0);
        
        _land = land.GetComponent<MeshCollider>();

        _cellsController = new CellsController(Size);

        CreateDefaultBuildings(Size, StartBuilding);

        for (var i = 0; i < _cursor.Length; i++)
        {
            var landCursor = (GameObject)Instantiate(LandCursorPrefab, new Vector3(0, 0.005f, 0), Quaternion.AngleAxis(90, new Vector3(1, 0, 0)));
            var cursor = landCursor.GetComponent<LandCursor>();
            cursor.IsActive = false;
            _cursor[i] = cursor;
        }
    }

    private void CreateDefaultBuildings(int size, int startBuilding)
    {
        var countcubes = (int)Math.Round((startBuilding * size * size / 100f), 0, MidpointRounding.AwayFromZero);

        var currentCountcubes = 0;
        while (currentCountcubes < countcubes)
        {
            var typeBuilding = UnityEngine.Random.Range(1, 4);
            var sizeBuilding = typeBuilding * typeBuilding;

            if (currentCountcubes + sizeBuilding > countcubes)
                continue;

            var rndX = UnityEngine.Random.Range(-size / 2, size / 2);
            var rndY = UnityEngine.Random.Range(-size / 2, size / 2);
            var ray = new Vector3(rndX, 0, rndY);

            var result = SetBuilding(ray, (BuildingType)typeBuilding);
            if (!result)
                continue;

            currentCountcubes += sizeBuilding;

            if (currentCountcubes == countcubes)
            {
                break;
            }
        }
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

    private void GetPosition(Vector3 ray, out int x, out int y)
    {
        //проверяем блоки по спирали от центра
        x = (int)Math.Round(ray.x, 0, MidpointRounding.AwayFromZero);
        y = (int)Math.Round(ray.z, 0, MidpointRounding.AwayFromZero);
    }
    
    private bool SetBuilding(Vector3 ray, BuildingType type)
    {
        //проверяем блоки по спирали от центра
        int startX, startY;
        GetPosition(ray, out startX, out startY);

        var x = startX;
        var y = startY;

        // задаем границы движения 
        var minX = x; var maxX = x; // влево вправо
        var minY = y; var maxY = y; // вверх вниз
        var direction = SpiralDirection.Up; // сначала пойдем вверх
        
        var n = (int)type;

        var points = new Point[n * n];

        for (var i = 0; i < n * n; i++)
        {
            var result = _cellsController.CheckFree(x, y);
            if (result == null || !result.Value)
            {
                foreach (var point in points.Where(s => s != null))
                {
                    _cellsController.SetFree(point.X, point.Y);
                }
                return false;
            }

            points[i] = new Point {X = x, Y = y};
           _cellsController.SetBusy(x, y);

            FillSpiralStep(ref x, ref y, ref direction, ref minX, ref minY, ref maxX, ref maxY);
        }

        var position = type == BuildingType.Size2X2 ? new Vector3(startX + 0.5f, 0.5f, startY - 0.5f) : new Vector3(startX, 0.5f, startY);

        var buildingObject = (GameObject)Instantiate(Buildings[(int)type - 1], position, Quaternion.identity);
        var building = buildingObject.GetComponent<Building>();
        building.Points = points;

        if (CurrentBuildingSize != BuildingType.None)
            building.SetShadowMode();

        _buildings.Add(building);

        return true;
    }

    private void ClearCursor()
    {
        foreach (var cursor in _cursor)
        {
            cursor.IsActive = false;
        }
    }

    // Update is called once per frame
	void Update ()
    {
        if (CurrentBuildingSize != _oldBuildingSize)
        {
            ClearCursor();
            
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
        var points = building.Points;
        foreach (var point in points.Where(s => s != null))
        {
            _cellsController.SetFree(point.X, point.Y);
        }

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
        
        var countBuilding = (int)CurrentBuildingSize*(int)CurrentBuildingSize;

        if (delta > 10)
        {           
            _oldMousePosition = mousePosition;

            var ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            
            if (_land.Raycast(ray, out hit, 1000))
            {
                int startX, startY;

                GetPosition(hit.point, out startX, out startY);

                var x = startX;
                var y = startY;

                // задаем границы движения 
                var minX = x; var maxX = x; // влево вправо
                var minY = y; var maxY = y; // вверх вниз
                var direction = SpiralDirection.Up; // сначала пойдем вверх
                
                var any = false;
                for (var i = 0; i < countBuilding; i++)
                {
                    var cursor = _cursor[i];
                    var result = _cellsController.CheckFree(x, y);
                    if (result == null)
                    {
                        any = true;
                        cursor.IsActive = false;
                    }
                    else
                    {
                        cursor.IsActive = true;
                        cursor.IsBusy = !result.Value;
                    }

                    var position = cursor.gameObject.transform.position;
                    cursor.gameObject.transform.position = new Vector3(x, position.y, y);

                    FillSpiralStep(ref x, ref y, ref direction, ref minX, ref minY, ref maxX, ref maxY);
                }

                if (any)
                {
                    foreach (var cursor in _cursor.Where(s => s.IsActive))
                    {
                        cursor.IsBusy = true;
                    }
                }
            }
            else
            {
                ClearCursor();
            }
        }

        if (Input.GetMouseButtonDown(0) &&
            !EventSystem.current.IsPointerOverGameObject() &&
            _cursor.Count(s => s.IsActive && !s.IsBusy) == countBuilding)
        {
            SetBuilding(_cursor[0].gameObject.transform.position, CurrentBuildingSize);

            foreach (var cursor in _cursor.Where(s => s.IsActive))
            {
                cursor.IsBusy = true;
            }
        }
    }
}
