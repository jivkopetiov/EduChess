using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Linq;
using System.Drawing;
using MonoTouch.CoreAnimation;

namespace EduChess
{
    public class BoardView : UIView {

        public void Play(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void GoToStart(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void GoToEnd(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void NextMove(object sender, EventArgs e)
        {
            if (Moves.Count >= MovesStorage.Count)
            {
                Console.WriteLine("No additional moves, cannot go forward");
                return;
            }

            var nextMove = MovesStorage[Moves.Count];

            PerformMove(nextMove);
        }

        public void PreviousMove(object sender, EventArgs e)
        {
            if (Moves.Count == 0)
            {
                Console.WriteLine("No more moves, cannot go back");
                return;
            }

            var poppedMove = Moves.Pop();

            var piece = poppedMove.Piece;
            piece.X = poppedMove.FromX;
            piece.Y = poppedMove.FromY;
            piece.Frame = new RectangleF(GetX(piece.X), GetY(piece.Y), CellWidth, CellWidth);
            piece.CurrentFrame = piece.Frame;
            isWhiteToMove = !isWhiteToMove;

            if (poppedMove.CapturedPiece != null)
            {
                var captured = poppedMove.CapturedPiece;
                Add(captured);
            }

            control.RemoveMove(poppedMove);

            Console.WriteLine("Went back one move");
        }

        public const float BoardWidth = 680;

        private bool isWhiteToMove = true;
        private const float CellWidth = BoardWidth / 8;
        private readonly List<UIView> highlights = new List<UIView>();
        private readonly List<PieceView> Pieces = new List<PieceView>();
        private readonly Dictionary<int, Move> MovesStorage = new Dictionary<int, Move>();
        private readonly Stack<Move> Moves = new Stack<Move>();
        private ControlView control;

        public BoardView()
        {
            DrawBoardLines();
            DrawPieces();
        }

        public void SetControl(ControlView control)
        {
            this.control = control;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = touches.ToArray<UITouch>().First();
            var loc = touch.LocationInView(this);
            var piece = Pieces.FirstOrDefault(x => x.Frame.Contains(loc));

            if (piece != null)
            {
                drags[touch] = new DragInfo
                {
                    Touch = touch,
                    View = piece
                };

                if ((isWhiteToMove && piece.IsWhite) ||
                    (!isWhiteToMove && piece.IsBlack))
                {
                    HighlightPossibleMoves(piece);
                }
            }
        }

        private void HighlightPossibleMoves(PieceView piece)
        {
            var moves = piece.GetMoves();
            foreach (var move in moves)
            {
                if (!IsPossibleMove(piece, move.Item1, move.Item2))
                    continue;

                var highlight = new UIView();
                highlight.BackgroundColor = UIColor.LightGray;
                highlight.Frame = new RectangleF(GetX(move.Item1), GetY(move.Item2), CellWidth, CellWidth);
                Add(highlight);
                SendSubviewToBack(highlight);
                highlights.Add(highlight);
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            var touch = touches.ToArray<UITouch>().First();
            DragInfo drag;
            if (!drags.TryGetValue(touch, out drag))
                return;

            var loc = touch.LocationInView(this);
            var ploc = touch.PreviousLocationInView(this);

            BringSubviewToFront(drag.View);

            var fr = drag.View.Frame;
            fr.X += (loc.X - ploc.X);
            fr.Y += (loc.Y - ploc.Y);
            drag.View.Frame = fr;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            var touch = touches.ToArray<UITouch>().First();

            var droppedPoint = touch.LocationInView(this);
            if (droppedPoint == PointF.Empty)
            {
                CleanupAfterDrop(null);
                return;
            }

            DragInfo drag;
            if (!drags.TryGetValue(touch, out drag))
            {
                CleanupAfterDrop(null);
                return;
            }

            drags.Remove(touch);
            var piece = drag.View;

            if ((isWhiteToMove && piece.IsBlack) ||
                (!isWhiteToMove && piece.IsWhite))
            {
                CleanupAfterDrop(piece);
                return;
            }

            var droppedCoords = GetXYFromCoords(droppedPoint);

            if (droppedCoords == null)
            {
                CleanupAfterDrop(piece);
                return;
            }

            int newX = droppedCoords.Item1;
            int newY = droppedCoords.Item2;

            CleanupAfterDrop(null);

            var moves = piece.GetMoves();

            if (IsPossibleMove(piece, newX, newY) && moves.Any(m => m.Item1 == newX && m.Item2 == newY))
            {
                var move = new Move
                {
                    Piece = piece,
                    FromX = piece.X,
                    FromY = piece.Y,
                    ToX = droppedCoords.Item1,
                    ToY = droppedCoords.Item2
                };

                PerformMove(move);
            }
            else
            {
                piece.Frame = piece.CurrentFrame;
            }
        }

        private void PerformMove(Move move)
        {
            Console.WriteLine("before: {0} - {1}", Moves.Count, MovesStorage.Count);

            var piece = move.Piece;

            move.Number = Moves.Count;

            Moves.Push(move);
            MovesStorage[move.Number] = move;

            var existing = GetPieceOn(move.ToX, move.ToY);
            if (existing != null)
            {
                Pieces.Remove(existing);
                move.CapturedPiece = existing;
                existing.RemoveFromSuperview();
            }
            piece.X = move.ToX;
            piece.Y = move.ToY;
            piece.Frame = new RectangleF(GetX(piece.X), GetY(piece.Y), CellWidth, CellWidth);
            piece.CurrentFrame = piece.Frame;
            isWhiteToMove = !isWhiteToMove;

            control.AddMove(move);

            Console.WriteLine("Performed move of {0} from {1} to {2}", piece.GetType().Name, move.FromX + "." + move.FromY, move.ToX + "." + move.ToY);

            Console.WriteLine("after: {0} - {1}", Moves.Count, MovesStorage.Count);
        }

        private void CleanupAfterDrop(PieceView piece) {
            foreach (var highlight in highlights)
                highlight.RemoveFromSuperview();

            highlights.Clear();

            if (piece != null)
                piece.Frame = piece.CurrentFrame;
        }

        private Tuple<int, int> GetXYFromCoords(PointF point)
        {
            if (point.X < 0 || point.X > (8 * CellWidth))
                return null;

            if (point.Y < 0 || point.Y >  (8 * CellWidth))
                return null;

            int x = (int) ( point.X / CellWidth);
            int y = 7 - (int) ( point.Y / CellWidth);
            return Tuple.Create(x, y);
        }

        class DragInfo
        {
            public UITouch Touch;
            public PieceView View;
        }

        readonly Dictionary<UITouch, DragInfo> drags =
            new Dictionary<UITouch, DragInfo>();


        private bool IsPossibleMove(PieceView piece, int x, int y)
        {
            if (x < 0 || y < 0 || x > 7 || y > 7)
                return false;

            var existing = GetPieceOn(x, y);
            if (existing != null) {
                if (existing.Color == piece.Color)
                    return false;

                if (piece is Pawn && piece.X == existing.X)
                    return false;

                if (existing is King)
                    return false;
            }

            return true;
        }

        public bool ExistsPieçeOn(int x, int y) {
            return Pieces.Any(p => p.X == x && p.Y == y);
        }

        public PieceView GetPieceOn(int x, int y) {
            return Pieces.FirstOrDefault(p => p.X == x && p.Y == y);
        }

        private void DrawBoardLines()
        {
            for (int i = 0; i < 9; i++)
                AddLineLayer(new PointF(0, (i * CellWidth)), new PointF(8 * CellWidth, (i * CellWidth)));

            for (int i = 0; i < 9; i++)
                AddLineLayer(new PointF((i * CellWidth), 0), new PointF((i * CellWidth), 8 * CellWidth));
        }

        private void DrawPieces()
        {
            for (int i = 0; i < 8; i++)
                Pieces.Add(new Pawn(i, 1, PieceColor.White, this));

            Pieces.Add(new Rook(0, 0, PieceColor.White, this));
            Pieces.Add(new Rook(7, 0, PieceColor.White, this));
            Pieces.Add(new Knight(1, 0, PieceColor.White, this));
            Pieces.Add(new Knight(6, 0, PieceColor.White, this));
            Pieces.Add(new Bishop(2, 0, PieceColor.White, this));
            Pieces.Add(new Bishop(5, 0, PieceColor.White, this));
            Pieces.Add(new King(4, 0, PieceColor.White, this));
            Pieces.Add(new Queen(3, 0, PieceColor.White, this));

            for (int i = 0; i < 8; i++)
                Pieces.Add(new Pawn(i, 6, PieceColor.Black, this));

            Pieces.Add(new Rook(0, 7, PieceColor.Black, this));
            Pieces.Add(new Rook(7, 7, PieceColor.Black, this));
            Pieces.Add(new Knight(1, 7, PieceColor.Black, this));
            Pieces.Add(new Knight(6, 7, PieceColor.Black, this));
            Pieces.Add(new Bishop(2, 7, PieceColor.Black, this));
            Pieces.Add(new Bishop(5, 7, PieceColor.Black, this));
            Pieces.Add(new King(4, 7, PieceColor.Black, this));
            Pieces.Add(new Queen(3, 7, PieceColor.Black, this));

            foreach (var piece in Pieces)
                DrawPiece(piece);
        }

        public void DrawPiece(PieceView piece)
        {
            var image = UIImage.FromFile(string.Format("figures/{0}-{1}.png", piece.Color.ToString().ToLowerInvariant(), piece.GetType().Name.ToLowerInvariant()));

            piece.Image = image;
            piece.Frame = new RectangleF(GetX(piece.X), GetY(piece.Y), CellWidth, CellWidth);
            piece.CurrentFrame = piece.Frame;
            Add(piece);
        }

        private float GetX(int x)
        {
            return x * CellWidth;
        }

        private float GetY(int y)
        {
            return (8 - y - 1) * CellWidth;
        }

        private void AddLineLayer(PointF start, PointF end)
        {
            var layer = new CAShapeLayer();
            layer.Frame = Frame;
            layer.LineWidth = 0.5f;
            layer.StrokeColor = UIColor.Black.CGColor;
            var path = new UIBezierPath();
            path.LineWidth = 0.5f;
            path.MoveTo(start);
            path.AddLineTo(end);
            layer.Path = path.CGPath;
            Layer.AddSublayer(layer);
        }
    }

}

