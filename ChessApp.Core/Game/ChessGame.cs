using ChessApp.Core.Enums;
using ChessApp.Core.Models;
using ChessApp.Core.Pieces;
using ChessApp.Core.Services;
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
        public PieceColor CurrentPlayer { get; set; }
        public GameStatus Status { get; private set; }
        public List<Move> MoveHistory { get; private set; }
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }
        public DateTime StartTime { get; private set; }
        public Position? LastPawnDoubleMove { get; private set; }

        public ChessGame(string whitePlayer = "Blancas", string blackPlayer = "Negras")
        {
            Board = new Board();
            CurrentPlayer = PieceColor.White;
            Status = GameStatus.InProgress;
            MoveHistory = new List<Move>();
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
            StartTime = DateTime.Now;
            LastPawnDoubleMove = null;
        }

        public string GetPgnResult()
        {
            return Status switch
            {
                GameStatus.WhiteCheckMate => "1-0",
                GameStatus.BlackCheckMate => "0-1",
                GameStatus.Stalmate or GameStatus.Draw => "1/2-1/2",
                _ => "*"
            };
        }

        public PgnMetadata GetPgnMetadata()
        {
            return new PgnMetadata
            {
                WhitePlayer = WhitePlayer,
                BlackPlayer = BlackPlayer,
                Date = StartTime.ToString("yyyy.MM.dd"),
                Result = GetPgnResult()
            };
        }

        // Metodo para hacer test
        public void SetLastPawnDoubleMoveForTesting(Position position)
        {
            LastPawnDoubleMove = position;
        }

        // Metodo principal para intentar hacer un movimiento 
        public MoveResult AttemptMove(Position from, Position to, PieceType? promotionPiece = null)
        {
            // Verificar que el juego esta en progreso
            if (Status != GameStatus.InProgress && Status != GameStatus.WhiteCheck && Status != GameStatus.BlackCheck)
            {
                return MoveResult.Failure("El juego ha terminado");
            }

            // Verificar que las posiciones estan dentro del tablero
            if (!Board.IsValidPosition(from) || !Board.IsValidPosition(to))
            {
                return MoveResult.Failure("Posicion fuera del tablero");
            }

            // Obtener la pieza en la posicion de origen
            Piece? piece = Board.GetPieceAt(from);
            if (piece == null)
            {
                return MoveResult.Failure("No hay pieza en la posicion de origen");
            }

            // Verificar que la pieza es del jugador actual
            if (piece.Color != CurrentPlayer)
            {
                return MoveResult.Failure("No es tu turno");
            }

            // Verificar promocion ANTES del movimiento
            bool isPromotion = piece.Type == PieceType.Pawn &&
                              ((piece.Color == PieceColor.White && to.Row == 8) ||
                               (piece.Color == PieceColor.Black && to.Row == 1));

            if (isPromotion && !promotionPiece.HasValue)
            {
                return MoveResult.Failure("Debe especificar una pieza para la promoción del peón");
            }

            if (promotionPiece.HasValue && !IsValidPromotionPiece(promotionPiece.Value))
            {
                return MoveResult.Failure("Pieza de promoción no válida");
            }

            // Verificar captura al paso
            bool isEnPassant = piece.Type == PieceType.Pawn &&
                              ((Pawn)piece).IsValidEnPassantCapture(from, to, Board, LastPawnDoubleMove);

            Piece? capturedPiece = null;

            if (isEnPassant)
            {
                // Para captura al paso, la pieza capturada está en una posición diferente
                int capturedPawnRow = from.Row;
                int capturedPawnCol = to.Column;
                Position capturedPawnPosition = new Position(capturedPawnRow, capturedPawnCol);
                capturedPiece = Board.GetPieceAt(capturedPawnPosition);
            }
            else
            {
                // Captura normal
                capturedPiece = Board.GetPieceAt(to);

                // Verificar si el movimiento es valido para la pieza (excepto para captura al paso)
                if (!piece.IsValidMove(from, to, Board))
                {
                    return MoveResult.Failure("Movimiento no valido para esta pieza");
                }

                // Verificar que no se capture una pieza del mismo color
                if (capturedPiece != null && capturedPiece.Color == CurrentPlayer)
                {
                    return MoveResult.Failure("No puedes capturar tus propias piezas");
                }
            }

            // VERIFICACIÓN CRÍTICA: El movimiento no debe dejar al rey en jaque
            if (!IsMoveSafeForKing(from, to, piece, isEnPassant, promotionPiece))
            {
                return MoveResult.Failure("No puedes dejar a tu rey en jaque");
            }

            // Si todas las validaciones pasan, ejecutar el movimiento
            return ExecuteMove(from, to, piece, capturedPiece, promotionPiece, isEnPassant);
        }

        // Metodo para verificar si un movimiento deja al rey en jaque
        private bool IsMoveSafeForKing(Position from, Position to, Piece piece, bool isEnPassant, PieceType? promotionPiece)
        {
            // Guardar el estado actual del tablero
            Piece? originalPieceAtTo = Board.GetPieceAt(to);
            Piece? originalPieceAtFrom = Board.GetPieceAt(from);

            // Simular el movimiento
            if (isEnPassant)
            {
                // Simular captura al paso
                Position capturedPawnPosition = new Position(from.Row, to.Column);
                Piece? capturedPawn = Board.GetPieceAt(capturedPawnPosition);

                Board.PlacePieceAt(piece, to);
                Board.PlacePieceAt(null, from);
                Board.PlacePieceAt(null, capturedPawnPosition);

                // Verificar si el rey queda en jaque
                bool kingInCheck = CheckValidator.IsKingInCheck(Board, CurrentPlayer);

                // Revertir la simulacion
                Board.PlacePieceAt(originalPieceAtFrom, from);
                Board.PlacePieceAt(originalPieceAtTo, to);
                Board.PlacePieceAt(capturedPawn, capturedPawnPosition);

                return !kingInCheck;
            }
            else
            {
                // Simular movimiento normal
                Board.PlacePieceAt(piece, to);
                Board.PlacePieceAt(null, from);

                // Si es promocion, simular la pieza promocionada
                if (promotionPiece.HasValue && piece.Type == PieceType.Pawn &&
                    ((piece.Color == PieceColor.White && to.Row == 8) ||
                     (piece.Color == PieceColor.Black && to.Row == 1)))
                {
                    Piece promotedPiece = CreatePromotedPiece(promotionPiece.Value, CurrentPlayer);
                    Board.PlacePieceAt(promotedPiece, to);
                }

                // Verificar si el rey queda en jaque
                bool kingInCheck = CheckValidator.IsKingInCheck(Board, CurrentPlayer);

                // Revertir la simulacion
                Board.PlacePieceAt(originalPieceAtFrom, from);
                Board.PlacePieceAt(originalPieceAtTo, to);

                return !kingInCheck;
            }
        }

        private MoveResult ExecuteMove(Position from, Position to, Piece piece, Piece? capturedPiece,
                                     PieceType? promotionPiece = null, bool isEnPassant = false)
        {
            // Verificar si es un enroque
            bool isCastling = piece.Type == PieceType.King && Math.Abs(to.Column - from.Column) == 2;

            // Verificar si es una promocion
            bool isPromotion = piece.Type == PieceType.Pawn &&
                      ((piece.Color == PieceColor.White && to.Row == 8) ||
                       (piece.Color == PieceColor.Black && to.Row == 1));

            // Asegurar que promotionPiece tenga valor si es promoción
            if (isPromotion && !promotionPiece.HasValue)
            {
                promotionPiece = PieceType.Queen;
            }

            // Crear el objeto Move ANTES de modificar el tablero
            Move move = new Move(from, to, piece, capturedPiece);
            move.IsCastling = isCastling;
            move.IsPromotion = isPromotion;
            move.PromotedPieceType = promotionPiece;
            move.IsEnPassant = isEnPassant;

            // Manejar captura al paso
            if (isEnPassant)
            {
                // En captura al paso, el peón capturado está en la misma fila de origen pero columna de destino
                Position capturedPawnPosition = new Position(from.Row, to.Column);
                Piece? actualCapturedPiece = Board.GetPieceAt(capturedPawnPosition);

                // Remover el peón capturado del tablero
                if (actualCapturedPiece != null)
                {
                    Board.PlacePieceAt(null, capturedPawnPosition);
                }
            }

            // Actualizar el tablero
            Board.PlacePieceAt(piece, to);
            Board.PlacePieceAt(null, from);

            // Ejecutar enroque si es necesario
            if (isCastling)
            {
                ExecuteCastling(from, to);
            }

            // Ejecutar promocion si es necesario
            if (isPromotion && promotionPiece.HasValue)
            {
                ExecutePromotion(to, promotionPiece.Value);
            }

            // Actualizar LastPawnDoubleMove
            UpdateLastPawnDoubleMove(piece, from, to);

            // Marcar que la pieza se ha movido (DESPUÉS de verificar IsDoubleMove)
            piece.HasMoved = true;

            // Verificar si el movimiento resulta en jaque
            PieceColor opponentColor = CurrentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            move.IsCheck = CheckValidator.IsKingInCheck(Board, opponentColor);

            // Agregar al historial
            MoveHistory.Add(move);

            // Cambiar turno
            CurrentPlayer = (CurrentPlayer == PieceColor.White) ? PieceColor.Black : PieceColor.White;

            // Verificar estado del juego
            UpdateGameStatus();

            // Si hay jaque mate, actualizar el movimiento
            if (Status == GameStatus.WhiteCheckMate || Status == GameStatus.BlackCheckMate)
            {
                move.IsCheckmate = true;
            }

            return MoveResult.Success("Movimiento exitoso", move);
        }

        private void UpdateLastPawnDoubleMove(Piece piece, Position from, Position to)
        {
            if (piece.Type == PieceType.Pawn)
            {
                Pawn pawn = (Pawn)piece;
                if (pawn.IsDoubleMove(from, to))
                {
                    LastPawnDoubleMove = to;
                }
                else
                {
                    LastPawnDoubleMove = null;
                }
            }
            else
            {
                LastPawnDoubleMove = null;
            }
        }

        private void ExecuteCastling(Position kingFrom, Position kingTo)
        {
            bool isKingside = kingTo.Column > kingFrom.Column;
            int rookFromColumn = isKingside ? 8 : 1;
            int rookToColumn = isKingside ? kingTo.Column - 1 : kingTo.Column + 1;

            Position rookFrom = new Position(kingFrom.Row, rookFromColumn);
            Position rookTo = new Position(kingFrom.Row, rookToColumn);

            Piece? rook = Board.GetPieceAt(rookFrom);
            if (rook != null)
            {
                Board.PlacePieceAt(rook, rookTo);
                Board.PlacePieceAt(null, rookFrom);

                // Marcar que la torre se ha movido
                rook.HasMoved = true;
            }
        }

        private void ExecutePromotion(Position position, PieceType promotionPiece)
        {
            Piece? currentPiece = Board.GetPieceAt(position);
            if (currentPiece == null)
            {
                System.Console.WriteLine("ERROR: No hay pieza en la posición de promoción");
                return;
            }

            if (currentPiece.Type != PieceType.Pawn)
            {
                System.Console.WriteLine($"ERROR: Intento de promover una pieza que no es un peón: {currentPiece.Type}");
                return;
            }

            Piece promotedPiece = CreatePromotedPiece(promotionPiece, CurrentPlayer);
            Board.PlacePieceAt(promotedPiece, position);

            System.Console.WriteLine($"✓ Promoción ejecutada: Peón en {position} -> {promotionPiece}");
        }

        private Piece CreatePromotedPiece(PieceType pieceType, PieceColor color)
        {
            return pieceType switch
            {
                PieceType.Queen => new Queen(color),
                PieceType.Rook => new Rook(color),
                PieceType.Bishop => new Bishop(color),
                PieceType.Knight => new Knight(color),
                _ => new Queen(color) // Default to queen
            };
        }

        private bool IsValidPromotionPiece(PieceType pieceType)
        {
            return pieceType == PieceType.Queen ||
                   pieceType == PieceType.Rook ||
                   pieceType == PieceType.Bishop ||
                   pieceType == PieceType.Knight;
        }

        // Metodo para traer las opciones validas para promocion
        public List<PieceType> GetValidPromotionOptions()
        {
            return new List<PieceType>
            {
                PieceType.Queen,
                PieceType.Rook,
                PieceType.Bishop,
                PieceType.Knight
            };
        }

        private void UpdateGameStatus()
        {
            // Verificar jaque mate primero
            if (CheckValidator.IsCheckmate(Board, PieceColor.White))
            {
                Status = GameStatus.WhiteCheckMate;
                return;
            }

            if (CheckValidator.IsCheckmate(Board, PieceColor.Black))
            {
                Status = GameStatus.BlackCheckMate;
                return;
            }

            // Verificar jaque simple
            bool whiteInCheck = CheckValidator.IsKingInCheck(Board, PieceColor.White);
            bool blackInCheck = CheckValidator.IsKingInCheck(Board, PieceColor.Black);

            if (whiteInCheck)
            {
                Status = GameStatus.WhiteCheck;
            }
            else if (blackInCheck)
            {
                Status = GameStatus.BlackCheck;
            }
            else
            {
                Status = GameStatus.InProgress;
            }
        }

        // Obtener el historial de movimientos formateado
        public List<string> GetFormattedMoveHistory()
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
                    formattedMoves[formattedMoves.Count - 1] += $" {moveText}";
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
            LastPawnDoubleMove = null;
        }
    }
}