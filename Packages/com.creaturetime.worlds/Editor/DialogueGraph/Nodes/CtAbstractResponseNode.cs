
using UnityEngine;

namespace CreatureTime.Editor.Graph.DialogueGraph
{
    public abstract class CtAbstractResponseNode : CtDialogueNodeBase
    {
        [CtInputPortInfo("conditions", typeof(CtDialoguePortTypes.ConditionPort)), SerializeField]
        protected string[] conditions;

        [CtInputPortInfo("consequences", typeof(CtDialoguePortTypes.ConsequencePort)), SerializeField]
        protected string[] consequences;

        [CtOutputPortInfo("response", typeof(CtDialoguePortTypes.ResponsePort)), SerializeField]
        private string response;

        protected void ProcessConditions(CtDialogueGraphAsset asset)
        {
            foreach (var condition in conditions)
            {
                if (asset.TryGetNodeFromInput(Guid, condition, out var node))
                    node.Process(asset);
            }
        }

        protected void ProcessConsequences(CtDialogueGraphAsset asset)
        {
            foreach (var consequence in consequences)
            {
                if (asset.TryGetNodeFromInput(Guid, consequence, out var node))
                    node.Process(asset);
            }
        }
    }
}
