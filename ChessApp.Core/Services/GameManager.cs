using ChessApp.Core.Game;
using ChessApp.Core.Models;
using ChessApp.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChessApp.Core.Services
{
    public class GameManager
    {
        private readonly PgnService _pgnService;
        private readonly string _gamesDirectory;

        public GameManager()
        {
            _pgnService = new PgnService();
            _gamesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SavedGames");

            // Crear directorio si no existe
            if (!Directory.Exists(_gamesDirectory))
            {
                Directory.CreateDirectory(_gamesDirectory);
            }
        }

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
                    // Si hay error leyendo el archivo, continuar con el siguiente
                    Console.WriteLine($"Error reading PGN file {filePath}: {ex.Message}");
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
                foreach (var line in lines.Take(10)) // Solo leer primeras líneas para metadata
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
                        break; // Fin de headers
                    }
                }
            }
            catch
            {
                // Si hay error, usar valores por defecto
            }

            return metadata;
        }

        public ChessGame LoadGame(string filePath)
        {
            return _pgnService.LoadGameFromFile(filePath);
        }

        public void DeleteGame(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
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