using System;

namespace CreatureTime.Editor.Graph
{
    public class CtOutputPortInfoAttribute : CtPortInfoAttribute
    {
        public CtOutputPortInfoAttribute(string identifier, Type type) : base(identifier, type)
        {
        }
    }
}
