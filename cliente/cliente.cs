using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Algebra
{
    static class Programa
    {
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
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            SendExpression(inputTextBox.Text, resultLabel);
        }

        private void LogicSendButton_Click(object sender, EventArgs e)
        {
            SendExpression(logicInputTextBox.Text, logicResultLabel);
        }

        private void SendExpression(string expression, Label resultLabel)
        {
            string serverIp = "127.0.0.1"; // Dirección IP del servidor
            int port = 12345; // Puerto del servidor

            try
            {
                // Conectarse al servidor
                TcpClient client = new TcpClient(serverIp, port);
                NetworkStream stream = client.GetStream();

                // Enviar la expresión al servidor
                byte[] messageBytes = Encoding.UTF8.GetBytes(expression);
                stream.Write(messageBytes, 0, messageBytes.Length);

                // Leer la respuesta del servidor
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Mostrar el resultado
                resultLabel.Text = "Resultado: " + response;

                // Cerrar la conexión
                client.Close();
            }
            catch (Exception)
            {
                // Mostrar errores en la etiqueta
                resultLabel.Text = "No se pudo encontrar el servidor";
            }
        }
    }
}
