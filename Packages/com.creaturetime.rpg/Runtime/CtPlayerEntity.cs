
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerEntity : CtEntity
    {
        [SerializeField] private CtPlayerManager playerManager;
        private CtPlayerTurn playerTurn;

        protected override void _OnEntityIdChanged()
        {
            IsPlayer = true;

            var playerId = CtEntityManager.GetIdentifier(EntityId);
            EntityDef = playerManager.GetPlayerDefById(playerId);
            playerTurn = EntityDef.GetComponent<CtPlayerTurn>();
            SourceTransform = EntityDef.transform;
        }

        public override bool TryGetAttack(out int skillIndex, out int targetId)
        {
            return playerTurn.TryGetAttack(out skillIndex, out targetId);
        }
    }
}