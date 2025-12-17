
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtEntityBase : CtAbstractSignal
    {
        [SerializeField] protected CtEntityManager entityManager;
        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        public ushort Identifier
        {
            get => identifier;
            protected set => identifier = value;
        }

        public abstract ushort EntityId { get; }

        // TODO: Can we swap these to a Transform without huge cost?
        public abstract Vector3 Position { get; }
        public abstract Quaternion Rotation { get; }

        public abstract void ApplyDamage(int damage, EDamageType damageType,
            EDamageSourceType damageSourceType, ushort identifier, CtEntity instigator, bool isCritical);
    }
}