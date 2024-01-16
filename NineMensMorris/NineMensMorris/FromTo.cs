using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static NineMensMorris.Utils;

namespace NineMensMorris
{
    public partial class Computer
    {
        private class FromTo : IComparable<FromTo>, IDisposable
        {
            private class ScoreComparer : IComparer<FromTo>
            {
                public int Compare(FromTo x, FromTo y)
                {
                    return x.Score.CompareTo(y.Score);
                }
            }
            public FromTo Parent;
            public Position.PositionName From = Position.PositionName.Nothing;
            public Position.PositionName To = Position.PositionName.Nothing;
            public Position.PositionName Remove = Position.PositionName.Nothing;
            public Position.PositionName Remove2 = Position.PositionName.Nothing;
            public float Score;
            public float CumulatedScore;
            public int ManLeft;
            public int OpponentManLeft;
            public Dictionary<Position.PositionName, Position.PositionStruct> CurrentAnalyzis;
            public Position.Status ownPiece;
            public Position.Status OpponentPiece
            {
                get
                {
                    return Mill.GetOpponent(ownPiece);
                }
            }
            public GameModeEnum GameMode = GameModeEnum.SetUp;
            public Exception CalculationException;
            public List<FromTo> ScoreList = new List<FromTo>();
            static object ResetEventListLock = new object();
            static List<ManualResetEvent> _ResetEventList;
            public List<ManualResetEvent> ResetEventList {
                get
                {
                    lock (ResetEventListLock)
                    {
                        return _ResetEventList;
                    }
                }
                private set { _ResetEventList = value;}
            }
            private bool isdisposing;            
            public FromTo()
            {

            }
            public FromTo(FromTo fromTo)
            {
                Parent = fromTo;
                ManLeft = fromTo.OpponentManLeft;
                OpponentManLeft = fromTo.ManLeft;
                CurrentAnalyzis = CloneCurrentAnalyzis(fromTo.CurrentAnalyzis);
                ownPiece = fromTo.OpponentPiece;
            }
            
            private void CalculateScore()
            {
                Score += (Mill.NumberOfPieces(ownPiece, CurrentAnalyzis) - Mill.NumberOfPieces(OpponentPiece, CurrentAnalyzis));
                Score += ((Mill.MillCount(ownPiece, CurrentAnalyzis) * 10));
                Score += ((Mill.MillCount(ownPiece, CurrentAnalyzis) - Mill.MillCount(OpponentPiece, CurrentAnalyzis)) * 50);
                if (this.Parent?.Parent != null)
                {
                    Score += Math.Abs(((Mill.MillCount(ownPiece, CurrentAnalyzis) - Mill.MillCount(ownPiece, Parent.Parent.CurrentAnalyzis)) * 10));
                    Score += ((Mill.NumberOfPieces(ownPiece, CurrentAnalyzis) - Mill.NumberOfPieces(ownPiece, Parent.Parent.CurrentAnalyzis)) * 100);
                    Score += ((Mill.NumberOfPieces(OpponentPiece, Parent.Parent.CurrentAnalyzis) - Mill.NumberOfPieces(OpponentPiece, CurrentAnalyzis)) * 100);
                }
                Score += (Mill.IsLost(OpponentPiece, GameMode, CurrentAnalyzis) ? 1000 : 0);
                Score += (Mill.IsLost(ownPiece, GameMode, CurrentAnalyzis) ? -1000 : 0);
            }

            public void DoSteps()
            {
                if (GameMode == GameModeEnum.SetUp && ManLeft == 0)
                {
                    GameMode = GameModeEnum.Move;
                }
                switch (GameMode)
                {
                    case GameModeEnum.SetUp:
                        SetCurrentAnalyzis(To, ownPiece);
                        ManLeft--;
                        if (ManLeft == 0)
                        {
                            GameMode = GameModeEnum.Move;
                        }
                        RemovePiece();
                        break;
                    case GameModeEnum.Move:
                    case GameModeEnum.Jump:
                        SetCurrentAnalyzis(From, Position.Status.Empty);
                        SetCurrentAnalyzis(To, ownPiece);
                        RemovePiece();
                        if (Mill.NumberOfPieces(ownPiece, CurrentAnalyzis) == 3)
                        {
                            GameMode = GameModeEnum.Jump;
                        }
                        break;
                    default:
                        break;
                }
                CalculateScore();
                if (Mill.IsLost(ownPiece, GameMode, CurrentAnalyzis) || Mill.IsLost(OpponentPiece, GameMode, CurrentAnalyzis))
                {
                    GameMode = GameModeEnum.End;
                }
            }
            private void SetCurrentAnalyzis(Position.PositionName name, Position.Status status)
            {
                if (CurrentAnalyzis.ContainsKey(name))
                {
                    if (status == Position.Status.Empty)
                    {
                        CurrentAnalyzis.Remove(name);
                    }
                    else
                    {
                        CurrentAnalyzis[name] = new Position.PositionStruct() { positionName = name, CurrentStatus = status };
                    }
                }
                else if(status != Position.Status.Empty)
                {
                    CurrentAnalyzis.Add(name, new Position.PositionStruct() { positionName = name, CurrentStatus = status });
                }
            }
            private Position.PositionStruct GetCurrentAnalysis(Position.PositionName name)
            {
                if(CurrentAnalyzis.ContainsKey(name))
                {
                    return CurrentAnalyzis[name];
                }
                else
                {
                    return new Position.PositionStruct() { positionName = name, CurrentStatus = Position.Status.Empty };
                }
            }
            private void RemovePiece()
            {
                if (Remove != Position.PositionName.Nothing)
                {
                    SetCurrentAnalyzis(Remove, Position.Status.Empty);
                }
                if (Remove2 != Position.PositionName.Nothing)
                {
                    SetCurrentAnalyzis(Remove2, Position.Status.Empty);
                }
            }

            public static Dictionary<Position.PositionName, Position.PositionStruct> CloneCurrentAnalyzis(Dictionary<Position.PositionName, Position.PositionStruct> currentAnalyzis)
            {
                Dictionary<Position.PositionName, Position.PositionStruct> clonedAnalyzis = new Dictionary<Position.PositionName, Position.PositionStruct>(currentAnalyzis.Count);
                foreach (Position.PositionStruct pos in currentAnalyzis.Values.Where(t=> t.CurrentStatus != Position.Status.Empty))
                {
                    clonedAnalyzis.Add(pos.positionName, new Position.PositionStruct() { positionName = pos.positionName, CurrentStatus = pos.CurrentStatus });
                }                
                return clonedAnalyzis;
            }
            static void FindInitialMoveCallBack(object stateInfo)
            {
                ManualResetEvent manualResetEvent = null;
                FromTo move = null;
                try
                {
                    int level = (int)((object[])stateInfo)[0];
                    move = (FromTo)((object[])stateInfo)[1];
                    manualResetEvent = (ManualResetEvent)((object[])stateInfo)[2];
                    move.FindInitialMoves(level);
                }
                catch (Exception ex)
                {
                    if (move != null)
                    { move.CalculationException = ex; }
                    else
                    { throw ex; }
                }
                finally
                {
                    manualResetEvent?.Set();
                }
                
            }
            public void FindInitialMoves(int level)
            {
                if (level == 0)
                {
                    ResetEventList = new List<ManualResetEvent>();
                }
                GenerateMoves();
                int newLevel = level + 1;
                if (newLevel <= Mill.MaxDepth)
                {
                    if (ScoreList.Count > 0 && ScoreList.Exists(t => t.Score.CompareTo(ScoreList[0].Score) != 0)) 
                    {
                        ScoreList.Sort(new ScoreComparer());
                        ScoreList.Reverse();
                    }
                    int count = 0;
                    foreach (FromTo move in ScoreList)
                    {
                        if (level == 0)
                        {
                            ManualResetEvent resetEvent = new ManualResetEvent(false);
                            ResetEventList.Add(resetEvent);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(FindInitialMoveCallBack), new object[] { newLevel, move, resetEvent });
                        }
                        else
                        {
                            move.FindInitialMoves(newLevel);
                            if (count > 65 / (newLevel)) break;
                        }
                        count++;
                        if (count > 65) break;
                    }
                }
            }

            private void GenerateMoves()
            {
                if (this.OpponentManLeft > 0)
                {
                    foreach (Position.PositionName name in Position.PositionList)
                    {
                        Position.PositionStruct pos = GetCurrentAnalysis(name);
                        if (pos.CurrentStatus == Position.Status.Empty)
                        {
                            int mills = Mill.IsInMill(OpponentPiece, pos, CurrentAnalyzis);
                            FromTo newMove;
                            if (mills > 0)
                            {
                                ScoreList.AddRange(RemovePiece(name, Position.PositionName.Nothing, mills));
                            }
                            else
                            {
                                newMove = new FromTo(this) { To = name };
                                newMove.DoSteps();
                                ScoreList.Add(newMove);
                            }
                        }
                    }
                }
                else
                {
                    int numberOfOpponent = Mill.NumberOfPieces(this.OpponentPiece, CurrentAnalyzis);
                    if (numberOfOpponent > 3)
                    {
                        foreach (Position.PositionName name in this.CurrentAnalyzis.Keys)
                        {
                            Position.PositionStruct pos = this.CurrentAnalyzis[name];
                            if (pos.CurrentStatus == this.OpponentPiece)
                            {
                                foreach (Position.PositionStruct steps in Mill.GetValidMoves(CurrentAnalyzis, pos))
                                {
                                    if (steps.CurrentStatus == Position.Status.Empty)
                                    {
                                        int mills = Mill.IsInMill(OpponentPiece, steps, pos, CurrentAnalyzis);
                                        FromTo newMove;
                                        if (mills > 0)
                                        {
                                            ScoreList.AddRange(RemovePiece(steps.positionName, pos.positionName, mills));
                                        }
                                        else
                                        {
                                            newMove = new FromTo(this) { From = pos.positionName, To = steps.positionName};
                                            newMove.DoSteps();
                                            ScoreList.Add(newMove);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if(numberOfOpponent == 3)
                    {
                        foreach (Position.PositionName name in Position.PositionList)
                        {
                            Position.PositionStruct pos = GetCurrentAnalysis(name);

                            foreach (Position.PositionName ToPosName in Position.PositionList)
                            {
                                Position.PositionStruct ToPos = GetCurrentAnalysis(ToPosName);
                                if (pos.positionName != ToPos.positionName && pos.CurrentStatus == this.OpponentPiece && ToPos.CurrentStatus == Position.Status.Empty)
                                {
                                    int mills = Mill.IsInMill(OpponentPiece, ToPos, pos, CurrentAnalyzis);
                                    FromTo newMove;
                                    if (mills > 0)
                                    {
                                        ScoreList.AddRange(RemovePiece(ToPosName, pos.positionName, mills));
                                    }
                                    else
                                    {
                                        newMove = new FromTo(this) { From = pos.positionName, To = ToPosName };
                                        newMove.DoSteps();
                                        ScoreList.Add(newMove);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private List<FromTo> RemovePiece(Position.PositionName toName, Position.PositionName fromPos, int removes)
            {
                List<FromTo> ret = new List<FromTo>();
                Position.PositionName remove1 = Position.PositionName.Nothing;
                Position.PositionName remove2 = Position.PositionName.Nothing;
                bool isAllInMill = Mill.IsAllInMill(ownPiece, CurrentAnalyzis);
                FromTo newMove;
                foreach (Position.PositionStruct toRemove in this.CurrentAnalyzis.Values.Where(t => t.CurrentStatus == ownPiece))
                {
                    if ((isAllInMill || 0 == Mill.IsInMill(ownPiece, toRemove, fromPos != Position.PositionName.Nothing ? GetCurrentAnalysis(fromPos) : Position.NullPos, CurrentAnalyzis)) && remove2 != toRemove.positionName)
                    {
                        if (removes > 1)
                        {
                            foreach (Position.PositionStruct toRemove2 in this.CurrentAnalyzis.Values.Where(t => t.CurrentStatus == ownPiece && t.positionName!= remove1))
                            {
                                newMove = new FromTo(this) { From = fromPos, To = toName, Remove = toRemove.positionName, Remove2 = toRemove2.positionName};
                                newMove.DoSteps();
                                ret.Add(newMove);
                            }
                        }
                        else
                        {
                            newMove = new FromTo(this) { From = fromPos, To = toName, Remove = toRemove.positionName };
                            newMove.DoSteps();
                            ret.Add(newMove);
                        }
                    }
                }
                return ret;
            }

            public float GetCumulatedScore(int level)
            {
                float ret = 0;
                List<float> scoreList = new List<float>();
                int newLevel = level + 1;
                if (ScoreList.Count > 0)
                {
                    foreach (FromTo move in ScoreList)
                    {
                        scoreList.Add(move.GetCumulatedScore(newLevel));
                    }
                    ret = Score - (scoreList.Average() / 2);
                }
                else
                {
                    ret = Score;
                }
                CumulatedScore = ret;
                return ret;
            }

            public int Compare(FromTo x, FromTo y)
            {
                return x.CumulatedScore.CompareTo(y.CumulatedScore);
            }

            public int CompareTo(FromTo other)
            {
                return this.CumulatedScore.CompareTo(other.CumulatedScore);
            }

            public void Dispose()
            {
                if (!isdisposing)
                {
                    isdisposing = true;
                    Dispose(isdisposing);
                    GC.SuppressFinalize(this);
                }
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    CurrentAnalyzis = null;
                    foreach (FromTo move in ScoreList)
                    {
                        move.Dispose();
                    }
                    ScoreList = null;
                }
            }

        }
    }
}
