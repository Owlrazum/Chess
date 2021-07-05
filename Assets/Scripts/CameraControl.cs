using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private GameObject target;


    private Vector3 initialUp;
    private Vector3 initialRight;
    void Start()
    {
        PlayerInput.current.OnRotateAround += UpdateRotation;
        PlayerInput.current.OnZoom += Zoom;

        initialUp = transform.up;
        initialRight = transform.right;
    }

    private void Update()
    {
        GetComponent<Skybox>().material.SetFloat("_Rotation", Time.time * 1.23f); 
    }

    private void UpdateRotation(float horizontal, float vertical)
    {
        transform.RotateAround(target.transform.position, initialRight, vertical* 0.2f);
        transform.RotateAround(target.transform.position, transform.up, horizontal * -0.2f);
    }

    private void Zoom(float value)
    {
        Vector3 direction = target.transform.position - transform.position;
        transform.position += direction * value * 0.2f;
    }
}
