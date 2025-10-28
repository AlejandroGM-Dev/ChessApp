using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Services
{
    public interface IAnalysisService : IDisposable
    {
        Task<bool> InitializeAsync();
        Task<AnalysisResult> AnalyzePositionAsync(string fen);
        Task<AnalysisResult> AnalyzeGameAsync(List<string> moves);

        // Métodos opcionales con depth - agregar estos
        Task<AnalysisResult> AnalyzePositionAsync(string fen, int depth);
        Task<AnalysisResult> AnalyzeGameAsync(List<string> moves, int depth);
    }

    public class AnalysisResult
    {
        public double Evaluation { get; set; }
        public int CentipawnEval { get; set; }
        public bool IsMate { get; set; } = false;
        public int? MateIn { get; set; }
        public string BestMove { get; set; } = string.Empty;
        public List<string> PrincipalVariation { get; set; } = new List<string>();

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

        public string GetFormattedEvaluation()
        {
            if (IsMate && MateIn.HasValue)
            {
                return MateIn.Value > 0 ? $"+M{Math.Abs(MateIn.Value)}" : $"-M{Math.Abs(MateIn.Value)}";
            }
            else
            {
                return Evaluation > 0 ? $"+{Evaluation:F2}" : $"{Evaluation:F2}";
            }
        }
    }
}
