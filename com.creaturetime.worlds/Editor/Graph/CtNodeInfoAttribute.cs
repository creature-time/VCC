using System;
using UnityEngine;

namespace CreatureTime.Editor.Graph
{
    public class CtNodeInfoAttribute : Attribute
    {
        private string _title;
        private string _hexColor;
        private string _menuItem;
        private bool _hasFlowInput;
        private bool _hasFlowOutput;

        public string Title => _title;
        public Color Color
        {
            get
            {
                if (ColorUtility.TryParseHtmlString(_hexColor, out var color))
                    return color;
                return Color.black;
            }
        }

        public string MenuItem => _menuItem;
        public bool HasFlowInput => _hasFlowInput;
        public bool HasFlowOutput => _hasFlowOutput;

        public CtNodeInfoAttribute(string title, string hexColor, string menuItem = "", bool hasFlowInput = true, bool hasFlowOutput = true)
        {
            _title = title;
            _hexColor = hexColor;
            _menuItem = menuItem;
            _hasFlowInput = hasFlowInput;
            _hasFlowOutput = hasFlowOutput;
        }
    }
}
