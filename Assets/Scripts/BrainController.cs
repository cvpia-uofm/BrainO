using Assets.Func_Area_Model;
//using ExcelFactoryLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using UnityEngine;
using AutoMapperFactory;
using Assets.Models;

public class BrainController : MonoBehaviour
{
    private const float rotateSpeed = 200;

    public delegate void ScalingAction();
    public static event ScalingAction OnBrainScale;

    public delegate void RotationAction(float X, float Y);
    public static event RotationAction OnBrainRotate;

    private void Awake()
    {
       
    }

   

    private void Update()
    {
        Rotation();
        Scaling();
    }

    private void Scaling()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (!gameObject.transform.localScale.Equals(new Vector3(100, 100, 100)))
            {
                gameObject.transform.localScale += new Vector3(1f, 1f, 1f);
            }
            OnBrainScale();
            //gameObject.transform.Translate(Input.mousePosition.x, Input.mousePosition.y, Input.GetAxis("Mouse ScrollWheel") * 30f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (!gameObject.transform.localScale.Equals(new Vector3(40, 40, 40)))
            {
                gameObject.transform.localScale -= new Vector3(1f, 1f, 1f);
            }
            //OnBrainScale();
            // gameObject.transform.Translate(Input.mousePosition.x, Input.mousePosition.y, Input.GetAxis("Mouse ScrollWheel") * 30f);
        }
        
    }

    private void Rotation()
    {
        if (Input.GetMouseButton(0))
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Mathf.Deg2Rad;
            float rotateY = Input.GetAxis("Mouse Y") * rotateSpeed * Mathf.Deg2Rad;
           

            transform.Rotate(Vector3.up, -rotateX);
            transform.Rotate(Vector3.right, -rotateY);

            OnBrainRotate(rotateX, rotateY);
            
        }
    }
}
