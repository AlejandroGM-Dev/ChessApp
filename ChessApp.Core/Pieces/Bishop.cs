using ChessApp.Core.Enums;
using ChessApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Pieces
{
    public class Bishop : Piece
    {
        // Constructor que inicializa el Alfil con el color especificado.
        public Bishop(PieceColor color) : base(color, PieceType.Bishop)
        {
        }

        // Determina si el movimiento del alfil es valido segun las reglas del ajedrez
        public override bool IsValidMove(Position from, Position to, Board board)
        {
            // El alfil se mueve en diagonal
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            if (rowDiff != colDiff)
                return false;

            // Verificar que el camino este libre
            if (IsPathClear(from, to, board))
                return false;

            // Verificar destino
            Piece target = board.GetPieceAt(to);
            return target == null || target.Color != Color;
        }
    }
}
