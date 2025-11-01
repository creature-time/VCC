using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    public enum ENpcTurnState
    {
        Idle,
        MeleeAttacking,
        MeleeAttack,
        MeleeReturn,
    }

    public enum ENpcBattleControllerSignal
    {
        DamageTrigger = ENpcControllerSignal.Extensions
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtBattleController : CtNpcController
    {
        [Header("Battle Controller")]
        [SerializeField] private string displayName;

        public override string DisplayName => displayName;

        public Transform HomePosition { get; set; }

        private void MeleeAttackingState()
        {
            Brain.Context.SetEnum("TurnState", ENpcTurnState.MeleeAttacking);
        }

        private void _MeleeDoneState()
        {
            Brain.Context.SetEnum("TurnState", ENpcTurnState.MeleeReturn);
        }

        public void MeleeAttack()
        {
            animator.SetTrigger("MeleeAttack");
        }

        public float UseSkill(bool isMeleeSkill)
        {
            if (isMeleeSkill)
            {
                MeleeAttack();
                return 0f;
            }

            animator.SetTrigger("MeleeAttack");
            return 2f;
        }

        // public void _FinishedAttacking()
        // {
        //     _MeleeDoneState();
        // }
        //
        // public void InitiateAttack(ushort targetId)
        // {
        //     Brain.Context.SetUShort("TargetId", targetId);
        //     MeleeAttackingState();
        // }

        public void ResetAttack()
        {
            Brain.Context.SetInt("TurnState", 0);
        }
    }
}