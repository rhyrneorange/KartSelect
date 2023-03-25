using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : SingletonDontDestroy<InputManager>
{
    [HideInInspector] public bool InputForward;
    [HideInInspector] public bool InputBackward;
    [HideInInspector] public bool InputRight;
    [HideInInspector] public bool InputLeft;

    void Update()
    {
        InputForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        InputBackward = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        InputRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        InputLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    }
}