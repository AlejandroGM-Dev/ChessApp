using ChessApp.Core.Enums;
using ChessApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Pieces
{
    public class Queen : Piece
    {
        // Constructor que inicializa a la Reina con el color especificado.
        public Queen(PieceColor color) : base(color, PieceType.Queen)
        {
        }

        public override bool IsValidMove(Position from, Position to, Board board)
        {
            // El movimiento de la reina es como torre o alfil
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);
            bool isStraight = (from.Row == to.Row) || (from.Column == to.Column);
            bool isDiagonal = rowDiff == colDiff;

            if (!isStraight && !isDiagonal)
                return false;

            // Verificar que el camino este libre
            if(!IsPathClear(from, to, board))
                return false;

            // Verificar destino
            Piece target = board.GetPieceAt(to);
            return target == null || target.Color != Color;
        }
    }
}
