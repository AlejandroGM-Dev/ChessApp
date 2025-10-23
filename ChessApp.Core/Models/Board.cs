using ChessApp.Core.Enums;
using ChessApp.Core.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Models
{
    // Representa el tablero de ajedrez y contiene la logica para manipular las pieza
    public class Board
    {
        // Matriz que representa el tablero de ajedrez donde cada celda puede o no contener una pieza de ajedrez
        public Piece?[,] Squares {get; private set;}

        // Constructor de tablero. Inicializa la matriz y llama a la funcion de inicializacion
        public Board() 
        {
            Squares = new Piece?[9, 9]; // Se usa 9x9 para tener indices del 1 al 8
            InitializeBoard();
        }

        // Metodo privado para inicializar el tablero con las piezas en sus posiciones iniciales
        private void InitializeBoard()
        {
            // Limpiar tablero
            for (int row = 1; row <= 8; row++)
            {
                for (int col = 1; col <= 8; col++)
                {
                    Squares[row, col] = null;
                }
            }

            // === COLOCAR PIEZAS BLANCAS (filas 1 y 2) ===

            // Peones blancos en fila 2
            for (int col = 1; col <= 8; col++)
            {
                Squares[2, col] = new Pawn(PieceColor.White);
            }

            // Piezas blancas en fila 1
            Squares[1, 1] = new Rook(PieceColor.White);
            Squares[1, 2] = new Knight(PieceColor.White);
            Squares[1, 3] = new Bishop(PieceColor.White);
            Squares[1, 4] = new Queen(PieceColor.White);
            Squares[1, 5] = new King(PieceColor.White);
            Squares[1, 6] = new Bishop(PieceColor.White);
            Squares[1, 7] = new Knight(PieceColor.White);
            Squares[1, 8] = new Rook(PieceColor.White);

            // Peones negros en fila 7
            for (int col = 1; col <= 8; col++)
            {
                Squares[7, col] = new Pawn(PieceColor.Black);
            }

            // Piezas negras en fila 8
            Squares[8, 1] = new Rook(PieceColor.Black);
            Squares[8, 2] = new Knight(PieceColor.Black);
            Squares[8, 3] = new Bishop(PieceColor.Black);
            Squares[8, 4] = new Queen(PieceColor.Black);
            Squares[8, 5] = new King(PieceColor.Black);
            Squares[8, 6] = new Bishop(PieceColor.Black);
            Squares[8, 7] = new Knight(PieceColor.Black);
            Squares[8, 8] = new Rook(PieceColor.Black);
        }

        // Devuelve la pieza en una posicion especifica del tablero
        public Piece? GetPieceAt(Position position)
        {
            // Verificar que la posición está dentro de los límites
            if (position.Row < 1 || position.Row > 8 || position.Column < 1 || position.Column > 8)
                return null;

            return Squares[position.Row, position.Column];
        }

        // Coloca una pieza en una posicion especifica del tablero
        public void PlacePieceAt(Piece? piece, Position position)
        {
            Squares[position.Row, position.Column] = piece;
        }

        // Verifica que la posicion esta dentro de los limites del tablero
        public static bool IsValidPosition(Position position)
        {
            return position.Row >= 1 && position.Row <= 8 && position.Column >= 1 && position.Column <= 8;
        }
    }
}
