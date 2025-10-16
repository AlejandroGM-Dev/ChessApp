using System;

namespace ChessApp.Core.Models;

// Esta clase representa una posición en el tablero de ajedrez
public class Position
{
    public int Row { get; set; }        // Fila 1-8 (1 = abajo, 8 = arriba)
    public int Column { get; set; }     // Columna 1-8 (1 = a, 2 = b, ..., 8 = h)
    public string AlgebraicNotation { get; set; }  // Notación como "a1", "e4"

    // Constructor principal
    public Position(int row, int column)
    {
        // Validar que esté dentro del tablero
        if (row < 1 || row > 8 || column < 1 || column > 8)
            throw new ArgumentException($"Posición inválida: fila {row}, columna {column}");

        Row = row;
        Column = column;
        AlgebraicNotation = ConvertToAlgebraic(row, column);
    }

    // Constructor desde notación algebraica como "a1", "e4"
    public Position(string algebraicNotation)
    {
        if (string.IsNullOrEmpty(algebraicNotation) || algebraicNotation.Length != 2)
            throw new ArgumentException("Notación algebraica inválida. Use formato como 'a1', 'e4'");

        AlgebraicNotation = algebraicNotation.ToLower();
        Column = AlgebraicNotation[0] - 'a' + 1;  // 'a' -> 1, 'b' -> 2, etc.
        Row = AlgebraicNotation[1] - '0';         // '1' -> 1, '2' -> 2, etc.

        // Validar que esté dentro del tablero
        if (Row < 1 || Row > 8 || Column < 1 || Column > 8)
            throw new ArgumentException($"Posición fuera del tablero: {algebraicNotation}");
    }

    // Método privado para convertir coordenadas numéricas a notación algebraica
    private string ConvertToAlgebraic(int row, int column)
    {
        char columnChar = (char)('a' + (column - 1));  // 1 -> 'a', 2 -> 'b', etc.
        return $"{columnChar}{row}";
    }

    // Método para mostrar la posición en notación algebraica
    public override string ToString()
    {
        return AlgebraicNotation;
    }
}
