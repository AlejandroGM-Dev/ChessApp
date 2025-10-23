using ChessApp.Core.Enums;
using ChessApp.Core.Models;

namespace ChessApp.Core.Pieces
{
    public class Pawn : Piece
    {
        public Pawn(PieceColor color) : base(color, PieceType.Pawn)
        {
        }

        public override bool IsValidMove(Position from, Position to, Board board)
        {
            int direction = Color == PieceColor.White ? 1 : -1;
            int startRow = Color == PieceColor.White ? 2 : 7;

            // Movimiento hacia adelante (1 casilla)
            if (to.Column == from.Column && to.Row == from.Row + direction)
            {
                return board.GetPieceAt(to) == null;
            }

            // Movimiento inicial de 2 casillas
            if (!HasMoved && from.Row == startRow && to.Column == from.Column && to.Row == from.Row + (2 * direction))
            {
                Position intermediate = new Position(from.Row + direction, from.Column);
                return board.GetPieceAt(intermediate) == null && board.GetPieceAt(to) == null;
            }

            // Captura en diagonal normal
            if (Math.Abs(to.Column - from.Column) == 1 && to.Row == from.Row + direction)
            {
                Piece? target = board.GetPieceAt(to);
                return target != null && target.Color != Color;
            }

            return false;
        }

        // Método específico para captura al paso
        public bool IsValidEnPassantCapture(Position from, Position to, Board board, Position? lastPawnDoubleMove)
        {
            if (lastPawnDoubleMove == null) return false;

            int direction = Color == PieceColor.White ? 1 : -1;
            int enPassantRow = Color == PieceColor.White ? 5 : 4;

            // Verificar que el peón está en la fila correcta para captura al paso
            if (from.Row != enPassantRow) return false;

            // Verificar que el movimiento es diagonal
            if (Math.Abs(to.Column - from.Column) != 1 || to.Row != from.Row + direction)
                return false;

            // Verificar que la casilla destino está vacía
            if (board.GetPieceAt(to) != null) return false;

            // CORRECCIÓN: El peón enemigo está en la MISMA fila que el peón capturador
            // y en la columna de destino, no en una posición diferente
            Position enemyPawnPosition = new Position(from.Row, to.Column);
            Piece? enemyPawn = board.GetPieceAt(enemyPawnPosition);

            if (enemyPawn == null || enemyPawn.Type != PieceType.Pawn || enemyPawn.Color == Color)
                return false;

            // CORRECCIÓN: Verificar que la posición del último movimiento coincide con el peón enemigo
            return enemyPawnPosition.Equals(lastPawnDoubleMove);
        }

        public bool IsPromotionMove(Position targetPosition)
        {
            return (Color == PieceColor.White && targetPosition.Row == 8) ||
                   (Color == PieceColor.Black && targetPosition.Row == 1);
        }

        // Método para verificar si un movimiento es un avance de dos casillas
        public bool IsDoubleMove(Position from, Position to)
        {
            int direction = Color == PieceColor.White ? 1 : -1;
            int startRow = Color == PieceColor.White ? 2 : 7;

            // CORRECCIÓN: Verificar que el movimiento es exactamente de 2 filas
            bool isDoubleMove = !HasMoved &&
                               from.Row == startRow &&
                               to.Column == from.Column &&
                               to.Row == from.Row + (2 * direction);

            return isDoubleMove;
        }
    }
}