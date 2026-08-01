Console.WriteLine("Dime la cantidad de filas que seas ponerle al tablero... ");
int FILAS = Convert.ToInt16(Console.ReadLine());
Console.WriteLine("Ok, ahora dime la cantidad de columnas...");
int COLUMNAS = Convert.ToInt16(Console.ReadLine());

bool[,] tablero = new bool[FILAS,COLUMNAS];
int simulacion = 0;

Console.WriteLine($"Dado el tablero del tamano: {FILAS}x{COLUMNAS} escribe de la forma fila0,columna3;fila1,columna2;... en las celdas donde quieres poner las celulas vivas de tu tablero...");
string celulas_vivas = Console.ReadLine();

int[][] resultado = celulas_vivas
    .Split(';')                                    
    .Select(fila => fila.Split(',')                
        .Select(num => int.Parse(num))           
        .ToArray())                               
    .ToArray();                                   

foreach (int[] vector in resultado)
{
    int fila = vector[0];
    int columna = vector[1];
    tablero[fila,columna] = true;
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




int contador_de_vecinos (int fila, int columna, bool[,] tablero)
{
    int[,] array_de_direcciones = 
    {
        {-1,0}, 
        {1,0},  
        {0,-1}, 
        {0,1},   
        {-1,-1}, 
        {-1,1},  
        {1,-1},  
        {1,1}   
    };
    int contador = 0;
    if (fila > 0 && columna > 0  && fila < FILAS - 1 && columna < COLUMNAS - 1)
    {
        for (int i = 0; i < array_de_direcciones.GetLength(0); i++)
        {
            int fila_vecina = fila + array_de_direcciones[i,0];
            int columna_vecina = columna + array_de_direcciones[i,1];

            if (tablero[fila_vecina, columna_vecina])
            {
                contador++;
            }
        }
    }
    else
    {
        //aqui verificaria los bordes aunque no tenga los 8 vecinos, o sea si es una esquina tiene 3 vecinos y trabajo sobre eso, y si no tiene 5 vecinos y trabajo sobre eso
    }
    
    return contador;
}

/*int contador_de_vecinos(int fila, int columna,bool[,] tablero)
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
}*/

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
    
    var lista_tableros = new List<bool[,]>();
    lista_tableros.Add((bool[,])tablero_inicial.Clone());
    dibujar_tablero(tablero_inicial);     
    System.Threading.Thread.Sleep(200);

    bool [,] tablero_actual = tablero_inicial; 

    

    while (true)
    {
        bool[,] tablero_nuevo = calcular_siguiente_generacion(tablero_actual);
        lista_tableros.Add((bool[,])tablero_nuevo.Clone());

        int len = lista_tableros.Count;
        if (len >= 4)
        {
            if (lista_tableros[lista_tableros.Count -1] == lista_tableros[lista_tableros.Count -2] && lista_tableros[lista_tableros.Count -2] == lista_tableros[lista_tableros.Count -3])
            {
                Console.WriteLine("¡Lo sentimos entraste en un bucle! ¡Game over!"); 
                break;
            };
            if (lista_tableros[lista_tableros.Count-1] == lista_tableros[lista_tableros.Count-4]&& lista_tableros[lista_tableros.Count-2]==lista_tableros[lista_tableros.Count-5]&& lista_tableros[lista_tableros.Count - 3] == lista_tableros[lista_tableros.Count - 6])
            {
                Console.WriteLine("¡Lo sentimos entraste en un bucle! ¡Game over!"); 
                break;
            }
        } 
        dibujar_tablero(tablero_nuevo);
        tablero_actual = tablero_nuevo;
        System.Threading.Thread.Sleep(200);  
        
    }
} 
Play(tablero);

Console.ReadKey();