using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the camera's transform and the visibility of the mouse 
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] Game _game;
    [Range(3f,200.0f)]
    [SerializeField] float positionSensibility = 100f;
    [Range(3f,30.0f)]
    [SerializeField] float angleSensibility = 20f;
    Transform _myTransform;
    
    Vector3 _meshCenter;
    
    bool _inputPressed = false;
    public bool InputPressed
    {
        get { return _inputPressed; }
    }

    void Awake()
    {
        _myTransform = transform;
        _meshCenter = new Vector3(Map.width,Map.depth,-Map.height);
    }
    
    
   /// <summary>
   /// Moves transform using inputs
   /// </summary>
    void UpdateTransformByInput()
    {
        //Set camera position using axises
        if (Input.GetAxis("Horizontal") > .5f)
        {
            _myTransform.position += _myTransform.right * Time.deltaTime * positionSensibility;
        }
        else if (Input.GetAxis("Horizontal") < -.5f)
        {
            _myTransform.position -= _myTransform.right * Time.deltaTime * positionSensibility;
        }

        if (Input.GetAxis("Vertical") > .5f)
        {
            _myTransform.position += _myTransform.forward * Time.deltaTime * positionSensibility;
        }
        else if (Input.GetAxis("Vertical") < -.5f)
        {
            _myTransform.position -= _myTransform.forward * Time.deltaTime * positionSensibility;
        }



        //Set camera rotation using delta mouse
        Vector3 deltaMousePosition = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        _myTransform.Rotate(Vector3.up, deltaMousePosition.x * Time.deltaTime * angleSensibility);
        _myTransform.Rotate(Vector3.right, -deltaMousePosition.y * Time.deltaTime * angleSensibility);
        _myTransform.localRotation = _myTransform.localRotation.WithEulerZ(0);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Turn around looking center of the mesh
    /// </summary>
    void UpdateTransformLookingCenter()
    {
        const float vel = .5f;
        //Set the distance form the center of the mesh (r) as a smooth random transition between near and far 
        float r = 40f + Mathf.PerlinNoise(0, Time.time * vel * .6f) * 90f;

        //Elevation is a sinusoidal movement
        float h = Mathf.Sin(Time.time * vel + 2394.3789f) * 90f;

        //Position is a circle around the center of the mesh
        _myTransform.position =
            _meshCenter + new Vector3(r * Mathf.Sin(Time.time * vel), h, r * Mathf.Cos(Time.time * vel));

        //Always looking at the center of the mesh
        _myTransform.LookAt(_meshCenter);
        
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void LateUpdate()
    {
        //Rotates the camera looking the mesh until player presses an input axis while not drawing 
        if(_game.gameState != Game.GameState.Drawing 
           && Math.Abs(Input.GetAxis("Horizontal")) > .5f || Mathf.Abs(Input.GetAxis("Vertical")) > .5f)
                _inputPressed = true;      
        if (InputPressed)
            UpdateTransformByInput();
        else 
            UpdateTransformLookingCenter();
    }

}
