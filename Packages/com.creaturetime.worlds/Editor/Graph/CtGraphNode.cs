using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph
{
    [Serializable]
    public class CtGraphNode
    {
        [SerializeField] private string guid;
        [SerializeField] private Rect position;

        public string Guid => guid;

        public Rect Position
        {
            get => position;
            set => position = value;
        }

        public CtGraphNode()
        {
            guid = System.Guid.NewGuid().ToString();
        }
    }
}
