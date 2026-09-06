
using UdonSharp;
using UnityEngine;

namespace CreatureTime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtItemReward : CtAbstractQuestReward
    {
        [SerializeField] private CtRpgGame rpgGame;

        [CtItem, SerializeField] private string[] itemRewards;

        public string[] ItemRewards => itemRewards;

        public override void GrantRewards()
        {
            var playerInventory = rpgGame.LocalEntity.PlayerInventory;
            foreach (var reward in itemRewards)
            {
                playerInventory.TryGiveItem(CtDataBlock.Deserialize(reward));
            }
        }
    }
}
