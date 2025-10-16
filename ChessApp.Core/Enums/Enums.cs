using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessApp.Core.Enums
{
    // Enumeracion para los colores de las piezas
    public enum PieceColor
    {
        White,  // Blanco 
        Black   // Negro
    }

    // Enumeracion para los tipos de piezas
    public enum PieceType
    {
        Pawn,   // Peon
        Rook,   // Torre
        Knight, // Caballo
        Bishop, // Alfil
        Queen,  // Reina
        King    // Rey
    }

    // Enumeracion para el estado del juego
    public enum GameStatus
    {
        InProgress,     // En progreso
        WhiteCheck,     // Blanco en jaque
        BlackCheck,     // Negro en jaque
        WhiteCheckMate, // Blanco en jaque mate
        BlackCheckMate, // Negro en jaque mate
        Stalmate,       // Tablas por ahogado
        Draw            // Tablas por otras razones
    }
}
