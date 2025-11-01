using System;

namespace CreatureTime.Editor.Graph
{
    public class CtExposedArrayPortAttribute : Attribute
    {
        private Type _type;
        private Type _inputType;
        private Type _outputType;
        private string _createText;
        private string _titleProperty;

        public Type ExposedType => _type;
        public Type InputType => _inputType;
        public Type OutputType => _outputType;
        public string CreateText => _createText;
        public string TitleProperty => _titleProperty;

        public CtExposedArrayPortAttribute(Type type, Type inputType, Type outputType, string createText = "+", string titleProperty = null)
        {
            _type = type;
            _createText = createText;
            _titleProperty = titleProperty;
        }
    }
}
