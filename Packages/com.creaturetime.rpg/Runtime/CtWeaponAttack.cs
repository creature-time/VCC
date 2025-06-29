
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtWeaponAttack : CtUserData
    {
        public CtPlayerTurn PlayerTurn { private get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!Networking.IsOwner(gameObject))
                return;

            // TODO: Get the battle controller that would probably have the reference to the entity identifier.
            var npcUserData = other.GetComponent<CtNpcUserData>();
            if (!npcUserData)
                return;

            if (PlayerTurn.InteractType != CTBattleInteractType.Waiting)
                return;

            PlayerTurn.Submit(CTBattleInteractType.Attack, CtConstants.InvalidId, npcUserData.TargetId);
        }
    }
}