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
    public Func<Objective, bool> condition { get; set; }
}

public class ObjectivesManager : MonoBehaviour
{
    public static ObjectivesManager Instance;
    public List<Objective> allObjectives = new List<Objective>();
    public List<Objective> objsP1 = new List<Objective>();
    public List<Objective> objsP2 = new List<Objective>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetState()
    {
        objsP1.Clear();
        objsP2.Clear();
    }

    private void AllObjectives()
    {
        allObjectives = new List<Objective>
        {
            new Objective
            {
                index = 0,
                name = "Dont kill any more than 5 enemies",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var player = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    if(player == null) return false;
                    return player.hasOnly5Kills == true;
                }
            },
            new Objective
            {
                index = 1,
                name = "Never open any chests",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var player = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    if(player == null) return false;
                    return player.hasNeverOpenedChest == true;
                }
            },
            new Objective
            {
                index = 2,
                name = "Open and collect 20 chests",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var player = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    if(player == null) return false;
                    return player.chestCount >= 20;
                }
            },
            new Objective
            {
                index = 3,
                name = "Don't die more than 3 times",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {                    
                    var player = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    if(player == null) return false;
                    return player.hasOnly3Deaths == true;
                }
            },
            new Objective
            {
                index = 4,
                name = "Die at least 20 times",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var player = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    if(player == null) return false;
                    return player.deadCount >= 20;
                }
            },
            new Objective
            {
                index = 5,
                name = "Kill more enemies than your Partner",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var myPlayer = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    var otherPlayer = GameManager.Instance.players[obj.playerId == 0 ? 1 : 0]?.GetComponent<PlayerController>();
                    if(myPlayer == null || otherPlayer == null) return false;
                    return myPlayer.killCount > otherPlayer.killCount;
                }
            },
            new Objective
            {
                index = 6,
                name = "Kill less enemies than your Partner",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var myPlayer = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    var otherPlayer = GameManager.Instance.players[obj.playerId == 0 ? 1 : 0]?.GetComponent<PlayerController>();
                    if(myPlayer == null || otherPlayer == null) return false;
                    return myPlayer.killCount < otherPlayer.killCount;
                }
            },
            new Objective
            {
                index = 7,
                name = "Be the first to reach the Port",
                isCompleted = false,
                reward = 200,
                condition = (obj) =>
                {
                    var player = GameManager.Instance.players[obj.playerId]?.GetComponent<PlayerController>();
                    if(player == null) return false;
                    return player.reachedDestinationFirst;
                }
            }
        };
    }

    private void Start()
    {
        // Populate the master list of objectives
        AllObjectives();
    }

    public void AssignObjective(int _objIndex, int _playerIndex)
    {
        Objective objTemplate = allObjectives.Find(x => x.index == _objIndex);

        if (objTemplate != null)
        {
            // Create a new instance so each player tracks their own progress
            Objective newObj = new Objective { index = objTemplate.index, name = objTemplate.name, playerId = _playerIndex, isCompleted = false, reward = objTemplate.reward, condition = objTemplate.condition };

            if (_playerIndex == 0) objsP1.Add(newObj);
            else if (_playerIndex == 1) objsP2.Add(newObj);
        }
    }

    public void Update()
    {
        // Exit early if there are no objectives to check for any player.
        if (objsP1.Count == 0 && objsP2.Count == 0)
            return;

        // Using a reverse for-loop is safer if we ever want to remove completed objectives from the list.
        for (int i = objsP1.Count - 1; i >= 0; i--)
        {
            ObjectiveCheck(objsP1[i]);
        }
        for (int i = objsP2.Count - 1; i >= 0; i--)
        {
            ObjectiveCheck(objsP2[i]);
        }
    }

    public void ObjectiveCheck(Objective _obj)
    {
        if (_obj != null && !_obj.isCompleted && _obj.condition != null)
        {
            if (_obj.condition(_obj))
            {
                _obj.isCompleted = true;
                GameManager.Instance.players[_obj.playerId].GetComponent<PlayerController>().hiddenStash += _obj.reward;
                Debug.LogWarning("Objective completed: " + _obj.name);
            }
        }
    }
}