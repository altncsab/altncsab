using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static NineMensMorris.Utils;

namespace NineMensMorris
{
    public partial class frmNineMensMorris : Form
    {
        Dictionary<Position.PositionName, Position> PositionList;
        List<KeyValuePair<string, string>> actionList = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> opponentActionList = new List<KeyValuePair<string, string>>();
        Position.Status ownSide = Position.Status.White;
        Position.Status opponentSide = Position.Status.Black;

        private delegate void processMessageArrived(MessageArrivedEventArgs args);
        private delegate void writeMessage(string msg);
        private delegate void writeImage(Image img, Point point);
        private delegate void textToGraphics(string text, string fontName, int size, FontStyle fontStyle, PointF pointF);

        IOpponent CurrentOpponent;
        string OpponentName;
        GameModeEnum GameMode = GameModeEnum.SetUp;
        GameModeEnum GamePhase = GameModeEnum.SetUp;
        RemoteMode remoteMode = RemoteMode.Computer;
        Thread animationThread;
        bool initMill;
        bool? isWin;
        int manLeft = 9;
        int oponentManLeft = 9;
        Position marked;
        public frmNineMensMorris()
        {
            InitializeComponent();
            InitGame();
        }

        private void InitGame()
        {
            PositionList = Mill.InitPositionList();
            Mill.InitMoves(PositionList);
            WriteStatus("Started");
            CurrentOpponent = new Computer(Position.Status.Black);
            CurrentOpponent.MessageArrived += CurrentOpponent_MessageArrived;
            animationThread = new Thread(AnimateMill);
            animationThread.Start(Thread.CurrentThread);
            isWin = null;
        }

        private void pbBoard_MouseClick(object sender, MouseEventArgs e)
        {
            Point PozintonImage;
            Position position;
            string message = string.Empty;
            if (!e.Location.IsEmpty)
            {
                switch (GameMode)
                {
                    case GameModeEnum.WaitOnOther:
                        message = "Waiting for other player";
                        break;
                    case GameModeEnum.SetUp:
                        PozintonImage = ImagePoint(new Point(e.Location));
                        position = PositionList.FirstOrDefault(t => t.Value.IsHit(PozintonImage)).Value;
                        if (position != null)
                        {
                            if (position.CurrentStatus == Position.Status.Empty)
                            {
                                position.CurrentStatus = ownSide;
                                DrawPosition(position, false);
                                AddToActionList(GameMode, position.Name, true);
                                manLeft--;
                                UpdatePieceStatus(ownSide, manLeft);
                                int numberOfMills = Mill.IsInMill(ownSide, position.PositionStr, PositionList);
                                if (numberOfMills > 0)
                                {
                                    GameMode = numberOfMills == 1 ? GameModeEnum.Remove : GameModeEnum.Remove2;
                                    message = "Remove an opponent piece";
                                }
                                else
                                {
                                    OpponentTurn();                                    
                                }
                            }
                            else
                            {
                                message = "Choose an empty location!";
                            }
                        }
                        break;
                    case GameModeEnum.Mark:
                        PozintonImage = ImagePoint(new Point(e.Location));
                        position = PositionList.FirstOrDefault(t => t.Value.IsHit(PozintonImage)).Value;
                        if (position != null)
                        {
                            if (position.CurrentStatus == ownSide)
                            {
                                marked = position;
                                DrawPosition(position, true);
                                AddToActionList(GameMode, position.Name, true);
                                GameMode = Mill.NumberOfPieces(ownSide, PositionList) > 3 ? GameModeEnum.Move : GameModeEnum.Jump;
                            }
                            else
                            {
                                message = "Mark your own pieces";
                            }
                        }
                        break;
                    case GameModeEnum.Move:
                        if (marked != null)
                        {
                            PozintonImage = ImagePoint(new Point(e.Location));
                            position = PositionList.FirstOrDefault(t => t.Value.IsHit(PozintonImage)).Value;
                            if (position != null)
                            {
                                if (position == marked)
                                {
                                    marked = null;
                                    DrawPosition(position, false);
                                    message = "Mark Removed";
                                    GameMode = GameModeEnum.Mark;
                                    actionList.RemoveAt(actionList.Count - 1);
                                }
                                else
                                {
                                    if (marked.ValidMoves.Exists(t=> t.positionName == position.Name))
                                    {
                                        if (position.CurrentStatus == Position.Status.Empty)
                                        {
                                            marked.CurrentStatus = Position.Status.Empty;
                                            position.CurrentStatus = ownSide;
                                            DrawPosition(marked, false);
                                            marked = null;
                                            DrawPosition(position, false);
                                            AddToActionList(GameMode, position.Name, false);
                                            if (Mill.IsInMill(ownSide, position.PositionStr, PositionList) > 0)
                                            {
                                                GameMode = GameModeEnum.Remove;
                                                message = "Remove an opponent piece";
                                            }
                                            else
                                            {
                                                GamePhase = GameModeEnum.Move;
                                                OpponentTurn();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        message = "not a valid move";
                                    }
                                }
                            }
                        }
                        else
                        {
                            GameMode = GameModeEnum.Mark;
                        }
                        break;
                    case GameModeEnum.Jump:
                        if (marked != null)
                        {
                            PozintonImage = ImagePoint(new Point(e.Location));
                            position = PositionList.FirstOrDefault(t => t.Value.IsHit(PozintonImage)).Value;
                            if (position != null)
                            {
                                if (position == marked)
                                {
                                    marked = null;
                                    DrawPosition(position, false);
                                    message = "Mark Removed";
                                    GameMode = GameModeEnum.Mark;
                                }
                                else
                                {
                                    if (position.CurrentStatus == Position.Status.Empty)
                                    {
                                        marked.CurrentStatus = Position.Status.Empty;
                                        position.CurrentStatus = ownSide;
                                        DrawPosition(marked, false);
                                        marked = null;
                                        DrawPosition(position, false);
                                        AddToActionList(GameMode, position.Name, false);
                                        if (Mill.IsInMill(ownSide, position.PositionStr, PositionList) > 0)
                                        {
                                            GameMode = GameModeEnum.Remove;
                                            message = "Remove an opponent piece";
                                        }
                                        else
                                        {
                                            GamePhase = GameModeEnum.Jump;
                                            OpponentTurn();
                                        }
                                    }
                                    else
                                    {
                                        message = "Not a valid move";
                                    }
                                }
                            }
                        }
                        break;
                    case GameModeEnum.Remove:
                    case GameModeEnum.Remove2:
                        PozintonImage = ImagePoint(new Point(e.Location));
                        position = PositionList.FirstOrDefault(t => t.Value.IsHit(PozintonImage)).Value;
                        if (position != null)
                        {
                            if (position.CurrentStatus == opponentSide)
                            {
                                if (Mill.IsInMill(opponentSide, position.PositionStr, PositionList) > 0)
                                {
                                    if (Mill.IsAllInMill(opponentSide, PositionList))
                                    {
                                        position.CurrentStatus = Position.Status.Empty;
                                        DrawPosition(position, false);
                                        AddToActionList(GameMode, position.Name, false);
                                        GameMode = GameMode == GameModeEnum.Remove2 ? GameModeEnum.Remove : GameModeEnum.WaitOnOther;
                                        if (GameMode == GameModeEnum.WaitOnOther)
                                        {
                                            OpponentTurn();
                                        }
                                    }
                                }
                                else
                                {
                                    position.CurrentStatus = Position.Status.Empty;
                                    DrawPosition(position, false);
                                    AddToActionList(GameMode, position.Name, false);
                                    GameMode = GameMode == GameModeEnum.Remove2 ? GameModeEnum.Remove : GameModeEnum.WaitOnOther;
                                    if (GameMode == GameModeEnum.WaitOnOther)
                                    {
                                        OpponentTurn();
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                if (!(GameMode == GameModeEnum.Remove || GameMode == GameModeEnum.Remove2 || GameMode == GameModeEnum.Mark))
                {
                    if (Mill.IsLost(ownSide, GamePhase, PositionList))
                    {
                        GameMode = GameModeEnum.End;
                        GamePhase = GameModeEnum.End;
                        message = "Game Lost";
                        YouLost();
                    }
                    if (Mill.IsLost(opponentSide, GamePhase, PositionList))
                    {
                        GameMode = GameModeEnum.End;
                        GamePhase = GameModeEnum.End;
                        message = "Game Win";
                        YouWin();
                    }
                }
                WriteStatus(message);
            }
        }
        private void ProcessOpponentActions()
        {
            Position markedOpponent = null;
            foreach (KeyValuePair<string, string> actionPair in opponentActionList)
            {
                GameModeEnum mode;
                Position.PositionName positionName;
                Position position;
                if (Enum.GetNames(typeof(GameModeEnum)).Contains(actionPair.Key))
                {
                    mode = (GameModeEnum)Enum.Parse(typeof(GameModeEnum), actionPair.Key);
                }
                else
                {
                    WriteStatus("Error processing the opponent moves. Invalid game mode received:" + actionPair.Key);
                    throw new Exception("Error processing the opponent moves.Invalid game mode received: " + actionPair.Key);
                }
                if (Enum.GetNames(typeof(Position.PositionName)).Contains(actionPair.Value))
                {
                    positionName = (Position.PositionName)Enum.Parse(typeof(Position.PositionName), actionPair.Value);
                    position = PositionList[positionName];
                }
                else
                {
                    WriteStatus("Error processing the opponent moves. Invalid position:" + actionPair.Value);
                    throw new Exception("Error processing the opponent moves. Invalid position:" + actionPair.Value);
                }
                if (position != null)
                {
                    switch (mode)
                    {
                        case GameModeEnum.WaitOnOther:
                            break;
                        case GameModeEnum.SetUp:
                            if (position.CurrentStatus == Position.Status.Empty)
                            {
                                position.CurrentStatus = opponentSide;
                                DrawPosition(position, false);
                                oponentManLeft--;
                                UpdatePieceStatus(opponentSide, oponentManLeft);
                            }
                            else
                            {
                                WriteStatus("Incorrect location from Opponent! Answer is invalid");
                                throw new Exception("Incorrect location from Opponent! Answer is invalid");
                            }
                            break;
                        case GameModeEnum.Mark:
                            if (position.CurrentStatus == opponentSide)
                            {
                                markedOpponent = position;
                            }
                            else
                            {
                                WriteStatus("Mark failed on marking proper pieces");
                                throw new Exception("Mark failed on marking proper pieces");
                            }
                            break;
                        case GameModeEnum.Move:
                            if (markedOpponent != null)
                            {
                                if (markedOpponent.ValidMoves.Exists(t=> t.positionName == position.Name))
                                {
                                    if (position.CurrentStatus == Position.Status.Empty)
                                    {
                                        markedOpponent.CurrentStatus = Position.Status.Empty;
                                        position.CurrentStatus = opponentSide;
                                        DrawPosition(markedOpponent, false);
                                        markedOpponent = null;
                                        DrawPosition(position, false);
                                    }
                                    else
                                    {
                                        WriteStatus("Move marks invalid position");
                                        throw new Exception("Move marks invalid position");
                                    }
                                }
                                else
                                {
                                    WriteStatus("Invalid move sent");
                                    throw new Exception("Invalid move sent");
                                }
                            }
                            else
                            {
                                WriteStatus("Marking status failed to set");
                                throw new Exception("Marking status failed to set");
                            }
                            break;
                        case GameModeEnum.Remove:
                        case GameModeEnum.Remove2:
                            if (position.CurrentStatus == ownSide)
                            {
                                if (Mill.IsInMill(ownSide, position.PositionStr,  PositionList) > 0)
                                {
                                    if (Mill.IsAllInMill(ownSide, PositionList))
                                    {
                                        position.CurrentStatus = Position.Status.Empty;
                                        DrawPosition(position, false);
                                    }
                                    else
                                    {
                                        WriteStatus("Not all piece in Mill. Wrong remove");
                                        throw new Exception("Not all piece in Mill. Wrong remove");
                                    }
                                }
                                else
                                {
                                    position.CurrentStatus = Position.Status.Empty;
                                    DrawPosition(position, false);
                                }
                            }
                            else
                            {
                                WriteStatus("Invalid Remove command arrived");
                                throw new Exception("Invalid Remove command arrived");
                            }
                            break;
                        case GameModeEnum.Jump:
                            if (position.CurrentStatus == Position.Status.Empty)
                            {
                                markedOpponent.CurrentStatus = Position.Status.Empty;
                                position.CurrentStatus = opponentSide;
                                DrawPosition(markedOpponent, false);
                                markedOpponent = null;
                                DrawPosition(position, false);
                            }
                            else
                            {
                                WriteStatus("Jump marks and invalid position");
                                throw new Exception("Jump marks and invalid position");
                            }
                            break;
                        case GameModeEnum.End:
                            break;
                        default:
                            break;
                    }
                }
            }
            if (manLeft > 0)
            {
                GameMode = GameModeEnum.SetUp;
                WriteStatus("Place a piece");
            }
            else
            {
                int numPieces = Mill.NumberOfPieces(ownSide, PositionList);
                GameMode = numPieces >= 3 ? GameModeEnum.Mark : GameModeEnum.End;
                GamePhase = numPieces == 3 ? GameModeEnum.Jump : numPieces > 3 ? GameModeEnum.Move : GameModeEnum.End;
                WriteStatus("Make your move");
            }

        }
        private void AddToActionList(GameModeEnum mode, Position.PositionName name, bool clear)
        {
            if (clear)
            {
                actionList.Clear();
            }
            actionList.Add(new KeyValuePair<string, string>(mode.ToString(), name.ToString()));
        }

        private Point ImagePoint(Point ContainerPoint)
        {
            Point ret;
            int MinSize = Math.Min(pbBoard.Width, pbBoard.Height);
            Point center = new Point(pbBoard.Width / 2, pbBoard.Height / 2);
            float Zoom = pbBoard.Image.PhysicalDimension.Width / MinSize;
            Point pushMiddle = center - ContainerPoint;
            Point NewPoint = pushMiddle * Zoom;
            ret = new Point(pbBoard.Image.PhysicalDimension.Width / 2, pbBoard.Image.PhysicalDimension.Height / 2) - NewPoint;
            return ret;
        }

         private void UpdatePieceStatus(Position.Status side, int left)
        {
            List<Control> controls;
            if (side == Position.Status.Black)
            {
                controls = new List<Control> { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6, pictureBox7, pictureBox8, pictureBox9 };
            }
            else
            {
                controls = new List<Control> { pictureBox10, pictureBox11, pictureBox12, pictureBox13, pictureBox14, pictureBox15, pictureBox16, pictureBox17, pictureBox18 };
            }
            for (int i = 8; i >= 0; i--)
            {
                controls[i].Visible = i < left;
            }
        }
        private void ClearGame()
        {
            manLeft = 9;
            UpdatePieceStatus(ownSide, manLeft);
            oponentManLeft = 9;
            UpdatePieceStatus(opponentSide, oponentManLeft);
            isWin = null;
            foreach (Position pos in PositionList.Values)
            {
                pos.CurrentStatus = Position.Status.Empty;
            }
            ResetTable();
        }
        private void DrawPosition(Position pos, bool IsMark)
        {
            switch (pos.CurrentStatus)
            {
                case Position.Status.Empty:
                    ResetImage(pos.Location);
                    break;
                case Position.Status.Black:
                    WriteImage(IsMark ? global::NineMensMorris.Properties.Resources.BlackPiecMarked : global::NineMensMorris.Properties.Resources.BlackPiec, pos.Location);
                    break;
                case Position.Status.White:
                    WriteImage(IsMark ? global::NineMensMorris.Properties.Resources.WhitePiecMarked : global::NineMensMorris.Properties.Resources.WhitePiec, pos.Location);
                    break;
                default:
                    break;
            }
        }
        private void ResetTable()
        {
            using (Graphics drD = Graphics.FromImage(pbBoard.Image))
            {
                drD.DrawImage((Image)global::NineMensMorris.Properties.Resources.Board.Clone(), new PointF(0,0));
            }
            pbBoard.Refresh();
        }
        private void WriteImage(Image img, Point point)
        {
            if (pbBoard.InvokeRequired)
            {
                writeImage op = new writeImage(WriteImage);
                this.Invoke(op, img, point);
            }
            else
            {
                using (Graphics drD = Graphics.FromImage(pbBoard.Image))
                {
                    drD.DrawImage(img, (point - new Point(img.Width, img.Height) * 0.5F).PointF);
                }
                pbBoard.Refresh();
            }
        }
        private void OpponentTurn()
        {
            string messageXML = CreateMessageXML(false, null, false);
            if (CurrentOpponent.IsRunning)
            {
                GameMode = GameModeEnum.WaitOnOther;
                CurrentOpponent.SendMessage(messageXML);
            }
            else
            {
                GameMode = GameModeEnum.End;
                GamePhase = GameModeEnum.End;
                throw new Exception("Opponent lost");
            }
        }
        private void ResetImage(Point point)
        {
            Image lastImage = ownSide == Position.Status.Black ? global::NineMensMorris.Properties.Resources.BlackPiec : global::NineMensMorris.Properties.Resources.WhitePiec;
            using (Image cleanImage = global::NineMensMorris.Properties.Resources.Board.Clone(new Rectangle(new Point(point.PointF).Offset(-lastImage.Width / 2 - 1, -lastImage.Height / 2 - 1).PointI, new Size(lastImage.Size.Width + 1, lastImage.Size.Height + 1)), lastImage.PixelFormat))
            {
                using (Graphics drD = Graphics.FromImage(pbBoard.Image))
                {
                    drD.DrawImage(cleanImage, new Point(point.PointF).Offset(-lastImage.Width / 2 - 1, -lastImage.Height / 2 - 1).PointF);
                }
            }
            pbBoard.Refresh();
            initMill = true;
        }
        private void WriteStatus(string message)
        {
            if (this.InvokeRequired)
            {
                writeMessage op = new writeMessage(WriteStatus);
                op.Invoke(message);
            }
            else
            {
                toolStripStatusLabel1.Text = string.Format("Game Status:{0}; Game Mode: {1}; Message: {2}", remoteMode.ToString() + (string.IsNullOrEmpty(OpponentName) ? string.Empty : "(" + OpponentName + ")"), GameMode.ToString(), message);
                toolStripStatusLabel1.ToolTipText = toolStripStatusLabel1.Text;
                statusStrip1.Refresh();
            }
        }
        private void computerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (CurrentOpponent != null && CurrentOpponent.IsRunning)
            {
                SendGameAbort();
                CurrentOpponent.Stop();
                CurrentOpponent.MessageArrived -= CurrentOpponent_MessageArrived;
                CurrentOpponent = null;
            }
            ClearGame();
            remoteMode = RemoteMode.Computer;
            OpponentName = "Computer";
            CurrentOpponent = new Computer(Position.Status.Black);
            CurrentOpponent.MessageArrived += CurrentOpponent_MessageArrived;
            GameMode = GameModeEnum.WaitOnOther;
            GamePhase = GameModeEnum.SetUp;
            CurrentOpponent.SendMessage(CreateMessageXML(true, "Start", false));
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentOpponent != null && CurrentOpponent.IsRunning)
                {
                    SendGameAbort();
                    CurrentOpponent.Stop();
                    CurrentOpponent.MessageArrived -= CurrentOpponent_MessageArrived;
                    CurrentOpponent = null;
                }
                ClearGame();
                remoteMode = RemoteMode.Host;
                OpponentName = "Unknown";
                CurrentOpponent = new TCPServer();
                CurrentOpponent.MessageArrived += CurrentOpponent_MessageArrived;
                GameMode = GameModeEnum.WaitOnOther;
                GamePhase = GameModeEnum.SetUp;
                WriteStatus("Game Hosted on " + System.Net.Dns.GetHostName());
            }
            catch (Exception ex)
            {
                WriteStatus("Error:" + ex.Message);
            }
        }
        private void connectToToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentOpponent != null && CurrentOpponent.IsRunning)
                {
                    SendGameAbort();
                    CurrentOpponent.Stop();
                    CurrentOpponent.MessageArrived -= CurrentOpponent_MessageArrived;
                    CurrentOpponent = null;
                }
                remoteMode = RemoteMode.Client;
                if (Utils.InputBox("Enter Host name", "Enter the Game host address or URL:", ref OpponentName) == DialogResult.OK)
                {
                    ClearGame();
                    GameMode = GameModeEnum.SetUp;
                    GamePhase = GameModeEnum.SetUp;
                    initMill = true;
                    CurrentOpponent = new TCPClient(OpponentName);
                    CurrentOpponent.MessageArrived += CurrentOpponent_MessageArrived;
                    WriteStatus("Connecting to Server");
                }
            }
            catch (Exception ex)
            {

                WriteStatus("Error:" + ex.Message);
            }
        }

        private void SendGameAbort()
        {
            string msg = CreateMessageXML(true, "Abort Game", true);
            CurrentOpponent.SendMessage(msg);
        }

        private void ProcessMessageArrived(MessageArrivedEventArgs args)
        {
            try
            {
                if (args.Ex == null)
                {
                    ActionListMessage.ActionList actionListXML;
                    actionListXML = (ActionListMessage.ActionList)Utils.DeserializeXml(typeof(ActionListMessage.ActionList), args.Message);
                    if (actionListXML.Header.IsAbort)
                    {
                        GameMode = GameModeEnum.End;
                        GamePhase = GameModeEnum.End;
                        OpponentName = "Unknown";
                        CurrentOpponent.Stop();
                        CurrentOpponent.MessageArrived -= CurrentOpponent_MessageArrived;
                        ClearGame();
                        initMill = true;
                        WriteStatus("Game aborted" + (string.IsNullOrEmpty(actionListXML.Header.Message) ? string.Empty : ". Message: " + actionListXML.Header.Message));
                        return;
                    }
                    if (!string.IsNullOrEmpty(actionListXML.Header.Message))
                    {
                        WriteStatus(actionListXML.Header.Message);
                    }
                    if(OpponentName == "Unknown")
                    {
                        OpponentName = actionListXML.Header.HostName;
                        GamePhase = GameModeEnum.SetUp;
                    }
                    if (actionListXML.ActionList1 != null && actionListXML.ActionList1.Count() > 0)
                    {                        
                        opponentActionList = new List<KeyValuePair<string, string>>();
                        foreach (ActionListMessage.ActionListActionList dataRow in actionListXML.ActionList1)
                        {
                            opponentActionList.Add(new KeyValuePair<string, string>(dataRow.GameMode.ToString(), dataRow.Position.ToString()));
                        }
                        ProcessOpponentActions();
                        if (Mill.IsLost(ownSide, GamePhase, PositionList))
                        {
                            GameMode = GameModeEnum.End;
                            GamePhase = GameModeEnum.End;
                            WriteStatus("Game Lost");
                            YouLost();
                        }
                        if (Mill.IsLost(opponentSide, GamePhase, PositionList))
                        {
                            GameMode = GameModeEnum.End;
                            GamePhase = GameModeEnum.End;
                            WriteStatus("Game Win");
                            YouWin();
                        }
                    }
                }
                else
                {
                    throw args.Ex;
                }
            }
            catch (Exception ex)
            {
                WriteStatus("Error:" + ex.Message);
                GameMode = GameModeEnum.End;
                GamePhase = GameModeEnum.End;
            }
        }

        private void CurrentOpponent_MessageArrived(object sender, MessageArrivedEventArgs args)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    processMessageArrived op = new processMessageArrived(ProcessMessageArrived);
                    this.Invoke(op, args);
                }
                else
                {
                    ProcessMessageArrived(args);
                }
            }
            catch (Exception ex)
            {

                WriteStatus("Error:" + ex.Message);
            }
        }
        private string CreateMessageXML(bool headerOnly, string message, bool isAbort)
        {
            string ret = null;
            ActionListMessage.ActionList actionListXML = new ActionListMessage.ActionList();
            actionListXML.Header = new ActionListMessage.ActionListHeader() { HostName = System.Net.Dns.GetHostName(), IsAbort=isAbort, Message = message };
            if (!headerOnly)
            {
                List<ActionListMessage.ActionListActionList> list = new List<ActionListMessage.ActionListActionList>();
                foreach(KeyValuePair<string, string> pair in actionList)
                {
                    ActionListMessage.GameMode mode;
                    ActionListMessage.Position pos;
                    if (Enum.IsDefined(typeof (ActionListMessage.GameMode), pair.Key))
                    {
                        mode = (ActionListMessage.GameMode)Enum.Parse(typeof(ActionListMessage.GameMode), pair.Key);
                    }
                    else
                    {
                        throw new Exception("Invalid GameMode");
                    }
                    if (Enum.IsDefined(typeof (ActionListMessage.Position), pair.Value))
                    {
                        pos = (ActionListMessage.Position)Enum.Parse(typeof(ActionListMessage.Position), pair.Value);
                    }
                    else
                    {
                        throw new Exception("Invalid position");
                    }
                    list.Add(new ActionListMessage.ActionListActionList() { GameMode= mode, Position = pos });
                }
                actionListXML.ActionList1 = list.ToArray();
            }
            ret =  Utils.SerializeXml(actionListXML,typeof(ActionListMessage.ActionList));
            return ret;
        }

        private void frmNineMensMorris_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SendGameAbort();
                CurrentOpponent.MessageArrived -= CurrentOpponent_MessageArrived;
                CurrentOpponent.Stop();
                while (CurrentOpponent.IsRunning)
                {
                    System.Threading.Thread.Sleep(10);
                }
                animationThread.Abort();
            }
            catch (Exception ex)
            {
                WriteStatus("Error:" + ex.Message);
            }
        }

        private void newGameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentOpponent != null && CurrentOpponent.IsRunning)
                {
                    SendGameAbort();
                    CurrentOpponent.Stop();
                    CurrentOpponent.MessageArrived -= CurrentOpponent_MessageArrived;
                    CurrentOpponent = null;
                }
                ClearGame();
                remoteMode = RemoteMode.Computer;
                OpponentName = "Computer";
                initMill = true;
                CurrentOpponent = new Computer(Position.Status.Black);
                CurrentOpponent.MessageArrived += CurrentOpponent_MessageArrived;
                GameMode = GameModeEnum.SetUp;
                GamePhase = GameModeEnum.SetUp;
                WriteStatus("Please your piece");

            }
            catch (Exception ex) 
            {

                WriteStatus("Error:" + ex.Message);
            }
        }

        private void frmNineMensMorris_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (remoteMode != RemoteMode.Computer)
                {
                    if (e.KeyChar == 13)
                    {
                        string msg = string.Empty;
                        if (Utils.InputBox("Message to Opponent", "Write message to " + OpponentName, ref msg) == DialogResult.OK)
                        {
                            CurrentOpponent.SendMessage(CreateMessageXML(true, msg, false));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                WriteStatus("Error:" + ex.Message);
            }
        }
        private void YouWin()
        {
            isWin = true;
        }
        private void YouLost()
        {
            isWin = false;
        }
        private void AnimateMill(object arg)
        {
            Thread mainThread = (Thread)arg;
            bool resultMessageOut = false;
            float currentAngle = 0;
            List<Point> cloudPoints = new List<Point>();
            Random rnd = new Random();
            try
            {
                while (mainThread.IsAlive)
                {
                    if(GameMode == GameModeEnum.WaitOnOther || initMill)
                    {
                        initMill = false;
                        currentAngle += 10;
                        if (currentAngle >= 90) currentAngle = 0;
                        if (cloudPoints.Count <= 30)
                        {
                            cloudPoints.Add(new Point(cloudPoints.Count == 30 ? 600 : rnd.Next(1, 600), rnd.Next(95, 180)));
                        }
                        cloudPoints.ForEach(t => t.X = t.X - (t.Y - 95) / 20);
                        cloudPoints = cloudPoints.Where(t => t.X > -20).ToList();
                        Bitmap rotatedImage =(Bitmap) RotateImage(global::NineMensMorris.Properties.Resources.Sail, currentAngle);
                        Bitmap newMill = (Bitmap)global::NineMensMorris.Properties.Resources.Mill.Clone();
                        using (Graphics drD = Graphics.FromImage(newMill))
                        {
                            Image cloud = global::NineMensMorris.Properties.Resources.Coud;
                            cloudPoints.ForEach( t => drD.DrawImage(cloud, new RectangleF( t.PointF, new SizeF(cloud.Width * (t.Y - 95) / 20f, cloud.Height * (t.Y - 95) / 20f))));
                            drD.DrawImage(rotatedImage, new PointF() { X = 150f, Y = 150f });
                        }
                        newMill = new Bitmap(newMill, new Size(294, 294));
                        WriteImage(newMill, new Point(302f, 302f));
                        resultMessageOut = false;
                    }
                    if (GameMode == GameModeEnum.End)
                    {
                        if(isWin != null && !resultMessageOut)
                        {
                            if (isWin.Value)
                            {
                                TextToGraphics("You WIN!", "Verdana", 60, FontStyle.Bold, new PointF(75f, 165f));
                            }
                            else
                            {
                                TextToGraphics("You LOST!", "Verdana", 60, FontStyle.Bold, new PointF(60f, 75f));
                            }
                            resultMessageOut = true;
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception)
            {

                throw;
            }
        }
        private Bitmap RotateImage(Bitmap bmp, float angle)
        {
            Bitmap rotatedImage = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(bmp.Width / 2f, bmp.Height / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-bmp.Width / 2f, -bmp.Height / 2f);
                g.DrawImage(bmp, new PointF(0f, 0f));
            }

            return rotatedImage;
        }
        private void TextToGraphics(string text, string fontName, int size, FontStyle fontStyle, PointF pointF)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new textToGraphics(TextToGraphics), text, fontName, size, fontStyle, pointF);
            }
            else
            {
                using (Graphics drD = Graphics.FromImage(pbBoard.Image))
                {
                    using (Font font = new Font(fontName, size, fontStyle))
                    {
                        drD.DrawString(text, font, Brushes.Black, pointF);
                    }
                }
                pbBoard.Refresh();
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int.TryParse(toolStripComboBox1.SelectedItem.ToString(), out Mill.MaxDepth);            
        }

        private void difficultyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripComboBox1.SelectedItem = Mill.MaxDepth.ToString();
        }

        private void frmNineMensMorris_Load(object sender, EventArgs e)
        {
            initMill = true;
        }
    }

}
