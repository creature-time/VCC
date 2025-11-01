
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    public abstract class CtConditionNode : CtDialogueNodeBase
    {
        [SerializeField]
        [CtOutputPortInfo(null, typeof(CtDialoguePortTypes.ConditionPort))]
        private string condition;
    }
}
