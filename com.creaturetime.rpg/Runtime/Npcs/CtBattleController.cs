using System;
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
        [SerializeField] private Collider npcCollider;

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

        public float UseSkill(CtSkillDef skillDef)
        {
            if (skillDef.IsWeaponSkill)
            {
                MeleeAttack();
                return 0f;
            }

            animator.SetTrigger("CastSpell");
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

        private GameObject _weaponArt;

        public void SetWeaponDef(CtWeaponDef weaponDef)
        {
            if (_weaponArt)
            {
                Destroy(_weaponArt);
                _weaponArt = null;
            }

            var weaponType = EWeaponAttackType.None;
            if (weaponDef)
            {
                _weaponArt = Instantiate(weaponDef.UserData.gameObject, HandBoneR);
                weaponType = weaponDef.AttackType;
            }

            animator.SetFloat("WeaponType", Convert.ToInt32(weaponType));
        }

        public void ResetAttack()
        {
            Brain.Context.SetInt("TurnState", 0);
        }

        public void TakeDamage()
        {
            animator.SetTrigger("TakeDamage");
        }

        public void TakeHeal()
        {
            animator.SetTrigger("TakeHeal");
        }

        public void HandleDeath()
        {
            animator.SetBool("IsDead", true);
            npcCollider.enabled = false;
        }

        public void HandleRevive()
        {
            animator.SetBool("IsDead", false);
            npcCollider.enabled = false;
        }
    }
}