
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace CreatureTime
{
    public enum EPlayerWalletSignal
    {
        AmountChanged
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerWallet : CtAbstractSignal
    {
        [SerializeField, UdonSynced, FieldChangeCallback(nameof(_ValueCallback))]
        private int value;

        public int _ValueCallback
        {
            get => value;
            set
            {
                this.value = value;

#if DEBUG_LOGS
            LogDebug($"Player wallet updated (value={this.value}).");
#endif
                this.Emit(EPlayerWalletSignal.AmountChanged);
            }
        }

        public int Value
        {
            get => _ValueCallback;
            private set
            {
                _ValueCallback = value;
                RequestSerialization();
            }
        }

        public bool HasAmount(int amount)
        {
            return value >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!HasAmount(amount))
            {
#if DEBUG_LOGS
                LogWarning("Cannot spend amount because wallet did not have enough to spend " +
                           $"(value={value}, amount={amount})");
#endif
                return false;
            }

            Value -= amount;
            return true;
        }

        public void Add(int amount)
        {
            Value += amount;
        }
    }
}