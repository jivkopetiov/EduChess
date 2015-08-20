using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace EduChess
{
    public class PgnParser
    {
        public Board Parse(string file)
        {
            var board = new Board();
            var lines = File.ReadAllLines(file).Select(l => l.Trim());

            string gameText = "";
            foreach (var line in lines)
            {
                if (line.StartsWith("["))
                {
                    string res = line.TrimStart('[').TrimEnd(']').Trim();
                    int space = res.IndexOf(" ");
                    string propName = res.Substring(0, space);
                    string propValue = res.Substring(space + 1).Trim().Trim('"').Trim();
                    board.Metadata.Add(propName, propValue);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        gameText += line + " ";
                }
            }

            var tokens = Regex.Split(gameText, @"(\d{1,3}\.)")
                              .Where(t => !string.IsNullOrWhiteSpace(t))
                              .ToArray();

            for (int counter = 0; counter < tokens.Length; counter += 2)
            {
                var token = tokens[counter].Trim().Trim('.').Trim();
                int moveNumber;
                if (!int.TryParse(token, out moveNumber))
                    throw new InvalidOperationException("Failed to parse move number from token " + token);

                string move = tokens[counter + 1].Trim();
                if (move.Contains("{"))
                {
                    int start = move.IndexOf("{");
                    int end = move.IndexOf("}");
                    move = move.Substring(0, start) + move.Substring(end + 1);
                    move = Regex.Replace(move, @"\d{1,3}\.\.\.", "");
                    move = move.CollapseWhitespace().Trim();
                }

                move = move.Trim();

                var blackWhiteMoves = move.Split(' ');
                if (blackWhiteMoves.Length != 2)
                    throw new InvalidOperationException("Black White Move is not delimited by space: " + move);

                string whiteMove = blackWhiteMoves[0].Trim();
                string blackMove = blackWhiteMoves[1].Trim();

                var m = new Move();
                m.Number = moveNumber;
                m.Text = whiteMove;
                m.Color = PieceColor.White;
                board.Moves.Add(m);

                var m2 = new Move();
                m2.Number = moveNumber;
                m2.Text = blackMove;
                m2.Color = PieceColor.Black;
                board.Moves.Add(m2);
            }

            return board;
        }
    }
}
