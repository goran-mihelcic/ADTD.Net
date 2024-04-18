using System;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Container for Shape Size data
    /// </summary>
    public class XSize
    {
        /// <summary>
        /// Shape Width
        /// </summary>
        public Double Width { get; private set; }
        
        /// <summary>
        /// Shape Height
        /// </summary>
        public Double Height { get; private set; }

        /// <summary>
        /// Construct XSize object
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public XSize(Double width, Double height)
        {
            Width = width;
            Height = height;
        }
    }
}
