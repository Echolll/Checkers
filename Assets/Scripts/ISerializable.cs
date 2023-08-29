using Checkers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISerializable
{
    public event Action ObjectMoved;

    public event Action<ColorType> GameEnded;

    public event Action StepOver;
}
