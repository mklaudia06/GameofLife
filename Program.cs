const int FrameDelayMilliseconds = 200;

var rows = ReadPositiveInteger("Indica la cantidad de filas del tablero: ");
var columns = ReadPositiveInteger("Indica la cantidad de columnas del tablero: ");
var initialBoard = ReadInitialBoard(rows, columns);

RunSimulation(initialBoard);

static int ReadPositiveInteger(string prompt)
{
    while (true)
    {
        Console.Write(prompt);

        if (int.TryParse(Console.ReadLine(), out var value) && value > 0)
        {
            return value;
        }

        Console.WriteLine("Escribe un número entero mayor que cero.");
    }
}

static bool[,] ReadInitialBoard(int rows, int columns)
{
    while (true)
    {
        Console.WriteLine(
            $"Escribe las células vivas como fila,columna;fila,columna. " +
            $"Filas: 0 a {rows - 1}; columnas: 0 a {columns - 1}. " +
            "Deja la entrada vacía para empezar sin células vivas.");

        var input = Console.ReadLine() ?? string.Empty;
        if (!TryParseCoordinates(input, rows, columns, out var liveCells, out var error))
        {
            Console.WriteLine(error);
            continue;
        }

        var board = new bool[rows, columns];
        foreach (var (row, column) in liveCells)
        {
            board[row, column] = true;
        }

        return board;
    }
}

static bool TryParseCoordinates(
    string input,
    int rows,
    int columns,
    out HashSet<(int Row, int Column)> liveCells,
    out string error)
{
    liveCells = [];
    error = string.Empty;

    if (string.IsNullOrWhiteSpace(input))
    {
        return true;
    }

    foreach (var coordinate in input.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        var values = coordinate.Split(',', StringSplitOptions.TrimEntries);
        if (values.Length != 2 ||
            !int.TryParse(values[0], out var row) ||
            !int.TryParse(values[1], out var column))
        {
            error = $"La coordenada '{coordinate}' no tiene el formato fila,columna.";
            return false;
        }

        if (row < 0 || row >= rows || column < 0 || column >= columns)
        {
            error = $"La coordenada {row},{column} está fuera del tablero.";
            return false;
        }

        liveCells.Add((row, column));
    }

    return true;
}

static void RunSimulation(bool[,] initialBoard)
{
    var currentBoard = initialBoard;
    var generation = 0;
    var previousStates = new HashSet<string> { GetBoardKey(currentBoard) };

    DrawBoard(currentBoard, generation);

    if (!HasLiveCells(currentBoard))
    {
        Console.WriteLine("No hay células vivas. Fin de la simulación.");
        return;
    }

    while (true)
    {
        Thread.Sleep(FrameDelayMilliseconds);

        var nextBoard = CalculateNextGeneration(currentBoard);
        generation++;
        DrawBoard(nextBoard, generation);

        if (!HasLiveCells(nextBoard))
        {
            Console.WriteLine("Todas las células murieron. Fin de la simulación.");
            return;
        }

        if (!previousStates.Add(GetBoardKey(nextBoard)))
        {
            Console.WriteLine("La simulación alcanzó un estado repetido. Fin de la simulación.");
            return;
        }

        currentBoard = nextBoard;
    }
}

static bool[,] CalculateNextGeneration(bool[,] board)
{
    var rows = board.GetLength(0);
    var columns = board.GetLength(1);
    var nextBoard = new bool[rows, columns];

    for (var row = 0; row < rows; row++)
    {
        for (var column = 0; column < columns; column++)
        {
            var liveNeighbors = CountLiveNeighbors(board, row, column);
            nextBoard[row, column] = board[row, column]
                ? liveNeighbors is 2 or 3
                : liveNeighbors == 3;
        }
    }

    return nextBoard;
}

static int CountLiveNeighbors(bool[,] board, int row, int column)
{
    var rows = board.GetLength(0);
    var columns = board.GetLength(1);
    var count = 0;

    for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
    {
        for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
        {
            if (rowOffset == 0 && columnOffset == 0)
            {
                continue;
            }

            var neighborRow = row + rowOffset;
            var neighborColumn = column + columnOffset;

            if (neighborRow >= 0 && neighborRow < rows &&
                neighborColumn >= 0 && neighborColumn < columns &&
                board[neighborRow, neighborColumn])
            {
                count++;
            }
        }
    }

    return count;
}

static bool HasLiveCells(bool[,] board)
{
    foreach (var cell in board)
    {
        if (cell)
        {
            return true;
        }
    }

    return false;
}

static string GetBoardKey(bool[,] board)
{
    var cells = new char[board.Length];
    var index = 0;

    foreach (var cell in board)
    {
        cells[index++] = cell ? '1' : '0';
    }

    return new string(cells);
}

static void DrawBoard(bool[,] board, int generation)
{
    Console.WriteLine($"SIMULACIÓN: {generation}");

    for (var row = 0; row < board.GetLength(0); row++)
    {
        for (var column = 0; column < board.GetLength(1); column++)
        {
            Console.Write(board[row, column] ? "🟩" : "⬜");
        }

        Console.WriteLine();
    }

    Console.WriteLine();
}
