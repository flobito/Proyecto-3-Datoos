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
                return Izquierda.Evaluar() / 100;  // El operador % divide entre 100 (solo un operando)
            default:
                throw new InvalidOperationException("Operador no valido");
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
        Console.WriteLine("Servidor encendido, esperando al cliente");

        while (true)
        {
            TcpClient cliente = servidor.AcceptTcpClient();
            Console.WriteLine("Cliente conectado");

            NetworkStream stream = cliente.GetStream();
            byte[] buffer = new byte[1024];
            int bytesLeidos;

            bytesLeidos = stream.Read(buffer, 0, buffer.Length);
            string expresionInfija = Encoding.ASCII.GetString(buffer, 0, bytesLeidos);

            Console.WriteLine("Expresión recibida: " + expresionInfija);

            try
            {
                string postfija = ConvertirPostfija(expresionInfija);
                Console.WriteLine("Notación Postfija: " + postfija);

                NodoAritmetico arbol = ConstruirArbolPostfijo(postfija);
                double resultado = arbol.Evaluar();
                Console.WriteLine("Resultado de la expresión: " + resultado);

                byte[] respuesta = Encoding.ASCII.GetBytes(resultado.ToString());
                stream.Write(respuesta, 0, respuesta.Length);
            }
            catch (Exception)
            {
                byte[] respuesta = Encoding.ASCII.GetBytes("Error");
                stream.Write(respuesta, 0, respuesta.Length);
            }

            cliente.Close();
        }
    }

    private string ConvertirPostfija(string infija)
    {
        Stack<char> pila = new Stack<char>();
        List<string> resultado = new List<string>();

        ProcesarInfija(infija, 0, pila, resultado);

        while (pila.Count > 0)
        {
            resultado.Add(pila.Pop().ToString());
        }

        return string.Join(" ", resultado);
    }

    private void ProcesarInfija(string infija, int i, Stack<char> pila, List<string> resultado)
    {
        if (i >= infija.Length)
        {
            return;
        }

        char token = infija[i];

        if (char.IsDigit(token))
        {
            string numero = token.ToString();
            while (i + 1 < infija.Length && char.IsDigit(infija[i + 1]))
            {
                numero += infija[++i];
            }
            resultado.Add(numero);
        }
        else if (token == '+' || token == '-' || token == '*' || token == '/' || token == '%')
        {
            while (pila.Count > 0 && Precedencia(pila.Peek()) >= Precedencia(token))
            {
                resultado.Add(pila.Pop().ToString());
            }
            pila.Push(token);
        }
        else if (token == '(')
        {
            pila.Push(token);
        }
        else if (token == ')')
        {
            while (pila.Count > 0 && pila.Peek() != '(')
            {
                resultado.Add(pila.Pop().ToString());
            }
            pila.Pop();
        }

        ProcesarInfija(infija, i + 1, pila, resultado);
    }

    private int Precedencia(char operador)
    {
        if (operador == '+' || operador == '-')
            return 1;
        if (operador == '*' || operador == '/')
            return 2;
        if (operador == '%')
            return 3;
        return 0;
    }

    private NodoAritmetico ConstruirArbolPostfijo(string postfija)
    {
        Stack<NodoAritmetico> pila = new Stack<NodoAritmetico>();
        string[] tokens = postfija.Split(' ');

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

        if (double.TryParse(token, out double num))
        {
            pila.Push(new NodoAritmetico(num));
        }
        else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
        {
            if (token == "%")
            {
                NodoAritmetico operando = pila.Pop();
                NodoAritmetico operador = new NodoAritmetico(token)
                {
                    Izquierda = operando
                };
                pila.Push(operador);
            }
            else
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
