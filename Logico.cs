using System;
using System.Windows.Forms;

namespace OperacionesLogicas
{
    public class NodoLogico
    {
        public string Operador;  // Puede ser un operador lógico ('AND', 'OR', 'XOR', 'NOT')
        public int? Operando;    // Puede ser 0 o 1 para los nodos hoja
        public NodoLogico Izquierda;
        public NodoLogico Derecha;

        // Constructor para nodos operadores (internos)
        public NodoLogico(string operador)
        {
            Operador = operador;
            Operando = null;
            Izquierda = null;
            Derecha = null;
        }

        // Constructor para nodos hoja (valores 0 o 1)
        public NodoLogico(int operando)
        {
            Operador = null;
            Operando = operando;
            Izquierda = null;
            Derecha = null;
        }

        // Método para evaluar la expresión lógica
        public int Evaluar()
        {
            if (Operando.HasValue)  // Si es una hoja, devolver su valor (0 o 1)
            {
                return Operando.Value;
            }

            // Evaluar según el operador
            switch (Operador)
            {
                case "AND":
                    return Izquierda.Evaluar() & Derecha.Evaluar();  // Operación AND
                case "OR":
                    return Izquierda.Evaluar() | Derecha.Evaluar();  // Operación OR
                case "XOR":
                    return Izquierda.Evaluar() ^ Derecha.Evaluar();  // Operación XOR
                case "NOT":
                    return ~Izquierda.Evaluar() & 1;  // Operación NOT (negación bit a bit, limitando a un valor de 0 o 1)
                default:
                    throw new InvalidOperationException("Operador no reconocido");
            }
        }
    }

    public class MainForm : Form
    {
        private Label lblInput1;
        private Label lblInput2;
        private Label lblResult;
        private TextBox txtInput1;
        private TextBox txtInput2;
        private ComboBox cmbOperation;
        private Button btnCalculate;
        private TextBox txtResult;

        public MainForm()
        {
            // Configuración básica de la ventana
            this.Text = "Operaciones Lógicas";
            this.Size = new System.Drawing.Size(400, 300);

            // Etiqueta para el primer valor
            lblInput1 = new Label { Text = "Valor 1 (0 o 1):", Location = new System.Drawing.Point(20, 20) };
            this.Controls.Add(lblInput1);

            // Caja de texto para el primer valor
            txtInput1 = new TextBox { Location = new System.Drawing.Point(150, 20), Width = 50 };
            this.Controls.Add(txtInput1);

            // Etiqueta para el segundo valor
            lblInput2 = new Label { Text = "Valor 2 (0 o 1):", Location = new System.Drawing.Point(20, 60) };
            this.Controls.Add(lblInput2);

            // Caja de texto para el segundo valor
            txtInput2 = new TextBox { Location = new System.Drawing.Point(150, 60), Width = 50 };
            this.Controls.Add(txtInput2);

            // ComboBox para seleccionar la operación lógica
            cmbOperation = new ComboBox
            {
                Location = new System.Drawing.Point(20, 100),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOperation.Items.AddRange(new string[] { "OR", "AND", "NOT", "XOR" });
            this.Controls.Add(cmbOperation);

            // Botón para calcular
            btnCalculate = new Button { Text = "Calcular", Location = new System.Drawing.Point(20, 140) };
            btnCalculate.Click += BtnCalculate_Click;
            this.Controls.Add(btnCalculate);

            // Etiqueta para el resultado
            lblResult = new Label { Text = "Resultado:", Location = new System.Drawing.Point(20, 180) };
            this.Controls.Add(lblResult);

            // Caja de texto para mostrar el resultado
            txtResult = new TextBox { Location = new System.Drawing.Point(150, 180), Width = 100, ReadOnly = true };
            this.Controls.Add(txtResult);
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                int value1 = int.Parse(txtInput1.Text);
                int value2 = cmbOperation.SelectedItem.ToString() != "NOT" ? int.Parse(txtInput2.Text) : 0;
                string operation = cmbOperation.SelectedItem.ToString();

                if ((value1 != 0 && value1 != 1) || (value2 != 0 && value2 != 1 && operation != "NOT"))
                {
                    MessageBox.Show("Por favor, ingrese solo valores binarios (0 o 1).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Crear el árbol lógico dependiendo de la operación seleccionada
                NodoLogico nodo1 = new NodoLogico(value1);
                NodoLogico nodo2 = operation != "NOT" ? new NodoLogico(value2) : null;
                NodoLogico raiz;

                if (operation == "NOT")
                {
                    raiz = new NodoLogico("NOT");
                    raiz.Izquierda = nodo1;
                }
                else
                {
                    raiz = new NodoLogico(operation);
                    raiz.Izquierda = nodo1;
                    raiz.Derecha = nodo2;
                }

                // Evaluar el árbol lógico
                int result = raiz.Evaluar();
                txtResult.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
