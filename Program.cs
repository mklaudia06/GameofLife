const int FILAS = 10;
const int COLUMNAS = 10;

bool[,] tablero = new bool[FILAS,COLUMNAS];

tablero[3,4] = true;
tablero[4,4] = true;
tablero[4,5]= true;

void dibujar_tablero ()
{
    Console.Clear();
    for (int i = 0;i<FILAS;i++)
    {
        for (int j = 0; j<COLUMNAS;j++)
        {
            Console.Write(tablero[i,j]  ? "█" : ".");

        }
        Console.WriteLine();
    }
}
dibujar_tablero();
Console.ReadKey();
