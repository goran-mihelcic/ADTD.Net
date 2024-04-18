using System;
using System.Collections.Generic;
using System.Linq;

namespace Mihelcic.Net.Visio.Arrange
{
    /// <summary>
    /// Supporting basic Vector Math
    /// </summary>
    public struct Vector
    {
        private Guid _id;

        private Double _x;
        public Double X { get { return _x; } set { _x = value; } }

        private Double _y;
        public Double Y { get { return _y; } set { _y = value; } }

        /// <summary>
        /// Vector magnitude
        /// </summary>
        public Double Magnitude
        {
            get
            {
                return Math.Sqrt(_x * _x + _y * _y);
            }
        }

        /// <summary>
        /// Vector Direction in radians
        /// </summary>
        public Double Direction
        {
            get
            {
                Double result;
                if (_y == 0)
                    result = 0;
                else
                {

                    if (_x == 0)
                        result = Math.PI / 2;
                    else
                        result = Math.Atan(_y / _x);
                }

                if (_x < 0)
                    result = Math.PI - Math.Abs(result);

                if (_y < 0)
                    result = Math.Abs(result) * (-1);
                return result;
            }
        }

        /// <summary>
        /// Vector Direction in degrees
        /// </summary>
        public Double DirectionInDegrees
        {
            get
            {
                return Direction * (180 / Math.PI);
            }
        }

        /// <summary>
        /// Returns unit vector
        /// </summary>
        public Vector UnitVector
        {
            get
            {
                return new Vector(1, this.Direction, true);
            }
        }

        /// <summary>
        /// Creates vector from 0, 0 to x, y
        /// </summary>
        /// <param name="x">x coordinate of vector end point</param>
        /// <param name="y">y coordinate of vector end point</param>
        public Vector(Double x, Double y)
        {
            _x = x;
            _y = y;
            _id = Guid.NewGuid();
        }

        /// <summary>
        /// Creates vector from 0, 0 to x, y
        /// </summary>
        /// <param name="x">x coordinate of vector end point</param>
        /// <param name="y">y coordinate of vector end point</param>
        public Vector(Int32 x, Int32 y)
        {
            _x = Convert.ToDouble(x);
            _y = Convert.ToDouble(y);
            _id = Guid.NewGuid();
        }

        /// <summary>
        /// Creates vector with given magnitude and direction
        /// </summary>
        /// <param name="magnitude">Vector magnitude</param>
        /// <param name="direction">Vector direction</param>
        /// <param name="radians">Is direction given in radians?</param>
        public Vector(Double magnitude, Double direction, bool radians)
        {
            if (!radians)
            {
                direction = (Math.PI / 180) * direction;
            }

            _x = magnitude * Math.Cos(direction);
            _y = magnitude * Math.Sin(direction);
            _id = Guid.NewGuid();
        }

        /// <summary>
        /// Creates vector between two points
        /// </summary>
        /// <param name="ax">Vector originate x</param>
        /// <param name="ay">Vector originate y</param>
        /// <param name="bx">Vector end x</param>
        /// <param name="by">Vector end y</param>
        public Vector(Double ax, Double ay, Double bx, Double by)
            : this(bx - ax, by - ay)
        { }

        /// <summary>
        /// Scale vector to given magnitude
        /// </summary>
        /// <param name="magnitude">target magnitude</param>
        /// <returns>Scaled vector</returns>
        public Vector ScaleTo(double magnitude)
        {
            return new Vector(magnitude, this.Direction, true);
        }

        #region Arithmetic Operators

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            Vector vector = (Vector)obj;

            if (_x != vector.X)
                return false;

            return _y == vector.Y;
        }

        public static bool operator ==(Vector a, Vector b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vector a, Vector b)
        {
            return !a.Equals(b);
        }

        public static Vector operator -(Vector a)
        {
            return new Vector(a.X * -1, a.Y * -1);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator *(Vector a, Int32 b)
        {
            return new Vector(a.X * b, a.Y * b);
        }

        #endregion

        /// <summary>
        /// Returns vector of same magnitude but oposite direction
        /// </summary>
        /// <returns>Inverted vector</returns>
        public Vector Invert()
        {
            return new Vector(this.X * (-1), this.Y * (-1));
        }

        /// <summary>
        /// Returns vector of same magnitude but direction changed for 90 degrees in one side
        /// </summary>
        /// <returns>Rotated vector</returns>
        public Vector Rotate90(bool right = false)
        {
            if (right)
                return new Vector(this.Magnitude, this.Direction + Math.PI / 2, true);
            else
                return new Vector(this.Magnitude, this.Direction - Math.PI / 2, true);
        }

        /// <summary>
        /// Returns vector of magnitude 0 and direction 0
        /// </summary>
        public static Vector Null { get { return new Vector(0, 0); } }

        /// <summary>
        /// Calculate resulting vector of multiple vectors
        /// </summary>
        /// <param name="vectorList">Vector list to summarize</param>
        /// <returns>Vector sum</returns>
        public static Vector Sum(IEnumerable<Vector> vectorList)
        {
            if (vectorList == null)
                return Vector.Null;

            Vector result = new Vector();
            foreach (Vector v in vectorList)
                result += v;

            return result;
        }

        public static Double AvgMagnitude(IEnumerable<Vector> vectorList)
        {
            if (vectorList == null || vectorList.Count() == 0)
                return 0;

            return vectorList.Average(v => v.Magnitude);
        }

        /// <summary>
        /// Calculate angle between two vectors
        /// </summary>
        /// <param name="vector1">Calculate angle from this vector</param>
        /// <param name="vector2">Calculate angle to this vector</param>
        /// <returns>Angle between vectors in radians</returns>
        public static double AngleBetween(Vector vector1, Vector vector2)
        {
            return vector2.Direction - vector1.Direction;
        }

        public override string ToString()
        {
            return String.Format("A={0} degree, M={1}", this.DirectionInDegrees, this.Magnitude);
        }
    }
}
