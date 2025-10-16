using ChessApp.Core.Enums;
using ChessApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Pieces
{
    public class Knight : Piece
    {
        // Constructor que inicializa un Caballo con el color especificado
        public Knight(PieceColor color) : base(color, PieceType.Knight)
        {
        }

        // Determina si el movimiento del caballo es valido segun las reglas del ajedrez
        public override bool IsValidMove(Position from, Position to, Board board)
        {
            // El movimiento del caballo es en L
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            bool isValidMove = (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
            if (!isValidMove)
                return false;

            // El caballo puede saltar, solo verificar destino
            Piece target = board.GetPieceAt(to);
            return target == null || target.Color != Color;

        }
    }
}
