
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime.RpgGame
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtWeaponAttack : CtUserData
    {
        [SerializeField, Range(1, 15)] private int palette;

        public int Palette => palette;
        public bool CanMelee { get; set; }

        public CtPlayerTurn PlayerTurn { private get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (!CanMelee) return;

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