using ChessApp.Core.Enums;
using ChessApp.Core.Models;
using ChessApp.Core.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Game
{
    public class ChessGame
    {
        public Board Board { get; private set; }
        public PieceColor CurrentPlayer { get; private set; }
        public GameStatus Status { get; private set; }
        public List<Move> MoveHistory { get; private set; }
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }
        public DateTime StartTime { get; private set; }

        public ChessGame(string whitePlayer = "Blancas", string blackPlayer = "Negras")
        {
            Board = new Board();
            CurrentPlayer = PieceColor.White;
            Status = GameStatus.InProgress;
            MoveHistory = new List<Move>();
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
            StartTime = DateTime.Now;
        }

        // Metodo principal para intentar hacer un movimiento 
        public MoveResult AttempMove(Position from, Position to)
        {
            // Verificar que el juego esta en progreso
            if (Status != GameStatus.InProgress)
            {
                return MoveResult.Failure("El juego ha terminado");
            }

            // Verificar que las posiciones estan dentro del tablero
            if (!Board.IsValidPosition(from) || !Board.IsValidPosition(to))
            {
                return MoveResult.Failure("Posicion duera del tablero");
            }

            // Obtener la pieza en la posicion de origen
            Piece piece = Board.GetPieceAt(from);
            if ( piece == null)
            {
                return MoveResult.Failure("No hay pieza en la posicion de origen");
            }

            // Verificar que la pieza es del jugador actual
            if (piece.Color != CurrentPlayer)
            {
                return MoveResult.Failure("No es tu turno");
            }

            // Verificar si el movimiento es valido para la pieza
            if (!piece.IsValidMove(from, to, Board))
            {
                return MoveResult.Failure("Movimiento no valido para esta pieza");
            }

            // Verificar que no se capture una pieza del mismo color
            Piece targetPiece = Board.GetPieceAt(to);
            if ( targetPiece == null && targetPiece.Color == CurrentPlayer)
            {
                return MoveResult.Failure("No puedes capturar tus propias piezas");
            }

            // Si todas las validaciones pasan, ejecutar el movimiento
            return ExecuteMove(from, to, piece, targetPiece);
        }

        private MoveResult ExecuteMove(Position from, Position to, Piece piece, Piece capturedPiece)
        {
            // Crear el objeto Move
            Move move = new Move(from, to, piece, capturedPiece);

            // Actualizar el tablero
            Board.PlacePieceAt(piece, to);
            Board.PlacePieceAt(null, from);

            // Marcar que la pieza se ha movido (para peones, enroque, etc.)
            piece.HasMoved = true;

            // Agregar el movimiento al historial
            MoveHistory.Add(move);

            // Cambiar turno
            CurrentPlayer = (CurrentPlayer == PieceColor.White) ? PieceColor.Black : PieceColor.White;


            // Verificar estado del juego (jaque, jaque mate, etc.)
            UpdateGameStatus();
            return MoveResult.Success("Movimiento exitoso", move);
        }

        private void UpdateGameStatus()
        {
            // Por ahora, implementacion basica
            // En el futuro, aqui ira la logica para detectar jaque y jaque mate
            Status = GameStatus.InProgress;
        }

        // Obtener el historial de movimientos formateado
        public List<string> GetFormmattedMoveHistory()
        {
            var formattedMoves = new List<string>();

            for (int i = 0; i < MoveHistory.Count; i++)
            {
                Move move = MoveHistory[i];
                string moveNumber = (i / 2 + 1).ToString() + ".";
                string moveText = move.ToAlgebraicNotation();

                // Si es el primer movimiento del turno (blancas), agregar numero
                if (i % 2 == 0)
                {
                    formattedMoves.Add($"{moveNumber} {moveText}");
                }
                else
                {
                    // Si el movimiento es de las negras, agregarlo al ultimo elemento
                    formattedMoves[formattedMoves.Count - 1] += $"{ moveNumber}";
                }
            }
            return formattedMoves;
        }

        // Reiniciar el juego
        public void ResetGame()
        {
            Board = new Board();
            CurrentPlayer = PieceColor.White;
            Status = GameStatus.InProgress;
            MoveHistory.Clear();
            StartTime = DateTime.Now;
        }
    }
}
