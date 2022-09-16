using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum actionType
{
    move,
    wait,
    kill,
    changeDialog,
    setShop,
    changeSpeed,
    die,
    setIgnoreBodies,
    setGuard,
    heal,
    setStopToTalk,
    setPanic,
    changePrice,
    poison
}

[System.Serializable]
public class Action
{
    public actionType action;
    public int magnitude;
    public Vector2Int dir;
    public string dialog;
}
