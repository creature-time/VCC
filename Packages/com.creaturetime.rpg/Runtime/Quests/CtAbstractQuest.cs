
using UnityEngine;

namespace CreatureTime
{
    public abstract class CtAbstractQuest : CtAbstractDefinition
    {
        [SerializeField] private string questName;
        [SerializeField] private Texture2D icon;
        [SerializeField] private int levelReq;

        public virtual string Title => questName;
        public virtual Texture2D Icon => icon;
        public virtual int LevelReq => levelReq;

        public abstract void Execute();
    }
}