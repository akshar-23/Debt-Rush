using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IndividualObjective", menuName = "Game/Individual Objective")]
public class IndividualObjective : ScriptableObject
{
    [TextArea(1, 3)]
    public List<string> objectives = new List<string>();
}
