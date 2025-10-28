using ChessApp.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ChessApp.Analysis
{
    public class StockfishService : IAnalysisService
    {
        private Process? _stockfishProcess;  // Hacer nullable
        private readonly string _stockfishPath;
        private bool _isInitialized = false;

        public StockfishService(string? stockfishPath = null)  // Hacer parámetro nullable
        {
            _stockfishPath = stockfishPath ?? FindStockfish() ?? throw new FileNotFoundException("No se pudo encontrar Stockfish");
        }

        private string? FindStockfish()
        {
            // Buscar Stockfish en ubicaciones comunes
            var possiblePaths = new[]
            {
                "stockfish/stockfish.exe",    // Windows en carpeta del proyecto
                "stockfish/stockfish",        // Linux/Mac en carpeta del proyecto
                "stockfish.exe",              // Windows en directorio actual
                "stockfish",                  // Linux/Mac en directorio actual
                "/usr/bin/stockfish",         // Linux común
                "/usr/local/bin/stockfish",   // Linux común
                "/opt/homebrew/bin/stockfish" // Mac con Homebrew
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"✅ Stockfish encontrado en: {path}");
                    return path;
                }
            }

            return null;
        }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;

            try
            {
                if (!File.Exists(_stockfishPath))
                {
                    Console.WriteLine($"❌ Stockfish no encontrado en: {_stockfishPath}");
                    return false;
                }

                _stockfishProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _stockfishPath,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _stockfishProcess.Start();

                // Inicializar Stockfish
                await SendCommand("uci");
                await WaitForResponse("uciok");

                // Configurar parámetros por defecto
                await SendCommand("setoption name Threads value 2");
                await SendCommand("setoption name Hash value 128");

                _isInitialized = true;
                Console.WriteLine("✅ Stockfish inicializado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inicializando Stockfish: {ex.Message}");
                return false;
            }
        }

        // Implementación de la interfaz SIN depth
        public async Task<AnalysisResult> AnalyzePositionAsync(string fen)
        {
            return await AnalyzePositionAsync(fen, 15); // Depth por defecto
        }

        public async Task<AnalysisResult> AnalyzeGameAsync(List<string> moves)
        {
            return await AnalyzeGameAsync(moves, 15); // Depth por defecto
        }

        // Implementación CON depth
        public async Task<AnalysisResult> AnalyzePositionAsync(string fen, int depth = 15)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Stockfish no está inicializado");

            await SendCommand($"position fen {fen}");
            await SendCommand($"go depth {depth}");

            var analysis = new AnalysisResult();
            string? line;

            while ((line = await ReadOutputLineAsync()) != null)
            {
                if (line.StartsWith("info"))
                {
                    ParseInfoLine(line, analysis);
                }
                else if (line.StartsWith("bestmove"))
                {
                    ParseBestMove(line, analysis);
                    break;
                }
            }

            return analysis;
        }

        public async Task<AnalysisResult> AnalyzeGameAsync(List<string> moves, int depth = 15)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Stockfish no está inicializado");

            // Construir comando de posición con movimientos
            string movesString = string.Join(" ", moves);
            await SendCommand($"position startpos moves {movesString}");
            await SendCommand($"go depth {depth}");

            var analysis = new AnalysisResult();
            string? line;

            while ((line = await ReadOutputLineAsync()) != null)
            {
                if (line.StartsWith("info"))
                {
                    ParseInfoLine(line, analysis);
                }
                else if (line.StartsWith("bestmove"))
                {
                    ParseBestMove(line, analysis);
                    break;
                }
            }

            return analysis;
        }

        private void ParseInfoLine(string line, AnalysisResult analysis)
        {
            var parts = line.Split(' ');

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "cp" && i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i + 1], out int cp))
                    {
                        analysis.CentipawnEval = cp;
                        analysis.Evaluation = cp / 100.0;
                    }
                }
                else if (parts[i] == "mate" && i + 1 < parts.Length)
                {
                    if (int.TryParse(parts[i + 1], out int mateIn))
                    {
                        analysis.MateIn = mateIn;
                        analysis.IsMate = true;
                    }
                }
                else if (parts[i] == "pv" && i + 1 < parts.Length)
                {
                    analysis.PrincipalVariation = new List<string>();
                    for (int j = i + 1; j < parts.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(parts[j]))
                            analysis.PrincipalVariation.Add(parts[j]);
                    }
                    break;
                }
            }
        }

        private void ParseBestMove(string line, AnalysisResult analysis)
        {
            var parts = line.Split(' ');
            if (parts.Length > 1)
            {
                analysis.BestMove = parts[1];
            }
        }

        private async Task SendCommand(string command)
        {
            if (_stockfishProcess?.StandardInput != null)
            {
                await _stockfishProcess.StandardInput.WriteLineAsync(command);
                await _stockfishProcess.StandardInput.FlushAsync();
            }
        }

        private async Task<string?> ReadOutputLineAsync()
        {
            if (_stockfishProcess?.StandardOutput != null)
            {
                return await _stockfishProcess.StandardOutput.ReadLineAsync();
            }
            return null;
        }

        private async Task WaitForResponse(string expectedResponse)
        {
            string? line;
            while ((line = await ReadOutputLineAsync()) != null)
            {
                if (line.Contains(expectedResponse))
                    break;
            }
        }

        public void Dispose()
        {
            if (_stockfishProcess != null)
            {
                if (!_stockfishProcess.HasExited)
                {
                    try
                    {
                        SendCommand("quit").Wait(1000);
                        if (!_stockfishProcess.HasExited)
                            _stockfishProcess.Kill();
                    }
                    catch
                    {
                        // Ignorar errores al cerrar
                    }
                }
                _stockfishProcess.Dispose();
                _stockfishProcess = null;
            }
            _isInitialized = false;
        }
    }
}