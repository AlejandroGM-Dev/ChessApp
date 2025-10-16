using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessApp.Core.Models;
using ChessApp.Core.Enums;

namespace ChessApp.Core.Pieces
{
    public class Pawn : Piece
    {
        // Constructor que inicializa un Peon con el color especificado.
        public Pawn(PieceColor color) : base(color, PieceType.Pawn)
        {
        }

        // Determina si el movimiento es valido segun las reglas del ajedrez.
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

            // Captura en diagnonal
            if (Math.Abs(to.Column - from.Column) == 1 && to.Row == from.Row + direction)
            {
                Piece target = board.GetPieceAt(to);
                return target != null && target.Color != Color;
            }

            return false;
        }
    }
}
