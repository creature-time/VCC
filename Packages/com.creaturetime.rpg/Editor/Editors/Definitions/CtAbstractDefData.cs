using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractDefData : ScriptableObject
    {
        public abstract string GenerateName { get; }
        public abstract int Identifier { get; }
    }
}