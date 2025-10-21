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
    public static class CheckValidator
    {
        // Verificar si el rey del color especificado está en jaque
        public static bool IsKingInCheck(Board board, PieceColor kingColor)
        {
            // Encontrar la posición del rey
            Position kingPosition = FindKingPosition(board, kingColor);
            if (kingPosition == null) return false;

            // Verificar si alguna pieza enemiga puede capturar al rey
            PieceColor enemyColor = kingColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

            for (int row = 1; row <= 8; row++)
            {
                for (int col = 1; col <= 8; col++)
                {
                    Position position = new Position(row, col);
                    Piece piece = board.GetPieceAt(position);

                    if (piece != null && piece.Color == enemyColor)
                    {
                        if (piece.IsValidMove(position, kingPosition, board))
                        {
                            return true; // ¡Jaque!
                        }
                    }
                }
            }

            return false;
        }

        // Encontrar la posición del rey en el tablero
        private static Position? FindKingPosition(Board board, PieceColor kingColor)
        {
            for (int row = 1; row <= 8; row++)
            {
                for (int col = 1; col <= 8; col++)
                {
                    Position position = new Position(row, col);
                    Piece piece = board.GetPieceAt(position);

                    if (piece != null && piece.Type == PieceType.King && piece.Color == kingColor)
                    {
                        return position;
                    }
                }
            }
            return null;
        }

        // Verificar si el jugador está en jaque mate
        public static bool IsCheckmate(Board board, PieceColor playerColor)
        {
            // Primero verificar que el rey está en jaque
            if (!IsKingInCheck(board, playerColor))
                return false;

            // Verificar si existe algún movimiento legal que salve al rey
            for (int fromRow = 1; fromRow <= 8; fromRow++)
            {
                for (int fromCol = 1; fromCol <= 8; fromCol++)
                {
                    Position from = new Position(fromRow, fromCol);
                    Piece piece = board.GetPieceAt(from);

                    if (piece != null && piece.Color == playerColor)
                    {
                        // Probar todos los movimientos posibles de esta pieza
                        for (int toRow = 1; toRow <= 8; toRow++)
                        {
                            for (int toCol = 1; toCol <= 8; toCol++)
                            {
                                Position to = new Position(toRow, toCol);

                                if (piece.IsValidMove(from, to, board))
                                {
                                    // Simular el movimiento
                                    Piece? capturedPiece = SimulateMove(board, from, to);

                                    // Verificar si después del movimiento el rey sigue en jaque
                                    bool stillInCheck = IsKingInCheck(board, playerColor);

                                    // Revertir el movimiento
                                    RevertMove(board, from, to, capturedPiece);

                                    if (!stillInCheck)
                                    {
                                        return false; // Hay un movimiento que salva al rey
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true; // No hay movimientos que salven al rey - ¡Jaque mate!
        }

        // Simular un movimiento temporal para verificar jaque
        private static Piece? SimulateMove(Board board, Position from, Position to)
        {
            Piece movingPiece = board.GetPieceAt(from);
            Piece capturedPiece = board.GetPieceAt(to);

            board.PlacePieceAt(movingPiece, to);
            board.PlacePieceAt(null, from);

            return capturedPiece;
        }

        // Revertir un movimiento simulado
        private static void RevertMove(Board board, Position from, Position to, Piece? capturedPiece)
        {
            Piece movingPiece = board.GetPieceAt(to);
            board.PlacePieceAt(movingPiece, from);
            board.PlacePieceAt(capturedPiece, to);
        }
    }
}