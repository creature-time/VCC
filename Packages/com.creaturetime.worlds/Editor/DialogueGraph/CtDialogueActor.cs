using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    [Serializable]
    [CreateAssetMenu(fileName = "DialogueActor", menuName = "CreatureTime/DialogueGraph/Actor")]
    public class CtDialogueActor : ScriptableObject
    {
        [SerializeField] private ushort identifier;
        [SerializeField] private string actorName;

        public ushort Identifier => identifier;
        public string ActorName => actorName;
    }
}
