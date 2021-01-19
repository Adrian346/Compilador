using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices.WindowsRuntime;


namespace IDE
{
    public partial class Form1 : Form
    {
        int numeroTab = 1;
        String[] tokens;
        String[] tipo_token;
        String[] numeroLinea;
        int contadorLineas = 0;
        string numeroLineas2 = "";
        String[] errores_sintacticos = new string[20000];
        static int num_errores_sintacticos = 0;
        nodo raizSintactica, raizSemantica;
        String archivo;
        static String salida = "";
        static String errores = "";
        static String erroresSemanticos = "";
        string[] Reservadas = new string[] { "main", "if", "then", "else", "end", "do", "while", "repeat", "until", "cin", "cout", "float", "int", "bool", "real" };
        int posicionActual = 0;
        public Form1()
        {
            InitializeComponent();
            {
                ejecucion();

                this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;

                this.richTextBox1.TextChanged += (ob, ev) =>
                {
                    evitarParpadeo.Focus();
                    posicionActual = richTextBox1.SelectionStart;
                    ejecucion();
                };

            }
        }
        private void ejecucion()
        {
            //Dictionary<int, comentarios> diccionarioCierres = new Dictionary<int, comentarios>();
            Dictionary<int, int> diccionarioAperturas = new Dictionary<int, int>();
            if (!string.IsNullOrEmpty(richTextBox1.Text.ToString()))
            {
                //auxiliar para hacer todo ahi y no se vea parpadeo
                var richTextBoxAux = new RichTextBox();
                richTextBoxAux = richTextBox1;
                richTextBoxAux.SelectAll();
                richTextBoxAux.SelectionColor = Color.Black;
                richTextBoxAux.Select(richTextBoxAux.TextLength, 0);
                //el texto a buscar es lo que contiene el richtext
                String textoBuscador = richTextBoxAux.Text;
                string find = "";
                string find2 = "";
                int primerPintada = 0;
                foreach (string word in Reservadas)
                {
                    find = word;
                    //si el richtext contiene alguna palabra reservada
                    if (richTextBoxAux.Text.Contains(find))
                    {
                        //
                        var matchString = Regex.Escape(find);
                        //match regresa todas las posiciones iniciales donde se haya encontrado alguna palabra ya encontrada en reserv
                        foreach (Match match in Regex.Matches(richTextBoxAux.Text, matchString))
                        {
                            //si donde se encontro no es la primer posicion
                            if (match.Index > 0)
                            {
                                //y el index de donde esta mas el tamaño de la reservada es menor al campototal
                                if (match.Index + find.Length < textoBuscador.Length)
                                {
                                    //y en la posicion anterior no es letra o digito o la sigiente
                                    if (!Char.IsLetterOrDigit(textoBuscador[match.Index - 1]) && !Char.IsLetterOrDigit(textoBuscador[match.Index + find.Length]))
                                    /*if ((textoBuscador[match.Index - 1] == '\n' || textoBuscador[match.Index - 1] == ' ' || textoBuscador[match.Index - 1] == '{' || textoBuscador[match.Index - 1] == '}'
                                        || textoBuscador[match.Index - 1] == ';' || textoBuscador[match.Index - 1] == ')') && (textoBuscador[match.Index + find.Length] == ' '
                                        || textoBuscador[match.Index + find.Length] == '(' || textoBuscador[match.Index + find.Length] == '{' || textoBuscador[match.Index + find.Length] == '\n'
                                        || textoBuscador[match.Index + find.Length] == '\0' || textoBuscador[match.Index + find.Length] == ';'))*/
                                    {
                                        richTextBoxAux.Select(match.Index, find.Length);
                                        richTextBoxAux.SelectionColor = Color.Blue;
                                        richTextBoxAux.Select(richTextBoxAux.TextLength, 0);
                                        richTextBoxAux.SelectionColor = richTextBoxAux.ForeColor;
                                    }
                                }

                            }
                            if (match.Index + find.Length == textoBuscador.Length && match.Index > 0)
                            {
                                if (find.Length == textoBuscador.Length && !Char.IsLetterOrDigit(textoBuscador[match.Index + find.Length]))
                                /*if (find.Length == textoBuscador.Length && (textoBuscador[match.Index + find.Length] == ' '
                                    || textoBuscador[match.Index + find.Length] == '(' || textoBuscador[match.Index + find.Length] == '{'
                                    || textoBuscador[match.Index + find.Length] == '\n' || textoBuscador[match.Index + find.Length] == '\0'
                                    || textoBuscador[match.Index + find.Length] == ';'))*/
                                {
                                    primerPintada = 1;
                                    richTextBoxAux.Select(match.Index, find.Length);
                                    richTextBoxAux.SelectionColor = Color.Blue;
                                    richTextBoxAux.Select(richTextBoxAux.TextLength, 0);
                                    richTextBoxAux.SelectionColor = richTextBoxAux.ForeColor;
                                }
                            }
                            else if (match.Index + find.Length < textoBuscador.Length) //&& primerPintada == 0)
                            {
                                if (find.Length < textoBuscador.Length && !Char.IsLetterOrDigit(textoBuscador[match.Index + find.Length]))
                                /*if (find.Length < textoBuscador.Length && (textoBuscador[match.Index + find.Length] == ' '
                                    || textoBuscador[match.Index + find.Length] == '(' || textoBuscador[match.Index + find.Length] == '{'
                                    || textoBuscador[match.Index + find.Length] == '\n' || textoBuscador[match.Index + find.Length] == '\0'
                                    || textoBuscador[match.Index + find.Length] == ';'))*/
                                {
                                    primerPintada = 1;
                                    richTextBoxAux.Select(match.Index, find.Length);
                                    richTextBoxAux.SelectionColor = Color.Blue;
                                    richTextBoxAux.Select(richTextBoxAux.TextLength, 0);
                                    richTextBoxAux.SelectionColor = richTextBoxAux.ForeColor;
                                }
                            }
                        };
                    }
                }


                // una linea

                find = "//";

                //si contiene algun // Error /*//
                if (richTextBoxAux.Text.Contains(find))
                {
                    var matchString = Regex.Escape(find);
                    //regresa cada posicion inicial de donde se encontro
                    foreach (Match match in Regex.Matches(richTextBoxAux.Text, matchString))
                    {
                        bool banderaSimple = true;
                        if (banderaSimple == true)
                        {
                            string texto = richTextBoxAux.Text;

                            //minimo se pintaran 2 caracteres del //
                            int busca = 2;
                            for (int i = match.Index + 1; texto[i] != '\n' && texto[i] != '\0' && i < texto.Length - 1; i++)
                            {
                                busca++;
                            }
                            richTextBoxAux.Select(match.Index, busca);
                            richTextBoxAux.SelectionColor = Color.Green;
                            richTextBoxAux.Select(posicionActual, 0);

                        }
                    };
                }


                //Multiples Lineas

                find = "/*";

                //si contiene algun /* 
                if (richTextBoxAux.Text.Contains(find))
                {
                    var matchString = Regex.Escape(find);
                    //regresa cada posicion inicial de donde se encontro
                    foreach (Match match in Regex.Matches(richTextBoxAux.Text, matchString))
                    {
                        bool banderaSimple = true;
                        int comentarioLinea = 0;
                        if (banderaSimple == true)
                        {
                            string texto = richTextBoxAux.Text;
                            int primerCaracter = richTextBox1.GetFirstCharIndexFromLine(richTextBox1.GetLineFromCharIndex(match.Index));
                            for (int j = primerCaracter; j < match.Index + 1; j++)
                            {
                                if (j > primerCaracter && texto[j] == '/' && texto[j - 1] == '/')
                                {
                                    comentarioLinea = 1;
                                }
                            }
                            if (comentarioLinea == 0)
                            {
                                //minimo se pintaran 2 caracteres del /*
                                int busca = 2;
                                for (int i = match.Index + 1; i < texto.Length - 1; i++)
                                {
                                    if (texto[i - 1] == '*' && texto[i] == '/' && i > match.Index + 2)
                                    {
                                        break;
                                    }
                                    busca++;
                                }
                                richTextBoxAux.Select(match.Index, busca);
                                richTextBoxAux.SelectionColor = Color.Green;
                                richTextBoxAux.Select(posicionActual, 0);

                            }
                        }
                    };
                }



                richTextBox1 = richTextBoxAux;

                if (posicionActual >= 0)
                {
                    richTextBox1.Select(posicionActual, 0);
                }
                richTextBox1.Focus();
            }

        }
        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
            Form1.ActiveForm.Text = archivo + "IDE";
        }
        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            abrirArchivo();
        }
        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveFile = new SaveFileDialog();
            SaveFile.Filter = "Texto|*.txt";
            if (archivo != null)
            {
                using (StreamWriter sw = new StreamWriter(archivo))
                {
                    sw.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (SaveFile.ShowDialog() == DialogResult.OK)
                {
                    archivo = SaveFile.FileName;
                    using (StreamWriter sw = new StreamWriter(SaveFile.FileName))
                    {
                        sw.Write(richTextBox1.Text);
                    }
                }
            }
        }
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }


        [DllImport("User32.dll")]
        public extern static int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("User32.dll")]
        public extern static int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public enum ScrollBarType : int
        {
            SbHorz = 0,
            SbVert = 1,
            SbCtl = 2,
            SbBoth = 3
        }

        public enum Message : int
        {
            WM_VSCROLL = 0x0115
        }

        public enum ScrollBarCommands : int
        {
            SB_THUMBPOSITION = 4
        }



        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Back || (int)e.KeyChar == (int)Keys.Enter)
            {
                NumeroDeLineas();
            }
            AjustarScroll();

            //AjustarScroll2();
        }



        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            LineaCaracter();
        }

        private void richTextBox1_VScroll(object sender, EventArgs e)
        {
            AjustarScroll();
        }
        public void LineaCaracter()
        {
            int PosicionColumna = 0;
            PosicionColumna = (richTextBox1.SelectionStart - richTextBox1.GetFirstCharIndexOfCurrentLine()) + 1;

            label2.Text = "Línea: " + (richTextBox1.GetLineFromCharIndex(richTextBox1.GetFirstCharIndexOfCurrentLine()) + 1).ToString() + "        Carácter: " + PosicionColumna.ToString();
        }

        public void AjustarScroll()
        {
            LineaCaracter();
            int nPos = GetScrollPos(richTextBox1.Handle, (int)ScrollBarType.SbVert);
            nPos <<= 16;
            int wParam = (int)ScrollBarCommands.SB_THUMBPOSITION | (int)nPos;
            SendMessage(richTextBox2.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));
        }

        public void NumeroDeLineasAbrirArchivo()
        {
            int numeroLinea = richTextBox1.Lines.Count();
            richTextBox2.Text = "1\n";

            for (int i = 1; i <= numeroLinea; i++)
            {

                if (!richTextBox2.Text.Contains(i.ToString()))
                {
                    richTextBox2.Text += i.ToString() + "\n";
                }
                else
                {
                    richTextBox2.Text = i.ToString() + "\n";
                }
            }

        }

        public void NumeroDeLineas()
        {
            int numeroLinea = richTextBox1.Lines.Count();
            if (numeroLinea == 0)
            {
                richTextBox2.Text = "1\n";
            }
            if (numeroLinea > 0)
            {
                if (!richTextBox2.Text.Contains(numeroLinea.ToString()))
                {
                    richTextBox2.Text += numeroLinea.ToString() + "\n";
                }
                else
                {
                    for (int i = 1; i <= numeroLinea; i++)
                    {
                        if (!richTextBox2.Text.Contains(i.ToString()))
                        {
                            richTextBox2.Text += i.ToString() + "\n";
                        }
                        /*else
                        {
                            if(numeroLinea <= 16)
                            {
                                richTextBox2.Text = i.ToString() + "\n"; 
                            }
                        }*/
                    }
                }
            }
        }

        private void guardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveFile = new SaveFileDialog();
            SaveFile.Filter = "Texto|*.txt";
            if (SaveFile.ShowDialog() == DialogResult.OK)
            {
                archivo = SaveFile.FileName;
                using (StreamWriter sw = new StreamWriter(SaveFile.FileName))
                {
                    sw.Write(richTextBox1.Text);
                }
            }

        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            archivo = null;
            Form1.ActiveForm.Text = archivo + "IDE";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            abrirArchivo();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveFile = new SaveFileDialog();
            SaveFile.Filter = "Texto|*.txt";
            if (archivo != null)
            {
                using (StreamWriter sw = new StreamWriter(archivo))
                {
                    sw.Write(richTextBox1.Text);
                }
            }
            else
            {
                if (SaveFile.ShowDialog() == DialogResult.OK)
                {
                    archivo = SaveFile.FileName;
                    using (StreamWriter sw = new StreamWriter(SaveFile.FileName))
                    {
                        sw.Write(richTextBox1.Text);
                    }
                }
            }
        }

        private void cerrarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        public void AjustarScroll2()
        {
            LineaCaracter();
            int nPos = GetScrollPos(richTextBox2.Handle, (int)ScrollBarType.SbVert);
            nPos <<= 16;
            int wParam = (int)ScrollBarCommands.SB_THUMBPOSITION | (int)nPos;
            SendMessage(richTextBox1.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));

        }

        public void abrirArchivo()
        {
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "Texto|*.txt";
            if (OpenFile.ShowDialog() == DialogResult.OK)
            {
                archivo = OpenFile.FileName;
                using (StreamReader sr = new StreamReader(archivo))
                {
                    richTextBox1.Text = sr.ReadToEnd();
                }
                Form1.ActiveForm.Text = archivo + " | IDE";
            }
            NumeroDeLineasAbrirArchivo();
            AjustarScroll();
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            LineaCaracter();
            if ((int)e.KeyCode == (int)Keys.Delete)
            {
                NumeroDeLineas();
                AjustarScroll();
                AjustarScroll2();
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //            Lexico(richTextBox1.Text);
        }

        private void Lexico(string texto)
        {
            salida = "";
            errores = "";
            string line = "";
            String concat = "";
            String token = "";
            int i;
            int tamano;
            int estado = 0;
            numeroLineas2 = "";
            contadorLineas = 0;
            System.IO.StreamWriter File = new System.IO.StreamWriter(@"C:\Users\adria\OneDrive\Documentos\compilador\entrada.txt"); // aqui guarda el resultado en un txt 
            File.Write(texto);
            File.Close();
            /*String ruta = "";
            try
            {
                ruta = args[0];
                System.Console.WriteLine(ruta); //aqui pon el archivo que se va a analizar 
            }
            catch
            {

            }*/
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\adria\OneDrive\Documentos\compilador\entrada.txt");

            while ((line = file.ReadLine()) != null)
            {
                concat += line + "\n";
            }
            tamano = concat.Length;
            file.Close();
            char[] cadena = concat.ToCharArray();
            char caracter = ' ';
            int en_comentario = 0;

            for (i = 0; i < cadena.Length; i++)
            {
                caracter = cadena[i];

                switch (estado)
                {
                    case 0:

                        if (Char.IsNumber(caracter))
                        {
                            estado = 7;
                            token = caracter.ToString();
                        }
                        else if (Char.IsLetter(caracter))
                        {
                            estado = 1;
                            token = caracter.ToString();
                        }
                        else if (char.IsWhiteSpace(caracter))
                        {
                            estado = 0;
                        }
                        else
                        {
                            int linea_error = richTextBox1.GetLineFromCharIndex(i) + 1;
                            if (caracter == '%' || caracter == '(' || caracter == '{' || caracter == ';' || caracter == ',')
                            {
                                errores_sintacticos[num_errores_sintacticos] = " en la linea: " + (linea_error) + " despues de [ " + caracter + " ]\n";
                                num_errores_sintacticos++;
                            }
                            else if (caracter == ')' || caracter == '}')
                            {
                                errores_sintacticos[num_errores_sintacticos] = " en la linea: " + (linea_error) + " antes de [ " + caracter + " ]\n";
                                num_errores_sintacticos++;
                            }
                            if(caracter == '%' || caracter == '(' || caracter ==')' || caracter == '{' || caracter == '}' || caracter == ';' || caracter == ',')
                            {
                                numeroLineas2 += (richTextBox1.GetLineFromCharIndex(i) + 1) + "\n";
                                contadorLineas++;
                            }    
                            switch (caracter)
                            {
                                case ' ':
                                    estado = 0;
                                    break;
                                case '<':
                                    token = caracter.ToString();
                                    estado = 2;
                                    break;
                                case '>':
                                    token = caracter.ToString();
                                    estado = 3;
                                    break;
                                case ':':
                                    token = caracter.ToString();
                                    estado = 4;
                                    break;
                                case '=':
                                    token = caracter.ToString();
                                    estado = 5;
                                    break;
                                case '!':
                                    token = caracter.ToString();
                                    estado = 6;
                                    break;
                                case '+':
                                    token = caracter.ToString();
                                    estado = 9;
                                    break;
                                case '-':
                                    token = caracter.ToString();
                                    estado = 10;
                                    break;
                                case '/':
                                    estado = 11;
                                    break;
                                case '*':
                                    token = caracter.ToString();
                                    //estado = 15;
                                    estado = 12;
                                    break;
                                case '^':
                                    salida += "Potencia --> [^]\n";
                                    estado = 0;
                                    break;
                                case '\n':
                                    estado = 0;
                                    break;
                                case '%':
                                    salida += "Modulo --> [%]\n";
                                    estado = 0;
                                    break;
                                case '(':
                                    salida += "Parentesis que abre --> [(]\n";
                                    estado = 0;
                                    break;
                                case ')':
                                    salida += "Parentesis que cierra --> [)]\n";
                                    estado = 0;
                                    break;
                                case '{':
                                    salida += "Llave que abre --> [{]\n";
                                    estado = 0;
                                    break;
                                case '}':
                                    salida += "Llave que cierra --> [}]\n";
                                    estado = 0;
                                    break;
                                case ';':
                                    salida += "Delimitador --> [;]\n";
                                    estado = 0;
                                    break;
                                case ',':
                                    salida += "Coma --> [,]\n";
                                    estado = 0;
                                    break;
                                default:
                                    token = caracter.ToString();
                                    int linea = richTextBox1.GetLineFromCharIndex(i) + 1;
                                    int numCaracter = (i - richTextBox1.GetFirstCharIndexFromLine(richTextBox1.GetLineFromCharIndex(i)));
                                    errores += "[Error] Caracter invalido [" + token + "] --> En el caracter: " + (numCaracter + 1) + " linea: " + linea + "\n";
                                    estado = 0;
                                    token = "";
                                    break;
                            }
                        }
                        break;
                    case 1: //-----------------------------------------------------Estado letras-----------------------------
                        if (Char.IsLetter(caracter))
                        {
                            token += caracter.ToString();
                            estado = 1;
                        }
                        else if (char.IsDigit(caracter))
                        {
                            token += caracter.ToString();
                            estado = 1;
                        }
                        else if (caracter == '_')
                        {
                            token += caracter.ToString();
                            estado = 1;
                        }
                        else
                        {
                            separar(estado, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 2: //-----------------------------------------------------Estado menor que-----------------------------
                        if (caracter == '=')
                        {
                            token += caracter.ToString();
                            separar(22, token, i);
                            estado = 0;
                        }
                        else
                        {
                            //se finaliza
                            separar(estado, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 3://-----------------------------------------------------Estado mayor que--------------------------------
                        if (caracter == '=')
                        {
                            token += caracter.ToString();
                            separar(33, token, i);
                            estado = 0;
                        }
                        else
                        {
                            //se finaliza
                            separar(estado, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 4://-----------------------------------------------------Estado dos puntos--------------------------------
                        if (caracter == '=')
                        {
                            token += caracter.ToString();
                            separar(estado, token, i);
                            estado = 0;
                        }
                        else
                        {
                            //aqui va el error perro
                            separar(44, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 5://-----------------------------------------------------Estado igual--------------------------------
                        if (caracter == '=')
                        {
                            token += caracter.ToString();
                            separar(estado, token, i);
                            estado = 0;
                        }
                        else
                        {
                            separar(55, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 6://-----------------------------------------------------Estado diferente de--------------------------------
                        if (caracter == '=')
                        {
                            token += caracter.ToString();
                            separar(estado, token, i);
                            estado = 0;
                        }
                        else
                        {
                            //aqui va el error
                            separar(66, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 7://-----------------------------------------------------Estado numeros--------------------------------
                        if (char.IsDigit(caracter))
                        {
                            token += caracter.ToString();
                            estado = 7;
                        }
                        else if (caracter == '.')
                        {
                            token += caracter.ToString();
                            estado = 88;
                        }
                        else
                        {
                            separar(estado, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 8://-----------------------------------------Estado numeros despues de un punto--------------------------------
                        if (char.IsDigit(caracter))
                        {
                            token += caracter;
                            estado = 8;
                        }
                        else
                        {
                            separar(estado, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 88:
                        if (char.IsDigit(caracter))
                        {
                            token += caracter;
                            estado = 8;
                        }
                        else
                        {
                            separar(88, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 9://-----------------------------------------------------Estado suma--------------------------------
                        if (caracter == '+')
                        {
                            token += caracter.ToString();
                            separar(estado, token, i);
                            estado = 0;
                        }
                        else
                        {
                            separar(99, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 10://-----------------------------------------------------Estado resta--------------------------------
                        if (caracter == '-')
                        {
                            token += caracter.ToString();
                            separar(estado, token, i);
                            estado = 0;
                        }
                        else
                        {
                            separar(100, token, i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                    case 11://-----------------------------------------------------Estado comentarios--------------------------------
                        if (caracter == '/')
                        {
                            estado = 13;
                        }
                        else if (caracter == '*')
                        {
                            estado = 14;
                            en_comentario = 1;
                        }
                        else
                        {
                            //division
                            separar(11, token, i);
                            i--;
                            token = "";
                            estado = 0;
                        }
                        break;
                    case 13:
                        if (caracter == '\n')
                        {
                            estado = 0;
                        }
                        else
                        {
                            estado = 13;
                        }
                        break;
                    case 14:
                        if (caracter == '*')
                        {
                            estado = 12;
                        }
                        else
                        {
                            estado = 14;
                        }
                        break;
                    case 12://------------------------------------------------------Estado * ----------------------------------------
                        if (caracter == '/' && en_comentario == 1)
                        {
                            estado = 0;
                            en_comentario = 0;
                        }
                        else if (en_comentario == 1)
                        {
                            estado = 14;
                        }
                        else
                        {
                            //multiplicacion
                            separar(12, "*", i);
                            estado = 0;
                            i--;
                            token = "";
                        }
                        break;
                }

            }
            richTextBox3.Text = salida;
            richTextBox4.Text = errores;
            System.Console.WriteLine(salida);
            System.IO.StreamWriter File2 = new System.IO.StreamWriter(@"C:\Users\adria\OneDrive\Documentos\compilador\salida.txt"); // aqui guarda el resultado en un txt 
            File2.Write(salida);
            File2.Close();
            System.Console.WriteLine(numeroLineas2);
            System.IO.StreamWriter File3 = new System.IO.StreamWriter(@"C:\Users\adria\OneDrive\Documentos\compilador\lineas.txt"); // aqui guarda el resultado en un txt 
            File3.Write(numeroLineas2);
            File3.Close();
        }


        private void separar(int x, String token, int numCaracter)
        {
            int linea = richTextBox1.GetLineFromCharIndex(numCaracter) + 1;
            int caracter = (numCaracter - richTextBox1.GetFirstCharIndexFromLine(richTextBox1.GetLineFromCharIndex(numCaracter)));
            if (x != 44 && x != 55 && x != 66 && x != 88)
            {
                errores_sintacticos[num_errores_sintacticos] = " en la linea: " + (linea) + " despues de [ " + token + " ]\n";
                num_errores_sintacticos++;
                numeroLineas2 += linea.ToString() + "\n";
                contadorLineas++;
            }
            switch (x)
            {
                case 1:
                    reservadas(token);
                    break;
                case 2:
                    salida += "Menor que --> [" + token + "]\n";
                    break;
                case 22:
                    salida += "Menor o igual que --> [" + token + "]\n";
                    break;
                case 3:
                    salida += "Mayor que --> [" + token + "]\n";
                    break;
                case 33:
                    salida += "Mayor o igual que --> [" + token + "]\n";
                    break;
                case 4:
                    salida += "Asignacion --> [" + token + "]\n";
                    break;
                case 44:
                    errores += "[Error] Se esperaba un [=] --> Despues del caracter:" + caracter + " en la linea: " + (linea - 1) + "\n";
                    break;
                case 5:
                    salida += "Comparacion --> [" + token + "]\n";
                    break;
                case 55:
                    errores += "[Error] Se esperaba un [=] --> Despues del caracter:" + caracter + " en la linea: " + linea + "\n";
                    break;
                case 6:
                    salida += "Diferente de --> [" + token + "]\n";
                    break;
                case 66:
                    errores += "[Error] Se esperaba un [=] --> Despues del caracter:" + caracter + " en la linea: " + linea + "\n";
                    break;
                case 7:
                    salida += "Numero entero --> [" + token + "]\n";
                    break;
                case 8:
                    salida += "Numero flotante --> [" + token + "]\n";
                    break;
                case 88:
                    errores += "[Error] Se esperaba un [numero] --> Despues del punto [" + token + "] en la linea: " + linea + " caracter:" + caracter + "\n";
                    break;
                case 9:
                    salida += "Incremento --> [" + token + "]\n";
                    break;
                case 99:
                    salida += "Suma --> [+]\n";
                    break;
                case 10:
                    salida += "Decremento --> [" + token + "]\n";
                    break;
                case 100:
                    salida += "Resta --> [-]\n";
                    break;
                case 12:
                    salida += "Multiplicacion --> [*]\n";
                    break;
                case 11:
                    salida += "Division --> [/]\n";
                    break;

            }
        }
        public static void reservadas(string token)
        {
            int bandera = 0;

            if (String.Equals(token, "main") || String.Equals(token, "if") || String.Equals(token, "then") || String.Equals(token, "else") || String.Equals(token, "end") ||
               String.Equals(token, "do") || String.Equals(token, "while") || String.Equals(token, "cin") || String.Equals(token, "cout") || String.Equals(token, "float") ||
               String.Equals(token, "int") || String.Equals(token, "bool") || String.Equals(token, "real") || String.Equals(token, "until"))
            {
                bandera = 1;
            }

            if (bandera == 0)
            {
                salida += "Identificador --> [" + token + "]\n";
            }
            else
            {
                salida += "Palabra reservada --> [" + token + "]\n";
            }
        }

        private void asignarTokens()
        {
            String line = "";
            int i = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\adria\OneDrive\Documentos\compilador\salida.txt");
            while ((line = file.ReadLine()) != null)
            {
                i++;
            }
            file.Close();
            tipo_token = new string[i];
            tokens = new string[i];
            i = 0;
            int corchete = 0;
            String aux = "", aux_token = "";
            System.IO.StreamReader file1 = new System.IO.StreamReader(@"C:\Users\adria\OneDrive\Documentos\compilador\salida.txt");
            while ((line = file1.ReadLine()) != null)
            {
                aux = line;
                for (int j = 0; j < aux.Length; j++)
                {
                    if (aux[j + 1] != '-')
                    {
                        aux_token += aux[j];
                    }
                    if (aux[j + 1] == '-' && aux[j] != ' ')
                    {
                        tipo_token[i] = aux_token;
                        aux_token = "";
                        break;
                    }
                }
                for (int j = 0; j < aux.Length; j++)
                {
                    if (corchete == 1 && aux[j] != ']')
                    {
                        aux_token += aux[j];
                    }
                    if (aux[j] == '[')
                    {
                        corchete = 1;
                    }
                    if (aux[j] == ']')
                    {
                        tokens[i] = aux_token;
                        aux_token = "";
                        corchete = 0;
                        break;
                    }
                }
                i++;
            }
            file1.Close();
        }

        private void asignarLineas()
        {
            String line = "";
            int i = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\adria\OneDrive\Documentos\compilador\lineas.txt");
            while ((line = file.ReadLine()) != null)
            {
                i++;
            }
            file.Close();
            numeroLinea = new string[i];
            i = 0;
            String aux = "", aux_linea = "";
            System.IO.StreamReader file1 = new System.IO.StreamReader(@"C:\Users\adria\OneDrive\Documentos\compilador\lineas.txt");
            while ((line = file1.ReadLine()) != null)
            {
                aux = line;
                for (int j = 0; j < aux.Length; j++)
                {
                    aux_linea += aux[j];
                }
                numeroLinea[i] = aux_linea;
                aux_linea = "";
                i++;
            }
            file1.Close();
        }

        private void lexicoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Lexico(richTextBox1.Text);
            asignarTokens();
            asignarLineas();
        }

        private void sintacticoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            raizSintactica = null;
            for (int i = 0; i < num_errores_sintacticos; i++)
            {
                errores_sintacticos[i] = "";
            }
            num_errores_sintacticos = 0;
            ErroresSintacticos.Text = "";
            total_errores = 0;
            tabControl1.SelectedIndex = 1;
            lexicoToolStripMenuItem_Click(sender, e);
            index_token = 0;
            raizSintactica = programa();
            raizSemantica = raizSintactica;
            treeView1.Nodes.Clear();
            treeView1.BeginUpdate();

            var sintactico = verArbol(raizSintactica, treeView1);
            if (sintactico != null)
                treeView1.Nodes.Add(sintactico);
            treeView1.EndUpdate();
            treeView1.ExpandAll();
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------Sintactico----------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------

        public class nodo
        {
            public double valor;
            public string nombre;
            public string tipo;
            public string tipoVariable;
            public string tipoNodo { get; set; }
            public string TipoVariable { get; set; }
            public List<nodo> hijos = new List<nodo>();
            public int linea { get; set; }
            public double Valor { get; set; }
            public string Tipo { get; set; }
            public string Nombre { get; set; }
            public List<nodo> Hijos
            {
                get
                {
                    return hijos;
                }
                set
                {
                    hijos = value;
                }
            }

        }

        public TreeNode verArbol(nodo arbol, TreeView tree)
        {
            if (arbol != null)
            {
                TreeNode nuevoT = new TreeNode();
                if (arbol.Nombre == null)
                {
                    return null;
                }
                if (arbol.Tipo == "factor" || arbol.Tipo == "Identificador")
                {
                    //string x = arbol.Valor.ToString();
                    nuevoT.Text = arbol.Nombre.ToString();
                    return nuevoT;
                }
                nuevoT.Text = arbol.Nombre.ToString();
                var hijos = arbol.Hijos.Count;
                for (int i = 0; i < hijos; i++)
                {
                    if (arbol.Hijos[i] != null && arbol.Hijos[i].Nombre != null)
                        nuevoT.Nodes.Add(verArbol(arbol.Hijos[i], tree));
                }
                return nuevoT;
            }
            return null;
        }

        static int index_token = 0;

        public void error()
        {
            //ErroresSintacticos.Text += index_token+" Error Sintactico -->" + errores_sintacticos[index_token-4];//[((index_token)-3)];
        }

        public static int total_errores = 0;
        String[] errores_sintacticos_sin_repetir = new string[200000];

        public void agregar_error(String s)
        {
            int b = 0;
            for (int i = 0; i < total_errores; i++)
            {
                if (s == errores_sintacticos_sin_repetir[i]) b = 1;
            }
            if (b == 0)
            {
                errores_sintacticos_sin_repetir[total_errores] = s;
                ErroresSintacticos.Text += s;
                total_errores++;
            }
        }
        public bool match(String tokenActual)//funcion match verifica que el token actual es igual
        {//al token esperado
            if (tokens[index_token] == tokenActual)
            {
                index_token++;
                return true;
            }
            else
            {
                if (index_token > 0)
                {
                    if (tokens[index_token] == ")")
                    {
                        agregar_error("Se esperaba un [ ( ]" + errores_sintacticos[index_token] + "");
                    }
                    else if (tokens[index_token] == "}")
                    {
                        agregar_error("Se esperaba un [ { ]" + errores_sintacticos[index_token] + "");
                    }
                    else
                    {
                        agregar_error("Se esperaba un [ " + tokenActual + " ]" + errores_sintacticos[index_token - 1] + "");
                    }
                }
                error();
            }
            return false;
        }
        
        public nodo programa()
        {
            nodo temp = new nodo();
            match("main");
            match("{");
            temp.Nombre = "main";
            temp.Tipo = "main";
            temp.tipoNodo = "main";
            temp.Hijos.Add(lista_declaracion());
            temp.Hijos.Add(lista_sentencias());
            match("}");
            return temp;
        }

        public nodo lista_declaracion()
        {
            nodo nuevo = new nodo();
            nuevo.Nombre = "declaracion";
            nuevo.tipoNodo = "declaracion";
            while(tokens[index_token]=="int" || tokens[index_token] == "float" || tokens[index_token] == "bool")
            {
                nodo otro = new nodo();
                otro.Nombre = tokens[index_token];
                otro.Tipo = "variable";
                otro.tipoNodo = tokens[index_token];
                index_token++;
                nodo primero = new nodo();
                if (tipo_token[index_token] == "Identificador")
                {
                    primero.Nombre = tokens[index_token];
                    primero.Tipo = "Identificador";
                    primero.linea = int.Parse(numeroLinea[index_token]);
                    primero.tipoNodo = otro.tipoNodo;
                    primero.TipoVariable = otro.tipoNodo;
                    otro.Hijos.Add(primero);
                    index_token++;
                }
                while(tokens[index_token]!=";" && tokens[index_token] == ",")
                {
                    match(",");
                    if (tipo_token[index_token]=="Identificador")
                    {
                        nodo variable = new nodo();
                        variable.Nombre = tokens[index_token];
                        variable.Tipo = "Identificador";
                        variable.linea = int.Parse(numeroLinea[index_token]);
                        variable.tipoNodo = otro.tipoNodo;
                        variable.TipoVariable = otro.tipoNodo;
                        otro.Hijos.Add(variable);
                        index_token++;
                    }
                }
                if (tokens[index_token] == ";")
                {
                    match(";");
                }
                nuevo.Hijos.Add(otro);
            }
            return nuevo;
        }
     
        public nodo identificador()
        {
            nodo temp = new nodo();
            nodo nuevo = new nodo();
            if (tipo_token[index_token] == "Identificador")
            {
                nuevo.Nombre = tokens[index_token];
                nuevo.Tipo = "Identificador";
                nuevo.tipoNodo = "expresion";
                nuevo.linea = int.Parse(numeroLinea[index_token]);
                index_token++;
                temp = nuevo;
            }
            return temp;
        }

        public nodo lista_sentencias()
        {
            nodo temp = new nodo();
            temp.Nombre = "sentencias";
            switch (tokens[index_token])
            {
                case "if":
                    temp.Hijos.Add(seleccion());
                    break;
                case "while":
                    temp.Hijos.Add(iteracion());
                    break;
                case "do":
                    temp.Hijos.Add(repeticion());
                    break;
                case "cin":
                    temp.Hijos.Add(sent_read());
                    break;
                case "cout":
                    temp.Hijos.Add(sent_write());
                    break;
                case "{":
                    temp.Hijos.Add(bloque2());
                    break;
                default:
                    if (tipo_token[index_token] == "Identificador")
                    {
                        try
                        {
                            temp.Hijos.Add(asignacion());
                        }
                        catch (Exception ex) { }
                    }
                    else
                    {

                    }
                    break;
            }
            //temp.Hijos.Add(sentencia());
            try
            {
                while (tipo_token[index_token] == "Identificador" || (tipo_token[index_token] == "Palabra reservada" && tokens[index_token] != "until" && tokens[index_token] != "end" && tokens[index_token] != "else") || tokens[index_token] == "{")
                {
                    //temp.Hijos.Add(sentencia());
                    switch (tokens[index_token])
                    {
                        case "if":
                            temp.Hijos.Add(seleccion());
                            break;
                        case "while":
                            temp.Hijos.Add(iteracion());
                            break;
                        case "do":
                            temp.Hijos.Add(repeticion());
                            break;
                        case "cin":
                            temp.Hijos.Add(sent_read());
                            break;
                        case "cout":
                            temp.Hijos.Add(sent_write());
                            break;
                        case "{":
                            temp.Hijos.Add(bloque2());
                            break;
                        default:
                            if (tipo_token[index_token] == "Identificador")
                            {
                                try
                                {
                                    temp.Hijos.Add(asignacion());
                                }
                                catch (Exception ex) { }
                            }
                            else
                            {

                            }
                            break;
                    }
                }
            }
            catch (Exception e) { }

            return temp;
        }

        public nodo seleccion()
        {
            nodo temp = null;
            nodo nuevo = new nodo();
            nodo nuevo2 = new nodo();
            nodo nuevo3 = new nodo();
            nuevo.Nombre = "if";
            nuevo.tipoNodo = "statement";

            match("if");
            temp = expresion();
            match("then");

            //nuevo2.Nombre = "condicion";
            //nuevo2.Hijos.Add(temp);
            //nuevo.Hijos.Add(nuevo2);
            nuevo.Hijos.Add(temp);
            nuevo.Hijos.Add(bloque());
            if (tokens[index_token] == "else")
            {
                match("else");
                nuevo3.Nombre = "else";
                nuevo3.Hijos.Add(bloque());
                nuevo.Hijos.Add(nuevo3);
            }
            match("end");
            match(";");
            temp = nuevo;
            return temp;
        }

        public nodo iteracion()
        {
            nodo temp = new nodo();
            nodo nuevo = new nodo();
            match("while");
            temp = expresion();
            nuevo.Nombre = "while";
            nuevo.Hijos.Add(temp);
            nuevo.Hijos.Add(bloque2());
            temp = nuevo;
            return temp;
        }

        public nodo repeticion()
        {
            nodo temp = new nodo();
            nodo nuevo = new nodo();
            match("do");
            temp.Nombre = "do";
            temp.tipoNodo = "statement";
            temp.Hijos.Add(bloque());
            match("until");
            nuevo.Nombre = "until";
            nuevo.tipoNodo = "until";
            nuevo.Hijos.Add(expresion());
            temp.Hijos.Add(nuevo);
            match(";");
            return temp;
        }

        public nodo sent_read()
        {
            nodo temp = new nodo();
            match("cin");
            temp.Nombre = "cin";
            temp.tipoNodo = "statement";
            temp.Hijos.Add(identificador());
            match(";");
            return temp;
        }

        public nodo sent_write()
        {
            nodo temp = new nodo();
            match("cout");
            temp.Nombre = "cout";
            temp.tipoNodo = "statement";
            temp.Hijos.Add(expresion());
            match(";");
            return temp;
        }

        public nodo bloque()
        {
            nodo temp = new nodo();
            temp = lista_sentencias();
            return temp;
        }

        public nodo bloque2()
        {
            nodo temp = new nodo();
            match("{");
            temp = lista_sentencias();
            match("}");
            return temp;
        }

        public nodo asignacion()
        {
            nodo temp = new nodo();
            nodo variable = new nodo();
            nodo operacion = new nodo();
            nodo uno = new nodo();
            String tok = "";
            if (tipo_token[index_token] == "Identificador")
            {
                tok = tokens[index_token];
                index_token++;
                if (tokens[index_token] == "++")
                {
                    temp.Nombre = ":=";
                    temp.tipoNodo = "statement";
                    index_token++;
                    variable.Nombre = tok;
                    variable.Tipo = "Identificador";
                    variable.tipoNodo = "expresion";
                    variable.linea = int.Parse(numeroLinea[index_token]);
                    operacion.Nombre = "+";
                    operacion.Tipo = "Op";
                    operacion.tipoNodo = "expresion";
                    temp.Hijos.Add(variable);
                    operacion.Hijos.Add(variable);
                    uno.Nombre = "1";
                    uno.Tipo = "factor";
                    uno.tipoNodo = "expresion";
                    uno.Valor = Convert.ToDouble("1");
                    uno.linea = int.Parse(numeroLinea[index_token]);
                    uno.TipoVariable = "int";
                    operacion.Hijos.Add(uno);
                    temp.Hijos.Add(operacion);
                }
                else if (tokens[index_token] == "--")
                {
                    temp.Nombre = ":=";
                    temp.tipoNodo = "statement";
                    index_token++;
                    variable.Nombre = tok;
                    variable.Tipo = "Identificador";
                    variable.tipoNodo = "expresion";
                    variable.linea = int.Parse(numeroLinea[index_token]);
                    operacion.Nombre = "-";
                    operacion.tipoNodo = "expresion";
                    operacion.Tipo = "Op";
                    temp.Hijos.Add(variable);
                    operacion.Hijos.Add(variable);
                    uno.Nombre = "1";
                    uno.Tipo = "factor";
                    uno.tipoNodo = "expresion";
                    uno.Valor = Convert.ToDouble("1");
                    uno.linea = int.Parse(numeroLinea[index_token]);
                    uno.TipoVariable = "int";
                    operacion.Hijos.Add(uno);
                    temp.Hijos.Add(operacion);
                }
                else
                {
                    if (match(":="))
                    {
                        temp.Nombre = ":=";
                        temp.tipoNodo = "statement";
                        variable.Nombre = tok;
                        variable.Tipo = "Identificador";
                        variable.linea = int.Parse(numeroLinea[index_token]);
                        variable.tipoNodo = tok;
                        temp.Hijos.Add(variable);
                        temp.Hijos.Add(expresion());
                    }
                    else
                    {
                        while (tokens[index_token] != ";") index_token++;
                        error();
                    }
                }
                if (!match(";"))
                {
                    while (tokens[index_token] != ";") index_token++;
                    match(";");
                }
            }
            return temp;
        }

        public nodo expresion()
        {
            nodo temp = null;
            nodo nuevo = null;
            temp = expresion_simple();
            try
            {
                while (tokens[index_token] == "<=" || tokens[index_token] == "<" || tokens[index_token] == ">" || tokens[index_token] == ">="
                        || tokens[index_token] == "==" || tokens[index_token] == "!=")
                {
                    switch (tokens[index_token])
                    {
                        case "<=":
                            match("<=");
                            nuevo = new nodo();
                            nuevo.Nombre = "<=";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(expresion_simple());
                            temp = nuevo;
                            break;
                        case "<":
                            match("<");
                            nuevo = new nodo();
                            nuevo.Nombre = "<";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(expresion_simple());
                            temp = nuevo;
                            break;
                        case ">":
                            match(">");
                            nuevo = new nodo();
                            nuevo.Nombre = ">";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(expresion_simple());
                            temp = nuevo;
                            break;
                        case ">=":
                            match(">=");
                            nuevo = new nodo();
                            nuevo.Nombre = ">=";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(expresion_simple());
                            temp = nuevo;
                            break;
                        case "==":
                            match("==");
                            nuevo = new nodo();
                            nuevo.Nombre = "==";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(expresion_simple());
                            temp = nuevo;
                            break;
                        case "!=":
                            match("!=");
                            nuevo = new nodo();
                            nuevo.Nombre = "!=";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(expresion_simple());
                            temp = nuevo;
                            break;
                    }
                }
            }
            catch (Exception e) { }
            return temp;
        }
        public nodo expresion_simple()
        {
            nodo temp = null;
            nodo nuevo = null;
            temp = termino();
            try
            {
                while (tokens[index_token] == "+" || tokens[index_token] == "-" || tokens[index_token] == "++" || tokens[index_token] == "--")
                {
                    switch (tokens[index_token])
                    {
                        case "+":
                            match("+");
                            nuevo = new nodo();
                            nuevo.Nombre = "+";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(termino());
                            temp = nuevo;
                            break;
                        case "-":
                            match("-");
                            nuevo = new nodo();
                            nuevo.Nombre = "-";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(termino());
                            temp = nuevo;
                            break;
                        case "++":
                            match("++");
                            nuevo = new nodo();
                            nuevo.Nombre = "+";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nodo uno = new nodo();
                            uno.Nombre = "1";
                            uno.tipoNodo = "expresion";
                            uno.Tipo = "factor";
                            temp.Hijos.Add(uno);
                            nuevo.Hijos.Add(temp);
                            temp = nuevo;
                            break;
                        case "--":
                            match("--");
                            nuevo = new nodo();
                            nuevo.Nombre = "-";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nodo unoMenos = new nodo();
                            unoMenos.Nombre = "1";
                            unoMenos.tipoNodo = "expresion";
                            unoMenos.Tipo = "factor";
                            temp.Hijos.Add(unoMenos);
                            nuevo.Hijos.Add(temp);
                            temp = nuevo;
                            break;
                    }
                }
            }
            catch (Exception e) { }

            return temp;
        }

        public nodo termino()
        {
            nodo temp = null;
            nodo nuevo = null;
            temp = factor();
            try
            {
                while (tokens[index_token] == "*" || tokens[index_token] == "/" || tokens[index_token] == "%")
                {
                    switch (tokens[index_token])
                    {
                        case "*":
                            match("*");
                            nuevo = new nodo();
                            nuevo.Nombre = "*";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(factor());
                            temp = nuevo;
                            break;
                        case "/":
                            match("/");
                            nuevo = new nodo();
                            nuevo.Nombre = "/";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(factor());
                            temp = nuevo;
                            break;
                        case "%":
                            match("%");
                            nuevo = new nodo();
                            nuevo.Nombre = "%";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(factor());
                            temp = nuevo;
                            break;
                    }
                }
            }
            catch (Exception e) { }

            return temp;
        }

        public nodo factor()
        {
            nodo temp = null;
            nodo nuevo = null;
            temp = fin();
            try
            {
                while (tokens[index_token] == "^")
                {
                    switch (tokens[index_token])
                    {
                        case "^":
                            match("^");
                            nuevo = new nodo();
                            nuevo.Nombre = "^";
                            nuevo.tipoNodo = "expresion";
                            nuevo.Tipo = "Op";
                            nuevo.Hijos.Add(temp);
                            nuevo.Hijos.Add(fin());
                            temp = nuevo;
                            break;
                    }
                }
            }
            catch (Exception e) { }

            return temp;
        }

        public nodo fin()
        {
            nodo temp = new nodo();
            try
            {
                if (tokens[index_token] == "(")
                {
                    match("(");
                    temp = expresion();
                    match(")");
                }
                else if (tipo_token[index_token] == "Identificador")
                {
                    //temp = new TreeNode(tokens[index_token] + "");
                    temp.Tipo = "Identificador";
                    temp.Nombre = tokens[index_token];
                    temp.tipoNodo = "expresion";
                    temp.linea = int.Parse(numeroLinea[index_token]);
                    index_token++;

                }
                else if (tipo_token[index_token] == "Numero entero" || tipo_token[index_token] == "Numero flotante")
                {
                    int x = 0;
                    temp.Tipo = "factor";
                    temp.Nombre = tokens[index_token];
                    temp.tipoNodo = "expresion";
                    temp.Valor = Convert.ToDouble(tokens[index_token]);
                    temp.linea = int.Parse(numeroLinea[index_token]);
                    if (tipo_token[index_token] == "Numero entero")
                        temp.TipoVariable = "int";
                    else
                        temp.TipoVariable = "float";
                    index_token++;
                }
                else
                {
                    error();
                }
            }
            catch (Exception ex) { }
            return temp;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------Semantico----------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        Dictionary<string, HashTableItem> hashTable = new Dictionary<string, HashTableItem>();
        public int location = 0;
        public double pruebaResultadoOperacion = 0;
        private void semanticoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hashTable.Clear();
            location = 0;
            erroresSemanticos = "";
            index_token = 0;
            sintacticoToolStripMenuItem1_Click(sender, e);
            tabControl1.SelectedIndex = 2;
            tabControl2.SelectedIndex = 2;
            treeView2.Nodes.Clear();
            treeView2.BeginUpdate();
            raizSemantica = raizSintactica;
            recorrido(raizSemantica);
            var t = verArbolSemantico(raizSemantica, treeView2);
            if (t != null)
            {
                treeView2.Nodes.Add(t);
            }
            treeView2.EndUpdate();
            treeView2.ExpandAll();
            ErroresSemanticosRich.Text = erroresSemanticos;
            printHashTable();
        }
        public void recorrido(nodo arbol)
         {
             switch (arbol.Nombre)
             {
                 case "declaracion":
                     foreach (var rama in arbol.Hijos)
                         create_hashTable(rama);
                     break;
                 case ":=":
                     if (lookUp(arbol.Hijos.ElementAt(0).Nombre))//ignora asignaciones a valiebles no declaradas
                     {
                         //Actualizar sus valores
                         arbol.Hijos.ElementAt(0).TipoVariable = hashTable[arbol.Hijos.ElementAt(0).Nombre].tipoVariable;
                         arbol.Hijos.ElementAt(0).Valor = hashTable[arbol.Hijos.ElementAt(0).Nombre].valorActual;

                         var resultadoPruebaOperacion = calculos(arbol.Hijos.ElementAt(1));
                         //checar tipo de ambos hijos
                         string resultado = checkNode(arbol.Hijos.ElementAt(0), arbol.Hijos.ElementAt(1));
                         string actual = arbol.Hijos.ElementAt(0).TipoVariable;
                         if (actual == resultado)
                         {
                             arbol.Hijos.ElementAt(0).Valor = resultadoPruebaOperacion;
                             //agregando el valor obtenido a las asignaciones
                             arbol.Valor = resultadoPruebaOperacion;
                             arbol.TipoVariable = resultado;
                             actualizarHashTable(arbol.Hijos.ElementAt(0));
                         }
                         else
                         {
                            actualizarHashTable(arbol.Hijos.ElementAt(0));
                            erroresSemanticos += "El tipo de la variable " + arbol.Hijos[0].Nombre +
                                 " no corresponde al valor asignado, en la linea " + arbol.Hijos.ElementAt(0).linea + ".\n";
                         }

                     }
                     else
                     {
                        erroresSemanticos += "La variable " + arbol.Hijos[0].Nombre + " en la linea " + arbol.Hijos[0].linea + " no ha sido declarada.\n";
                        var resultadoPruebaOperacion = calculos(arbol.Hijos.ElementAt(1));
                        //checar tipo de ambos hijos
                        string resultado = checkNode(arbol.Hijos.ElementAt(0), arbol.Hijos.ElementAt(1));
                        string actual = arbol.Hijos.ElementAt(0).TipoVariable;
                        if (actual == resultado)
                        {
                            arbol.Hijos.ElementAt(0).Valor = resultadoPruebaOperacion;
                            //agregando el valor obtenido a las asignaciones
                            arbol.Valor = resultadoPruebaOperacion;
                            arbol.TipoVariable = resultado;
                        }
                    }
                     break;
                 case "cin":
                     insert_node(arbol.Hijos.ElementAt(0));
                     break;
                 case "cout":
                     arbol.Valor = calculos(arbol.Hijos.ElementAt(0));
                     break;
                 case "if":
                 case "until":
                 case "while":
                     expressionCheck(arbol.Hijos.ElementAt(0));
                     break;
             }
             if (arbol.hijos.Count() > 0)
             {
                 foreach (var rama in arbol.Hijos)
                 {
                     recorrido(rama);
                 }
             }
         }
         public void expressionCheck(nodo t)
         {
            nodo a = new nodo();
            nodo b = new nodo();
            a.Valor = calculos(t.Hijos.ElementAt(0));
            b.Valor = calculos(t.Hijos.ElementAt(1));
            t.TipoVariable = "bool";
            switch (t.Nombre)
            {
                case "==":
                    if (a.Valor == b.Valor)
                        t.Valor = 1;
                    else
                        t.Valor = 0;
                    break;
                case "!=":
                    if (a.Valor != b.Valor)
                        t.Valor = 1;
                    else
                        t.Valor = 0;
                    break;
                case "<=":
                    if (a.Valor <= b.Valor)
                        t.Valor = 1;
                    else
                        t.Valor = 0;
                    break;
                case ">=":
                    if (a.Valor >= b.Valor)
                        t.Valor = 1;
                    else
                        t.Valor = 0;
                    break;
                case "<":
                    if (a.Valor < b.Valor)
                        t.Valor = 1;
                    else
                        t.Valor = 0;
                    break;
                case ">":
                    if (a.Valor > b.Valor)
                        t.Valor = 1;
                    else
                        t.Valor = 0;
                    break;
            }
         }
         public TreeNode verArbolSemantico(nodo arbol, TreeView tree)
         {
             if (arbol != null)
             {
                 TreeNode nuevoT = new TreeNode();
                 if (arbol.Nombre == null)
                 {
                     return null;
                 }
                 if (arbol.Tipo == "factor" || arbol.Tipo == "Identificador")
                 {
                     string x = arbol.Valor.ToString();
                     if (arbol.Tipo == "Identificador")
                     {
                         switch (arbol.TipoVariable)
                         {
                             case "int":
                                 nuevoT.Text = arbol.Nombre.ToString() + ". Tipo: " + arbol.TipoVariable + " (" + arbol.Valor.ToString("N0") + ")";
                                 break;
                             case "float":
                                 nuevoT.Text = arbol.Nombre.ToString() + ". Tipo: " + arbol.TipoVariable + " (" + arbol.Valor.ToString("F") + ")";
                                 break;
                             default:
                                 nuevoT.Text = arbol.Nombre.ToString() + ". Tipo: Sin tipo (" + arbol.Valor.ToString("N0") + ")";
                                 break;
                         }
                     }
                     else
                     {
                         nuevoT.Text = arbol.Nombre.ToString();
                     }
                     return nuevoT;
                 }
                 if (arbol.Nombre == "+" || arbol.Nombre == "-" || arbol.Nombre == "*"
                     || arbol.Nombre == "/" || arbol.Nombre == "%" || arbol.Nombre == "^" || arbol.Nombre == ":=")
                 {
                     switch (arbol.TipoVariable)
                     {
                         case "int":
                             nuevoT.Text = arbol.Nombre.ToString() + " (" + arbol.Valor.ToString("N0") + ") ";
                             break;
                         case "float":
                             nuevoT.Text = arbol.Nombre.ToString() + " (" + arbol.Valor.ToString("F") + ")";
                             break;
                         default:
                             nuevoT.Text = arbol.Nombre.ToString() + " (Error)";
                             break;
                     }
                 }
                 else if (arbol.Nombre == ">" || arbol.Nombre == "<" || arbol.Nombre == ">="
                     || arbol.Nombre == "<=" || arbol.Nombre == "==" || arbol.Nombre == "!=")
                 {
                     if (arbol.Valor == 0)
                         nuevoT.Text = arbol.Nombre.ToString() + "Tipo: Boolean (False)";
                     else
                         nuevoT.Text = arbol.Nombre.ToString() + "Tipo: Boolean (True)";

                 }
                 else
                 {
                     nuevoT.Text = arbol.Nombre.ToString();
                 }

                 var hijos = arbol.Hijos.Count;
                 for (int i = 0; i < hijos; i++)
                 {
                     if (arbol.Hijos[i] != null && arbol.Hijos[i].Nombre != null)
                         nuevoT.Nodes.Add(verArbolSemantico(arbol.Hijos[i], tree));
                 }
                 return nuevoT;
             }
             return null;
         }
         public class HashTableItem
         {
             public string nombre { get; set; }
             public List<int> lineas { get; set; }
             public int localidad { get; set; }
             public string tipoVariable { get; set; }
             public double valorActual { get; set; }
         }
         public void create_hashTable(nodo t)
         {
             for (int i = 0; i < t.Hijos.Count(); i++)
             {
                 if (!lookUp(t.Hijos.ElementAt(i).Nombre))
                 {
                     hashTable.Add(t.Hijos.ElementAt(i).Nombre, new HashTableItem
                     {
                         nombre = t.Hijos.ElementAt(i).Nombre,
                         lineas = new List<int>(),
                         localidad = location++,
                         tipoVariable = t.Hijos.ElementAt(i).TipoVariable,
                         valorActual = 0
                     });
                     hashTable[t.Hijos.ElementAt(i).Nombre].lineas.Add(t.Hijos.ElementAt(i).linea);
                     //propagacion de tipo de variables
                     t.Hijos.ElementAt(i).tipoNodo = t.tipoNodo;
                 }
                 else
                 {
                     // error, se declaro la misma variable dos veces
                     erroresSemanticos += "Variable " + t.Hijos.ElementAt(i).Nombre + " duplicada en la linea " + t.Hijos.ElementAt(i).linea+"\n";
                 }
             }
         }
         public bool lookUp(string nombre)
         {
             if (hashTable.ContainsKey(nombre))
             {
                 return true;
             }
             else
             {
                 return false;
             }
         }
         //igual a como recorre el sintactico pero con condiciones extra    
         public void insert_node(nodo t)
         {
             if (!lookUp(t.Nombre))
             {
                 //error no esta declarada la variable
                 erroresSemanticos += "La variable " + t.Nombre + " en la linea "+ t.linea + " no ha sido declarada.\n";
             }
             else
             {
                 //si se encuentra, se agrega la linea donde se encontro
                 hashTable[t.Nombre].lineas.Add(t.linea);
                 //propagacion de tipo
                 t.TipoVariable = hashTable[t.Nombre].tipoVariable;
                 t.Valor = hashTable[t.Nombre].valorActual;
             }
         }
        public void printHashTable()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("Variable", "Variable");
            dataGridView1.Columns[0].Width = 70;
            dataGridView1.Columns.Add("Valor", "Valor");
            dataGridView1.Columns[1].Width = 70;
            dataGridView1.Columns.Add("Tipo", "Tipo");
            dataGridView1.Columns[2].Width = 70;
            dataGridView1.Columns.Add("Localizacion", "Localizacion");
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns.Add("Lineas", "Lineas");
            dataGridView1.Columns[4].Width = 200;
            //agregando el grid view 
            int localizacion = 0;
            foreach (var item in hashTable)
            {
                // un nuevo renglon
                localizacion = dataGridView1.Rows.Add();
                //colocamos la info
                dataGridView1.Rows[localizacion].Cells[0].Value = item.Value.nombre;
                if(item.Value.tipoVariable == "float")
                    dataGridView1.Rows[localizacion].Cells[1].Value = item.Value.valorActual.ToString("F");
                else
                    dataGridView1.Rows[localizacion].Cells[1].Value = item.Value.valorActual;
                dataGridView1.Rows[localizacion].Cells[2].Value = item.Value.tipoVariable;
                dataGridView1.Rows[localizacion].Cells[3].Value = item.Value.localidad;
                string lineas = "";
                foreach(int linea in item.Value.lineas)
                {
                    if(lineas == "")
                    {
                        lineas = linea.ToString();
                    }
                    else
                    {
                        lineas = lineas + ", " + linea.ToString();
                    }
                }
                    dataGridView1.Rows[localizacion].Cells[4].Value = lineas;

            }
        }
        public string checkNode(nodo a, nodo b)
         {
             if (a.TipoVariable == b.TipoVariable)
             {
                 return a.TipoVariable;
             }
             else
             {
                 return "float";
             }
         }
         public double calculos(nodo raiz)
         {
             if (raiz.Hijos.Count > 0)
             {
                 switch (raiz.Nombre)
                 {
                     case "+":
                         raiz.Valor = calculos(raiz.Hijos.ElementAt(0)) + calculos(raiz.Hijos.ElementAt(1));
                         //propagacion de tipo en operaciones
                         raiz.TipoVariable = checkNode(raiz.Hijos.ElementAt(0), raiz.Hijos.ElementAt(1));
                         if (raiz.TipoVariable == "int")
                             raiz.Valor = Math.Truncate(raiz.Valor);
                         break;
                     case "-":
                         raiz.Valor = calculos(raiz.Hijos.ElementAt(0)) - calculos(raiz.Hijos.ElementAt(1));
                         //propagacion de tipo en operaciones
                         raiz.TipoVariable = checkNode(raiz.Hijos.ElementAt(0), raiz.Hijos.ElementAt(1));
                         if (raiz.TipoVariable == "int")
                             raiz.Valor = Math.Truncate(raiz.Valor);
                         break;
                     case "*":
                         raiz.Valor = calculos(raiz.Hijos.ElementAt(0)) * calculos(raiz.Hijos.ElementAt(1));
                         //propagacion de tipo en operaciones
                         raiz.TipoVariable = checkNode(raiz.Hijos.ElementAt(0), raiz.Hijos.ElementAt(1));
                         if (raiz.TipoVariable == "int")
                             raiz.Valor = Math.Truncate(raiz.Valor);
                         break;
                     case "/":
                         raiz.Valor = calculos(raiz.Hijos.ElementAt(0)) / calculos(raiz.Hijos.ElementAt(1));
                         //propagacion de tipo en operaciones
                         raiz.TipoVariable = checkNode(raiz.Hijos.ElementAt(0), raiz.Hijos.ElementAt(1));
                         if (raiz.TipoVariable == "int")
                             raiz.Valor = Math.Truncate(raiz.Valor);
                         break;
                     case "%":
                         raiz.Valor = calculos(raiz.Hijos.ElementAt(0)) % calculos(raiz.Hijos.ElementAt(1));
                         //propagacion de tipo en operaciones
                         raiz.TipoVariable = checkNode(raiz.Hijos.ElementAt(0), raiz.Hijos.ElementAt(1));
                         if (raiz.TipoVariable == "int")
                             raiz.Valor = Math.Truncate(raiz.Valor);
                         break;
                     case "^":
                         raiz.Valor = Math.Pow(calculos(raiz.Hijos.ElementAt(0)), calculos(raiz.Hijos.ElementAt(1)));
                         //propagacion de tipo en operaciones
                         raiz.TipoVariable = checkNode(raiz.Hijos.ElementAt(0), raiz.Hijos.ElementAt(1));
                         if (raiz.TipoVariable == "int")
                             raiz.Valor = Math.Truncate(raiz.Valor);
                         break;

                 }
                 return raiz.Valor;
             }
             else
             {
                 if (raiz.Tipo == "Identificador")
                 {
                    if (hashTable.ContainsKey(raiz.Nombre))
                    {
                        insert_node(raiz);
                        //propagacion de tipo y valor actual
                        raiz.TipoVariable = hashTable[raiz.Nombre].tipoVariable;
                        raiz.Valor = hashTable[raiz.Nombre].valorActual;
                        return hashTable[raiz.Nombre].valorActual;
                     }
                     else
                         return 0; //se uso variable que no se encuentra en la hashtable
                 }
                 else
                 {
                     return raiz.Valor;
                 }
             }
         }

        public void actualizarHashTable(nodo t)
         {
             if (hashTable.ContainsKey(t.Nombre))
             {
                 hashTable[t.Nombre].valorActual = t.Valor;
                 hashTable[t.Nombre].lineas.Add(t.linea);
             }
             else
             {
                 erroresSemanticos += "La variable " + t.Nombre + " en la linea " + t.linea + " no ha sido declaradaa.\n";
             }


         }
        //-------------------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------Codigo Intermedio---------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        public int mp = 6;
        public int ac = 0;
        public int ac1 = 1;
        public int gp = 5;
        public static int tmpOffset = 0;
        public static int emitLoc = 0;
        public static int highEmitLoc = 0;
        public int pc = 7;
        private void ejecutarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            semanticoToolStripMenuItem_Click(sender, e);
            codigoIntermedioRrichTextBox.Text = "";
            tabControl1.SelectedIndex = 4;
            mp = 6;
            ac = 0;
            ac1 = 1;
            gp = 5;
            tmpOffset = 0;
            emitLoc = 0;
            highEmitLoc = 0;
            pc = 7;
            foreach (var arbol in raizSemantica.Hijos)
            {
                if (arbol.Nombre == "sentencias")
                {
                    codigoIntermedio(arbol);
                }
            }
            guardarIntermedio();
        }

        public void codigoIntermedio(nodo tree)
        {
            emitRM("LD",mp,0,ac, "Load max address");
            emitRM("ST", ac, 0, ac, "Clear location 0");
            foreach(var hijo in tree.Hijos)
            {
                cGen(hijo);
            }
            emitRO("HALT", 0, 0, 0, "");
        }

        public int st_lookup(string nombre)
        {
            if (hashTable.ContainsKey(nombre))
            {
                return hashTable[nombre].localidad;
            }
            else 
            {  
                return -1;
            }
        }
        public void cGen(nodo tree)
        {
            if(tree != null)
            {
                switch(tree.tipoNodo)
                {
                    case "expresion":
                        genExp(tree);
                        break;
                    case "statement":
                        genStmt(tree);
                        break;
                }
            }
        }
        public void genStmt(nodo tree)
        {
            int savedLoc1, savedLoc2, currentLoc;
            int loc;
            if (tree != null)
            {
                switch (tree.Nombre)
                {
                    case ":=":
                        cGen(tree.Hijos[1]);
                        loc = st_lookup(tree.Hijos[0].Nombre);
                        emitRM("ST", ac, loc, gp, "Asgina valor guardado");
                        break;
                    case "cout":
                        cGen(tree.Hijos[0]);
                        emitRO("OUT", ac, 0, 0, "Write ac");
                        break;
                    case "cin":
                        emitRO("IN", ac, 0, 0, "Read Integer Value");
                        loc = st_lookup(tree.Hijos[0].Nombre);
                        emitRM("ST", ac, loc, gp, "Read: Store Value");
                        break;
                    case "if":
                        cGen(tree.Hijos[0]);
                        savedLoc1 = emitSkip(1);

                        foreach (var rama in tree.Hijos[1].Hijos) {
                            cGen(rama);
                        }
                        //cGen(tree.Hijos[1].Hijos[0]);
                        savedLoc2 = emitSkip(1);
                        currentLoc = emitSkip(0);
                        emitBackup(savedLoc1);
                        emitRM_Abs("JEQ", ac, currentLoc, "if: jmp to else");
                        emitRestore();
                        
                        foreach (var rama in tree.Hijos[2].Hijos[0].Hijos)
                        {
                            cGen(rama);
                        }
                        //cGen(tree.Hijos[2].Hijos[0].Hijos[0]);
                        currentLoc = emitSkip(0);
                        emitBackup(savedLoc2);
                        emitRM_Abs("LDA", pc, currentLoc, "jmp to end");
                        emitRestore();
                        break;
                    case "do":
                        savedLoc1 = emitSkip(0);
                        //cGen(tree.Hijos[0].Hijos[0]);
                        foreach (var rama in tree.Hijos[0].Hijos)
                        {
                            cGen(rama);
                        }
                        cGen(tree.Hijos[1].Hijos[0]);
                        emitRM_Abs("JEQ", ac, savedLoc1, "repeat: jmp back to body");
                        break;
                }
            }
        }
        public void genExp(nodo tree)
        {
            int loc;
            if (tree != null)
            {
                switch (tree.Tipo)
                {
                    case "Identificador":
                        loc = st_lookup(tree.Nombre);
                        emitRM("LD", ac, loc, gp, "load id value");
                        break;
                    case "factor":
                        emitRM("LDC", ac, Convert.ToInt32(tree.Valor), 0, "load const");
                        break;
                    case "Op":
                        cGen(tree.Hijos[0]);
                        emitRM("ST", ac, tmpOffset--, mp, "op: push left");
                        cGen(tree.Hijos[1]);
                        emitRM("LD", ac1, ++tmpOffset, mp, "op: load left");
                        switch (tree.Nombre)
                        {
                            case "+":
                                emitRO("ADD", ac, ac1, ac, "op +");
                                break;
                            case "-":
                                emitRO("SUB", ac, ac1, ac, "op -");
                                break;
                            case "*":
                                emitRO("MUL", ac, ac1, ac, "op *");
                                break;
                            case "/":
                                emitRO("DIV", ac, ac1, ac, "op /");
                                break;
                            case "^":
                                emitRO("POT", ac, ac1, ac, "op ^");
                                break;
                            case "%":
                                emitRO("MOD", ac, ac1, ac, "op %");
                                break;
                            case "<":
                                emitRO("SUB", ac, ac1, ac, "op <");
                                emitRM("JLT", ac, 2, pc, "br if true");
                                emitRM("LDC", ac, 0, ac, "false case");
                                emitRM("LDA", pc, 1, pc, "unconditional jmp");
                                emitRM("LDC", ac, 1, ac, "true case");
                                break;
                            case "<=":
                                emitRO("SUB", ac, ac1, ac, "op <=");
                                emitRM("JLE", ac, 2, pc, "br if true");
                                emitRM("LDC", ac, 0, ac, "false case");
                                emitRM("LDA", pc, 1, pc, "unconditional jmp");
                                emitRM("LDC", ac, 1, ac, "true case");
                                break;
                            case ">":
                                emitRO("SUB", ac, ac1, ac, "op >");
                                emitRM("JGT", ac, 2, pc, "br if true");
                                emitRM("LDC", ac, 0, ac, "false case");
                                emitRM("LDA", pc, 1, pc, "unconditional jmp");
                                emitRM("LDC", ac, 1, ac, "true case");
                                break;
                            case ">=":
                                emitRO("SUB", ac, ac1, ac, "op >=");
                                emitRM("JGE", ac, 2, pc, "br if true");
                                emitRM("LDC", ac, 0, ac, "false case");
                                emitRM("LDA", pc, 1, pc, "unconditional jmp");
                                emitRM("LDC", ac, 1, ac, "true case");
                                break;
                            case "==":
                                emitRO("SUB", ac, ac1, ac, "op ==");
                                emitRM("JEQ", ac, 2, pc, "br if true");
                                emitRM("LDC", ac, 0, ac, "false case");
                                emitRM("LDA", pc, 1, pc, "unconditional jmp");
                                emitRM("LDC", ac, 1, ac, "true case");
                                break;
                            case "!=":
                                emitRO("SUB", ac, ac1, ac, "op !=");
                                emitRM("JNE", ac, 2, pc, "br if true");
                                emitRM("LDC", ac, 0, ac, "false case");
                                emitRM("LDA", pc, 1, pc, "unconditional jmp");
                                emitRM("LDC", ac, 1, ac, "true case");
                                break;
                        }
                        break;
                }
            }
        }
        public void guardarIntermedio()
        {
            System.IO.StreamWriter File = new System.IO.StreamWriter(@"C:\Users\adria\OneDrive\Documentos\compilador\codigoIntermedio.txt"); // aqui guarda el resultado en un txt 
            File.Write(codigoIntermedioRrichTextBox.Text);
            File.Close();
        }

        public void emitRO(string op, int r, int s, int t, string comentario)
        {
            codigoIntermedioRrichTextBox.Text += emitLoc++.ToString() + ": " + op + " " + r.ToString() + "," + s.ToString() +","+t.ToString()+ "\n";
            if (highEmitLoc < emitLoc) 
                highEmitLoc = emitLoc;
        }
        public void emitRM(string op, int r, int d, int s, string comentario)
        {
            codigoIntermedioRrichTextBox.Text += emitLoc++.ToString()+": "+op+" "+r.ToString()+","+d.ToString()+"("+s.ToString()+")\n";
            if (highEmitLoc < emitLoc) 
                highEmitLoc = emitLoc;
        }
        public int emitSkip(int howMany)
        {
            int i = emitLoc;
            emitLoc += howMany;
            if (highEmitLoc < emitLoc) highEmitLoc = emitLoc;
            return i;
        }
        public void emitBackup(int loc)
        {
            //if (loc > highEmitLoc) //emitComment("BUG in emitBackup");
            emitLoc = loc;
        }
        public void emitRestore()
        { 
            emitLoc = highEmitLoc; 
        }
        public void emitRM_Abs(string op, int r, int a, string comentario)
        {
            codigoIntermedioRrichTextBox.Text += emitLoc.ToString() + ": " +op+" "+r.ToString()+","+(a-(emitLoc+1)).ToString()+"("+pc+")\n";
            ++emitLoc;
            if (highEmitLoc < emitLoc) 
                highEmitLoc = emitLoc;
        }
        private void compilarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ejecutarToolStripMenuItem_Click(sender, e);
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo("C:\\Users\\adria\\OneDrive\\Escritorio\\TM.exe");
            psi.Arguments = "C:\\Users\\adria\\OneDrive\\Documentos\\compilador\\codigoIntermedio.txt";
            p.StartInfo = psi;
            p.Start();
        }

    }
}