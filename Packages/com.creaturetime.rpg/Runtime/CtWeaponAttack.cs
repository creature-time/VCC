
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
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
            var npcController = other.GetComponent<CtNpcController>();
            if (!npcController)
                return;

            if (PlayerTurn.InteractType != CTBattleInteractType.Waiting)
                return;

            PlayerTurn.Submit(CTBattleInteractType.Attack, -1, npcController.Identifier);
        }
    }
}