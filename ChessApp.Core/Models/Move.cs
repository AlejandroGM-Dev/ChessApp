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
        public Position From {  get; set; }
        public Position To { get; set; }
        public Piece Piece { get; set; }
        public Piece CapturedPiece { get; set; }
        public DateTime Timestamp { get; set; }
        public string AlgebraicNotation { get; set; }

        public Move(Position from, Position to, Piece piece, Piece capturedPiece = null)
        {
            From = from;
            To = to;
            Piece = piece;
            CapturedPiece = capturedPiece;
            Timestamp = DateTime.Now;
            AlgebraicNotation = ToAlgebraicNotation();
        }

        public string ToAlgebraicNotation()
        {
            // Para peones, solo mostramos la casilla de destino
            if (Piece.Type == PieceType.Pawn)
            {
                return To.ToString();
            }

            // Para otras piezas usamos el simbolo y la casilla de destino
            return $"{Piece.GetSymbol()}{To}";
        }

        public override string ToString()
        {
            return AlgebraicNotation;
        }
    }
}
