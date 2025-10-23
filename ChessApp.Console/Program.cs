using ChessApp.Core.Game;
using ChessApp.Core.Models;

// Método helper para probar movimientos (debe estar definido antes de usarse)
static void TestMove(ChessGame game, string fromAlgebraic, string toAlgebraic)
{
    try
    {
        Console.WriteLine($"--- Probando: {fromAlgebraic} -> {toAlgebraic} ---");

        Position from = new Position(fromAlgebraic);
        Position to = new Position(toAlgebraic);

        Console.WriteLine($"Posición origen: Fila {from.Row}, Columna {from.Column}");
        Console.WriteLine($"Posición destino: Fila {to.Row}, Columna {to.Column}");

        // Verificar si hay pieza en origen
        var pieceAtFrom = game.Board.GetPieceAt(from);
        if (pieceAtFrom == null)
        {
            Console.WriteLine($"❌ No hay pieza en {fromAlgebraic}");
            return;
        }

        Console.WriteLine($"Pieza en origen: {pieceAtFrom.Type} {pieceAtFrom.Color}");

        MoveResult result = game.AttemptMove(from, to);

        if (result.IsSuccess)
        {
            Console.WriteLine($"✅ {result.Message}");
            Console.WriteLine($"   Movimiento: {result.Move.ToAlgebraicNotation()}");
            Console.WriteLine($"   Jugador actual: {game.CurrentPlayer}");

            if (result.Move.CapturedPiece != null)
            {
                Console.WriteLine($"   ¡Captura! Pieza capturada: {result.Move.CapturedPiece.Type}");
            }

            if (result.Move.IsCastling)
            {
                Console.WriteLine($"   ¡Enroque! ({result.Move.ToAlgebraicNotation()})");
            }
        }
        else
        {
            Console.WriteLine($"❌ {result.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine($"   StackTrace: {ex.StackTrace}");
    }
    Console.WriteLine();
}

Console.WriteLine("=== SISTEMA DE AJEDREZ COMPLETO ===");
Console.WriteLine("Probando movimientos y capturas...");
Console.WriteLine();

// Crear una nueva partida
ChessGame game = new ChessGame("Jugador1", "Jugador2");

// Mostrar estado inicial
Console.WriteLine($"Jugador actual: {game.CurrentPlayer}");
Console.WriteLine($"Estado del juego: {game.Status}");
Console.WriteLine();

// Probar movimientos más simples primero
Console.WriteLine("=== MOVIMIENTOS BÁSICOS DE PEONES ===");
TestMove(game, "e2", "e4");  // Peón blanco avanza 2 casillas
TestMove(game, "e7", "e5");  // Peón negro avanza 2 casillas

Console.WriteLine("=== MOVIMIENTOS DE CABALLOS ===");
TestMove(game, "g1", "f3");  // Caballo blanco se mueve
TestMove(game, "b8", "c6");  // Caballo negro se mueve

// Mostrar historial de movimientos
Console.WriteLine();
Console.WriteLine("=== HISTORIAL DE MOVIMIENTOS ===");
var moveHistory = game.GetFormattedMoveHistory();
foreach (string move in moveHistory)
{
    Console.WriteLine(move);
}

Console.WriteLine();
Console.WriteLine("=== PRUEBA DE ALFILES ===");

// Crear nueva partida para probar alfiles
ChessGame bishopGame = new ChessGame("Test Blanco", "Test Negro");

// Preparar tablero moviendo peones para liberar a los alfiles
TestMove(bishopGame, "d2", "d4");
TestMove(bishopGame, "d7", "d5");

// Probar movimiento de alfil blanco
Console.WriteLine("--- Movimiento de alfil blanco ---");
TestMove(bishopGame, "c1", "f4"); // Alfil blanco de c1 a f4

// Probar movimiento de alfil negro  
Console.WriteLine("--- Movimiento de alfil negro ---");
TestMove(bishopGame, "c8", "f5"); // Alfil negro de c8 a f5

Console.WriteLine();
Console.WriteLine("=== PRUEBA DE ENROQUE SIMPLIFICADA ===");

// Crear nueva partida para enroque
ChessGame castlingGame = new ChessGame("Enroque Blanco", "Enroque Negro");

// Movimientos mínimos para enroque
Console.WriteLine("Preparando para enroque...");
TestMove(castlingGame, "e2", "e4");
TestMove(castlingGame, "e7", "e5");
TestMove(castlingGame, "g1", "f3");
TestMove(castlingGame, "b8", "c6");
TestMove(castlingGame, "f1", "e2"); // Alfil se mueve para liberar enroque
TestMove(castlingGame, "f8", "e7"); // Alfil negro se mueve

// Intentar enroque
Console.WriteLine("--- Intentando enroque ---");
TestMove(castlingGame, "e1", "g1"); // Enroque corto

Console.WriteLine();
Console.WriteLine("Prueba completada. Presiona cualquier tecla para salir.");
Console.ReadKey();


//using ChessApp.Core.Game;
//using ChessApp.Core.Models;

//Console.WriteLine("=== SISTEMA DE AJEDREZ COMPLETO ===");
//Console.WriteLine("Probando movimientos y capturas...");
//Console.WriteLine();

//// Crear una nueva partida
//ChessGame game = new ChessGame("Jugador1", "Jugador2");

//// Mostrar estado inicial
//Console.WriteLine($"Jugador actual: {game.CurrentPlayer}");
//Console.WriteLine($"Estado del juego: {game.Status}");
//Console.WriteLine();

//// Probar movimientos más simples primero
//Console.WriteLine("=== MOVIMIENTOS BÁSICOS DE PEONES ===");
//TestMove(game, "e2", "e4");  // Peón blanco avanza 2 casillas
//TestMove(game, "e7", "e5");  // Peón negro avanza 2 casillas

//Console.WriteLine("=== MOVIMIENTOS DE CABALLOS ===");
//TestMove(game, "g1", "f3");  // Caballo blanco se mueve
//TestMove(game, "b8", "c6");  // Caballo negro se mueve

//// Mostrar historial de movimientos
//Console.WriteLine();
//Console.WriteLine("=== HISTORIAL DE MOVIMIENTOS ===");
//var moveHistory = game.GetFormattedMoveHistory();
//foreach (string move in moveHistory)
//{
//    Console.WriteLine(move);
//}

//Console.WriteLine();
//Console.WriteLine("=== PRUEBA SIMPLIFICADA DE ENROQUE ===");

//// Crear nueva partida para probar enroque
//ChessGame castlingGame = new ChessGame("Test Blanco", "Test Negro");

//// Solo movimientos esenciales para enroque
//Console.WriteLine("Preparando tablero para enroque...");
//TestMove(castlingGame, "e2", "e4");
//TestMove(castlingGame, "e7", "e5");
//TestMove(castlingGame, "g1", "f3");
//TestMove(castlingGame, "b8", "c6");
//TestMove(castlingGame, "f1", "c4");
//TestMove(castlingGame, "f8", "c5");

//// Intentar enroque corto (kingside)
//Console.WriteLine("--- Intentando enroque corto blanco ---");
//TestMove(castlingGame, "e1", "g1"); // Enroque corto blanco

//// Mostrar resultado
//Console.WriteLine($"Estado del juego: {castlingGame.Status}");
//if (castlingGame.MoveHistory.Count > 0)
//{
//    var lastMove = castlingGame.MoveHistory.Last();
//    Console.WriteLine($"¿Enroque exitoso? {lastMove.IsCastling}");
//    Console.WriteLine($"Último movimiento: {lastMove.ToAlgebraicNotation()}");
//}

//// Mostrar historial final
//Console.WriteLine("=== HISTORIAL FINAL ===");
//var finalHistory = castlingGame.GetFormattedMoveHistory();
//foreach (string move in finalHistory)
//{
//    Console.WriteLine(move);
//}

//Console.WriteLine();
//Console.WriteLine("Prueba completada. Presiona cualquier tecla para salir.");
//Console.ReadKey();

//// Método helper para probar movimientos
//static void TestMove(ChessGame game, string fromAlgebraic, string toAlgebraic)
//{
//    try
//    {
//        Console.WriteLine($"--- Probando: {fromAlgebraic} -> {toAlgebraic} ---");

//        Position from = new Position(fromAlgebraic);
//        Position to = new Position(toAlgebraic);

//        Console.WriteLine($"Posición origen: Fila {from.Row}, Columna {from.Column}");
//        Console.WriteLine($"Posición destino: Fila {to.Row}, Columna {to.Column}");

//        // Verificar si hay pieza en origen
//        var pieceAtFrom = game.Board.GetPieceAt(from);
//        if (pieceAtFrom == null)
//        {
//            Console.WriteLine($"❌ No hay pieza en {fromAlgebraic}");
//            return;
//        }

//        Console.WriteLine($"Pieza en origen: {pieceAtFrom.Type} {pieceAtFrom.Color}");

//        MoveResult result = game.AttemptMove(from, to);

//        if (result.IsSuccess)
//        {
//            Console.WriteLine($"✅ {result.Message}");
//            Console.WriteLine($"   Movimiento: {result.Move.ToAlgebraicNotation()}");
//            Console.WriteLine($"   Jugador actual: {game.CurrentPlayer}");

//            if (result.Move.CapturedPiece != null)
//            {
//                Console.WriteLine($"   ¡Captura! Pieza capturada: {result.Move.CapturedPiece.Type}");
//            }

//            if (result.Move.IsCastling)
//            {
//                Console.WriteLine($"   ¡Enroque! ({result.Move.ToAlgebraicNotation()})");
//            }
//        }
//        else
//        {
//            Console.WriteLine($"❌ {result.Message}");
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"❌ Error: {ex.Message}");
//        Console.WriteLine($"   StackTrace: {ex.StackTrace}");
//    }
//    Console.WriteLine();
//}


/* ---------------------------------------------------------------
using ChessApp.Core.Models;

Console.WriteLine("=== PRUEBA DE POSICIONES DE AJEDREZ ===");
Console.WriteLine("Sistema de coordenadas: filas 1-8, columnas 1-8 (1=a, 2=b, ..., 8=h)");
Console.WriteLine();

// Probar diferentes formas de crear posiciones
try
{
    // Crear posición desde coordenadas numéricas
    Position pos1 = new Position(1, 1);  // a1 (fila 1, columna 1)
    Position pos2 = new Position(2, 5);  // e2 (fila 2, columna 5)
    Position pos3 = new Position(8, 8);  // h8 (fila 8, columna 8)

    Console.WriteLine("Posiciones desde coordenadas numéricas:");
    Console.WriteLine($"Fila 1, Columna 1 = {pos1.AlgebraicNotation}");
    Console.WriteLine($"Fila 2, Columna 5 = {pos2.AlgebraicNotation}");
    Console.WriteLine($"Fila 8, Columna 8 = {pos3.AlgebraicNotation}");
    Console.WriteLine();

    // Crear posición desde notación algebraica
    Position pos4 = new Position("a1");
    Position pos5 = new Position("e4");
    Position pos6 = new Position("h8");

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
        Position invalid1 = new Position(0, 1);  // Fila 0 no existe
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✅ Correcto: {ex.Message}");
    }

    try
    {
        Position invalid2 = new Position("i9");  // Columna i no existe
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✅ Correcto: {ex.Message}");
    }

    try
    {
        Position invalid3 = new Position(5, 9);  // Columna 9 no existe
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
*/