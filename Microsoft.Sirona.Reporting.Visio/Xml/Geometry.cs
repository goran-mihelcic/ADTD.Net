using System;
using System.Drawing;
using System.Globalization;

namespace Mihelcic.Net.Visio.Xml
{
    /// <summary>
    /// Helper methods for Cell formula creation
    /// </summary>
    internal class Geometry
    {
        private static Double baseWidth = 3.248; //8.75 cm in Inch
        private static Double baseHeight = 1.87; //4.75 cm in Inch

        /// <summary>
        /// Returns Container Width formula for container with ServerNum server objects
        /// </summary>
        /// <param name="ServerNum">Number of Servers in the Shape</param>
        /// <returns>Width cell content</returns>
        public static string GetWidth(int ServerNum)
        {
            int x;
            x = Convert.ToInt32(Math.Truncate(Math.Sqrt(ServerNum)) + 1);
            // 1.476377952755 = 3.75 cm in Inch
            return (baseWidth + (Math.Max((x - 2), 0) * 1.476377952755)).ToString("F15", new CultureInfo("en-US"));
        }

        /// <summary>
        /// Returns Container Height formula for container with ServerNum server objects
        /// </summary>
        /// <param name="ServerNum">Number of Servers in the Shape</param>
        /// <returns>Height cell content</returns>
        public static string GetHeight(int ServerNum)
        {
            int x, y;
            x = Convert.ToInt32(Math.Truncate(Math.Sqrt(ServerNum)) + 1);
            y = Convert.ToInt32(Math.Truncate(Math.Sqrt(ServerNum + x)));
            // 1.476377952755 = 2.75 cm in Inch
            return (baseHeight + Math.Max((y - 1), 0) * 1.0826771653543).ToString("F15", new CultureInfo("en-US"));
        }

        /// <summary>
        /// X coordinate for object relative to parent
        /// </summary>
        /// <param name="parent">Parent shape nameU</param>
        /// <param name="x">Relative horizntal distance from Parent</param>
        /// <returns>X Cell content</returns>
        public static string GetXFormula(string parent, Double x)
        {
            if (String.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException("parent", "Parent shuldn't be empty");

            return String.Format("('{0}'!{1}-'{0}'!{2}+{3})", parent, XVisioUtils.VISIO_PINX_ATTRIBUTE, XVisioUtils.VISIO_LOCPINX_ATTRIBUTE,
                (x / XVisioUtils.INCH_TO_MM).ToString(new CultureInfo("en-US")));
        }

        /// <summary>
        /// Y coordinate for object relative to parent
        /// </summary>
        /// <param name="parent">Parent shape nameU</param>
        /// <param name="y">Relative vertical distance from Parent</param>
        /// <returns>Y Cell content</returns>
        public static string GetYFormula(string parent, Double y)
        {
            if (String.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException("parent", "Parent shuldn't be empty");

            return String.Format("('{0}'!{1}+'{0}'!{2}-{3})", parent, XVisioUtils.VISIO_PINY_ATTRIBUTE, XVisioUtils.VISIO_LOCPINY_ATTRIBUTE,
                (y / XVisioUtils.INCH_TO_MM).ToString(new CultureInfo("en-US")));
        }

        /// <summary>
        /// X coordinate for server object relative to container
        /// </summary>
        /// <param name="parent">Container shape</param>
        /// <param name="order">Server shape order</param>
        /// <param name="count">Number of Server objects</param>
        /// <returns>X Cell content</returns>
        public static string GetMatrixXFormula(string parent, int order, int count)
        {
            if (String.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException("parent", "Parent shuldn't be empty");

            return String.Format("('{0}'!{1}-'{0}'!{2}+{3})", parent, XVisioUtils.VISIO_PINX_ATTRIBUTE, XVisioUtils.VISIO_LOCPINX_ATTRIBUTE,
                (GetServerPosition(order, count).X / XVisioUtils.INCH_TO_MM).ToString(new CultureInfo("en-US")));
        }

        /// <summary>
        /// Y coordinate for server object relative to container
        /// </summary>
        /// <param name="parent">Container shape</param>
        /// <param name="order">Server shape order</param>
        /// <param name="count">Number of Server objects</param>
        /// <returns>Y Cell content</returns>
        public static string GetMatrixYFormula(string parent, int order, int count)
        {
            if (String.IsNullOrWhiteSpace(parent))
                throw new ArgumentNullException("parent", "Parent shuldn't be empty");

            return String.Format("('{0}'!{1}+'{0}'!{2}-{3})", parent, XVisioUtils.VISIO_PINY_ATTRIBUTE, XVisioUtils.VISIO_LOCPINY_ATTRIBUTE,
                (GetServerPosition(order, count).Y / XVisioUtils.INCH_TO_MM).ToString(new CultureInfo("en-US")));
        }

        private static Point GetServerPosition(int i, int serverNum)
        {
            Point pos = new Point();
            int x;

            x = Convert.ToInt32(Math.Truncate(Math.Sqrt(serverNum)) + 1);

            Double posX = i % x;
            Double posY = Convert.ToInt32(Math.Truncate(Convert.ToDouble(i) / x));

            pos.X = Convert.ToInt32(22.5 + (posX * 37.5));
            pos.Y = Convert.ToInt32(17.5 + (posY * 27.5));

            return pos;

        }
    }
}
