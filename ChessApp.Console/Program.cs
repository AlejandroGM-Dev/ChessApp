using ChessApp.Core.Models;

Console.WriteLine("=== PRUEBA DE POSICIONES DE AJEDREZ ===");
Console.WriteLine("Sistema de coordenadas: filas 1-8, columnas 1-8 (1=a, 2=b, ..., 8=h)");
Console.WriteLine();

// Probar diferentes formas de crear posiciones
try
{
    // Crear posición desde coordenadas numéricas
    ChessPosition pos1 = new ChessPosition(1, 1);  // a1 (fila 1, columna 1)
    ChessPosition pos2 = new ChessPosition(2, 5);  // e2 (fila 2, columna 5)
    ChessPosition pos3 = new ChessPosition(8, 8);  // h8 (fila 8, columna 8)

    Console.WriteLine("Posiciones desde coordenadas numéricas:");
    Console.WriteLine($"Fila 1, Columna 1 = {pos1.AlgebraicNotation}");
    Console.WriteLine($"Fila 2, Columna 5 = {pos2.AlgebraicNotation}");
    Console.WriteLine($"Fila 8, Columna 8 = {pos3.AlgebraicNotation}");
    Console.WriteLine();

    // Crear posición desde notación algebraica
    ChessPosition pos4 = new ChessPosition("a1");
    ChessPosition pos5 = new ChessPosition("e4");
    ChessPosition pos6 = new ChessPosition("h8");

    Console.WriteLine("Posiciones desde notación algebraica:");
    Console.WriteLine($"a1 -> Fila {pos4.Row}, Columna {pos4.Column}");
    Console.WriteLine($"e4 -> Fila {pos5.Row}, Columna {pos5.Column}");
    Console.WriteLine($"h8 -> Fila {pos6.Row}, Columna {pos6.Column}");
    Console.WriteLine();

    // Probar método ToString
    Console.WriteLine("Usando ToString():");
    Console.WriteLine($"Posición 1: {pos1}");
    Console.WriteLine($"Posición 2: {pos2}");
    Console.WriteLine($"Posición 3: {pos3}");
    Console.WriteLine();

    // Probar posiciones inválidas
    Console.WriteLine("Probando posiciones inválidas...");
    try
    {
        ChessPosition invalid1 = new ChessPosition(0, 1);  // Fila 0 no existe
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✅ Correcto: {ex.Message}");
    }

    try
    {
        ChessPosition invalid2 = new ChessPosition("i9");  // Columna i no existe
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✅ Correcto: {ex.Message}");
    }

    try
    {
        ChessPosition invalid3 = new ChessPosition(5, 9);  // Columna 9 no existe
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✅ Correcto: {ex.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error inesperado: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("Prueba completada. Presiona cualquier tecla para salir.");
Console.ReadKey();
