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
        public Piece? CapturedPiece { get; set; }
        public DateTime Timestamp { get; set; }
        public string AlgebraicNotation => ToAlgebraicNotation();
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsCapture { get; set; }
        public bool IsCastling { get; set; }
        public bool IsPromotion { get; set; }
        public PieceType? PromotedPieceType{ get; set; }
        public bool IsEnPassant { get; set; }

        public Move(Position from, Position to, Piece piece, Piece? capturedPiece = null)
        {
            From = from;
            To = to;
            Piece = piece;
            CapturedPiece = capturedPiece;
            Timestamp = DateTime.Now;
            IsCapture = capturedPiece != null;
        }

        public string ToAlgebraicNotation()
        {
            // Notación especial para enroque
            if (IsCastling)
            {
                return To.Column > From.Column ? "O-O" : "O-O-O"; // O-O (corto) / O-O-O (largo)
            }

            string notation;

            if (Piece.Type == PieceType.Pawn)
            {
                // Captura al paso - notación especial
                if (IsEnPassant)
                {
                    notation = $"{From.GetColumnLetter()}x{To} e.p.";
                }
                // Captura normal de peón
                else if (IsCapture)
                {
                    notation = $"{From.GetColumnLetter()}x{To}";
                }
                // Movimiento simple
                else
                {
                    notation = To.ToString();
                }

                // Agregar notación de promoción
                if (IsPromotion && PromotedPieceType.HasValue)
                {
                    string promotionSymbol = GetPieceSymbol(PromotedPieceType.Value);
                    notation += $"={promotionSymbol}";
                }
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

        private string GetPieceSymbol(PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                _ => "Q" // Default to queen
            };
        }

        public override string ToString()
        {
            return AlgebraicNotation;
        }

        public string ToUciNotation()
        {
            string uci = From.ToString() + To.ToString();

            // Agregar pieza de promoción si es necesario
            if (IsPromotion && PromotedPieceType.HasValue)
            {
                char promotionChar = PromotedPieceType.Value switch
                {
                    PieceType.Queen => 'q',
                    PieceType.Rook => 'r',
                    PieceType.Bishop => 'b',
                    PieceType.Knight => 'n',
                    _ => 'q'
                };
                uci += promotionChar;
            }

            return uci;
        }
    }
}
