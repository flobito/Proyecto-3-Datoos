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

        public MainForma()
        {
            // Configuración básica de la ventana
            this.Text = "Calculadora";
            this.Width = 400;
            this.Height = 200;

            // Campo de texto para ingresar la expresión
            inputTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 250
            };
            this.Controls.Add(inputTextBox);

            // Botón para enviar la expresión
            sendButton = new Button
            {
                Text = "Calc",
                Location = new System.Drawing.Point(280, 18),
                Width = 80
            };
            sendButton.Click += SendButton_Click;
            this.Controls.Add(sendButton);

            // Etiqueta para mostrar el resultado
            resultLabel = new Label
            {
                Text = "Resultado: ",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };
            this.Controls.Add(resultLabel);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string serverIp = "127.0.0.1"; // Dirección IP del servidor
            int port = 12345; // Puerto del servidor
            string expression = inputTextBox.Text;

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
            catch (Exception ex)
            {
                // Mostrar errores en la etiqueta
                resultLabel.Text = "No se pudo encontrar el servidor";
            }
        }
    }
}
