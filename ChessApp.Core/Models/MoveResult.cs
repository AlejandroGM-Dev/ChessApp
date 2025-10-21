using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Models
{
    public class MoveResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Move Move { get; set; }

        public MoveResult (bool isSuccess, string message, Move move = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Move = move;
        }

        public static MoveResult Success(string message, Move move)
        {
            return new MoveResult(true, message, move);
        }

        public static MoveResult Failure(string message)
        {
            return new MoveResult(false, message);
        }
    }
}
