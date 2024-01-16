using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NineMensMorris
{
    public static class Mill
    {
        static Mill()
        {
            InitRule();
            InitMove();
        }
        public static List<List<Position.PositionName>> MillRule { get; set; } = new List<List<Position.PositionName>>();
        public static Dictionary<Position.PositionName, List<Position.PositionName>> ValidMove { get; set; } = new Dictionary<Position.PositionName, List<Position.PositionName>>();
        public static int MaxDepth = 3;
        public static void AddRule(Position.PositionName a, Position.PositionName b, Position.PositionName c)
        {
            MillRule.Add(new List<Position.PositionName> { a, b, c });
        }

        public static int IsInMill(Position.Status color, Position.PositionStruct position, Dictionary<Position.PositionName, Position> positionList)
        {
            return IsInMill(color, position, Position.NullPos, positionList);
        }
        public static int IsInMill(Position.Status color, Position.PositionStruct position, Position.PositionStruct from, Dictionary<Position.PositionName, Position> positionList)
        {
            Dictionary<Position.PositionName, Position.PositionStruct> newPosList = positionList.ToDictionary(k => k.Key, v => v.Value.PositionStr);
            return IsInMill(color, position, from, newPosList);
        }
        public static int IsInMill(Position.Status color, Position.PositionStruct position, Dictionary<Position.PositionName, Position.PositionStruct> positionList)
        {
            return IsInMill(color, position, Position.NullPos, positionList);
        }
        public static int IsInMill(Position.Status color, Position.PositionStruct position, Position.PositionStruct from, Dictionary<Position.PositionName, Position.PositionStruct> positionList)
        {
            int ret = 0;
            foreach (List<Position.PositionName> rule in MillRule)
            {
                Position.PositionStruct Rule0 = GetPosition(rule[0], positionList);
                Position.PositionStruct Rule1 = GetPosition(rule[1], positionList);
                Position.PositionStruct Rule2 = GetPosition(rule[2], positionList);
                if (rule.Contains(position.positionName) && 
                    ((Rule0.positionName == position.positionName || (color == Rule0.CurrentStatus && from.positionName != Rule0.positionName)) &&
                    (Rule1.positionName == position.positionName || (color == Rule1.CurrentStatus && from.positionName != Rule1.positionName)) &&
                    (Rule2.positionName == position.positionName || (color == Rule2.CurrentStatus && from.positionName != Rule2.positionName))))
                {
                    ret++;                    
                }
            }
            return ret;
        }
        public static Position.PositionStruct GetPosition(Position.PositionName name, Dictionary<Position.PositionName, Position.PositionStruct> positionList)
        {
            if (positionList.ContainsKey(name))
            {
                return positionList[name];
            }
            else
            {
                return new Position.PositionStruct() { positionName = name, CurrentStatus = Position.Status.Empty };
            }
        }
        public static int MillCount(Position.Status color, Dictionary<Position.PositionName, Position.PositionStruct> positionList)
        {
            int ret = 0;
            foreach (List<Position.PositionName> rule in MillRule)
            {
                if (color == GetPosition(rule[0], positionList).CurrentStatus && color == GetPosition(rule[1], positionList).CurrentStatus && color == GetPosition(rule[2], positionList).CurrentStatus)
                {
                    ret++;
                }
            }
            return ret;
        }

        public static Position.Status GetOpponent(Position.Status ownPiece)
        {
            return ((Position.Status[])Enum.GetValues(typeof(Position.Status))).Where(t => !(t == Position.Status.Empty || t == ownPiece)).FirstOrDefault();
        }
        public static bool IsAllInMill(Position.Status status, Dictionary<Position.PositionName, Position> positionList)
        {
            Dictionary<Position.PositionName, Position.PositionStruct> newPosList = positionList.ToDictionary(k => k.Key, v => v.Value.PositionStr);
            return IsAllInMill(status, newPosList);
        }
        public static bool IsAllInMill(Position.Status status, Dictionary<Position.PositionName, Position.PositionStruct> positionList)
        {
            bool oret = false;
            foreach (Position.PositionStruct pos in positionList.Values.Where(t => t.CurrentStatus == status))
            {
                if (Mill.IsInMill(status, pos, positionList) > 0)
                {
                    oret = true;
                }
                else
                {
                    oret = false;
                    break;
                }
            }
            return oret;
        }
        public static bool IsLost(Position.Status own, Utils.GameModeEnum gamePhase, Dictionary<Position.PositionName, Position> positionList)
        {
            Dictionary<Position.PositionName, Position.PositionStruct> newPosList = positionList.ToDictionary(k => k.Key, v => v.Value.PositionStr);
            return IsLost(own, gamePhase, newPosList);
        }

        public static bool IsLost(Position.Status own, Utils.GameModeEnum gamePhase, Dictionary<Position.PositionName, Position.PositionStruct> positionList)
        {
            bool ret = false;
            if (gamePhase != Utils.GameModeEnum.SetUp)
            {
                int numPieces = NumberOfPieces(own, positionList);
                if (numPieces < 3)
                {
                    ret = true;
                }
                else if (numPieces > 3)
                {
                    bool found = false;
                    foreach (Position.PositionStruct pos in positionList.Values.Where(t => t.CurrentStatus == own))
                    {
                        foreach (Position.PositionStruct moves in GetValidMoves(positionList, pos))
                        {
                            if (moves.CurrentStatus == Position.Status.Empty)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                    ret = !found;
                }
            }
            return ret;
        }

        public static int NumberOfPieces(Position.Status side, Dictionary<Position.PositionName, Position.PositionStruct> positionList) => positionList.Values.Where(t => t.CurrentStatus == side).Count();
        public static int NumberOfPieces(Position.Status side, Dictionary<Position.PositionName, Position> positionList) => positionList.Values.Where(t => t.CurrentStatus == side).Count();
        public static Dictionary<Position.PositionName, Position> InitPositionList()
        {
            Dictionary<Position.PositionName, Position>  PositionList = new Dictionary<Position.PositionName, Position>();
            PositionList.Add(Position.PositionName.A1, new Position(Position.PositionName.A1, new Point(30, 30)));
            PositionList.Add(Position.PositionName.A2, new Position(Position.PositionName.A2, new Point(300, 30)));
            PositionList.Add(Position.PositionName.A3, new Position(Position.PositionName.A3, new Point(570, 30)));
            PositionList.Add(Position.PositionName.A4, new Position(Position.PositionName.A4, new Point(570, 300)));
            PositionList.Add(Position.PositionName.A5, new Position(Position.PositionName.A5, new Point(570, 570)));
            PositionList.Add(Position.PositionName.A6, new Position(Position.PositionName.A6, new Point(300, 570)));
            PositionList.Add(Position.PositionName.A7, new Position(Position.PositionName.A7, new Point(30, 570)));
            PositionList.Add(Position.PositionName.A8, new Position(Position.PositionName.A8, new Point(30, 300)));
            PositionList.Add(Position.PositionName.B1, new Position(Position.PositionName.B1, new Point(90, 90)));
            PositionList.Add(Position.PositionName.B2, new Position(Position.PositionName.B2, new Point(300, 90)));
            PositionList.Add(Position.PositionName.B3, new Position(Position.PositionName.B3, new Point(510, 90)));
            PositionList.Add(Position.PositionName.B4, new Position(Position.PositionName.B4, new Point(510, 300)));
            PositionList.Add(Position.PositionName.B5, new Position(Position.PositionName.B5, new Point(510, 510)));
            PositionList.Add(Position.PositionName.B6, new Position(Position.PositionName.B6, new Point(300, 510)));
            PositionList.Add(Position.PositionName.B7, new Position(Position.PositionName.B7, new Point(90, 510)));
            PositionList.Add(Position.PositionName.B8, new Position(Position.PositionName.B8, new Point(90, 300)));
            PositionList.Add(Position.PositionName.C1, new Position(Position.PositionName.C1, new Point(150, 150)));
            PositionList.Add(Position.PositionName.C2, new Position(Position.PositionName.C2, new Point(300, 150)));
            PositionList.Add(Position.PositionName.C3, new Position(Position.PositionName.C3, new Point(450, 150)));
            PositionList.Add(Position.PositionName.C4, new Position(Position.PositionName.C4, new Point(450, 300)));
            PositionList.Add(Position.PositionName.C5, new Position(Position.PositionName.C5, new Point(450, 450)));
            PositionList.Add(Position.PositionName.C6, new Position(Position.PositionName.C6, new Point(300, 450)));
            PositionList.Add(Position.PositionName.C7, new Position(Position.PositionName.C7, new Point(150, 450)));
            PositionList.Add(Position.PositionName.C8, new Position(Position.PositionName.C8, new Point(150, 300)));
            return PositionList;
        }
        public static List<Position.PositionStruct> GetValidMoves(Dictionary<Position.PositionName, Position.PositionStruct> PositionList, Position.PositionStruct positionStr)
        {
            List<Position.PositionStruct> ret = new List<Position.PositionStruct>();
            List<Position.PositionName> moves = ValidMove[positionStr.positionName];
            foreach (Position.PositionName move in moves)
            {
                ret.Add(GetPosition(move, PositionList));
            }
            return ret;
        }
        public static void InitMoves(Dictionary<Position.PositionName,Position> PositionList)
        {
            PositionList[Position.PositionName.A1].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A2].PositionStr, PositionList[Position.PositionName.A8].PositionStr };
            PositionList[Position.PositionName.A2].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A1].PositionStr, PositionList[Position.PositionName.A3].PositionStr, PositionList[Position.PositionName.B2].PositionStr };
            PositionList[Position.PositionName.A3].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A2].PositionStr, PositionList[Position.PositionName.A4].PositionStr };
            PositionList[Position.PositionName.A4].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A3].PositionStr, PositionList[Position.PositionName.A5].PositionStr, PositionList[Position.PositionName.B4].PositionStr };
            PositionList[Position.PositionName.A5].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A4].PositionStr, PositionList[Position.PositionName.A6].PositionStr };
            PositionList[Position.PositionName.A6].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A5].PositionStr, PositionList[Position.PositionName.A7].PositionStr, PositionList[Position.PositionName.B6].PositionStr };
            PositionList[Position.PositionName.A7].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A6].PositionStr, PositionList[Position.PositionName.A8].PositionStr };
            PositionList[Position.PositionName.A8].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.A7].PositionStr, PositionList[Position.PositionName.A1].PositionStr, PositionList[Position.PositionName.B8].PositionStr };
            PositionList[Position.PositionName.B1].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B2].PositionStr, PositionList[Position.PositionName.B8].PositionStr };
            PositionList[Position.PositionName.B2].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B1].PositionStr, PositionList[Position.PositionName.B3].PositionStr, PositionList[Position.PositionName.A2].PositionStr, PositionList[Position.PositionName.C2].PositionStr };
            PositionList[Position.PositionName.B3].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B2].PositionStr, PositionList[Position.PositionName.B4].PositionStr };
            PositionList[Position.PositionName.B4].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B3].PositionStr, PositionList[Position.PositionName.B5].PositionStr, PositionList[Position.PositionName.A4].PositionStr, PositionList[Position.PositionName.C4].PositionStr };
            PositionList[Position.PositionName.B5].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B4].PositionStr, PositionList[Position.PositionName.B6].PositionStr };
            PositionList[Position.PositionName.B6].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B5].PositionStr, PositionList[Position.PositionName.B7].PositionStr, PositionList[Position.PositionName.A6].PositionStr, PositionList[Position.PositionName.C6].PositionStr };
            PositionList[Position.PositionName.B7].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B6].PositionStr, PositionList[Position.PositionName.B8].PositionStr };
            PositionList[Position.PositionName.B8].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.B7].PositionStr, PositionList[Position.PositionName.B1].PositionStr, PositionList[Position.PositionName.A8].PositionStr, PositionList[Position.PositionName.C8].PositionStr };
            PositionList[Position.PositionName.C1].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C2].PositionStr, PositionList[Position.PositionName.C8].PositionStr };
            PositionList[Position.PositionName.C2].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C1].PositionStr, PositionList[Position.PositionName.C3].PositionStr, PositionList[Position.PositionName.B2].PositionStr };
            PositionList[Position.PositionName.C3].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C2].PositionStr, PositionList[Position.PositionName.C4].PositionStr };
            PositionList[Position.PositionName.C4].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C3].PositionStr, PositionList[Position.PositionName.C5].PositionStr, PositionList[Position.PositionName.B4].PositionStr };
            PositionList[Position.PositionName.C5].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C4].PositionStr, PositionList[Position.PositionName.C6].PositionStr };
            PositionList[Position.PositionName.C6].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C5].PositionStr, PositionList[Position.PositionName.C7].PositionStr, PositionList[Position.PositionName.B6].PositionStr };
            PositionList[Position.PositionName.C7].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C6].PositionStr, PositionList[Position.PositionName.C8].PositionStr };
            PositionList[Position.PositionName.C8].ValidMoves = new List<Position.PositionStruct> { PositionList[Position.PositionName.C7].PositionStr, PositionList[Position.PositionName.C1].PositionStr, PositionList[Position.PositionName.B8].PositionStr };
        }
        public static void InitMove()
        {
            ValidMove.Add(Position.PositionName.A1, new List<Position.PositionName>() {Position.PositionName.A2, Position.PositionName.A8 });
            ValidMove.Add(Position.PositionName.A2, new List<Position.PositionName> { Position.PositionName.A1, Position.PositionName.A3, Position.PositionName.B2 });
            ValidMove.Add(Position.PositionName.A3, new List<Position.PositionName> { Position.PositionName.A2, Position.PositionName.A4 });
            ValidMove.Add(Position.PositionName.A4 , new List<Position.PositionName> { Position.PositionName.A3, Position.PositionName.A5, Position.PositionName.B4 });
            ValidMove.Add(Position.PositionName.A5 , new List<Position.PositionName> { Position.PositionName.A4, Position.PositionName.A6 });
            ValidMove.Add(Position.PositionName.A6 , new List<Position.PositionName> { Position.PositionName.A5, Position.PositionName.A7, Position.PositionName.B6 });
            ValidMove.Add(Position.PositionName.A7 , new List<Position.PositionName> { Position.PositionName.A6, Position.PositionName.A8 });
            ValidMove.Add(Position.PositionName.A8 , new List<Position.PositionName> { Position.PositionName.A7, Position.PositionName.A1, Position.PositionName.B8 });
            ValidMove.Add(Position.PositionName.B1 , new List<Position.PositionName> { Position.PositionName.B2, Position.PositionName.B8 });
            ValidMove.Add(Position.PositionName.B2 , new List<Position.PositionName> { Position.PositionName.B1, Position.PositionName.B3, Position.PositionName.A2, Position.PositionName.C2 });
            ValidMove.Add(Position.PositionName.B3 , new List<Position.PositionName> { Position.PositionName.B2, Position.PositionName.B4 });
            ValidMove.Add(Position.PositionName.B4 , new List<Position.PositionName> { Position.PositionName.B3, Position.PositionName.B5, Position.PositionName.A4, Position.PositionName.C4 });
            ValidMove.Add(Position.PositionName.B5 , new List<Position.PositionName> { Position.PositionName.B4, Position.PositionName.B6 });
            ValidMove.Add(Position.PositionName.B6 , new List<Position.PositionName> { Position.PositionName.B5, Position.PositionName.B7, Position.PositionName.A6, Position.PositionName.C6 });
            ValidMove.Add(Position.PositionName.B7 , new List<Position.PositionName> { Position.PositionName.B6, Position.PositionName.B8 });
            ValidMove.Add(Position.PositionName.B8 , new List<Position.PositionName> { Position.PositionName.B7, Position.PositionName.B1, Position.PositionName.A8, Position.PositionName.C8 });
            ValidMove.Add(Position.PositionName.C1 , new List<Position.PositionName> { Position.PositionName.C2, Position.PositionName.C8 });
            ValidMove.Add(Position.PositionName.C2 , new List<Position.PositionName> { Position.PositionName.C1, Position.PositionName.C3, Position.PositionName.B2 });
            ValidMove.Add(Position.PositionName.C3 , new List<Position.PositionName> { Position.PositionName.C2, Position.PositionName.C4 });
            ValidMove.Add(Position.PositionName.C4 , new List<Position.PositionName> { Position.PositionName.C3, Position.PositionName.C5, Position.PositionName.B4 });
            ValidMove.Add(Position.PositionName.C5 , new List<Position.PositionName> { Position.PositionName.C4, Position.PositionName.C6 });
            ValidMove.Add(Position.PositionName.C6 , new List<Position.PositionName> { Position.PositionName.C5, Position.PositionName.C7, Position.PositionName.B6 });
            ValidMove.Add(Position.PositionName.C7 , new List<Position.PositionName> { Position.PositionName.C6, Position.PositionName.C8 });
            ValidMove.Add(Position.PositionName.C8 , new List<Position.PositionName> { Position.PositionName.C7, Position.PositionName.C1, Position.PositionName.B8 });
        }
        private static void InitRule()
        {
            Mill.AddRule(Position.PositionName.A1, Position.PositionName.A2, Position.PositionName.A3);
            Mill.AddRule(Position.PositionName.B1, Position.PositionName.B2, Position.PositionName.B3);
            Mill.AddRule(Position.PositionName.C1, Position.PositionName.C2, Position.PositionName.C3);
            Mill.AddRule(Position.PositionName.A2, Position.PositionName.B2, Position.PositionName.C2);
            Mill.AddRule(Position.PositionName.A3, Position.PositionName.A4, Position.PositionName.A5);
            Mill.AddRule(Position.PositionName.B3, Position.PositionName.B4, Position.PositionName.B5);
            Mill.AddRule(Position.PositionName.C3, Position.PositionName.C4, Position.PositionName.C5);
            Mill.AddRule(Position.PositionName.A4, Position.PositionName.B4, Position.PositionName.C4);
            Mill.AddRule(Position.PositionName.A5, Position.PositionName.A6, Position.PositionName.A7);
            Mill.AddRule(Position.PositionName.B5, Position.PositionName.B6, Position.PositionName.B7);
            Mill.AddRule(Position.PositionName.C5, Position.PositionName.C6, Position.PositionName.C7);
            Mill.AddRule(Position.PositionName.A6, Position.PositionName.B6, Position.PositionName.C6);
            Mill.AddRule(Position.PositionName.A7, Position.PositionName.A8, Position.PositionName.A1);
            Mill.AddRule(Position.PositionName.B7, Position.PositionName.B8, Position.PositionName.B1);
            Mill.AddRule(Position.PositionName.C7, Position.PositionName.C8, Position.PositionName.C1);
            Mill.AddRule(Position.PositionName.A8, Position.PositionName.B8, Position.PositionName.C8);
        }

    }
}
