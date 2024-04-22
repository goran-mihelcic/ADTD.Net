using System;
using System.Drawing;

namespace Mihelcic.Net.Visio.Arrange
{
    public class ShapePresentation
    {
        /// <summary>
        /// Visio Master UName from Stencil that applies to this shape
        /// </summary>
        public string MasterName { get; set; }

        /// <summary>
        /// First line of text describing the shape
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        /// Text style for heading
        /// </summary>
        public string HeadingStyle { get; set; }

        /// <summary>
        /// Other text describing the shape
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Style for other text
        /// </summary>
        public string TextStyle { get; set; }

        /// <summary>
        /// Shape color
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// Subshape to color (if we don't want hole shape to be colored)
        /// </summary>
        public string SubShapeToColor { get; set; }

        /// <summary>
        /// Popup text associated with shape
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Line tickness
        /// </summary>
        public Double? Thickness { get; set; }

         public ShapePresentation(string master)
        {
            this.MasterName = master;
        }

        public ShapePresentation(
            string master,
            string heading,
            string headingStyle,
            string text,
            string textStyle,
            Color? color,
            string comment,
            string subShapeToColor,
            Double? tickness = null)
            : this(master)
        {
            this.Heading = heading;
            this.HeadingStyle = headingStyle;
            this.Text = text;
            this.TextStyle = textStyle;
            this.Color = color;
            this.Comment = comment;
            this.SubShapeToColor = subShapeToColor;
            this.Thickness = tickness;
        }
    }
}
