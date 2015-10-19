using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class Сombine : MonoBehaviour
{
    //http://gamesetup.ru/topic/259.html
    //http://gamesmaker.ru/3d-game-engines/unity3d/optimizaciya-proizvoditelnosti-v-unity3d/
    // Use this for initialization
    void Start()
    {
        var meshFilters = GetComponentsInChildren<MeshFilter>();

        var combine = new CombineInstance[meshFilters.Length];

        for (var i = 0; i < meshFilters.Length; i++)
        {
            var mf = meshFilters[i];
            combine[i].mesh = mf.sharedMesh;
            combine[i].transform = mf.transform.localToWorldMatrix;
        }

        var newMf = transform.gameObject.AddComponent<MeshFilter>();
        newMf.mesh.CombineMeshes(combine);

        transform.gameObject.GetComponent<Renderer>().sharedMaterial =
            transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;

        for (var j = 0; j < meshFilters.Length; j++)
            transform.GetChild(j).GetComponent<Renderer>().enabled = false;
    }
}
