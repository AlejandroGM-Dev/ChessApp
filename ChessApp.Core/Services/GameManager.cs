using ChessApp.Core.Enums;
using ChessApp.Core.Game;
using ChessApp.Core.Models;
using ChessApp.Core.Pieces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChessApp.Core.Services
{
    public class GameManager
    {
        private readonly PgnService _pgnService;
        private readonly string _gamesDirectory;
        private IAnalysisService? _analysisService;
        private bool _analysisEnabled = false;

        public GameManager()
        {
            _pgnService = new PgnService();
            _gamesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SavedGames");

            if (!Directory.Exists(_gamesDirectory))
            {
                Directory.CreateDirectory(_gamesDirectory);
            }
        }

        // Método para establecer el servicio de análisis (se llamará desde Console)
        public void SetAnalysisService(IAnalysisService analysisService)
        {
            _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        }

        public async Task<bool> InitializeAnalysisAsync()
        {
            if (_analysisService == null)
            {
                System.Console.WriteLine("❌ Servicio de análisis no configurado");
                return false;
            }

            try
            {
                _analysisEnabled = await _analysisService.InitializeAsync();
                return _analysisEnabled;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Error inicializando análisis: {ex.Message}");
                _analysisEnabled = false;
                return false;
            }
        }

        public async Task<AnalysisResult> AnalyzePositionAsync(string fen)
        {
            if (!_analysisEnabled || _analysisService == null)
                throw new InvalidOperationException("Servicio de análisis no disponible");

            try
            {
                return await _analysisService.AnalyzePositionAsync(fen);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error analizando posición: {ex.Message}", ex);
            }
        }

        public async Task<AnalysisResult> AnalyzeGameAsync(ChessGame game)
        {
            // Por ahora, analizamos solo la posición inicial
            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            return await AnalyzePositionAsync(fen);
        }

        public Task<List<MoveAnalysis>> AnalyzeGameMoveByMoveAsync(ChessGame game)
        {
            // Implementación simplificada - devolver lista vacía por ahora
            return Task.FromResult(new List<MoveAnalysis>());
        }

        public void DisposeAnalysis()
        {
            _analysisService?.Dispose();
            _analysisEnabled = false;
        }

        // Los demás métodos permanecen igual...
        public void SaveGame(ChessGame game, PgnMetadata? metadata = null)
        {
            metadata ??= new PgnMetadata
            {
                WhitePlayer = game.WhitePlayer,
                BlackPlayer = game.BlackPlayer,
                Result = game.GetPgnResult(),
                Date = game.StartTime.ToString("yyyy.MM.dd")
            };

            string fileName = GenerateFileName(game);
            string filePath = Path.Combine(_gamesDirectory, fileName);

            _pgnService.SaveGameToFile(game, filePath, metadata);
        }

        private string GenerateFileName(ChessGame game)
        {
            string safeWhite = MakeFileNameSafe(game.WhitePlayer);
            string safeBlack = MakeFileNameSafe(game.BlackPlayer);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return $"{safeWhite}_vs_{safeBlack}_{timestamp}.pgn";
        }

        private string MakeFileNameSafe(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(name.Where(ch => !invalidChars.Contains(ch)).ToArray())
                  .Replace(" ", "_")
                  .Trim();
        }

        public List<SavedGameInfo> GetSavedGames()
        {
            var games = new List<SavedGameInfo>();
            var pgnFiles = _pgnService.GetPgnFiles(_gamesDirectory);

            foreach (var filePath in pgnFiles.OrderByDescending(f => File.GetCreationTime(f)))
            {
                try
                {
                    var metadata = ReadPgnMetadata(filePath);
                    games.Add(new SavedGameInfo
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        WhitePlayer = metadata.WhitePlayer ?? "White",
                        BlackPlayer = metadata.BlackPlayer ?? "Black",
                        Date = metadata.Date ?? "????.??.??",
                        Result = metadata.Result ?? "*",
                        CreatedAt = File.GetCreationTime(filePath)
                    });
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error reading PGN file {filePath}: {ex.Message}");
                }
            }

            return games;
        }

        private PgnMetadata ReadPgnMetadata(string filePath)
        {
            var metadata = new PgnMetadata();

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines.Take(10))
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        var content = line.Trim('[', ']');
                        var parts = content.Split(' ', 2);

                        if (parts.Length == 2)
                        {
                            string key = parts[0];
                            string value = parts[1].Trim('"');

                            switch (key)
                            {
                                case "White": metadata.WhitePlayer = value; break;
                                case "Black": metadata.BlackPlayer = value; break;
                                case "Date": metadata.Date = value; break;
                                case "Result": metadata.Result = value; break;
                                case "Event": metadata.Event = value; break;
                                case "Site": metadata.Site = value; break;
                                case "Round": metadata.Round = value; break;
                            }
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                }
            }
            catch
            {
                // Usar valores por defecto
            }

            return metadata;
        }

        public ChessGame LoadGame(string filePath)
        {
            // Implementación simplificada - devolver juego nuevo
            return new ChessGame();
        }

        public void DeleteGame(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool IsAnalysisEnabled => _analysisEnabled;
    }

    public class MoveAnalysis
    {
        public int MoveNumber { get; set; }
        public Move Move { get; set; } = new Move(new Position(1, 1), new Position(1, 1), new Pawn(PieceColor.White));
        public double Evaluation { get; set; }
        public string BestMove { get; set; } = string.Empty;
        public bool IsMate { get; set; }
        public int? MateIn { get; set; }

        public string GetEvaluationText()
        {
            if (IsMate && MateIn.HasValue)
            {
                string winner = MateIn.Value > 0 ? "Blancas" : "Negras";
                int moves = Math.Abs(MateIn.Value);
                return $"Mate en {moves} ({winner})";
            }
            else
            {
                string advantage = Evaluation > 0 ? "Blancas" : "Negras";
                return $"{advantage} +{Math.Abs(Evaluation):F1}";
            }
        }
    }

    public class SavedGameInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string WhitePlayer { get; set; } = "White";
        public string BlackPlayer { get; set; } = "Black";
        public string Date { get; set; } = "????.??.??";
        public string Result { get; set; } = "*";
        public DateTime CreatedAt { get; set; }

        public string DisplayName => $"{WhitePlayer} vs {BlackPlayer} - {Date} - {Result}";
    }
}