namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Visio Text Style refferences
    /// </summary>
    public class XVisioTextStyleRefs
    {
        /// <summary>
        /// Text Style
        /// </summary>
        public TextStyle Style  { get; set; }

        /// <summary>
        /// Paragraph style Id
        /// </summary>
        public int Para { get; set; }
        
        /// <summary>
        /// Character Style Id
        /// </summary>
        public int Char { get; set; }
        
        /// <summary>
        /// Tab Style Id
        /// </summary>
        public int Tab { get; set; }

        /// <summary>
        /// Constructs XVisioTextStyleRefs for a Style
        /// </summary>
        /// <param name="style">Text Style</param>
        public XVisioTextStyleRefs(TextStyle style)
        {
            this.Style = style;
            this.Para = -1;
            this.Char = -1;
            this.Tab = -1;
        }
    }
}
