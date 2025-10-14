namespace ChessApp.Core.Models
{
    // Esta clase representa una posición en el tablero de ajedrez
    public class ChessPosition
    {
        public int Row { get; set; } // Filas de 1 - 8
        public int Column { get; set; } // Columnas de 0 - 7
        public string AlgebraicNotation { get; set; } // Notación para registro de movimientos

        // Constructor principal
        public ChessPosition(int row, int column)
        {
            // Validar posición dentro del tablero
            if (row < 1 || row > 8 || column < 0 || column > 7)
                throw new ArgumentException($"Posición invalida: fila {row}, columna {column}");

            Row = row;
            Column = column;
            AlgebraicNotation = ConvertToAlgebraic(row, column);
        }

        // Constructor desde notación algebraica
        public ChessPosition(string algebraicNotation)
        {
            if (string.IsNullOrEmpty(algebraicNotation) || algebraicNotation.Length != 2)
                throw new ArgumentException("Notación algebraica invalida. Use formato como 'a1'");

            AlgebraicNotation = algebraicNotation.ToLower();
            Column = AlgebraicNotation[0] - 'a';
            Row = AlgebraicNotation[1] - '0';

            // Validar que esté dentro del tablero
            if (Row < 1 || Row > 8 || Column < 0 || Column > 7)
                throw new ArgumentException($"Posición fuera del tablero {algebraicNotation}");
        }

        // Método privado para convertir coordenadas numéricas a notación algebraica
        private string ConvertToAlgebraic(int row, int column)
        {
            char columnChar = (char)('a' + column);
            return $"{columnChar}{row}";
        }

        // Método para mostrar la posición en notación algebraica
        public override string ToString()
        {
            return AlgebraicNotation;
        }

        // Método para verificar si dos posiciones son iguales
        public override bool Equals(object obj)
        {
            if (obj is ChessPosition other)
                return Row == other.Row && Column == other.Column;

            return false;
        }

        // Método para obtener el código hash (Es necesario para sobreescribir Equals)
        public override int GetHashCode()
        {
            return (Row * 10) + Column;
        }
    }
}

