
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

        public abstract Transform RootTransform { get; }
        public abstract Transform HeadTransform { get; }
        public abstract Transform LeftHandTransform { get; }
        public abstract Transform RightHandTransform { get; }

        public abstract void ApplyDamage(int damage, EDamageType damageType,
            EDamageSourceType damageSourceType, ushort identifier, CtEntity instigator, bool isCritical);
    }
}