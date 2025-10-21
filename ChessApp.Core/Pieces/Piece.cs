using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessApp.Core.Models;
using ChessApp.Core.Enums;

namespace ChessApp.Core.Pieces
{
    // Clase abstracta base para todas las piezas del ajedrez
    public abstract class Piece
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
        public bool HasMoved { get; set; } = false; // Para enroque y movimiento inicial de peones

        protected Piece(PieceColor color, PieceType type)
        {
            Color = color;
            Type = type;
        }

        // Metodo abstracto que todas las piezas deben implementar con su propia logica

        public abstract bool IsValidMove(Position from, Position to, Board board);

        // Metodo para obtener el simbolo de la pieza en notacion algebraica
        public virtual string GetSymbol()
        {
            return Type switch
            {
                PieceType.Rook => "R",
                PieceType.Knight => "N", // Knight usa "N" porque "K" es para King
                PieceType.Bishop => "B",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => "" // Peon no tiene simbolo
            };
        }

        // Metodo para verificar si el camino está libre (Para torres, alfiles y reinas)
        protected bool IsPathClear(Position from, Position to, Board board)
        {
            int rowStep = Math.Sign(to.Row - from.Row);
            int colStep = Math.Sign(to.Column - from.Column);

            int currentRow = from.Row + rowStep;
            int currentCol = from.Column + colStep;

            // Verificar cada casilla en el camino hasta llegar al destino
            while (currentRow != to.Row || currentCol != to.Column)
            {
                // Verificar que la posición actual es válida
                if (!Board.IsValidPosition(new Position(currentRow, currentCol)))
                    return false;

                Position currentPos = new Position(currentRow, currentCol);
                if (board.GetPieceAt(currentPos) != null)
                    return false;

                currentRow += rowStep;
                currentCol += colStep;
            }

            return true;
        }
    }
}
