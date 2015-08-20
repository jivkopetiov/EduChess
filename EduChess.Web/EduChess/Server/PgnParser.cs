using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace EduChess
{
    public class PgnParser
    {
        public object Parse(string file)
        {
            var properties = new ExpandoObject() as IDictionary<string, Object>;
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
                    properties.Add(propName, propValue);
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

            var moves = new List<object>();

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

                moves.Add(new { Number = moveNumber, Move = move });
            }

            var game = new { Properties = properties, Moves = moves };
            return game;
        }

        private Exception Error(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            return new Exception(errorMessage);
        }
    }
}
