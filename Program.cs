using System.Diagnostics;

const int DefaultFrameDelayMilliseconds = 200;
const int MinimumFrameDelayMilliseconds = 25;
const int MaximumFrameDelayMilliseconds = 1_000;
const int FrameDelayStepMilliseconds = 25;
const string SavedBoardsDirectory = "tableros";

while (RunSimulation(ChooseInitialBoard()) == SimulationExit.Restart)
{
}

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

static bool[,] ChooseInitialBoard()
{
    var patterns = GetPredefinedPatterns();
    var categories = new[] { "Todos", "Estables", "Osciladores", "Naves", "Methuselahs", "Cañones" };
    var selectedOption = 0;
    var categoryIndex = 0;

    while (true)
    {
        var visiblePatterns = patterns.Where(pattern =>
            categoryIndex == 0 || GetPatternCategory(pattern) == categories[categoryIndex]).ToList();
        selectedOption = Math.Min(selectedOption, visiblePatterns.Count);
        DrawPatternMenu(visiblePatterns, selectedOption, categories[categoryIndex]);

        switch (Console.ReadKey(intercept: true).Key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                selectedOption = Math.Max(0, selectedOption - 1);
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                selectedOption = Math.Min(visiblePatterns.Count, selectedOption + 1);
                break;

            case ConsoleKey.F:
                categoryIndex = (categoryIndex + 1) % categories.Length;
                selectedOption = 0;
                break;

            case ConsoleKey.L:
                var loadedBoard = LoadSavedBoard();
                if (loadedBoard is not null)
                {
                    return loadedBoard;
                }

                break;

            case ConsoleKey.Enter:
                if (selectedOption == 0)
                {
                    Console.Clear();
                    var rows = ReadPositiveInteger("Indica la cantidad de filas del tablero: ");
                    var columns = ReadPositiveInteger("Indica la cantidad de columnas del tablero: ");
                    return CreateInitialBoardEditor(rows, columns);
                }

                var pattern = visiblePatterns[selectedOption - 1];
                return CreateBoardFromPattern(pattern);
        }
    }
}

static void DrawPatternMenu(
    IReadOnlyList<PatternDefinition> patterns,
    int selectedOption,
    string category)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╭────────────────── PATRONES INICIALES ──────────────────╮");
    Console.ResetColor();
    Console.WriteLine($"  Categoría: {category} | F: filtrar | Flechas/W/S: elegir | Enter: confirmar | L: cargar");
    Console.WriteLine();

    WritePatternOption("Crear mi propio patrón", "Editor interactivo", selectedOption == 0, null);

    for (var index = 0; index < patterns.Count; index++)
    {
        var pattern = patterns[index];
        WritePatternOption(
            pattern.Name,
            pattern.Description,
            selectedOption == index + 1,
            $"tablero {pattern.BoardRows} × {pattern.BoardColumns}");
    }

    if (selectedOption > 0)
    {
        DrawPatternPreview(patterns[selectedOption - 1]);
    }
}

static string GetPatternCategory(PatternDefinition pattern) => pattern.Name switch
{
    "Bloque" => "Estables",
    "Blinker" or "Toad" or "Pulsar" => "Osciladores",
    "Glider" => "Naves",
    "Diehard" => "Methuselahs",
    _ => "Cañones"
};

static void WritePatternOption(string name, string description, bool isSelected, string? size)
{
    Console.ForegroundColor = isSelected ? ConsoleColor.Yellow : ConsoleColor.White;

    var marker = isSelected ? "▶" : " ";
    Console.WriteLine($" {marker} {name,-24} {description} {size}");
    Console.ResetColor();
}

static void DrawPatternPreview(PatternDefinition pattern)
{
    Console.WriteLine($"\n  Vista previa: {pattern.Name}");

    foreach (var row in pattern.Cells)
    {
        Console.Write("  ");
        foreach (var cell in row)
        {
            Console.ForegroundColor = cell == '#' ? ConsoleColor.Green : ConsoleColor.DarkGray;
            Console.Write(cell == '#' ? "● " : "· ");
        }

        Console.ResetColor();
        Console.WriteLine();
    }
}

static bool[,] CreateBoardFromPattern(PatternDefinition pattern)
{
    var board = new bool[pattern.BoardRows, pattern.BoardColumns];
    var rowOffset = (pattern.BoardRows - pattern.Height) / 2;
    var columnOffset = (pattern.BoardColumns - pattern.Width) / 2;

    for (var row = 0; row < pattern.Height; row++)
    {
        for (var column = 0; column < pattern.Width; column++)
        {
            board[rowOffset + row, columnOffset + column] = pattern.Cells[row][column] == '#';
        }
    }

    return board;
}

static bool[,] CreateInitialBoardEditor(int rows, int columns)
{
    var board = new bool[rows, columns];
    var cursorRow = 0;
    var cursorColumn = 0;

    while (true)
    {
        DrawBoardEditor(board, cursorRow, cursorColumn);
        var key = Console.ReadKey(intercept: true).Key;

        switch (key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                cursorRow = Math.Max(0, cursorRow - 1);
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                cursorRow = Math.Min(rows - 1, cursorRow + 1);
                break;

            case ConsoleKey.LeftArrow:
            case ConsoleKey.A:
                cursorColumn = Math.Max(0, cursorColumn - 1);
                break;

            case ConsoleKey.RightArrow:
            case ConsoleKey.D:
                cursorColumn = Math.Min(columns - 1, cursorColumn + 1);
                break;

            case ConsoleKey.Spacebar:
                board[cursorRow, cursorColumn] = !board[cursorRow, cursorColumn];
                break;

            case ConsoleKey.C:
                Array.Clear(board);
                break;

            case ConsoleKey.Enter:
                Console.Clear();
                return board;
        }
    }
}

static void DrawBoardEditor(bool[,] board, int cursorRow, int cursorColumn)
{
    Console.Clear();
    var liveNeighbors = CountLiveNeighbors(board, cursorRow, cursorColumn);
    var cellState = board[cursorRow, cursorColumn] ? "viva" : "muerta";

    Console.WriteLine("CREA EL TABLERO INICIAL");
    Console.WriteLine("Flechas o W/A/S/D: mover | Espacio: cambiar célula | C: limpiar | Enter: iniciar");
    Console.WriteLine("● viva   · muerta   [ ] cursor");
    Console.WriteLine($"Celda ({cursorRow}, {cursorColumn}): {cellState} | Vecinos vivos: {liveNeighbors}");
    Console.WriteLine();

    for (var row = 0; row < board.GetLength(0); row++)
    {
        for (var column = 0; column < board.GetLength(1); column++)
        {
            var cell = board[row, column] ? "●" : "·";
            var isCursor = row == cursorRow && column == cursorColumn;
            Console.Write(isCursor ? $"[{cell}]" : $" {cell} ");
        }

        Console.WriteLine();
    }
}

static void SaveBoard(bool[,] board)
{
    Console.Clear();
    Console.WriteLine("GUARDAR TABLERO");
    Console.Write("Nombre del tablero: ");

    var boardName = GetSafeBoardName(Console.ReadLine() ?? string.Empty);
    if (string.IsNullOrWhiteSpace(boardName))
    {
        ShowMessage("No se guardó el tablero: el nombre no es válido.", ConsoleColor.Red);
        return;
    }

    var directory = Path.Combine(Environment.CurrentDirectory, SavedBoardsDirectory);
    Directory.CreateDirectory(directory);

    var path = GetAvailableBoardPath(directory, boardName);
    File.WriteAllLines(path, SerializeBoard(board));
    ShowMessage($"Tablero guardado en {Path.GetFileName(path)}.", ConsoleColor.Green);
}

static void SaveUniverse(HashSet<Cell> liveCells)
{
    if (liveCells.Count == 0)
    {
        SaveBoard(new bool[1, 1]);
        return;
    }

    var minimumRow = liveCells.Min(cell => cell.Row);
    var maximumRow = liveCells.Max(cell => cell.Row);
    var minimumColumn = liveCells.Min(cell => cell.Column);
    var maximumColumn = liveCells.Max(cell => cell.Column);
    var board = new bool[maximumRow - minimumRow + 1, maximumColumn - minimumColumn + 1];

    foreach (var cell in liveCells)
    {
        board[cell.Row - minimumRow, cell.Column - minimumColumn] = true;
    }

    SaveBoard(board);
}

static bool[,]? LoadSavedBoard()
{
    var directory = Path.Combine(Environment.CurrentDirectory, SavedBoardsDirectory);
    var savedBoardPaths = Directory.Exists(directory)
        ? Directory.GetFiles(directory, "*.life").OrderBy(Path.GetFileName).ToArray()
        : [];

    if (savedBoardPaths.Length == 0)
    {
        ShowMessage("No hay tableros guardados todavía.", ConsoleColor.DarkYellow);
        return null;
    }

    var selectedIndex = 0;

    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╭──────────────── TABLEROS GUARDADOS ────────────────╮");
        Console.ResetColor();
        Console.WriteLine("  Flechas o W/S: elegir | Enter: cargar | Esc: volver");
        Console.WriteLine();

        for (var index = 0; index < savedBoardPaths.Length; index++)
        {
            Console.ForegroundColor = index == selectedIndex ? ConsoleColor.Yellow : ConsoleColor.White;
            Console.WriteLine($" {((index == selectedIndex) ? "▶" : " ")} {Path.GetFileNameWithoutExtension(savedBoardPaths[index])}");
        }

        Console.ResetColor();

        switch (Console.ReadKey(intercept: true).Key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                selectedIndex = Math.Max(0, selectedIndex - 1);
                break;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                selectedIndex = Math.Min(savedBoardPaths.Length - 1, selectedIndex + 1);
                break;

            case ConsoleKey.Enter:
                try
                {
                    return DeserializeBoard(savedBoardPaths[selectedIndex]);
                }
                catch (InvalidDataException exception)
                {
                    ShowMessage($"No se pudo cargar el tablero: {exception.Message}", ConsoleColor.Red);
                    return null;
                }

            case ConsoleKey.Escape:
                return null;
        }
    }
}

static string[] SerializeBoard(bool[,] board)
{
    var rows = board.GetLength(0);
    var columns = board.GetLength(1);
    var lines = new string[rows + 1];
    lines[0] = $"{rows},{columns}";

    for (var row = 0; row < rows; row++)
    {
        var cells = new char[columns];
        for (var column = 0; column < columns; column++)
        {
            cells[column] = board[row, column] ? '#' : '.';
        }

        lines[row + 1] = new string(cells);
    }

    return lines;
}

static bool[,] DeserializeBoard(string path)
{
    var lines = File.ReadAllLines(path);
    if (lines.Length < 2)
    {
        throw new InvalidDataException("el archivo está vacío o incompleto.");
    }

    var dimensions = lines[0].Split(',', StringSplitOptions.TrimEntries);
    if (dimensions.Length != 2 ||
        !int.TryParse(dimensions[0], out var rows) ||
        !int.TryParse(dimensions[1], out var columns) ||
        rows <= 0 || columns <= 0 ||
        lines.Length != rows + 1)
    {
        throw new InvalidDataException("el formato de las dimensiones no es válido.");
    }

    var board = new bool[rows, columns];
    for (var row = 0; row < rows; row++)
    {
        var cells = lines[row + 1];
        if (cells.Length != columns || cells.Any(cell => cell is not '#' and not '.'))
        {
            throw new InvalidDataException($"la fila {row + 1} no es válida.");
        }

        for (var column = 0; column < columns; column++)
        {
            board[row, column] = cells[column] == '#';
        }
    }

    return board;
}

static string GetSafeBoardName(string name)
{
    var nameWithoutExtension = Path.GetFileNameWithoutExtension(name.Trim());
    var invalidCharacters = Path.GetInvalidFileNameChars();
    return string.Concat(nameWithoutExtension.Where(character => !invalidCharacters.Contains(character))).Trim();
}

static string GetAvailableBoardPath(string directory, string boardName)
{
    var path = Path.Combine(directory, $"{boardName}.life");
    var duplicateNumber = 2;

    while (File.Exists(path))
    {
        path = Path.Combine(directory, $"{boardName} ({duplicateNumber}).life");
        duplicateNumber++;
    }

    return path;
}

static void ShowMessage(string message, ConsoleColor color)
{
    Console.Clear();
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ResetColor();
    Console.WriteLine("Pulsa cualquier tecla para continuar.");
    Console.ReadKey(intercept: true);
}

static SimulationExit RunSimulation(bool[,] initialBoard)
{
    var liveCells = ToLiveCells(initialBoard);
    var ages = liveCells.ToDictionary(cell => cell, _ => 1);
    var generation = 0;
    var isPaused = false;
    var showHeatmap = false;
    var explainStep = false;
    var zoom = 1;
    var viewRows = Math.Clamp(initialBoard.GetLength(0), 10, 18);
    var viewColumns = Math.Clamp(initialBoard.GetLength(1), 12, 32);
    var cameraRow = (initialBoard.GetLength(0) - viewRows) / 2;
    var cameraColumn = (initialBoard.GetLength(1) - viewColumns) / 2;
    var frameDelayMilliseconds = DefaultFrameDelayMilliseconds;
    var highestPopulation = liveCells.Count;
    var peakGeneration = 0;
    var detectedPatterns = new HashSet<string>();
    var changes = new Dictionary<Cell, CellChange>();
    var stopwatch = Stopwatch.StartNew();

    while (true)
    {
        var pattern = DetectKnownPattern(liveCells);
        if (pattern is not null)
        {
            detectedPatterns.Add(pattern);
        }

        DrawUniverse(liveCells, ages, changes, generation, isPaused, showHeatmap, explainStep, zoom,
            viewRows, viewColumns, cameraRow, cameraColumn, highestPopulation, peakGeneration,
            detectedPatterns.Count, pattern, stopwatch.Elapsed, frameDelayMilliseconds);

        if (liveCells.Count == 0)
        {
            Console.WriteLine("Todas las células murieron. Fin de la simulación.");
            return SimulationExit.Completed;
        }

        while (isPaused || Console.KeyAvailable)
        {
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(25);
                continue;
            }

            var keyInfo = Console.ReadKey(intercept: true);
            if (keyInfo.KeyChar is '+' or '=')
            {
                frameDelayMilliseconds = Math.Max(MinimumFrameDelayMilliseconds, frameDelayMilliseconds - FrameDelayStepMilliseconds);
                continue;
            }

            switch (keyInfo.Key)
            {
                case ConsoleKey.Q:
                    return SimulationExit.Completed;
                case ConsoleKey.R:
                    return SimulationExit.Restart;
                case ConsoleKey.Spacebar:
                    isPaused = !isPaused;
                    if (isPaused) stopwatch.Stop(); else stopwatch.Start();
                    break;
                case ConsoleKey.N when isPaused:
                    (liveCells, ages, changes) = AdvanceUniverse(liveCells, ages);
                    generation++;
                    UpdatePopulationRecord(liveCells.Count, generation, ref highestPopulation, ref peakGeneration);
                    break;
                case ConsoleKey.G:
                    stopwatch.Stop();
                    SaveUniverse(liveCells);
                    if (!isPaused) stopwatch.Start();
                    break;
                case ConsoleKey.L:
                    showHeatmap = !showHeatmap;
                    break;
                case ConsoleKey.E:
                    explainStep = !explainStep;
                    break;
                case ConsoleKey.I:
                    ShowUniverseStatistics(generation, liveCells.Count, highestPopulation, peakGeneration, ages, detectedPatterns, stopwatch.Elapsed);
                    break;
                case ConsoleKey.Z:
                    zoom = Math.Max(1, zoom - 1);
                    break;
                case ConsoleKey.X:
                    zoom = Math.Min(16, zoom * 2);
                    break;
                case ConsoleKey.UpArrow:
                    cameraRow -= Math.Max(1, zoom * 2);
                    break;
                case ConsoleKey.DownArrow:
                    cameraRow += Math.Max(1, zoom * 2);
                    break;
                case ConsoleKey.LeftArrow:
                    cameraColumn -= Math.Max(1, zoom * 2);
                    break;
                case ConsoleKey.RightArrow:
                    cameraColumn += Math.Max(1, zoom * 2);
                    break;
                case ConsoleKey.OemMinus:
                case ConsoleKey.Subtract:
                    frameDelayMilliseconds = Math.Min(MaximumFrameDelayMilliseconds, frameDelayMilliseconds + FrameDelayStepMilliseconds);
                    break;
            }

            break;
        }

        if (isPaused || !WaitForNextGeneration(frameDelayMilliseconds))
        {
            continue;
        }

        (liveCells, ages, changes) = AdvanceUniverse(liveCells, ages);
        generation++;
        UpdatePopulationRecord(liveCells.Count, generation, ref highestPopulation, ref peakGeneration);
        if (!explainStep)
        {
            changes.Clear();
        }
    }
}

static bool WaitForNextGeneration(int frameDelayMilliseconds)
{
    var waitTimer = Stopwatch.StartNew();

    while (waitTimer.ElapsedMilliseconds < frameDelayMilliseconds)
    {
        if (Console.KeyAvailable)
        {
            return false;
        }

        var remainingMilliseconds = frameDelayMilliseconds - (int)waitTimer.ElapsedMilliseconds;
        Thread.Sleep(Math.Min(25, Math.Max(1, remainingMilliseconds)));
    }

    return true;
}

static HashSet<Cell> ToLiveCells(bool[,] board)
{
    var cells = new HashSet<Cell>();
    for (var row = 0; row < board.GetLength(0); row++)
    for (var column = 0; column < board.GetLength(1); column++)
        if (board[row, column]) cells.Add(new Cell(row, column));
    return cells;
}

static (HashSet<Cell> Cells, Dictionary<Cell, int> Ages, Dictionary<Cell, CellChange> Changes) AdvanceUniverse(
    HashSet<Cell> currentCells, Dictionary<Cell, int> currentAges)
{
    var neighborCounts = GetNeighborCounts(currentCells);
    var nextCells = new HashSet<Cell>();
    var nextAges = new Dictionary<Cell, int>();
    var changes = new Dictionary<Cell, CellChange>();

    foreach (var (cell, neighbors) in neighborCounts)
    {
        var wasAlive = currentCells.Contains(cell);
        var willLive = neighbors == 3 || (wasAlive && neighbors == 2);
        if (willLive)
        {
            nextCells.Add(cell);
            nextAges[cell] = wasAlive ? currentAges[cell] + 1 : 1;
            changes[cell] = wasAlive ? CellChange.Survived : CellChange.Born;
        }
        else if (wasAlive)
        {
            changes[cell] = CellChange.Died;
        }
    }

    return (nextCells, nextAges, changes);
}

static Dictionary<Cell, int> GetNeighborCounts(IEnumerable<Cell> liveCells)
{
    var counts = new Dictionary<Cell, int>();
    foreach (var cell in liveCells)
    {
        counts.TryAdd(cell, 0);
        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
        {
            if (rowOffset == 0 && columnOffset == 0) continue;
            var neighbor = new Cell(cell.Row + rowOffset, cell.Column + columnOffset);
            counts[neighbor] = counts.GetValueOrDefault(neighbor) + 1;
        }
    }
    return counts;
}

static void UpdatePopulationRecord(int population, int generation, ref int highestPopulation, ref int peakGeneration)
{
    if (population > highestPopulation)
    {
        highestPopulation = population;
        peakGeneration = generation;
    }
}

static int CountLiveNeighbors(bool[,] board, int row, int column) =>
    GameOfLifeEngine.CountLiveNeighbors(board, row, column);

static void WriteFramedLine(string text, int innerWidth, ConsoleColor textColor)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("│");
    Console.ForegroundColor = textColor;
    Console.Write(text.PadLeft((innerWidth + text.Length) / 2).PadRight(innerWidth));
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("│");
}

static void DrawUniverse(
    HashSet<Cell> liveCells, Dictionary<Cell, int> ages, Dictionary<Cell, CellChange> changes,
    int generation, bool isPaused, bool showHeatmap, bool explainStep, int zoom,
    int viewRows, int viewColumns, int cameraRow, int cameraColumn,
    int highestPopulation, int peakGeneration, int detectedPatternCount, string? detectedPattern,
    TimeSpan elapsed, int frameDelayMilliseconds)
{
    Console.Clear();
    var status = isPaused ? "PAUSA" : "EN CURSO";
    var title = $" UNIVERSO INFINITO • GEN {generation} • VIVAS {liveCells.Count} • PICO {highestPopulation} (G{peakGeneration}) • {elapsed:mm\\:ss} • {status} ";
    var width = Math.Max(viewColumns * 2 + 2, title.Length + 2);
    var line = new string('─', width);
    var neighbors = showHeatmap ? GetNeighborCounts(liveCells) : null;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"╭{line}╮");
    WriteFramedLine(title, width, ConsoleColor.White);
    Console.WriteLine($"├{line}┤");

    for (var screenRow = 0; screenRow < viewRows; screenRow++)
    {
        Console.Write("│ ");
        for (var screenColumn = 0; screenColumn < viewColumns; screenColumn++)
        {
            var origin = new Cell(cameraRow + screenRow * zoom, cameraColumn + screenColumn * zoom);
            DrawUniverseTile(origin, zoom, liveCells, ages, changes, neighbors, explainStep, showHeatmap);
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(" │");
    }

    Console.WriteLine($"╰{line}╯");
    Console.ResetColor();
    Console.WriteLine($"Cámara: ({cameraRow}, {cameraColumn}) | Zoom: {zoom}× | Patrones detectados: {detectedPatternCount}{(detectedPattern is null ? string.Empty : $" | Actual: {detectedPattern}")}");
    Console.WriteLine("Flechas: cámara | Z/X: zoom | L: heatmap | E: explicar paso | Espacio: pausa | N: paso | +/-: velocidad");
    if (explainStep)
    {
        Console.WriteLine("Explicación: + nace (3 vecinos) | ● sobrevive (2 o 3) | × muere (menos de 2 o más de 3)");
    }
    Console.WriteLine("G: guardar | I: estadísticas | R: reiniciar | Q: salir");
}

static void DrawUniverseTile(Cell origin, int zoom, HashSet<Cell> liveCells, Dictionary<Cell, int> ages,
    Dictionary<Cell, CellChange> changes, Dictionary<Cell, int>? neighbors, bool explainStep, bool showHeatmap)
{
    var tileCells = new List<Cell>();
    for (var row = origin.Row; row < origin.Row + zoom; row++)
    for (var column = origin.Column; column < origin.Column + zoom; column++)
        tileCells.Add(new Cell(row, column));

    var changedCell = tileCells.Select(cell => (Cell: cell, Change: changes.GetValueOrDefault(cell)))
        .FirstOrDefault(item => item.Change != CellChange.None);
    var liveCell = tileCells.Where(liveCells.Contains).Select(cell => (Cell?)cell).FirstOrDefault();

    if (explainStep && changedCell.Change != CellChange.None)
    {
        Console.ForegroundColor = changedCell.Change switch
        {
            CellChange.Born => ConsoleColor.Green,
            CellChange.Survived => ConsoleColor.Cyan,
            _ => ConsoleColor.Red
        };
        Console.Write(changedCell.Change == CellChange.Died ? "× " : changedCell.Change == CellChange.Born ? "+ " : "● ");
        return;
    }

    if (liveCell is { } aliveCell)
    {
        Console.ForegroundColor = GetAgeColor(ages[aliveCell]);
        Console.Write("● ");
        return;
    }

    var neighborCount = neighbors is null ? 0 : tileCells.Max(cell => neighbors.GetValueOrDefault(cell));
    Console.ForegroundColor = showHeatmap && neighborCount > 0 ? ConsoleColor.DarkYellow : ConsoleColor.DarkGray;
    Console.Write(showHeatmap && neighborCount > 0 ? $"{neighborCount} " : "· ");
}

static ConsoleColor GetAgeColor(int age) => age switch
{
    1 => ConsoleColor.Green,
    < 5 => ConsoleColor.Yellow,
    < 15 => ConsoleColor.Cyan,
    _ => ConsoleColor.Magenta
};

static void ShowUniverseStatistics(int generation, int population, int peakPopulation, int peakGeneration,
    Dictionary<Cell, int> ages, HashSet<string> detectedPatterns, TimeSpan elapsed)
{
    Console.Clear();
    Console.WriteLine("ESTADÍSTICAS DE LA SESIÓN");
    Console.WriteLine($"Generación actual: {generation}");
    Console.WriteLine($"Población actual: {population}");
    Console.WriteLine($"Récord de población: {peakPopulation} (generación {peakGeneration})");
    Console.WriteLine($"Mayor edad de una célula: {(ages.Count == 0 ? 0 : ages.Values.Max())}");
    Console.WriteLine($"Tiempo activo: {elapsed:mm\\:ss}");
    Console.WriteLine($"Patrones reconocidos: {(detectedPatterns.Count == 0 ? "ninguno" : string.Join(", ", detectedPatterns))}");
    Console.WriteLine("Pulsa cualquier tecla para continuar.");
    Console.ReadKey(intercept: true);
}

static string? DetectKnownPattern(HashSet<Cell> liveCells)
{
    if (liveCells.Count == 0 || liveCells.Count > 100)
    {
        return null;
    }

    var normalized = Normalize(liveCells);
    foreach (var pattern in GetPredefinedPatterns())
    {
        var patternCells = new HashSet<Cell>();
        for (var row = 0; row < pattern.Height; row++)
        for (var column = 0; column < pattern.Width; column++)
            if (pattern.Cells[row][column] == '#') patternCells.Add(new Cell(row, column));

        if (normalized.SetEquals(patternCells))
        {
            return pattern.Name;
        }
    }

    return normalized.SetEquals([new(0, 0), new(0, 1), new(1, 0), new(1, 1)]) ? "Bloque" : null;
}

static HashSet<Cell> Normalize(IEnumerable<Cell> cells)
{
    var cellList = cells.ToList();
    var minimumRow = cellList.Min(cell => cell.Row);
    var minimumColumn = cellList.Min(cell => cell.Column);
    return cellList.Select(cell => new Cell(cell.Row - minimumRow, cell.Column - minimumColumn)).ToHashSet();
}

static IReadOnlyList<PatternDefinition> GetPredefinedPatterns() =>
[
    new(
        "Bloque",
        "Vida estable: no cambia.",
        8,
        8,
        [
            "##",
            "##"
        ]),
    new(
        "Blinker",
        "Oscilador de período 2.",
        9,
        9,
        [
            "###"
        ]),
    new(
        "Toad",
        "Oscilador de período 2.",
        10,
        10,
        [
            ".###",
            "###."
        ]),
    new(
        "Glider",
        "Se desplaza en diagonal.",
        20,
        20,
        [
            ".#.",
            "..#",
            "###"
        ]),
    new(
        "Pulsar",
        "Gran oscilador de período 3.",
        17,
        17,
        [
            "..###...###..",
            ".............",
            "#....#.#....#",
            "#....#.#....#",
            "#....#.#....#",
            "..###...###..",
            ".............",
            "..###...###..",
            "#....#.#....#",
            "#....#.#....#",
            "#....#.#....#",
            ".............",
            "..###...###.."
        ]),
    new(
        "Diehard",
        "Evoluciona durante 130 generaciones y se extingue.",
        25,
        35,
        [
            "......#.",
            "##......",
            ".#...###"
        ]),
    new(
        "Pistola de Gosper",
        "Emite gliders cada 30 generaciones.",
        15,
        42,
        [
            "........................#...........",
            "......................#.#...........",
            "............##......##............##",
            "...........#...#....##............##",
            "##........#.....#...##..............",
            "##........#...#.##....#.#...........",
            "..........#.....#.......#...........",
            "...........#...#....................",
            "............##......................"
        ])
];

sealed record PatternDefinition(
    string Name,
    string Description,
    int BoardRows,
    int BoardColumns,
    string[] Cells)
{
    public int Height => Cells.Length;
    public int Width => Cells[0].Length;
}

enum SimulationExit
{
    Completed,
    Restart
}

readonly record struct Cell(int Row, int Column);

enum CellChange
{
    None,
    Born,
    Survived,
    Died
}

public static class GameOfLifeEngine
{
    public static bool[,] CalculateNextGeneration(bool[,] board)
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

    public static int CountLiveNeighbors(bool[,] board, int row, int column)
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
}
