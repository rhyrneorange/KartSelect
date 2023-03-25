using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonDontDestroy<GameManager>
{
    protected override void OnAwake()
    {
        Application.targetFrameRate = 90;
    }
}