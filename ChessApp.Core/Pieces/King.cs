using ChessApp.Core.Enums;
using ChessApp.Core.Game;
using ChessApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Pieces
{
    public class King : Piece
    {
        // Constructor que inicializa al Rey segun el color especificado
        public King(PieceColor color) : base(color, PieceType.King)
        {
        }

        // Determina si el movimiento es valido segun las reglas del ajedrez
        public override bool IsValidMove(Position from, Position to, Board board)
        {
            // El rey se mueve una casilla en cualquier direccion
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            if (rowDiff <= 1 && colDiff <= 1)
            {
                // Verificar destino: vacío o pieza enemiga
                Piece? target = board.GetPieceAt(to);
                return target == null || target.Color != Color;
            }

            // Verificar enroque
            if (rowDiff == 0 && colDiff == 2 && !HasMoved)
            {
                return IsCastlingMove(from, to, board);
            }

            return false;
        }

        private bool IsCastlingMove(Position from, Position to, Board board)
        {
            // Determinar si es enroque corto (kingside) o largo (queenside)
            bool isKingside = to.Column > from.Column;
            int rookColumn = isKingside ? 8 : 1;
            int direction = isKingside ? 1 : -1;

            // Verificar que la torre existe y no se ha movido
            Position rookPosition = new Position(from.Row, rookColumn);
            Piece? rook = board.GetPieceAt(rookPosition);

            if (rook == null || rook.Type != PieceType.Rook || rook.Color != Color || rook.HasMoved)
                return false;

            // Verificar que las casillas entre el rey y la torre estén vacías
            for (int col = from.Column + direction; col != rookColumn; col += direction)
            {
                Position betweenPosition = new Position(from.Row, col);
                if (board.GetPieceAt(betweenPosition) != null)
                    return false;
            }

            // Verificar que el rey no está en jaque ni pasa por casillas en jaque
            if (CheckValidator.IsKingInCheck(board, Color))
                return false;

            // Verificar que no pasa por casillas en jaque
            for (int col = from.Column; col != to.Column + direction; col += direction)
            {
                Position checkPosition = new Position(from.Row, col);
                if (WouldBeInCheckAfterMove(board, from, checkPosition))
                    return false;
            }

            return true;
        }

        private bool WouldBeInCheckAfterMove(Board board, Position from, Position to)
        {
            // Simular movimiento temporal
            Piece? originalPiece = board.GetPieceAt(to);
            board.PlacePieceAt(this, to);
            board.PlacePieceAt(null, from);

            bool inCheck = CheckValidator.IsKingInCheck(board, Color);

            // Revertir movimiento
            board.PlacePieceAt(this, from);
            board.PlacePieceAt(originalPiece, to);

            return inCheck;
        }

    }
}
