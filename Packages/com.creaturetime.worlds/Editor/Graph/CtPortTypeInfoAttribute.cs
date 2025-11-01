using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph
{
    public class CtPortTypeInfoAttribute : Attribute
    {
        private string _hexColor;

        public Color Color
        {
            get
            {
                if (ColorUtility.TryParseHtmlString(_hexColor, out var color))
                    return color;
                return Color.black;
            }
        }

        public CtPortTypeInfoAttribute(string hexColor)
        {
            _hexColor = hexColor;
        }
    }
}
