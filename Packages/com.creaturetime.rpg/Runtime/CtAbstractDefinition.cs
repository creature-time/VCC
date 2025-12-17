
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractDefinition : CtLoggerUdonScript
    {
        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        public ushort Identifier => identifier;
    }
}