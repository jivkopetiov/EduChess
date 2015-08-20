using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;

namespace EduChess
{
    public class ControlView : UIView
    {
        private readonly UIButton gotoStartButton;
        private readonly UIButton prevButton;
        private readonly UIButton playButton;
        private readonly UIButton nextButton;
        private readonly UIButton gotoEndButton;

        private readonly UIView noteContainer;

        private readonly Dictionary<Guid, NoteMoveView> noteMoves = new Dictionary<Guid, NoteMoveView>();

        public ControlView(BoardView board)
        {
            noteContainer = new UIView();
            noteContainer.AddRedBorder();
            Add(noteContainer);

            gotoStartButton = UIButton.FromType(UIButtonType.Custom);
            gotoStartButton.SetBackgroundImage(UIImage.FromFile("controls/start.png"), UIControlState.Normal);
            Add(gotoStartButton);

            prevButton = UIButton.FromType(UIButtonType.Custom);
            prevButton.SetBackgroundImage(UIImage.FromFile("controls/previous.png"), UIControlState.Normal);
            Add(prevButton);

            playButton = UIButton.FromType(UIButtonType.Custom);
            playButton.SetBackgroundImage(UIImage.FromFile("controls/play.png"), UIControlState.Normal);
            Add(playButton);

            nextButton = UIButton.FromType(UIButtonType.Custom);
            nextButton.SetBackgroundImage(UIImage.FromFile("controls/next.png"), UIControlState.Normal);
            Add(nextButton);

            gotoEndButton = UIButton.FromType(UIButtonType.Custom);
            gotoEndButton.SetBackgroundImage(UIImage.FromFile("controls/end.png"), UIControlState.Normal);
            Add(gotoEndButton);

            gotoStartButton.TouchUpInside += board.GoToStart;
            prevButton.TouchUpInside += board.PreviousMove;
            playButton.TouchUpInside += board.Play;
            nextButton.TouchUpInside += board.NextMove;
            gotoEndButton.TouchUpInside += board.GoToEnd;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            gotoStartButton.Frame = new RectangleF(10, 10, 44, 44);
            prevButton.Frame = new RectangleF(60, 10, 44, 44);
            playButton.Frame = new RectangleF(110, 10, 44, 44);
            nextButton.Frame = new RectangleF(160, 10, 44, 44);
            gotoEndButton.Frame = new RectangleF(210, 10, 44, 44);

            noteContainer.Frame = new RectangleF(10, 70, 210, Frame.Height - 70);
        }

        public void AddMove(Move move) {
            var view = new NoteMoveView();

            const float verticalOffset = 30;
            const float width = (210 - 18) / 2;

            if (move.Piece.Color == PieceColor.White)
                view.Frame = new RectangleF(6, (move.Number / 2) * verticalOffset, width, verticalOffset);
            else 
                view.Frame = new RectangleF(12 + width, (move.Number / 2) * verticalOffset, width, verticalOffset);

            view.Text = Notator.GetMoveNotation(move);

            noteContainer.Add(view);

            noteMoves[move.Id] = view;
        }

        public void RemoveMove(Move move) {
            if (noteMoves.ContainsKey(move.Id))
            {
                noteMoves[move.Id].RemoveFromSuperview();
                noteMoves.Remove(move.Id);
            }
            else
            {
                Console.WriteLine("Missing move, cannot remove view from notation");
            }
        }
    }

    public class NoteMoveView : UILabel {

    }

    public class Notator {

        public static string GetMoveNotation(Move move) {

            string result = "";

            if (move.Piece.Color == PieceColor.White)
                result += ((move.Number + 2) / 2) + ". ";

            result += GetPieceCode(move.Piece);

            result += move.CapturedPiece != null ? "x" : "";

            result += GetRow(move.FromX);

            result += (move.FromY + 1);

            result += "-" + GetRow(move.ToX);

            result += move.ToY + 1;

            return result;
        }

        private static string GetPieceCode(PieceView Piece) {

            if (Piece is Pawn)
                return "";
            else if (Piece is Queen)
                return "Q";
            else if (Piece is Bishop)
                return "B";
            else if (Piece is Knight)
                return "N";
            else if (Piece is Rook)
                return "R";
            else if (Piece is King)
                return "K";
            else
                throw new InvalidOperationException("Invalid Piece type: " + Piece.GetType().Name);
        }

        private static string GetRow(int rowNum) {
            switch (rowNum)
            {
                case 0: return "a";
                case 1: return "b";
                case 2: return "c";
                case 3: return "d";
                case 4: return "e";
                case 5: return "f";
                case 6: return "g";
                case 7: return "h";
                default:
                    throw new InvalidOperationException("Invalid Row index: " + rowNum);
            }
        }
    }
}









