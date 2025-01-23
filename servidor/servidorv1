using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NodoAritmetico
{
    public string Operador;  // Puede ser un operador aritmético ('+', '-', '*', '/', '%')
    public double? Operando; // Puede ser un número (operando hoja)
    public NodoAritmetico Izquierda;
    public NodoAritmetico Derecha;

    // Constructor para nodos operadores (internos)
    public NodoAritmetico(string operador)
    {
        Operador = operador;
        Operando = null;
        Izquierda = null;
        Derecha = null;
    }

    // Constructor para nodos hoja (valores numéricos)
    public NodoAritmetico(double operando)
    {
        Operador = null;
        Operando = operando;
        Izquierda = null;
        Derecha = null;
    }

    // Método para evaluar la expresión
    public double Evaluar()
    {
        if (Operando.HasValue)  // Si es una hoja, devolver su valor
        {
            return Operando.Value;
        }

        // Evaluar según el operador
        switch (Operador)
        {
            case "+":
                return Izquierda.Evaluar() + Derecha.Evaluar();
            case "-":
                return Izquierda.Evaluar() - Derecha.Evaluar();
            case "*":
                return Izquierda.Evaluar() * Derecha.Evaluar();
            case "/":
                return Izquierda.Evaluar() / Derecha.Evaluar();
            case "%":
                return Izquierda.Evaluar() * 0.01;  // Calculamos el porcentaje
            default:
                throw new InvalidOperationException("Operador no reconocido");
        }
    }
}

public class Server
{
    private const int puerto = 12345;
    private TcpListener servidor;

    public Server()
    {
        servidor = new TcpListener(IPAddress.Any, puerto);
    }

    public void Iniciar()
    {
        servidor.Start();
        Console.WriteLine("Servidor iniciado, esperando conexiones...");

        while (true)
        {
            TcpClient cliente = servidor.AcceptTcpClient();
            Console.WriteLine("Cliente conectado.");

            // Obtener el stream para enviar y recibir datos
            NetworkStream stream = cliente.GetStream();
            byte[] buffer = new byte[1024];
            int bytesLeidos;

            // Leer la expresión infija del cliente
            bytesLeidos = stream.Read(buffer, 0, buffer.Length);
            string expresionInfija = Encoding.ASCII.GetString(buffer, 0, bytesLeidos);

            Console.WriteLine("Expresión recibida: " + expresionInfija);

            try
            {
                // Convertir la expresión infija a postfija
                string postfija = ConvertirPostfija(expresionInfija);
                Console.WriteLine("Notación Postfija: " + postfija);

                // Construir el árbol de expresión desde la notación postfija
                NodoAritmetico arbol = ConstruirArbolPostfijo(postfija);

                // Evaluar el árbol de expresión
                double resultado = arbol.Evaluar();
                Console.WriteLine("Resultado de la expresión: " + resultado);

                // Enviar solo el resultado al cliente
                byte[] respuesta = Encoding.ASCII.GetBytes(resultado.ToString());
                stream.Write(respuesta, 0, respuesta.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                // En caso de error, enviar solo "Error"
                byte[] respuesta = Encoding.ASCII.GetBytes("Error");
                stream.Write(respuesta, 0, respuesta.Length);
            }

            // Cerrar la conexión con el cliente
            cliente.Close();
        }
    }

    // Función para convertir una expresión infija a postfija (notación polaca inversa)
    private string ConvertirPostfija(string infija)
    {
        Stack<char> pila = new Stack<char>();
        List<string> resultado = new List<string>();
        for (int i = 0; i < infija.Length; i++)
        {
            char token = infija[i];

            if (char.IsDigit(token))  // Si es un operando
            {
                string numero = ProcesarNumeroRecursivo(infija, i, out int nuevoIndice);
                i = nuevoIndice;
                resultado.Add(numero);
            }
            else if (token == '+' || token == '-' || token == '*' || token == '/' || token == '%')
            {
                ProcesarOperadorRecursivo(pila, resultado, token);
                pila.Push(token);
            }
            else if (token == '(')
            {
                pila.Push(token);
            }
            else if (token == ')')
            {
                ProcesarCierreParentesisRecursivo(pila, resultado);
            }
        }

        ProcesarRestantesRecursivo(pila, resultado);
        return string.Join(" ", resultado);
    }

    private void ProcesarOperadorRecursivo(Stack<char> pila, List<string> resultado, char token)
    {
        if (pila.Count == 0 || Precedencia(pila.Peek()) < Precedencia(token))
        {
            return;
        }

        resultado.Add(pila.Pop().ToString());
        ProcesarOperadorRecursivo(pila, resultado, token);
    }

    private void ProcesarCierreParentesisRecursivo(Stack<char> pila, List<string> resultado)
    {
        if (pila.Count == 0)
        {
            throw new InvalidOperationException("Paréntesis desbalanceados.");
        }

        if (pila.Peek() == '(')
        {
            pila.Pop();
            return;
        }

        resultado.Add(pila.Pop().ToString());
        ProcesarCierreParentesisRecursivo(pila, resultado);
    }

    private void ProcesarRestantesRecursivo(Stack<char> pila, List<string> resultado)
    {
        if (pila.Count == 0)
        {
            return;
        }

        char op = pila.Pop();
        if (op == '(' || op == ')')
        {
            throw new InvalidOperationException("Paréntesis desbalanceados.");
        }

        resultado.Add(op.ToString());
        ProcesarRestantesRecursivo(pila, resultado);
    }

    private string ProcesarNumeroRecursivo(string infija, int indice, out int nuevoIndice)
    {
        if (indice >= infija.Length || (!char.IsDigit(infija[indice]) && infija[indice] != '.'))
        {
            nuevoIndice = indice - 1;
            return "";
        }

        return infija[indice] + ProcesarNumeroRecursivo(infija, indice + 1, out nuevoIndice);
    }

    // Función para obtener la precedencia de los operadores
    private int Precedencia(char operador)
    {
        if (operador == '+' || operador == '-')
            return 1;
        if (operador == '*' || operador == '/')
            return 2;
        if (operador == '%')
            return 2;  // El porcentaje tiene la misma precedencia que la multiplicación y división
        return 0;
    }

    // Función recursiva para construir un árbol de expresión desde una notación postfija
    private NodoAritmetico ConstruirArbolPostfijo(string postfija)
    {
        string[] tokens = postfija.Split(' ');
        Stack<NodoAritmetico> pila = new Stack<NodoAritmetico>();
        ProcesarTokens(tokens, 0, pila);
        return pila.Pop();
    }

    private void ProcesarTokens(string[] tokens, int indice, Stack<NodoAritmetico> pila)
    {
        if (indice >= tokens.Length)
        {
            return;
        }

        string token = tokens[indice];
        if (double.TryParse(token, out double num))  // Si es un número, creamos un nodo hoja
        {
            pila.Push(new NodoAritmetico(num));
        }
        else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
        {
            NodoAritmetico derecha = pila.Pop();
            NodoAritmetico izquierda = pila.Pop();
            NodoAritmetico operador = new NodoAritmetico(token)
            {
                Izquierda = izquierda,
                Derecha = derecha
            };
            pila.Push(operador);
        }

        ProcesarTokens(tokens, indice + 1, pila);
    }
}

class Programa
{
    static void Main()
    {
        Server servidor = new Server();
        servidor.Iniciar();
    }
}
