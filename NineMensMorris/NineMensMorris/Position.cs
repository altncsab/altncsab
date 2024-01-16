using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NineMensMorris
{
    public class Position : IComparer<Position> , ICloneable
    {
        public struct PositionStruct
        {
           public PositionName positionName;
           public Status CurrentStatus;
        }
        public static Position.PositionStruct NullPos = new Position.PositionStruct() { positionName = Position.PositionName.Nothing, CurrentStatus = Position.Status.Empty };

        public enum PositionName
        {
            Nothing,
            A1, A2, A3, A4, A5, A6, A7, A8,
            B1, B2, B3, B4, B5, B6, B7, B8,
            C1, C2, C3, C4, C5, C6, C7, C8
        }
        PositionStruct positionStr;
        public static IEnumerable<PositionName> PositionList
        {
            get { return ((PositionName[])Enum.GetValues(typeof(PositionName))).Where(t=>t != PositionName.Nothing); }
        }
        public PositionStruct PositionStr { get {return positionStr; } }
        public List<PositionStruct> ValidMoves { get; set; }
        public Point Location { get; set; }
        public enum Status
        {
            Empty,
            Black,
            White
        }
        public Rectangle Hotspot { get; private set; }
        Status currentStatus;
        public Status CurrentStatus { get { return currentStatus; } set { currentStatus = value; positionStr.CurrentStatus = value; } }
        PositionName name;
        public PositionName Name { get { return name; } set { name = value; positionStr.positionName = value; } }
        public Position(PositionName name, Point location)
        {
            Location = location;
            Name = name;
            Hotspot = new Rectangle((location - 45).PointI, new Size(90, 90));
            positionStr = new PositionStruct() { CurrentStatus = Status.Empty, positionName = Name };
        }
        public Position(PositionStruct positionStruct)
        {
            positionStr = positionStruct;
            Name = PositionStr.positionName;
            CurrentStatus = positionStruct.CurrentStatus;
        }
        public bool IsHit(Point p)
        {
            return Hotspot.Contains(p.PointI);
        }
        public bool IsValidMove(PositionStruct p)
        {
            return ValidMoves.Contains(p) && p.CurrentStatus == Status.Empty;
        }
        public int Compare(Position x, Position y)
        {
            return string.Compare(x.Name.ToString(), y.Name.ToString(), StringComparison.CurrentCulture);
        }

        public object Clone()
        {
            return new Position(this.Name, this.Location) { CurrentStatus = this.CurrentStatus};
        }
    }
}
