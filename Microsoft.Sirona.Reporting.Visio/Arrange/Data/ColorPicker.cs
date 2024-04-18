using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange.Data
{
    /// <summary>
    /// Mantains list of indexes and collors assigned
    /// Selects next available color for dynamically colord objects from chosen pallete
    /// </summary>
    public static class ColorPicker
    {
        private static List<Color> _lightColors;
        private static List<Color> _darkColors;
        private static List<Color> _vividColors;
        private static List<Color> _allColors;
        private static Dictionary<string, Dictionary<string, Color>> _colorMap;

        /// <summary>
        /// Initialize class
        /// </summary>
        static ColorPicker()
        {
            InitAllColors();
            _colorMap = new Dictionary<string, Dictionary<string, Color>>();
        }

        /// <summary>
        /// Get color for the group
        /// If value already has assigned colloe returns it
        /// if doesn't picks next avalable color from the pallete and records selection
        /// </summary>
        /// <param name="group">Colloring group. Usually attribute name</param>
        /// <param name="value">Attribute value for which you need color</param>
        /// <param name="schema">Color schema</param>
        /// <returns>Color for the value in the group</returns>
        public static Color GetColor(string group, string value, ColorSchema schema)
        {
            if (group == null)
                throw new ArgumentNullException("group", "Color group shouldn't be null");
            if (value == null)
                throw new ArgumentNullException("value", "Color value shouldn't be null");

            group = group.ToLowerInvariant();
            value = value.ToLowerInvariant();
            Dictionary<string, Color> groupColors = null;

            if (_colorMap.ContainsKey(group))
                groupColors = _colorMap[group];
            else
                lock (_colorMap)
                {
                    groupColors = new Dictionary<string, Color>();
                    _colorMap.Add(group, groupColors);
                }

            if (groupColors.ContainsKey(value))
                return groupColors[value];
            else
            {
                List<Color> colorList = GetColorList(schema);
                int size = colorList.Count;
                Color resultColor = Color.White;
                lock (groupColors)
                {
                    int nextPosition = groupColors.Count;
                    if (nextPosition >= size)
                        nextPosition = 0;
                    resultColor = colorList[nextPosition];
                    groupColors.Add(value, resultColor);
                }
                return resultColor;
            }

        }

        private static List<Color> GetColorList(ColorSchema schema)
        {
            switch (schema)
            {
                case ColorSchema.AllColors:
                    return _allColors;
                case ColorSchema.DarkColors:
                    return _darkColors;
                case ColorSchema.LightColors:
                    return _lightColors;
                case ColorSchema.VividColors:
                    return _vividColors;
            }

            return new List<Color>();
        }

        private static List<Color> InitColorList(Type schema)
        {
            HashSet<Color> resultSet = new HashSet<Color>();

            foreach (string c in Enum.GetNames(schema))
                resultSet.Add(Color.FromName(c));

            return resultSet.ToList();
        }

        private static void InitAllColors()
        {
            HashSet<Color> resultSet = new HashSet<Color>();

            _darkColors = InitColorList(typeof(DarkColors));
            foreach (Color color in _darkColors)
                resultSet.Add(color);

            _vividColors = InitColorList(typeof(VividColors));
            foreach (Color color in _vividColors)
                resultSet.Add(color);

            _lightColors = InitColorList(typeof(LightColors));
            foreach (Color color in _lightColors)
                resultSet.Add(color);

            _allColors = resultSet.ToList();
        }
    }
}
