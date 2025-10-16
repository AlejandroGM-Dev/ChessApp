using ChessApp.Core.Enums;
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

            if(rowDiff > 1 || colDiff > 1)
                return false;

            // Verificar destino
            Piece target = board.GetPieceAt(to);
            return target == null || target.Color != Color;
        }

    }
}
