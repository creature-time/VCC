
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace CreatureTime
{
    public enum ESelectionModelSignal
    {
        SelectionChanged
    }

    public enum ESelectionFlags
    {
        ClearSelection
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class CtSelectionModel : CtAbstractSignal
    {
        [SerializeField] private bool multiSelection;

        private DataList _selection = new DataList();

        public DataList Selection => _selection;

        public void SetSelection(DataToken selected, ESelectionFlags selectionMode)
        {
            var prev = _selection.DeepClone();

            if (!multiSelection || selectionMode == ESelectionFlags.ClearSelection)
                _selection.Clear();

            if (!selected.IsNull)
                _selection.Add(selected);

            SetArgs.Add(prev);
            SetArgs.Add(_selection.DeepClone());
            this.Emit(ESelectionModelSignal.SelectionChanged);
        }

        public void Clear()
        {
            SetArgs.Add(_selection.DeepClone());
            _selection.Clear();
            SetArgs.Add(_selection.DeepClone());
            this.Emit(ESelectionModelSignal.SelectionChanged);
        }
    }
}