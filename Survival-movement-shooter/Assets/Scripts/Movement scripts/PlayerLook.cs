using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
	public float sensitivity = 43;
	public Transform orientation;

	private float xRotation;
	private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
    }

    void LateUpdate()
    {
        //get mouse input
		float mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * sensitivity;
		float mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * sensitivity;

		yRotation += mouseX;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		//rotate camera and orientation
		transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
		orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

	
}
