
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractDefinition : UdonSharpBehaviour
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        public ushort Identifier => identifier;
    }
}