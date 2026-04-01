using System;
using UnityEngine;

namespace CreatureTime
{
    [Serializable]
    [CreateAssetMenu(fileName = "DialogueActor", menuName = "CreatureTime/DialogueGraph/Actor")]
    public class CtActor : ScriptableObject
    {
        [SerializeField] private ushort identifier;
        [SerializeField] private string actorName;

        public ushort Identifier => identifier;
        public string ActorName => actorName;
    }
}
