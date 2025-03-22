
using VRC.SDK3.Data;

namespace CreatureTime
{
    public abstract class CtAbstractSignalData : CtLoggerUdonScript
    {
        protected DataDictionary _changedCallbacks = new DataDictionary();
        protected DataDictionary _addCallbacks = new DataDictionary();
        protected DataDictionary _removeCallbacks = new DataDictionary();

        protected bool _blocked;

        // TODO: Can we make these arguments stack in a DataList?
        protected int _signalIndex;
    }
}