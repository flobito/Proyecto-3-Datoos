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

    public NodoAritmetico(string operador)
    {
        Operador = operador;
        Operando = null;
    }

    public NodoAritmetico(double operando)
    {
        Operador = null;
        Operando = operando;
    }

    public double Evaluar()
    {
        if (Operando.HasValue)
        {
            return Operando.Value;
        }

        return Operador switch
        {
            "+" => Izquierda.Evaluar() + Derecha.Evaluar(),
            "-" => Izquierda.Evaluar() - Derecha.Evaluar(),
            "*" => Izquierda.Evaluar() * Derecha.Evaluar(),
            "/" => Izquierda.Evaluar() / Derecha.Evaluar(),
            "%" => Izquierda.Evaluar() / 100,
            _ => throw new InvalidOperationException("Operador no valido")
        };
    }
}

public class NodoLogico
{
    public string Operador;
    public int? Operando;
    public NodoLogico Izquierda;
    public NodoLogico Derecha;

    public NodoLogico(string operador)
    {
        Operador = operador;
        Operando = null;
    }

    public NodoLogico(int operando)
    {
        Operador = null;
        Operando = operando;
    }

    public int Evaluar()
    {
        if (Operando.HasValue)
        {
            return Operando.Value;
        }

        return Operador switch
        {
            "AND" => Izquierda.Evaluar() & Derecha.Evaluar(),
            "OR" => Izquierda.Evaluar() | Derecha.Evaluar(),
            "XOR" => Izquierda.Evaluar() ^ Derecha.Evaluar(),
            "NOT" => ~Izquierda.Evaluar() & 1,
            _ => throw new InvalidOperationException("Operador no válido.")
        };
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
            int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
            string expresion = Encoding.ASCII.GetString(buffer, 0, bytesLeidos);

            Console.WriteLine("Expresión recibida: " + expresion);

            string respuesta;

            try
            {
                if (EsExpresionLogica(expresion))
                {
                    NodoLogico arbol = ConstruirArbolLogico(expresion);
                    int resultado = arbol.Evaluar();
                    respuesta = resultado.ToString();
                }
                else
                {
                    string postfija = ConvertirPostfija(expresion);
                    NodoAritmetico arbol = ConstruirArbolPostfijo(postfija);
                    double resultado = arbol.Evaluar();
                    respuesta = resultado.ToString();
                }
            }
            catch (Exception ex)
            {
                respuesta = "Error: " + ex.Message;
            }

            byte[] respuestaBytes = Encoding.ASCII.GetBytes(respuesta);
            stream.Write(respuestaBytes, 0, respuestaBytes.Length);
            cliente.Close();
        }
    }

    private bool EsExpresionLogica(string expresion)
    {
        return expresion.Contains("AND") || expresion.Contains("OR") || expresion.Contains("XOR") || expresion.Contains("NOT");
    }

    private NodoLogico ConstruirArbolLogico(string expresion)
    {
        string[] tokens = expresion.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Stack<NodoLogico> operandos = new Stack<NodoLogico>();
        Stack<string> operadores = new Stack<string>();

        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int valor))
            {
                if (valor != 0 && valor != 1)
                    throw new ArgumentException("Solo se permiten valores binarios (0 o 1).");
                operandos.Push(new NodoLogico(valor));
            }
            else if (EsOperadorLogico(token))
            {
                while (operadores.Count > 0 && PrioridadLogica(operadores.Peek()) >= PrioridadLogica(token))
                {
                    CrearSubArbolLogico(operandos, operadores.Pop());
                }
                operadores.Push(token);
            }
        }

        while (operadores.Count > 0)
        {
            CrearSubArbolLogico(operandos, operadores.Pop());
        }

        if (operandos.Count != 1)
            throw new InvalidOperationException("La expresión lógica no es válida.");

        return operandos.Pop();
    }

    private bool EsOperadorLogico(string token)
    {
        return token == "AND" || token == "OR" || token == "XOR" || token == "NOT";
    }

    private int PrioridadLogica(string operador)
    {
        return operador switch
        {
            "NOT" => 3,
            "AND" => 2,
            "OR" => 1,
            "XOR" => 1,
            _ => 0
        };
    }

    private void CrearSubArbolLogico(Stack<NodoLogico> operandos, string operador)
    {
        NodoLogico nodo = new NodoLogico(operador);

        if (operador == "NOT")
        {
            if (operandos.Count < 1)
                throw new InvalidOperationException("Operación NOT requiere un operando.");
            nodo.Izquierda = operandos.Pop();
        }
        else
        {
            if (operandos.Count < 2)
                throw new InvalidOperationException($"Operación {operador} requiere dos operandos.");
            nodo.Derecha = operandos.Pop();
            nodo.Izquierda = operandos.Pop();
        }

        operandos.Push(nodo);
    }

    private string ConvertirPostfija(string infija)
    {
        Stack<char> pila = new Stack<char>();
        List<string> resultado = new List<string>();

        foreach (char token in infija.Replace(" ", ""))
        {
            if (char.IsDigit(token))
            {
                resultado.Add(token.ToString());
            }
            else if ("+-*/%".Contains(token))
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
        }

        while (pila.Count > 0)
        {
            resultado.Add(pila.Pop().ToString());
        }

        return string.Join(" ", resultado);
    }

    private int Precedencia(char operador)
    {
        return operador switch
        {
            '+' or '-' => 1,
            '*' or '/' => 2,
            '%' => 3,
            _ => 0
        };
    }

    private NodoAritmetico ConstruirArbolPostfijo(string postfija)
    {
        Stack<NodoAritmetico> pila = new Stack<NodoAritmetico>();
        string[] tokens = postfija.Split(' ');

        foreach (string token in tokens)
        {
            if (double.TryParse(token, out double num))
            {
                pila.Push(new NodoAritmetico(num));
            }
            else if ("+-*/%".Contains(token))
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

        return pila.Pop();
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
