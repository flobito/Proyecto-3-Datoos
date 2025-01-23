using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NodoAritmetico
{
    public string Operador;
    public double? Operando;
    public NodoAritmetico Izquierda;
    public NodoAritmetico Derecha;

    public NodoAritmetico(string operador)
    {
        Operador = operador;
        Operando = null;
        Izquierda = null;
        Derecha = null;
    }

    public NodoAritmetico(double operando)
    {
        Operador = null;
        Operando = operando;
        Izquierda = null;
        Derecha = null;
    }

    public double Evaluar()
    {
        if (Operando.HasValue)
        {
            return Operando.Value;
        }

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
                return Izquierda.Evaluar() * 0.01;
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
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                byte[] respuesta = Encoding.ASCII.GetBytes("Error");
                stream.Write(respuesta, 0, respuesta.Length);
            }

            cliente.Close();
        }
    }

    private string ConvertirPostfija(string infija)
    {
        List<string> resultado = new List<string>();
        Stack<char> pila = new Stack<char>();
        ProcesarInfijaRecursivo(infija, 0, pila, resultado);
        ProcesarRestantesRecursivo(pila, resultado);
        return string.Join(" ", resultado);
    }

    private void ProcesarInfijaRecursivo(string infija, int indice, Stack<char> pila, List<string> resultado)
    {
        if (indice >= infija.Length)
            return;

        char token = infija[indice];

        if (char.IsDigit(token))
        {
            string numero = ProcesarNumeroRecursivo(infija, indice, out int nuevoIndice);
            resultado.Add(numero);
            ProcesarInfijaRecursivo(infija, nuevoIndice + 1, pila, resultado);
        }
        else if (token == '+' || token == '-' || token == '*' || token == '/' || token == '%')
        {
            ProcesarOperadorRecursivo(pila, resultado, token);
            pila.Push(token);
            ProcesarInfijaRecursivo(infija, indice + 1, pila, resultado);
        }
        else if (token == '(')
        {
            pila.Push(token);
            ProcesarInfijaRecursivo(infija, indice + 1, pila, resultado);
        }
        else if (token == ')')
        {
            ProcesarCierreParentesisRecursivo(pila, resultado);
            ProcesarInfijaRecursivo(infija, indice + 1, pila, resultado);
        }
        else
        {
            ProcesarInfijaRecursivo(infija, indice + 1, pila, resultado);
        }
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
        if (double.TryParse(token, out double num))
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
