using System;

namespace CreatureTime.Editor.Graph
{
    public class CtInputPortInfoAttribute : CtPortInfoAttribute
    {
        public CtInputPortInfoAttribute(string identifier, Type type) : base(identifier, type)
        {
        }
    }
}
