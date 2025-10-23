using ChessApp.Core.Game;
using ChessApp.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChessApp.Core.Services
{
    public class PgnService
    {
        public string ExportToPgn(ChessGame game, PgnMetadata metadata)
        {
            var pgn = new StringBuilder();

            // Encabezados PGN estándar
            AddPgnHeader(pgn, "Event", metadata.Event ?? "?");
            AddPgnHeader(pgn, "Site", metadata.Site ?? "?");
            AddPgnHeader(pgn, "Date", metadata.Date ?? DateTime.Now.ToString("yyyy.MM.dd"));
            AddPgnHeader(pgn, "Round", metadata.Round ?? "?");
            AddPgnHeader(pgn, "White", metadata.WhitePlayer ?? game.WhitePlayer);
            AddPgnHeader(pgn, "Black", metadata.BlackPlayer ?? game.BlackPlayer);
            AddPgnHeader(pgn, "Result", metadata.Result ?? game.GetPgnResult());

            // Encabezados adicionales
            AddPgnHeader(pgn, "WhiteElo", metadata.WhiteElo ?? "?");
            AddPgnHeader(pgn, "BlackElo", metadata.BlackElo ?? "?");
            AddPgnHeader(pgn, "ECO", metadata.EcoCode ?? "?");
            AddPgnHeader(pgn, "Opening", metadata.Opening ?? "?");

            pgn.AppendLine();

            // Movimientos
            var moves = game.GetFormattedMoveHistory();
            foreach (var move in moves)
            {
                pgn.Append(move);
                pgn.Append(" ");
            }

            // Resultado al final
            pgn.Append(metadata.Result ?? game.GetPgnResult());

            return pgn.ToString();
        }

        private void AddPgnHeader(StringBuilder pgn, string key, string value)
        {
            pgn.AppendLine($"[{key} \"{value}\"]");
        }

        public ChessGame ImportFromPgn(string pgnString)
        {
            // Implementación básica - en una versión completa se parsearía todo el PGN
            var game = new ChessGame();

            // Por ahora, simplemente devolvemos un juego nuevo
            // En una implementación completa, aquí se cargarían los movimientos desde el PGN
            return game;
        }

        public void SaveGameToFile(ChessGame game, string filePath, PgnMetadata? metadata = null)
        {
            metadata ??= new PgnMetadata();
            string pgn = ExportToPgn(game, metadata);
            File.WriteAllText(filePath, pgn);
        }

        public ChessGame LoadGameFromFile(string filePath)
        {
            string pgnString = File.ReadAllText(filePath);
            return ImportFromPgn(pgnString);
        }

        public List<string> GetPgnFiles(string directoryPath)
        {
            var pgnFiles = new List<string>();

            if (Directory.Exists(directoryPath))
            {
                pgnFiles.AddRange(Directory.GetFiles(directoryPath, "*.pgn"));
            }

            return pgnFiles;
        }
    }

    public class PgnMetadata
    {
        public string? Event { get; set; } = "Casual Game";
        public string? Site { get; set; } = "ChessApp";
        public string? Date { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public string? Round { get; set; } = "?";
        public string? WhitePlayer { get; set; } = "White";
        public string? BlackPlayer { get; set; } = "Black";
        public string? Result { get; set; } = "*";
        public string? WhiteElo { get; set; } = "?";
        public string? BlackElo { get; set; } = "?";
        public string? EcoCode { get; set; } = "?";
        public string? Opening { get; set; } = "?";
    }
}