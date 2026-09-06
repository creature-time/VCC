using System;

namespace CreatureTime.Editor.Graph
{
    public abstract class CtPortInfoAttribute : Attribute
    {
        private string _identifier;
        private Type _type;

        public string Identifier => _identifier;
        public Type Type => _type;

        public CtPortInfoAttribute(string identifier, Type type)
        {
            _identifier = identifier;
            _type = type;
        }
    }
}
