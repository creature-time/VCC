
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtEntityBase : CtAbstractSignal
    {
        protected CtEntityManager EntityManager;
        private ushort _identifier = CtConstants.InvalidId;

        public ushort Identifier => _identifier;

        [UdonSynced, FieldChangeCallback(nameof(EntityIdCallback))]
        private ushort _entityId = CtConstants.InvalidId;

        public ushort EntityIdCallback
        {
            get => _entityId;
            set
            {
                var previousId = _entityId;
                _entityId = value;

                _OnEntityIdChanged();

                SetArgs.Add(previousId);
                SetArgs.Add(_entityId);
                this.Emit(EEntitySignal.IdentifierChanged);
            }
        }

        public ushort EntityId
        {
            get => EntityIdCallback;
            set
            {
                EntityIdCallback = value;
                RequestSerialization();
            }
        }

        public Transform SourceTransform { get; protected set; }

        public virtual void Init(CtEntityManager entityManager, ushort identifier)
        {
            EntityManager = entityManager;
            _identifier = identifier;
        }

        protected abstract void _OnEntityIdChanged();

        public abstract void ApplyDamage(ushort instanceId, int damage, EDamageType damageType,
            EDamageSourceType damageSourceType, int identifier,
            CtEntity instigator, bool isCritical);
    }
}