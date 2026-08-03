using Xunit;

public class GameOfLifeEngineTests
{
    [Fact]
    public void Block_RemainsUnchanged()
    {
        var block = CreateBoard(4, 4, (1, 1), (1, 2), (2, 1), (2, 2));

        var nextGeneration = GameOfLifeEngine.CalculateNextGeneration(block);

        AssertBoardsEqual(block, nextGeneration);
    }

    [Fact]
    public void Blinker_AlternatesToVerticalLine()
    {
        var blinker = CreateBoard(5, 5, (2, 1), (2, 2), (2, 3));
        var expected = CreateBoard(5, 5, (1, 2), (2, 2), (3, 2));

        var nextGeneration = GameOfLifeEngine.CalculateNextGeneration(blinker);

        AssertBoardsEqual(expected, nextGeneration);
    }

    [Fact]
    public void Glider_MovesDiagonallyAfterFourGenerations()
    {
        var glider = CreateBoard(10, 10, (2, 3), (3, 4), (4, 2), (4, 3), (4, 4));
        var expected = CreateBoard(10, 10, (3, 4), (4, 5), (5, 3), (5, 4), (5, 5));

        var generation = glider;
        for (var index = 0; index < 4; index++)
        {
            generation = GameOfLifeEngine.CalculateNextGeneration(generation);
        }

        AssertBoardsEqual(expected, generation);
    }

    [Fact]
    public void Pulsar_ReturnsToItsInitialStateAfterThreeGenerations()
    {
        var pulsar = CreatePaddedBoard(
            2,
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
            "..###...###..");

        var generation = pulsar;
        for (var index = 0; index < 3; index++)
        {
            generation = GameOfLifeEngine.CalculateNextGeneration(generation);
        }

        AssertBoardsEqual(pulsar, generation);
    }

    [Fact]
    public void CellsOnTheBorder_CountTheirValidNeighbors()
    {
        var board = CreateBoard(3, 3, (0, 0), (0, 1), (1, 0));
        var expected = CreateBoard(3, 3, (0, 0), (0, 1), (1, 0), (1, 1));

        var nextGeneration = GameOfLifeEngine.CalculateNextGeneration(board);

        AssertBoardsEqual(expected, nextGeneration);
    }

    private static bool[,] CreateBoard(int rows, int columns, params (int Row, int Column)[] liveCells)
    {
        var board = new bool[rows, columns];

        foreach (var (row, column) in liveCells)
        {
            board[row, column] = true;
        }

        return board;
    }

    private static bool[,] CreateBoard(params string[] rows)
    {
        var board = new bool[rows.Length, rows[0].Length];

        for (var row = 0; row < rows.Length; row++)
        {
            for (var column = 0; column < rows[row].Length; column++)
            {
                board[row, column] = rows[row][column] == '#';
            }
        }

        return board;
    }

    private static bool[,] CreatePaddedBoard(int padding, params string[] patternRows)
    {
        var board = new bool[patternRows.Length + (padding * 2), patternRows[0].Length + (padding * 2)];

        for (var row = 0; row < patternRows.Length; row++)
        {
            for (var column = 0; column < patternRows[row].Length; column++)
            {
                board[row + padding, column + padding] = patternRows[row][column] == '#';
            }
        }

        return board;
    }

    private static void AssertBoardsEqual(bool[,] expected, bool[,] actual)
    {
        Assert.Equal(expected.GetLength(0), actual.GetLength(0));
        Assert.Equal(expected.GetLength(1), actual.GetLength(1));

        for (var row = 0; row < expected.GetLength(0); row++)
        {
            for (var column = 0; column < expected.GetLength(1); column++)
            {
                Assert.Equal(expected[row, column], actual[row, column]);
            }
        }
    }
}
