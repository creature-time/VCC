using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph
{
    [Serializable]
    public struct CtGraphEdge
    {
        [SerializeField] private string guid;
        [SerializeField] private string outputId;
        [SerializeField] private string outputPortId;
        [SerializeField] private string inputId;
        [SerializeField] private string inputPortId;

        public string Guid => guid;
        public string OutputId => outputId;
        public string OutputPortId => outputPortId;
        public string InputId => inputId;
        public string InputPortId => inputPortId;

        public CtGraphEdge(string guid, string outputId, string outputPortId, string inputId, string inputPortId)
        {
            this.guid = guid;
            this.outputId = outputId;
            this.outputPortId = outputPortId;
            this.inputId = inputId;
            this.inputPortId = inputPortId;
        }
    }
}
