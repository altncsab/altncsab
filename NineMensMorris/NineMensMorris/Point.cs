using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NineMensMorris
{
    public class Point
    {
        private System.Drawing.PointF _pointF;
        public float X { get {return _pointF.X; } set {_pointF.X = value; } }
        public float Y { get {return _pointF.Y; } set {_pointF.Y = value; } }
        public bool IsMepty { get {return _pointF.IsEmpty; } }
        public System.Drawing.Point PointI { get { return new System.Drawing.Point(Convert.ToInt32(X), Convert.ToInt32(Y)); } set { X = Convert.ToInt32(value.X); Y = Convert.ToInt32(value.Y); } }
        public System.Drawing.PointF PointF { get { return _pointF; } set { _pointF = value; } }
        public Point(System.Drawing.PointF point)
        {
            _pointF = point;
        }
        public Point(System.Drawing.Point point)
        {
            _pointF = new System.Drawing.PointF(Convert.ToSingle(point.X), Convert.ToSingle(point.Y));

        }
        public Point(int x, int y)
        {
            _pointF = new System.Drawing.PointF(x, y);
        }
        public Point(float x, float y)
        {
            _pointF = new System.Drawing.PointF(x, y);
        }
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        public static Point operator *(Point a, Point b)
        {
            return new Point(a.X * b.X, a.Y * b.Y);
        }
        public static Point operator -(Point a, float b)
        {
            return new Point(a.X - b, a.Y - b);
        }
        public static Point operator +(Point a, float b)
        {
            return new Point(a.X + b, a.Y + b);
        }
        public static Point operator *(Point a, float b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        public Point Offset(float dx, float dy)
        {
            _pointF.X += dx;
            _pointF.Y += dy;
            return this;
        }
        public Point Offset(int dx, int dy)
        {
            _pointF.X += Convert.ToSingle(dx);
            _pointF.Y += Convert.ToSingle(dy);
            return this;
        }
        public Point Offset(Point p)
        {
            return this.Offset(p.X, p.Y);
        }
    }
}
