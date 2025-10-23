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
        public MoveResult AttemptMove(Position from, Position to, PieceType? promotionPiece = null)
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
            Piece? piece = Board.GetPieceAt(from);
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
            Piece? targetPiece = Board.GetPieceAt(to);
            if ( targetPiece != null && targetPiece.Color == CurrentPlayer)
            {
                return MoveResult.Failure("No puedes capturar tus propias piezas");
            }

            // Verificar la promoción de un peon
            bool isPromotion = piece.Type == PieceType.Pawn && ((Pawn)piece).IsPromotionMove(to);
            if (isPromotion && !promotionPiece.HasValue)
            {
                return MoveResult.Failure("Promotion piece must be specified for pawn promotion");
            }

            // Si la promocion es especificada y verificada y pro tanto valida
            if (promotionPiece.HasValue && !IsValidPromotionPiece(promotionPiece.Value))
            {
                return MoveResult.Failure("Invalid promotion piece");
            }

            // Si todas las validaciones pasan, ejecutar el movimiento
            return ExecuteMove(from, to, piece, targetPiece, promotionPiece);
        }

        private MoveResult ExecuteMove(Position from, Position to, Piece piece, Piece capturedPiece, PieceType? promotionPiece = null)
        {
            // Verificar si es un enroque
            bool isCastling = piece.Type == PieceType.King && Math.Abs(to.Column - from.Column) == 2;

            // Verificar si es una promocion
            bool isPromotion = piece.Type == PieceType.Pawn &&
                      ((piece.Color == PieceColor.White && to.Row == 8) ||
                       (piece.Color == PieceColor.Black && to.Row == 1));

            if (isPromotion && !promotionPiece.HasValue)
            {
                promotionPiece = PieceType.Queen;
            }


            // Crear el objeto Move
            Move move = new Move(from, to, piece, capturedPiece);
            move.IsCastling = isCastling;
            move.IsPromotion = isPromotion;
            move.PromotedPieceType = promotionPiece;

            // Actualizar el tablero
            Board.PlacePieceAt(piece, to);
            Board.PlacePieceAt(null, from);

            // Ejecutar enroque si es necesario
            if (isCastling)
            {
                ExecuteCastling(from, to);
            }

            // Ejecutar promocion si es necesario
            if (isPromotion)
            {
                // Usar Queen como valor por defecto si promotionPiece es null
                PieceType pieceToPromote = promotionPiece ?? PieceType.Queen;
                ExecutePromotion(to, pieceToPromote);
            }

            // Marcar que la pieza se ha movido
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

        private void ExecuteCastling(Position kingFrom, Position kingTo)
        {
            bool isKingside = kingTo.Column > kingFrom.Column;
            int rookFromColumn = isKingside ? 8 : 1;
            int rookToColumn = isKingside ? kingTo.Column - 1 : kingTo.Column + 1;

            Position rookFrom = new Position(kingFrom.Row, rookFromColumn);
            Position rookTo = new Position(kingFrom.Row, rookToColumn);

            Piece? rook = Board.GetPieceAt(rookFrom);
            Board.PlacePieceAt(rook, rookTo);
            Board.PlacePieceAt(null, rookFrom);

            // Marcar que la torre se ha movido
            rook.HasMoved = true;
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
            // Verificar jaque
            bool whiteInCheck = CheckValidator.IsKingInCheck(Board, PieceColor.White);
            bool blackInCheck = CheckValidator.IsKingInCheck(Board, PieceColor.Black);

            if (whiteInCheck)
            {
                if (CheckValidator.IsCheckmate(Board, PieceColor.White))
                {
                    Status = GameStatus.WhiteCheckMate;
                }
                else
                {
                    Status = GameStatus.WhiteCheck;
                }
            }
            else if (blackInCheck)
            {
                if (CheckValidator.IsCheckmate(Board, PieceColor.Black))
                {
                    Status = GameStatus.BlackCheckMate;
                }
                else
                {
                    Status = GameStatus.BlackCheck;
                }
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
        }
    }
}
