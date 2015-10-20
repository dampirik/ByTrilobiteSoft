using UnityEngine;
using Assets.Scipts;

[RequireComponent(typeof(MeshRenderer))]
public class Building : MonoBehaviour
{
    public BuildingType Size;

    public Point[] Points { get; set; }
    
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
