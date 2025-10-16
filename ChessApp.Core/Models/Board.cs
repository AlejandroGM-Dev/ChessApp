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
        public Piece[,] Squares {get; private set;}

        // Constructor de tablero. Inicializa la matriz y llama a la funcion de inicializacion
        public Board() 
        {
            Squares = new Piece[9, 9]; // Se usa 9x9 para tener indices del 1 al 8
            InitializeBoard();
        }

        // Metodo privado para inicializar el tablero con las piezas en sus posiciones iniciales
        private void InitializeBoard()
        {
            // Por ahora estara vacio
        }

        // Devuelve la pieza en una posicion especifica del tablero
        public Piece GetPieceAt(Position position)
        {
            return Squares[position.Row, position.Column];
        }

        // Coloca una pieza en una posicion especifica del tablero
        public void PlacePieceAt(Piece piece, Position position)
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
