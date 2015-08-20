using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace EduChess
{

    public enum PieceType
    {
        Pawn
,
        Bishop
,
        Rook
,
        King
,
        Queen
,
        Knight
    }

    public class Move
    {
        public Move()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id;
        public int FromX;
        public int ToX;
        public int FromY;
        public int ToY;
        public PieceView Piece;
        public PieceView CapturedPiece;
        public int Number;
    }


    public enum PieceColor
    {
        White
        ,
        Black
    }
    
}
