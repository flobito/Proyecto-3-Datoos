using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Algebra
{
    static class Programa
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForma());
        }
    }

    public class MainForma : Form
    {
        private TextBox inputTextBox;
        private Button sendButton;
        private Label resultLabel;

        private TextBox logicInputTextBox;
        private Button logicSendButton;
        private Label logicResultLabel;

        private Button newCalcButton;
        private Button viewLogButton;  // Nuevo botón para ver el registro

        public static string CsvPath = "registro.csv"; // Cambio a público

        public MainForma()
        {
            // Configuración básica de la ventana
            this.Text = "Calculadora";
            this.Width = 400;
            this.Height = 300;

            // Campo de texto para ingresar la expresión matemática
            inputTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 250
            };
            this.Controls.Add(inputTextBox);

            // Botón para enviar la expresión matemática
            sendButton = new Button
            {
                Text = "Calc",
                Location = new System.Drawing.Point(280, 18),
                Width = 80
            };
            sendButton.Click += SendButton_Click;
            this.Controls.Add(sendButton);

            // Etiqueta para mostrar el resultado matemático
            resultLabel = new Label
            {
                Text = "Resultado: ",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };
            this.Controls.Add(resultLabel);

            // Campo de texto para ingresar la expresión lógica
            logicInputTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 120),
                Width = 250
            };
            this.Controls.Add(logicInputTextBox);

            // Botón para enviar la expresión lógica
            logicSendButton = new Button
            {
                Text = "Logic",
                Location = new System.Drawing.Point(280, 118),
                Width = 80
            };
            logicSendButton.Click += LogicSendButton_Click;
            this.Controls.Add(logicSendButton);

            // Etiqueta para mostrar el resultado lógico
            logicResultLabel = new Label
            {
                Text = "Resultado lógico: ",
                Location = new System.Drawing.Point(20, 160),
                AutoSize = true
            };
            this.Controls.Add(logicResultLabel);

            // Botón para crear una nueva calculadora
            newCalcButton = new Button
            {
                Text = "Nueva Calculadora",
                Location = new System.Drawing.Point(20, 200),
                Width = 150
            };
            newCalcButton.Click += NewCalcButton_Click;
            this.Controls.Add(newCalcButton);

            // Botón para ver el registro
            viewLogButton = new Button
            {
                Text = "Ver Registro",
                Location = new System.Drawing.Point(200, 200),
                Width = 150
            };
            viewLogButton.Click += ViewLogButton_Click;
            this.Controls.Add(viewLogButton);

            // Crear archivo CSV vacío al iniciar
            File.WriteAllText(CsvPath, "Expresion,Resultado,Fecha y Hora\n");
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            ProcesarExpresion(inputTextBox.Text, resultLabel);
        }

        private void LogicSendButton_Click(object sender, EventArgs e)
        {
            ProcesarExpresion(logicInputTextBox.Text, logicResultLabel);
        }

        private void NewCalcButton_Click(object sender, EventArgs e)
        {
            MainForma nuevaCalculadora = new MainForma();
            nuevaCalculadora.Show();
        }

        private void ViewLogButton_Click(object sender, EventArgs e)
        {
            // Crear e iniciar una nueva ventana con los registros
            LogForm logForm = new LogForm();
            logForm.Show();
        }

        private void ProcesarExpresion(string expresion, Label etiquetaResultado)
        {
            string serverIp = "127.0.0.1"; // Dirección IP del servidor
            int port = 12345; // Puerto del servidor

            try
            {
                // Conectarse al servidor
                TcpClient client = new TcpClient(serverIp, port);
                NetworkStream stream = client.GetStream();

                // Enviar la expresión al servidor
                byte[] messageBytes = Encoding.UTF8.GetBytes(expresion);
                stream.Write(messageBytes, 0, messageBytes.Length);

                // Leer la respuesta del servidor
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Mostrar el resultado
                etiquetaResultado.Text = "Resultado: " + response;

                // Guardar en el archivo CSV
                RegistrarEnCsv(expresion, response);

                // Cerrar la conexión
                client.Close();
            }
            catch (Exception)
            {
                // Mostrar errores en la etiqueta
                etiquetaResultado.Text = "No se pudo encontrar el servidor";
            }
        }

        private void RegistrarEnCsv(string expresion, string resultado)
        {
            string fechaHora = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string registro = $"{expresion},{resultado},{fechaHora}\n";
            File.AppendAllText(CsvPath, registro);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // Eliminar o vaciar el archivo CSV al cerrar la aplicación
            if (File.Exists(CsvPath))
            {
                File.Delete(CsvPath);
            }
        }
    }

    // Nuevo formulario para mostrar los registros
    public class LogForm : Form
    {
        private TextBox logTextBox;

        public LogForm()
        {
            this.Text = "Registro de Cálculos";
            this.Width = 600;
            this.Height = 400;

            logTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new System.Drawing.Point(20, 20),
                Width = 540,
                Height = 300
            };
            this.Controls.Add(logTextBox);

            // Cargar el registro desde el archivo CSV
            CargarRegistro();
        }

        private void CargarRegistro()
        {
            try
            {
                if (File.Exists(MainForma.CsvPath)) // Ahora CsvPath es accesible
                {
                    string[] registros = File.ReadAllLines(MainForma.CsvPath);
                    foreach (string registro in registros)
                    {
                        logTextBox.AppendText(registro + Environment.NewLine);
                    }
                }
                else
                {
                    logTextBox.AppendText("No hay registros disponibles.");
                }
            }
            catch (Exception ex)
            {
                logTextBox.AppendText("Error al cargar el registro: " + ex.Message);
            }
        }
    }
}
