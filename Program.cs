
const int FILAS = 20;
const int COLUMNAS = 20;

bool[,] tablero = new bool[FILAS,COLUMNAS];
int simulacion =0;

Random numero_aleatorio =  new Random();

for (int i = 1; i < FILAS - 1 ; i++)
{
    for (int j = 1; j < COLUMNAS -1 ; j++)
    {
        tablero[i,j] = numero_aleatorio.NextDouble() < 0.4; 
    }
}

void dibujar_tablero (bool[,] tablero1)
{
    Console.WriteLine("SIMULACION: " + simulacion);
    for (int i = 0; i < FILAS ; i++)
    {
        for (int j = 0; j < COLUMNAS; j++)
        {
            if (tablero1[i, j])
            {
                Console.Write("🟩");
            }
            else
            {
                Console.Write("⬜");
            }

        }
        Console.WriteLine();
    }
    Console.WriteLine();
}


int contador_de_vecinos(int fila, int columna,bool[,] tablero)
{
    int contador = 0;
    if ( fila > 0 && tablero [fila-1,columna])
    {
        contador ++;
    }
    if (fila < FILAS - 1 && tablero [fila+1,columna])
    {
        contador ++;
    }
    if (columna > 0 && tablero [fila,columna-1])
    {
        contador ++;
    }
    if (columna < COLUMNAS -1 && tablero [fila,columna+1])
    {
        contador ++;
    }
    if (fila > 0 && columna > 0 && tablero [fila-1,columna-1])
    {
        contador ++;
    }
    if (fila > 0 && columna < COLUMNAS-1 && tablero [fila-1,columna+1])
    {
        contador ++;
    }
    if (fila < FILAS-1 && columna > 0 && tablero [fila+1,columna-1])
    {
        contador ++;
    }
    if (fila < FILAS-1 && columna < COLUMNAS-1  && tablero [fila+1,columna+1])
    {
        contador ++;
    }
    return contador;
}

bool[,] calcular_siguiente_generacion(bool [,] tablero2)
{
    bool[,] tablero_nuevo = new bool[FILAS,COLUMNAS];

    for (int i = 1; i < FILAS - 1; i++)
    {
        for (int j = 1; j < COLUMNAS -1 ; j++)
        {
            int vecinos = contador_de_vecinos(i,j,tablero2);
            if (tablero2[i, j])
            {
                if (vecinos == 2 || vecinos == 3)
                {
                    tablero_nuevo[i,j] = true;
                }
                if (vecinos < 2)
                {
                    tablero_nuevo[i,j] = false;
                }
                if (vecinos  > 3)
                {
                    tablero_nuevo[i,j] = false;
                }
            }
            else
            {
                if (vecinos == 3)
                {
                    tablero_nuevo[i,j] = true;
                }
            }
        }
    }
    simulacion++;
    return tablero_nuevo;
}

void Play (bool[,]tablero_inicial)
{
    dibujar_tablero(tablero_inicial);      
    bool[,] tablero_nuevo = calcular_siguiente_generacion(tablero_inicial);

    while (true)
    {
        dibujar_tablero(tablero_nuevo);
        tablero_nuevo = calcular_siguiente_generacion(tablero_nuevo);
        System.Threading.Thread.Sleep(200);
    }
} 
Play(tablero);

Console.ReadKey();