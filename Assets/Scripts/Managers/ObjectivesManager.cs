using System.Collections.Generic;
using UnityEngine;
using System;

public class Objective
{
    public int index { get; set; }
    public string name { get; set; }
    public int playerId { get; set; }
    public bool isCompleted { get; set; }
    public int reward { get; set; }
    public Func<bool> condition { get; set; } // stores a function that returns a bool
}

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager Instance;
    public List<Objective> objsP1 = new List<Objective>();
    public List<Objective> objsP2 = new List<Objective>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        objsP1.Add(new Objective 
        { 
            index = 0, 
            name = "First Kill", 
            playerId = 0,
            isCompleted = false, 
            reward = 100,
            condition = () => GameManager.Instance.players[0].GetComponent<PlayerController>().killCount >= 1 
        });
    }

    public void Update()
    {
        foreach (Objective obj in objsP1)
        {
            ObjectiveCheck(obj);
        }
        foreach (Objective obj in objsP2)
        {
            ObjectiveCheck(obj);
        }
    }

    public void ObjectiveCheck(Objective _obj)
    {
        if (_obj != null && !_obj.isCompleted)
        {
            if (_obj.condition()) 
            {
                _obj.isCompleted = true;
                GameManager.Instance.players[_obj.playerId].GetComponent<PlayerController>().hiddenStash += _obj.reward;
                Debug.LogWarning("Objective completed: " + _obj.name);
            }
        }
    }
}