using ChessApp.Core.Enums;
using ChessApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Pieces
{
    public class Rook : Piece
    {
        // Constructor que inicializa una Torre con el color especificado.
        public Rook(PieceColor color) : base(color, PieceType.Rook)
        {
        }

        // Determina si el movimiento de la torre es valido segun las reglas del ajedrez.
        public override bool IsValidMove(Position from, Position to, Board board)
        {
            // El movimiento de la torre es en linea recta por la misma fila o misma columna
            bool isStraight = (from.Row == to.Row) || (from.Column == to.Column);

            // Si no es en línea recta, el movimiento no es válido para una torre.
            if (!isStraight)
                return false;

            // Verificar que el camino este libre
            if (!IsPathClear(from, to, board))
                return false;

            // Verificar captura: puede mover a casilla vacia o capturar pieza
            Piece targetPiece = board.GetPieceAt(to);
            return targetPiece == null || targetPiece.Color != this.Color;
        }
    }
}
