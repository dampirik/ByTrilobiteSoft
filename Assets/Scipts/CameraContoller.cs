using UnityEngine;

public class CameraContoller : MonoBehaviour
{
    public float CameraSpeed = 1f;
    
	// Update is called once per frame
	void Update ()
	{
	    var moveHorizontal = Input.GetAxis("Horizontal");
	    var moveVertical = Input.GetAxis("Vertical");

	    var position = gameObject.transform.position;

        var speed = CameraSpeed * Time.deltaTime;

        gameObject.transform.position = new Vector3(position.x + moveHorizontal * speed, position.y, position.z + moveVertical * speed);
	}
}
