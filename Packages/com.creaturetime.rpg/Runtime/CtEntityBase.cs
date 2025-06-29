
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtEntityBase : CtAbstractSignal
    {
        [SerializeField] private CtEntityManager entityManager;
        [SerializeField] private ushort identifier = CtConstants.InvalidId;

        public CtEntityManager EntityManager => entityManager;

        public ushort Identifier
        {
            get => identifier;
            protected set => identifier = value;
        }

        public abstract ushort EntityId
        {
            get;
        }

        public Transform SourceTransform { get; protected set; }

        public abstract void ApplyDamage(int damage, EDamageType damageType,
            EDamageSourceType damageSourceType, ushort skillId, CtEntity instigator, bool isCritical);
    }
}