using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    public abstract class CtConsequenceNode : CtDialogueNodeBase
    {
        [SerializeField]
        [CtOutputPortInfo(null, typeof(CtDialoguePortTypes.ConsequencePort))]
        private string consequence;
    }
}
