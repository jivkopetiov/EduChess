using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EduChess
{
    public enum PieceType
    {
        Pawn,
        Bishop,
        Rook,
        King,
        Queen,
        Knight
    }

    public class Piece
    {
        public int X;
        public int Y;
        public PieceType Type;
    }

    public class Board
    {
        public Board()
        {
            Pieces = new List<Piece>();
            Moves = new List<Move>();
            Metadata = new Dictionary<string, string>();
        }

        public List<Piece> Pieces;
        public List<Move> Moves;
        public Dictionary<string, string> Metadata;

        public void PrintDump()
        {
            foreach (var prop in Metadata)
            {
                Console.WriteLine(prop.Key + ": " + prop.Value);
            }

            foreach (var move in Moves)
            {
                Console.WriteLine("Move: " + move.Number + ". " + move.Text);
            }
        }
    }

    public class Move
    {
        public string Text { get; set; }
        public int FromX;
        public int ToX;
        public int FromY;
        public int ToY;
        public int Number;

        public PieceColor Color { get; set; }
    }


    public enum PieceColor
    {
        White,
        Black
    }
}
