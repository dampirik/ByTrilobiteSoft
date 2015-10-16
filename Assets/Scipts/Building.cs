using System.Collections.Generic;
using Assets.Scipts;
using UnityEngine;

public class Building : MonoBehaviour {

    public BuildingType Size;

    private List<LandCube> _landCubes;

    public void SetLandCubes(LandCube[] cubes)
    {
        _landCubes = new List<LandCube>(cubes.Length);
        _landCubes.AddRange(cubes);

        foreach (var cube in cubes)
        {
            cube.IsBusy = true;
        }
    }

    public void Release()
    {
        if (_landCubes != null)
        {
            foreach (var cube in _landCubes)
            {
                cube.IsBusy = false;
            }
            _landCubes = null;
        }
    }

    public void SetInfoMode()
    {
        var color = gameObject.GetComponent<MeshRenderer>().material.color;

        gameObject.GetComponent<MeshRenderer>().material.color =
            new Color(color.r, color.g, color.b, 1f);
    }

    public void SetShadowMode()
    {
        var color = gameObject.GetComponent<MeshRenderer>().material.color;
        
        gameObject.GetComponent<MeshRenderer>().material.color =
            new Color(color.r, color.g, color.b, 0.33f);
    }
}
