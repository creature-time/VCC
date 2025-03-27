
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractDefinition : UdonSharpBehaviour
    {
        [SerializeField] protected ushort identifier = CtConstants.InvalidId;

        public ushort Identifier => identifier;
    }
}