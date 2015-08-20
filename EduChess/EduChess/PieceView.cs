using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;

namespace EduChess
{
    public class PieceView : UIImageView {

        public int X;
        public int Y;
        public PieceColor Color;
        public RectangleF CurrentFrame;
        protected BoardView board;

        public PieceView (int x, int y, PieceColor color, BoardView controller)
        {
            this.board = controller;
            X = x;
            Y = y;
            Color = color;
        }

        public bool IsWhite { get { return Color == PieceColor.White; } } 
        public bool IsBlack { get { return Color == PieceColor.Black; } } 

        public virtual List<Tuple<int, int>> GetMoves() {
            return new List<Tuple<int, int>>();
        }

        protected List<Tuple<int, int>> RookMovements() {
            var list = new List<Tuple<int, int>>();

            if (X > 0) {
                for (var i = X - 1; i >= 0; i--) {
                    list.Add(Tuple.Create(i, Y));

                    if (board.ExistsPieçeOn(i, Y))
                        break;
                }
            }

            if (X < 7) {
                for (var i = X + 1; i <= 7; i++) {
                    list.Add(Tuple.Create(i, Y));

                    if (board.ExistsPieçeOn(i, Y))
                        break;
                }
            }

            if (Y < 7) {
                for (var i = Y + 1; i <= 7; i++) {
                   
                    list.Add(Tuple.Create(X, i));

                    if (board.ExistsPieçeOn(X, i))
                        break;
                }
            }

            if (Y > 0) {
                for (var i = Y - 1; i >= 0; i--) {
                    list.Add(Tuple.Create(X, i));

                    if (board.ExistsPieçeOn(X, i))
                        break;
                }
            }

            return list;
        }

        protected List<Tuple<int, int>> BishopMovements()
        {
            var list = new List<Tuple<int, int>>();

            if (X > 0)
            {
                for (var i = X - 1; i >= 0; i--)
                {
                    var r = i;
                    var c = Y - X + i;

                    list.Add(Tuple.Create(r, c));

                    if (board.ExistsPieçeOn(r, c))
                        break;
                }
            }
            if (X < 7)
            {
                for (var i = X + 1; i <= 7; i++)
                {
                    var r = i;
                    var c = Y - X + i;

                    list.Add(Tuple.Create(r, c));

                    if (board.ExistsPieçeOn(r, c))
                        break;
                }
            }
            if (Y < 7)
            {
                for (var i = Y + 1; i <= 7; i++)
                {
                    var c = i;
                    var r = X + Y - i;

                    list.Add(Tuple.Create(r, c));

                    if (board.ExistsPieçeOn(r, c))
                        break;
                }
            }
            if (Y > 0)
            {
                for (var i = Y - 1; i >= 0; i--)
                {
                    var c = i;
                    var r = X + Y - i;

                    list.Add(Tuple.Create(r, c));

                    if (board.ExistsPieçeOn(r, c))
                        break;
                }
            }

            return list;
        }
    }

    public class Bishop : PieceView {
        public Bishop (int x, int y, PieceColor color, BoardView board) : base(x, y, color, board) { }

        public override List<Tuple<int, int>> GetMoves()
        {
            var list = new List<Tuple<int, int>>();
            list.AddRange(BishopMovements());
            return list;
        }
    }

    public class Knight : PieceView {
        public Knight (int x, int y, PieceColor color, BoardView board) : base(x, y, color, board) { }

        public override List<Tuple<int, int>> GetMoves()
        {
            var list = new List<Tuple<int, int>>();

            list.Add(Tuple.Create(X + 1, Y + 2));
            list.Add(Tuple.Create(X + 1, Y - 2));
            list.Add(Tuple.Create(X - 1, Y + 2));
            list.Add(Tuple.Create(X - 1, Y - 2));
            list.Add(Tuple.Create(X + 2, Y + 1));
            list.Add(Tuple.Create(X + 2, Y - 1));
            list.Add(Tuple.Create(X - 2, Y + 1));
            list.Add(Tuple.Create(X - 2, Y - 1));

            return list;
        }
    }

    public class Rook : PieceView {
        public Rook (int x, int y, PieceColor color, BoardView board) : base(x, y, color, board) { }

        public override List<Tuple<int, int>> GetMoves()
        {
            var list = new List<Tuple<int, int>>();
            list.AddRange(RookMovements());
            return list;
        }
    }

    public class Pawn : PieceView {
        public Pawn (int x, int y, PieceColor color, BoardView board) : base(x, y, color, board) { }

        public override List<Tuple<int, int>> GetMoves()
        {
            var list = new List<Tuple<int, int>>();

            if (Color == PieceColor.White) {

                list.Add(Tuple.Create(X, Y + 1));

                if (Y == 1) {
                    if (!board.ExistsPieçeOn(X, Y + 1))
                        list.Add(Tuple.Create(X, Y + 2));
                }

                var rightEnemy = board.GetPieceOn(X + 1, Y + 1);
                if (rightEnemy != null && rightEnemy.Color != Color)
                    list.Add(Tuple.Create(X + 1, Y + 1));

                var leftEnemy = board.GetPieceOn(X - 1, Y + 1);
                if (leftEnemy != null && leftEnemy.Color != Color)
                    list.Add(Tuple.Create(X - 1, Y + 1));
            }
            else {

                list.Add(Tuple.Create(X, Y - 1));

                if (Y == 6) {
                    if (!board.ExistsPieçeOn(X, Y - 1))
                        list.Add(Tuple.Create(X, Y - 2));
                }

                var rightEnemy = board.GetPieceOn(X + 1, Y - 1);
                if (rightEnemy != null && rightEnemy.Color != Color)
                    list.Add(Tuple.Create(X + 1, Y - 1));

                var leftEnemy = board.GetPieceOn(X - 1, Y - 1);
                if (leftEnemy != null && leftEnemy.Color != Color)
                    list.Add(Tuple.Create(X - 1, Y - 1));
            }

            return list;
        }
    }

    public class King : PieceView {
        public King (int x, int y, PieceColor color, BoardView board) : base(x, y, color, board) { }

        public override List<Tuple<int, int>> GetMoves()
        {
            var list = new List<Tuple<int, int>>();

            list.Add(Tuple.Create(X, Y + 1));
            list.Add(Tuple.Create(X, Y - 1));
            list.Add(Tuple.Create(X + 1, Y));
            list.Add(Tuple.Create(X + 1, Y - 1));
            list.Add(Tuple.Create(X + 1, Y + 1));
            list.Add(Tuple.Create(X - 1, Y));
            list.Add(Tuple.Create(X - 1, Y - 1));
            list.Add(Tuple.Create(X - 1, Y + 1));

            return list;
        }
    }

    public class Queen : PieceView {
        public Queen (int x, int y, PieceColor color, BoardView board) : base(x, y, color, board) { }

        public override List<Tuple<int, int>> GetMoves()
        {
            var list = new List<Tuple<int, int>>();
            list.AddRange(RookMovements());
            list.AddRange(BishopMovements());
            return list;
        }
    }
    
}
