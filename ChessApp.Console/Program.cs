using ChessApp.Core.Enums;
using ChessApp.Core.Game;
using ChessApp.Core.Models;
using ChessApp.Core.Pieces;
using ChessApp.Core.Services;
using System;
using System.IO;
using System.Linq;

namespace ChessApp.Console
{
    class Program
    {
        private static GameManager _gameManager = new GameManager();

        static void Main(string[] args)
        {
            System.Console.WriteLine("=== CHESS APP - SISTEMA COMPLETO CON GUARDADO ===");

            while (true)
            {
                ShowMainMenu();
            }
        }

        static void ShowMainMenu()
        {
            System.Console.WriteLine("\n--- MENÚ PRINCIPAL ---");
            System.Console.WriteLine("1. Nueva partida");
            System.Console.WriteLine("2. Ver partidas guardadas");
            System.Console.WriteLine("3. Salir");
            System.Console.Write("Seleccione una opción: ");

            string input = System.Console.ReadLine();
            switch (input)
            {
                case "1":
                    PlayNewGame();
                    break;
                case "2":
                    ShowSavedGames();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    System.Console.WriteLine("Opción no válida.");
                    break;
            }
        }

        static void PlayNewGame()
        {
            System.Console.Write("Nombre del jugador blanco: ");
            string white = System.Console.ReadLine() ?? "White";
            System.Console.Write("Nombre del jugador negro: ");
            string black = System.Console.ReadLine() ?? "Black";

            var game = new ChessGame(white, black);

            System.Console.WriteLine($"\nNueva partida: {white} vs {black}");
            System.Console.WriteLine("Ingrese movimientos en formato 'desde hasta' (ej: e2 e4)");
            System.Console.WriteLine("Para promoción: ingrese movimiento y luego la pieza (Q, R, B, N)");
            System.Console.WriteLine("Escriba 'salir' para terminar la partida\n");

            while (game.Status == GameStatus.InProgress ||
                   game.Status == GameStatus.WhiteCheck ||
                   game.Status == GameStatus.BlackCheck)
            {
                DisplayCurrentGameState(game);

                System.Console.WriteLine($"Turno de: {(game.CurrentPlayer == PieceColor.White ? white : black)}");
                System.Console.Write("Movimiento: ");
                string? moveInput = System.Console.ReadLine();
                if (string.IsNullOrEmpty(moveInput))
                    continue;

                moveInput = moveInput.Trim();

                if (moveInput.ToLower() == "salir")
                    break;

                string[] parts = moveInput.Split(' ');
                if (parts.Length < 2)
                {
                    System.Console.WriteLine("Formato incorrecto. Use: [desde] [hasta]");
                    continue;
                }

                try
                {
                    Position from = new Position(parts[0]);
                    Position to = new Position(parts[1]);

                    PieceType? promotion = null;
                    if (parts.Length > 2)
                    {
                        promotion = parts[2].ToUpper() switch
                        {
                            "Q" => PieceType.Queen,
                            "R" => PieceType.Rook,
                            "B" => PieceType.Bishop,
                            "N" => PieceType.Knight,
                            _ => null
                        };
                    }

                    var result = game.AttemptMove(from, to, promotion);
                    if (result.IsSuccess)
                    {
                        System.Console.WriteLine($"✅ {result.Message}: {result.Move}");
                    }
                    else
                    {
                        System.Console.WriteLine($"❌ {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"❌ Error: {ex.Message}");
                }
            }

            // Mostrar resultado final
            DisplayCurrentGameState(game);
            System.Console.WriteLine($"\n--- PARTIDA TERMINADA ---");
            System.Console.WriteLine($"Resultado: {game.Status}");

            // Guardar la partida
            try
            {
                _gameManager.SaveGame(game);
                System.Console.WriteLine("✅ Partida guardada correctamente");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error al guardar partida: {ex.Message}");
            }

            System.Console.WriteLine("Presione cualquier tecla para continuar...");
            System.Console.ReadKey();
        }

        static void ShowSavedGames()
        {
            var savedGames = _gameManager.GetSavedGames();

            if (!savedGames.Any())
            {
                System.Console.WriteLine("\nNo hay partidas guardadas.");
                return;
            }

            System.Console.WriteLine("\n--- PARTIDAS GUARDADAS ---");
            for (int i = 0; i < savedGames.Count; i++)
            {
                System.Console.WriteLine($"{i + 1}. {savedGames[i].DisplayName}");
            }

            System.Console.Write("\nSeleccione una partida para ver detalles (0 para volver): ");
            if (int.TryParse(System.Console.ReadLine(), out int selection) && selection > 0 && selection <= savedGames.Count)
            {
                ShowGameDetails(savedGames[selection - 1]);
            }
        }

        static void ShowGameDetails(SavedGameInfo gameInfo)
        {
            System.Console.WriteLine($"\n--- DETALLES DE PARTIDA ---");
            System.Console.WriteLine($"Archivo: {gameInfo.FileName}");
            System.Console.WriteLine($"Blancas: {gameInfo.WhitePlayer}");
            System.Console.WriteLine($"Negras: {gameInfo.BlackPlayer}");
            System.Console.WriteLine($"Fecha: {gameInfo.Date}");
            System.Console.WriteLine($"Resultado: {gameInfo.Result}");
            System.Console.WriteLine($"Guardado: {gameInfo.CreatedAt:yyyy-MM-dd HH:mm}");

            try
            {
                // Mostrar contenido del archivo PGN
                string pgnContent = File.ReadAllText(gameInfo.FilePath);
                System.Console.WriteLine("\n--- CONTENIDO PGN ---");
                System.Console.WriteLine(pgnContent);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error al leer archivo: {ex.Message}");
            }

            System.Console.WriteLine("\nPresione cualquier tecla para continuar...");
            System.Console.ReadKey();
        }

        static void DisplayCurrentGameState(ChessGame game)
        {
            System.Console.WriteLine("\n--- TABLERO ACTUAL ---");
            DisplayBoard(game.Board);

            System.Console.WriteLine("\n--- HISTORIAL DE MOVIMIENTOS ---");
            var moves = game.GetFormattedMoveHistory();
            foreach (var move in moves)
            {
                System.Console.WriteLine(move);
            }
            System.Console.WriteLine();
        }

        static void DisplayBoard(Board board)
        {
            for (int row = 8; row >= 1; row--)
            {
                System.Console.Write($"{row} ");
                for (int col = 1; col <= 8; col++)
                {
                    var piece = board.GetPieceAt(new Position(row, col));
                    if (piece == null)
                    {
                        System.Console.Write(". ");
                    }
                    else
                    {
                        char symbol = GetPieceSymbol(piece);
                        System.Console.Write(symbol + " ");
                    }
                }
                System.Console.WriteLine();
            }
            System.Console.WriteLine("  a b c d e f g h");
        }

        static char GetPieceSymbol(Piece piece)
        {
            char symbol = piece.Type switch
            {
                PieceType.Pawn => 'P',
                PieceType.Rook => 'R',
                PieceType.Knight => 'N',
                PieceType.Bishop => 'B',
                PieceType.Queen => 'Q',
                PieceType.King => 'K',
                _ => '?'
            };

            return piece.Color == PieceColor.White ? char.ToUpper(symbol) : char.ToLower(symbol);
        }
    }
}