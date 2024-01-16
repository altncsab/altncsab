using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static NineMensMorris.Utils;

namespace NineMensMorris
{
    public partial class Computer : IOpponent
    {
        Dictionary<Position.PositionName, Position.PositionStruct> PositionList;
        List<FromTo> ScoreList;
        int manLeft = 9;
        int oponentManLeft = 9;
        Position.Status ownPiece;
        Position.Status opponentPiece;
        public event MessageArrivedHandler MessageArrived;
        Thread ClientThread;
        Thread MainThread;
        string messageToSend;
        string messageReceived;
        object messageToSendLock = new object();
        object messageReceivedLock = new object();
        ActionListMessage.ActionList ActionList;
        bool isRunning;
        private List<KeyValuePair<string, string>> opponentActionList;
        GameModeEnum GameMode = GameModeEnum.SetUp;
        public GameModeEnum OpponentGameMode;
        GameModeEnum GamePhase = GameModeEnum.SetUp;
        private FromTo bestStep;

        public bool IsRunning { get { return isRunning; } }
        public bool IsMessage { get { return messageToSend != null; } }

        public Computer(Position.Status side)
        {
            ownPiece = side;
            opponentPiece = Mill.GetOpponent(ownPiece);
            PositionList = InitPositionList();            
            ScoreList = new List<FromTo>();
            ClientThread = new Thread(ListenerThread);
            MainThread = Thread.CurrentThread;
            isRunning = true;
            ClientThread.Start();

        }

        private Dictionary<Position.PositionName, Position.PositionStruct> InitPositionList()
        {
            Dictionary<Position.PositionName, Position.PositionStruct> ret = new Dictionary<Position.PositionName, Position.PositionStruct>();

            return ret;
        }

        private void SetMessageArrived(string Message)
        {
            lock (messageReceivedLock)
            {
                messageReceived = Message;
            }
            MessageArrived?.Invoke(this, new MessageArrivedEventArgs(messageReceived));
        }
        private void ListenerThread()
        {
            try
            {
                bool gameStarted = false;
                while (isRunning)
                {
                    if (!string.IsNullOrEmpty(messageToSend))
                    {
                        ActionList = (ActionListMessage.ActionList)Utils.DeserializeXml(typeof(ActionListMessage.ActionList), messageToSend);
                        SendMessage(null);
                        if (ActionList.Header.IsAbort)
                        {
                            isRunning = false;
                            break;
                        }
                        if (!gameStarted && ActionList.Header.Message == "Start")
                        {
                            gameStarted = true;
                            FindStep();
                        }
                        if(ActionList.ActionList1 != null && ActionList.ActionList1.Length > 0)
                        {
                            
                            gameStarted = true;
                            opponentActionList = new List<KeyValuePair<string, string>>();
                            foreach (ActionListMessage.ActionListActionList dataRow in ActionList.ActionList1)
                            {
                                opponentActionList.Add(new KeyValuePair<string, string>(dataRow.GameMode.ToString(), dataRow.Position.ToString()));
                            }

                            ProcessOpponentActions();

                            if (Mill.IsLost(ownPiece, GamePhase, PositionList) || Mill.IsLost(opponentPiece, GamePhase, PositionList))
                            {
                                GameMode = GameModeEnum.End;
                                GamePhase = GameModeEnum.End;
                                isRunning = false;
                                break;
                            }
                            FindStep();
                        }
                    }
                    
                    Thread.Sleep(10);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                MessageArrived?.Invoke(this, new MessageArrivedEventArgs(ex));
            }
            finally
            {
                isRunning = false;
                bestStep?.Dispose();
            }
        }

        private void SendResult()
        {
            ActionListMessage.ActionList actionList = new ActionListMessage.ActionList();
            actionList.Header = new ActionListMessage.ActionListHeader() { HostName = "Computer", IsAbort = false };
            List<ActionListMessage.ActionListActionList> actionMessageList = new List<ActionListMessage.ActionListActionList>();
            ActionListMessage.Position position;
            if (bestStep != null)
            {
                if (GameMode == GameModeEnum.SetUp)
                {
                    position = (ActionListMessage.Position)Enum.Parse(typeof(ActionListMessage.Position), bestStep.To.ToString());
                    actionMessageList.Add(new ActionListMessage.ActionListActionList() { GameMode = ActionListMessage.GameMode.SetUp, Position = position });
                }
                else if (GameMode == GameModeEnum.Move || GameMode == GameModeEnum.Jump || GameMode == GameModeEnum.End)
                {
                    position = (ActionListMessage.Position)Enum.Parse(typeof(ActionListMessage.Position), bestStep.From.ToString());
                    actionMessageList.Add(new ActionListMessage.ActionListActionList() { GameMode = ActionListMessage.GameMode.Mark, Position = position });
                    position = (ActionListMessage.Position)Enum.Parse(typeof(ActionListMessage.Position), bestStep.To.ToString());
                    actionMessageList.Add(new ActionListMessage.ActionListActionList() { GameMode = GameMode == GameModeEnum.Move ? ActionListMessage.GameMode.Move : ActionListMessage.GameMode.Jump, Position = position });
                }
                if (bestStep.Remove != Position.PositionName.Nothing)
                {
                    position = (ActionListMessage.Position)Enum.Parse(typeof(ActionListMessage.Position), bestStep.Remove.ToString());
                    actionMessageList.Add(new ActionListMessage.ActionListActionList() { GameMode = ActionListMessage.GameMode.Remove, Position = position });
                }
                if (bestStep.Remove2 != Position.PositionName.Nothing)
                {
                    position = (ActionListMessage.Position)Enum.Parse(typeof(ActionListMessage.Position), bestStep.Remove2.ToString());
                    actionMessageList.Add(new ActionListMessage.ActionListActionList() { GameMode = ActionListMessage.GameMode.Remove2, Position = position });
                }
            }
            actionList.ActionList1 = actionMessageList.ToArray();
            string returnXML = Utils.SerializeXml(actionList, typeof(ActionListMessage.ActionList));
            SetMessageArrived(returnXML);
        }

        private void ProcessOpponentActions()
        {
            Position.PositionStruct markedOpponent = Position.NullPos;
            foreach (KeyValuePair<string, string> actionPair in opponentActionList)
            {
                GameModeEnum mode;
                Position.PositionName positionName;
                Position.PositionStruct position = Position.NullPos;
                if (Enum.GetNames(typeof(GameModeEnum)).Contains(actionPair.Key))
                {
                    mode = (GameModeEnum)Enum.Parse(typeof(GameModeEnum), actionPair.Key);
                }
                else
                {
                    throw new Exception("Error processing the opponent moves.Invalid game mode received: " + actionPair.Key);
                }
                if (Enum.GetNames(typeof(Position.PositionName)).Contains(actionPair.Value))
                {
                    positionName = (Position.PositionName)Enum.Parse(typeof(Position.PositionName), actionPair.Value);
                    position = Mill.GetPosition(positionName, PositionList);
                }
                else
                {
                    throw new Exception("Error processing the opponent moves. Invalid position:" + actionPair.Value);
                }
                if (position.positionName != Position.PositionName.Nothing)
                {
                    switch (mode)
                    {
                        case GameModeEnum.WaitOnOther:
                            break;
                        case GameModeEnum.SetUp:
                            if (position.CurrentStatus == Position.Status.Empty)
                            {
                                SetPositionList(position.positionName, opponentPiece);
                                oponentManLeft--;
                                OpponentGameMode = GameModeEnum.SetUp;
                            }
                            else
                            {
                                throw new Exception("Incorrect location from Opponent! Answer is invalid");
                            }
                            break;
                        case GameModeEnum.Mark:
                            if (position.CurrentStatus == opponentPiece)
                            {
                                markedOpponent = position;
                            }
                            else
                            {
                                throw new Exception("Mark failed on marking proper pieces");
                            }
                            break;
                        case GameModeEnum.Move:
                            if (markedOpponent.positionName != Position.PositionName.Nothing)
                            {
                                if (Mill.GetValidMoves(PositionList, markedOpponent).Exists(t=> t.positionName == position.positionName))
                                {
                                    if (position.CurrentStatus == Position.Status.Empty)
                                    {
                                        SetPositionList(markedOpponent.positionName, Position.Status.Empty);
                                        SetPositionList( position.positionName,  opponentPiece);
                                        markedOpponent = Position.NullPos;

                                        OpponentGameMode = GameModeEnum.Move;
                                    }
                                    else
                                    {
                                        throw new Exception("Move marks invalid position");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Invalid move sent");
                                }
                            }
                            else
                            {
                                throw new Exception("Marking status failed to set");
                            }
                            break;
                        case GameModeEnum.Remove:
                        case GameModeEnum.Remove2:
                            if (position.CurrentStatus == ownPiece)
                            {
                                if (Mill.IsInMill(ownPiece, position, PositionList) > 0)
                                {
                                    if (Mill.IsAllInMill(ownPiece, PositionList))
                                    {
                                        SetPositionList(position.positionName, Position.Status.Empty);
                                    }
                                    else
                                    {
                                        throw new Exception("Not all piece in Mill. Wrong remove");
                                    }
                                }
                                else
                                {
                                    SetPositionList(position.positionName, Position.Status.Empty);
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid Remove command arrived");
                            }
                            break;
                        case GameModeEnum.Jump:
                            if (position.CurrentStatus == Position.Status.Empty)
                            {
                                SetPositionList(markedOpponent.positionName, Position.Status.Empty);
                                SetPositionList(position.positionName, opponentPiece);
                                markedOpponent = Position.NullPos;
                                OpponentGameMode = GameModeEnum.Jump;
                            }
                            else
                            {
                                throw new Exception("Jump marks and invalid position");
                            }
                            break;
                        case GameModeEnum.End:
                            OpponentGameMode = GameModeEnum.End;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (manLeft > 0)
            {
                GameMode = GameModeEnum.SetUp;
            }
            else
            {
                int numPieces = Mill.NumberOfPieces(ownPiece, PositionList);
                GameMode = numPieces > 3 ? GameModeEnum.Move : numPieces == 3 ? GameModeEnum.Jump :  GameModeEnum.End;
                GamePhase = numPieces == 3 ? GameModeEnum.Jump : numPieces > 3 ? GameModeEnum.Move : GameModeEnum.End;
            }

        }
        private void SetPositionList(Position.PositionStruct str)
        {
            if (PositionList.ContainsKey(str.positionName))
            {
                if(str.CurrentStatus == Position.Status.Empty)
                {
                    PositionList.Remove(str.positionName);
                }
                else
                {
                    PositionList[str.positionName] = new Position.PositionStruct() { positionName = str.positionName, CurrentStatus = str.CurrentStatus };
                }
            }
            else if(str.CurrentStatus != Position.Status.Empty)
            {
                PositionList.Add(str.positionName, new Position.PositionStruct() { positionName = str.positionName, CurrentStatus = str.CurrentStatus });
            }
        }
        private void SetPositionList(Position.PositionName name, Position.Status status)
        {
           SetPositionList( new Position.PositionStruct() { positionName = name, CurrentStatus = status });
        }
        private void FindStep()
        {
            bool isRetry = false;
            do
            {
                isRetry = false;
                try
                {
                    using (FromTo ret = new FromTo()
                    {
                        CurrentAnalyzis = FromTo.CloneCurrentAnalyzis(PositionList),
                        ManLeft = this.oponentManLeft,
                        OpponentManLeft = manLeft,
                        GameMode = OpponentGameMode,
                        ownPiece = opponentPiece})
                    {
                        ret.FindInitialMoves(0);
                        if (ret.ResetEventList.Count > 0)
                        {
                            WaitHandle.WaitAll(ret.ResetEventList.ToArray(), -1, false);
                        }
                        Exception ex = ret.ScoreList.FirstOrDefault(t => t.CalculationException != null)?.CalculationException;
                        if(ex != null)
                        {
                            throw ex;
                        }
                        ret.GetCumulatedScore(0);
                        bestStep = ret.ScoreList.Max(t => t);
                        if (bestStep != null)
                        {
                            List<FromTo> beststeps = ret.ScoreList.Where(t => Math.Abs(t.CumulatedScore - bestStep.CumulatedScore) < 0.0001f).ToList();
                            if (beststeps.Count > 1)
                            {
                                Random rnd = new Random(DateTime.Now.Millisecond);
                                bestStep = beststeps[rnd.Next(0, beststeps.Count - 1)];
                            }
                        }
                        DoSteps();
                        SendResult();
                    }
                }
                catch (OutOfMemoryException)
                {
                    Mill.MaxDepth--;
                    bestStep?.Dispose();
                    isRetry = Mill.MaxDepth > 0;
                }
            } while (isRetry);

        }
        private void DoSteps()
        {
            if (GameMode == GameModeEnum.SetUp && manLeft == 0)
            {
                GameMode = GameModeEnum.Move;
            }
            if (bestStep != null)
            {
                switch (GameMode)
                {
                    case GameModeEnum.SetUp:
                        SetPositionList(bestStep.To, ownPiece);
                        manLeft--;
                        RemovePiece(bestStep);
                        break;
                    case GameModeEnum.Move:
                    case GameModeEnum.Jump:
                        SetPositionList(bestStep.From, Position.Status.Empty);
                        SetPositionList(bestStep.To, ownPiece);
                        RemovePiece(bestStep);
                        if (Mill.NumberOfPieces(ownPiece, PositionList) == 3)
                        {
                            GameMode = GameModeEnum.Jump;
                            GamePhase = GameModeEnum.Jump;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (Mill.IsLost(ownPiece, GameMode, PositionList) || Mill.IsLost(opponentPiece, GameMode, PositionList) || bestStep == null)
            {
                GameMode = GameModeEnum.End;
                GamePhase = GameModeEnum.End;
            }
        }

        private void RemovePiece(FromTo step)
        {
            if (step.Remove != Position.PositionName.Nothing)
            {
                SetPositionList(step.Remove, Position.Status.Empty);
            }
            if (step.Remove2 != Position.PositionName.Nothing)
            {
                SetPositionList(step.Remove2, Position.Status.Empty);
            }
        }
        public void Stop()
        {
            if(IsMessage)
            {
                Thread.Sleep(100);
                messageToSend = null;
            }
            isRunning = false;
            ClientThread.Join(100);
            ClientThread.Abort();
        }

        public void SendMessage(string Message)
        {
            lock (messageToSendLock)
            {
                messageToSend = Message;
            }
        }
    }
}
