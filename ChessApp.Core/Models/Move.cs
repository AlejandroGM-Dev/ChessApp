using ChessApp.Core.Enums;
using ChessApp.Core.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Models
{
    public class Move
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public Piece Piece { get; set; }
        public Piece CapturedPiece { get; set; }
        public DateTime Timestamp { get; set; }
        public string AlgebraicNotation { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsCapture { get; set; }

        public Move(Position from, Position to, Piece piece, Piece capturedPiece = null)
        {
            From = from;
            To = to;
            Piece = piece;
            CapturedPiece = capturedPiece;
            Timestamp = DateTime.Now;
            IsCapture = capturedPiece != null;
            AlgebraicNotation = ToAlgebraicNotation();
        }

        public string ToAlgebraicNotation()
        {
            string notation;

            if (Piece.Type == PieceType.Pawn)
            {
                notation = IsCapture ? $"{From.GetColumnLetter()}x{To}" : To.ToString();
            }
            else
            {
                notation = IsCapture ? $"{Piece.GetSymbol()}x{To}" : $"{Piece.GetSymbol()}{To}";
            }

            // Agregar símbolos de jaque y jaque mate
            if (IsCheckmate)
                notation += "#";
            else if (IsCheck)
                notation += "+";

            return notation;
        }

        public override string ToString()
        {
            return AlgebraicNotation;
        }
    }
}
