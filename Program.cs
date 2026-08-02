Console.WriteLine("Dime la cantidad de filas que seas ponerle al tablero... ");
int FILAS = Convert.ToInt16(Console.ReadLine());
Console.WriteLine("Ok, ahora dime la cantidad de columnas...");
int COLUMNAS = Convert.ToInt16(Console.ReadLine());

bool[,] tablero = new bool[FILAS,COLUMNAS];
int simulacion = 0;

Console.WriteLine($"Dado el tablero del tamano: {FILAS}x{COLUMNAS} escribe de la forma fila0,columna3;fila1,columna2;... en las celdas donde quieres poner las celulas vivas de tu tablero inicial, dado que es un tablero de {FILAS}x{COLUMNAS}, las filas van desde la 0 hasta la {FILAS-1} y columnas van desde la cero hasta la {COLUMNAS-1} porfavor no salirse del indice");
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
    if (fila<0 || fila >= FILAS || columna < 0 || columna >= COLUMNAS)
    {
        Console.WriteLine($"Error, la coordenada {fila},{columna} esta fuera del tablero");
    }
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

bool TodasMuertas(bool[,] tablero)
{
    for (int i = 0; i < FILAS; i++)
    {
        for (int j = 0; j < COLUMNAS; j++)
        {
            if (tablero[i,j])
            {
                return false;
            }
        }
    }
    return true;
}

bool TablerosIguales(bool[,] tablero1, bool[,] tablero2)
{
    for (int i = 0; i < FILAS; i++)
    {
        for (int j = 0; j < COLUMNAS; j++)
        {
            if(tablero1[i,j] != tablero2[i, j])
            {
                return false;
            }
        }
    }
    return true;
}

void Play (bool[,]tablero_inicial)
{
    
    var lista_tableros = new List<bool[,]>();
    dibujar_tablero(tablero_inicial);   
    lista_tableros.Add((bool[,])tablero_inicial.Clone());               
    System.Threading.Thread.Sleep(200);

    bool [,] tablero_actual = tablero_inicial; 

    

    while (true)
    {
        bool[,] tablero_nuevo = calcular_siguiente_generacion(tablero_actual);
        lista_tableros.Add((bool[,])tablero_nuevo.Clone());

        int len_lista_tableros = lista_tableros.Count;
        dibujar_tablero(tablero_nuevo);
        if (TodasMuertas(lista_tableros[len_lista_tableros - 1]))
        {
            Console.WriteLine("¡Lo sentimos todas las celulas murieron! ¡Game over!"); 
            break;
        }
        if (len_lista_tableros >= 4)
        {
            if (TablerosIguales(lista_tableros[len_lista_tableros -1],lista_tableros[len_lista_tableros -2]))
            {
                Console.WriteLine("¡Lo sentimos entraste en un bucle! ¡Game over!"); 
                break;
            };
            if (TablerosIguales(lista_tableros[len_lista_tableros -1],lista_tableros[len_lista_tableros -3]) &&
                TablerosIguales(lista_tableros[len_lista_tableros -2],lista_tableros[len_lista_tableros - 4]))
            {
                Console.WriteLine("¡Lo sentimos entraste en un bucle! ¡Game over!");
                break;
            }
        if (len_lista_tableros >= 6)
            {
                if (TablerosIguales(lista_tableros[len_lista_tableros -1],lista_tableros[len_lista_tableros -4]) && 
                TablerosIguales(lista_tableros[len_lista_tableros -2],lista_tableros[len_lista_tableros -5]) && 
                TablerosIguales(lista_tableros[len_lista_tableros -3],lista_tableros[len_lista_tableros -6]))
                {
                    Console.WriteLine("¡Lo sentimos entraste en un bucle! ¡Game over!"); 
                    break;
                }
            }
       
        } 
        
        tablero_actual = tablero_nuevo;
        System.Threading.Thread.Sleep(200);  
        
    }
} 
Play(tablero);

Console.ReadKey();