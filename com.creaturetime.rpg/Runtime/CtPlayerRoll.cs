
using System;
using UdonSharp;

namespace CreatureTime
{
    public enum EPlayerRollSignal
    {
        RollTypesReset,
        RollTypeChanged
    }

    public enum ERollType
    {
        None = 0,
        Pass = 1,
        Greed = 2,
        Need = 3
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CtPlayerRoll : CtAbstractSignal
    {
        [UdonSynced] private ERollType[] _rollTypes = { };
        private ERollType[] _cmpRollType = { };

        [UdonSynced] private bool[] _hasTakenLoot = { };

        public ERollType GetRollType(int index)
        {
            if (index >= _rollTypes.Length)
                return ERollType.None;
            return _rollTypes[index];
        }

        private void _UpdateRollType(int index, ERollType rollType)
        {
#if DEBUG_LOGS
            LogDebug($"Update roll type updated (rollType={rollType})");
#endif

            if (_rollTypes.Length <= index)
                CtArrayUtils.Resize(ref _rollTypes, index + 1);

            _rollTypes[index] = rollType;

            RequestSerialization();
            OnDeserialization();
        }

        public void SetNeed(int index)
        {
            _UpdateRollType(index, ERollType.Need);
        }

        public void SetGreed(int index)
        {
            _UpdateRollType(index, ERollType.Greed);
        }

        public void SetPass(int index)
        {
            _UpdateRollType(index, ERollType.Pass);
        }

        private void _UpdateTakenLoot(int index)
        {
#if DEBUG_LOGS
            LogDebug("Update taken loot updated.");
#endif

            if (_hasTakenLoot.Length <= index)
                CtArrayUtils.Resize(ref _hasTakenLoot, index + 1);

            _hasTakenLoot[index] = true;

            RequestSerialization();
            OnDeserialization();
        }

        public bool HasTakenLoot(int index) => index < _hasTakenLoot.Length && _hasTakenLoot[index];

        public void TakeLoot(int index)
        {
            _UpdateTakenLoot(index);
        }

        public void Reset()
        {
            _rollTypes = new ERollType[] { };
            _hasTakenLoot = new bool[] { };
            RequestSerialization();
            OnDeserialization();
        }

        public override void OnDeserialization()
        {
            if (_rollTypes.Length != _cmpRollType.Length)
            {
                CtArrayUtils.Resize(ref _cmpRollType, _rollTypes.Length);
                Array.Copy(_rollTypes, _cmpRollType, _rollTypes.Length);
                this.Emit(EPlayerRollSignal.RollTypesReset);
            }
            else
            {
                for (var i = 0; i < _rollTypes.Length; i++)
                {
                    if (_rollTypes[i] != _cmpRollType[i])
                    {
                        _cmpRollType[i] = _rollTypes[i];
                        SetArgs.Add(i);
                        this.Emit(EPlayerRollSignal.RollTypeChanged);
                    }
                }
            }
        }
    }
}