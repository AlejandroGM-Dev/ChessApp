using ChessApp.Core.Enums;
using ChessApp.Core.Game;
using ChessApp.Core.Models;
using ChessApp.Core.Pieces;
using ChessApp.Core.Services;
using ChessApp.Analysis;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChessApp.Console
{
    class Program
    {
        private static GameManager _gameManager = new GameManager();

        static async Task Main(string[] args)
        {
            System.Console.WriteLine("=== CHESS APP - SISTEMA COMPLETO CON GUARDADO Y ANÁLISIS ===");

            // CONFIGURAR el servicio de análisis - CON RUTA ESPECÍFICA
            IAnalysisService? stockfishService = null;
            try
            {
                // Ruta específica donde tienes Stockfish
                string absolutePath = @"C:\Users\GUTIERREZALE\Documents\MyApps\ChessApp\stockfish\stockfish.exe";

                System.Console.WriteLine($"🔍 Buscando Stockfish en: {absolutePath}");

                if (File.Exists(absolutePath))
                {
                    System.Console.WriteLine($"✅ Stockfish encontrado en ruta específica");
                    stockfishService = new StockfishService(absolutePath);
                }
                else
                {
                    System.Console.WriteLine($"❌ Stockfish NO encontrado en la ruta específica");
                    System.Console.WriteLine("🔍 Intentando búsqueda automática...");
                    stockfishService = new StockfishService();
                }

                _gameManager.SetAnalysisService(stockfishService);

                // INICIALIZAR ANÁLISIS
                bool analysisReady = await _gameManager.InitializeAnalysisAsync();
                if (analysisReady)
                {
                    System.Console.WriteLine("✅ Motor de análisis inicializado correctamente");
                }
                else
                {
                    System.Console.WriteLine("⚠️  Análisis no disponible - continuando sin análisis");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error con el análisis: {ex.Message}");
                System.Console.WriteLine("💡 Para usar el análisis, asegúrate de que:");
                System.Console.WriteLine("   1. Stockfish esté en: C:\\Users\\GUTIERREZALE\\Documents\\MyApps\\ChessApp\\stockfish\\stockfish.exe");
                System.Console.WriteLine("   2. El archivo sea ejecutable");
                System.Console.WriteLine("🚀 Continuando sin análisis...");
            }

            while (true)
            {
                await ShowMainMenuAsync();
            }
        }

        static async Task ShowMainMenuAsync()
        {
            System.Console.WriteLine("\n--- MENÚ PRINCIPAL ---");
            System.Console.WriteLine("1. Nueva partida");
            System.Console.WriteLine("2. Ver partidas guardadas");
            System.Console.WriteLine("3. Analizar partida guardada");
            System.Console.WriteLine("4. Salir");
            System.Console.Write("Seleccione una opción: ");

            string? input = System.Console.ReadLine();
            switch (input)
            {
                case "1":
                    PlayNewGame();
                    break;
                case "2":
                    ShowSavedGames();
                    break;
                case "3":
                    await AnalyzeSavedGameAsync();
                    break;
                case "4":
                    _gameManager.DisposeAnalysis();
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

        static async Task AnalyzeSavedGameAsync()
        {
            var savedGames = _gameManager.GetSavedGames();

            if (!savedGames.Any())
            {
                System.Console.WriteLine("\nNo hay partidas guardadas.");
                return;
            }

            System.Console.WriteLine("\n--- PARTIDAS GUARDADAS PARA ANÁLISIS ---");
            for (int i = 0; i < savedGames.Count; i++)
            {
                System.Console.WriteLine($"{i + 1}. {savedGames[i].DisplayName}");
            }

            System.Console.Write("\nSeleccione una partida para analizar (0 para volver): ");
            if (int.TryParse(System.Console.ReadLine(), out int selection) && selection > 0 && selection <= savedGames.Count)
            {
                await AnalyzeGameAsync(savedGames[selection - 1]);
            }
        }

        static async Task AnalyzeGameAsync(SavedGameInfo gameInfo)
        {
            System.Console.WriteLine($"\n--- ANALIZANDO PARTIDA: {gameInfo.DisplayName} ---");

            // Verificar si el análisis está disponible
            if (!_gameManager.IsAnalysisEnabled)
            {
                System.Console.WriteLine("❌ El análisis no está disponible");
                System.Console.WriteLine("💡 Asegúrate de que Stockfish esté instalado correctamente");
                System.Console.WriteLine("💡 Ruta esperada: C:\\Users\\GUTIERREZALE\\Documents\\MyApps\\ChessApp\\stockfish\\stockfish.exe");
                System.Console.WriteLine("\nPresione cualquier tecla para continuar...");
                System.Console.ReadKey();
                return;
            }

            try
            {
                // En lugar de cargar la partida completa, analizamos el archivo PGN directamente
                string pgnContent = File.ReadAllText(gameInfo.FilePath);
                System.Console.WriteLine("Analizando posición final de la partida...");

                // Usar posición inicial por ahora (esto debería mejorarse)
                string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

                var finalAnalysis = await _gameManager.AnalyzePositionAsync(fen);
                DisplayFinalAnalysis(finalAnalysis);

                // CORRECCIÓN: Manejar correctamente la entrada del usuario
                System.Console.WriteLine("\n¿Desea analizar movimiento por movimiento? (s/n): ");
                string? response = System.Console.ReadLine();

                if (!string.IsNullOrEmpty(response) && response.ToLower() == "s")
                {
                    System.Console.WriteLine("Analizando movimientos individuales...");
                    var game = new ChessGame(); // Juego temporal para análisis
                    var moveAnalysis = await _gameManager.AnalyzeGameMoveByMoveAsync(game);

                    DisplayMoveAnalysis(moveAnalysis);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error al analizar partida: {ex.Message}");
                System.Console.WriteLine($"💡 Detalles: {ex.InnerException?.Message}");
            }

            System.Console.WriteLine("\nPresione cualquier tecla para continuar...");
            System.Console.ReadKey();
        }

        static void DisplayFinalAnalysis(AnalysisResult analysis)
        {
            System.Console.WriteLine("\n--- ANÁLISIS DE POSICIÓN FINAL ---");
            System.Console.WriteLine($"Evaluación: {analysis.GetEvaluationText()}");

            if (!string.IsNullOrEmpty(analysis.BestMove))
            {
                System.Console.WriteLine($"Mejor movimiento: {analysis.BestMove}");
            }

            if (analysis.PrincipalVariation != null && analysis.PrincipalVariation.Any())
            {
                System.Console.WriteLine("Variación principal:");
                System.Console.Write("  ");
                for (int i = 0; i < Math.Min(analysis.PrincipalVariation.Count, 6); i++)
                {
                    System.Console.Write($"{analysis.PrincipalVariation[i]} ");
                }
                System.Console.WriteLine();
            }

            // Interpretación de la evaluación
            System.Console.WriteLine("\n--- INTERPRETACIÓN ---");
            if (analysis.IsMate)
            {
                if (analysis.MateIn > 0)
                {
                    System.Console.WriteLine("¡Las blancas tienen jaque mate!");
                }
                else
                {
                    System.Console.WriteLine("¡Las negras tienen jaque mate!");
                }
            }
            else
            {
                double absEval = Math.Abs(analysis.Evaluation);
                if (absEval < 0.3)
                {
                    System.Console.WriteLine("Posición equilibrada");
                }
                else if (absEval < 0.8)
                {
                    System.Console.WriteLine("Ligera ventaja");
                }
                else if (absEval < 1.5)
                {
                    System.Console.WriteLine("Ventaja clara");
                }
                else
                {
                    System.Console.WriteLine("Ventaja decisiva");
                }
            }
        }

        static void DisplayMoveAnalysis(List<MoveAnalysis> moveAnalysis)
        {
            if (!moveAnalysis.Any())
            {
                System.Console.WriteLine("No hay datos de análisis disponibles.");
                return;
            }

            System.Console.WriteLine("\n--- ANÁLISIS POR MOVIMIENTOS ---");
            System.Console.WriteLine("Mov | Jugada | Evaluación | Mejor Movimiento");
            System.Console.WriteLine("----|--------|------------|-----------------");

            foreach (var analysis in moveAnalysis)
            {
                string moveText = analysis.Move.AlgebraicNotation.PadRight(8);
                string evalText = analysis.GetEvaluationText().PadRight(15);
                string bestMove = analysis.BestMove ?? "-";

                System.Console.WriteLine($"{analysis.MoveNumber,3} | {moveText} | {evalText} | {bestMove}");
            }

            // Encontrar los mejores y peores movimientos
            if (moveAnalysis.Any())
            {
                var bestMove = moveAnalysis.OrderBy(m => Math.Abs(m.Evaluation)).First();
                var worstMove = moveAnalysis.OrderByDescending(m => Math.Abs(m.Evaluation)).First();

                System.Console.WriteLine($"\nMejor movimiento: {bestMove.MoveNumber}. {bestMove.Move.AlgebraicNotation}");
                System.Console.WriteLine($"Peor movimiento: {worstMove.MoveNumber}. {worstMove.Move.AlgebraicNotation}");
            }
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