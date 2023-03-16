using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace holsted
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            dataGrid.Columns.Add("Operators", "Операторы");
            dataGrid.Columns.Add("Count", "Количество");
            dataGrid.Columns.Add("Operands", "Операнды");
            dataGrid.Columns.Add("Count", "Количество");
        }

        private void ButOpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string filename = openFileDialog1.FileName;
            string fileText = System.IO.File.ReadAllText(filename);
            inputText.Text = fileText;
            MessageBox.Show("Файл успешно открыт");
            
        }

        private void ButCount_Click(object sender, EventArgs e)
        {
            Lexer lex = new Lexer(inputText.Text);
            List<Token> tokens = lex.fillTokensArr();
            List<Token> operators = new List<Token>();
            List<Token> operands = new List<Token>();
            foreach (Token element in tokens)
            {
                if (element.type == TokenType.KEYWORD || element.type == TokenType.OPERATOR || element.type == TokenType.METHOD)
                    operators.Add(element);
                else
                    operands.Add(element);
            }
            
            var resOprnds = operands.GroupBy(x => x.lexeme).Where(g => g.Count() > 0).Select(x => new { Element = x.Key, Count = x.Count() }).ToList();
            var resOprtrs = operators.GroupBy(x => x.lexeme).Where(g => g.Count() > 0).Select(x => new { Element = x.Key, Count = x.Count() }).ToList();
            int i = 0;
            foreach (var oprnd in resOprnds)
            {
                dataGrid.Rows.Add();
                dataGrid[2,i].Value = oprnd.Element;
                dataGrid[3,i].Value = oprnd.Count;
                i++;
            }

            i = 0;
            int count = 0;
            foreach (var oprtr in resOprtrs)
            {
                dataGrid.Rows.Add();
                dataGrid[0,i].Value = oprtr.Element;
                if (oprtr.Element == "()")
                dataGrid[1,i].Value = oprtr.Count;
                i++;
            }

            richTextBox1.Text = "Уникальные операнды: " + resOprnds.Count + "   " + "Все операнды: " + operands.Count + "\n";
            richTextBox1.Text += "Уникальные операторы: " + resOprtrs.Count + "   " + "Все операторы: " + operators.Count + "\n";
            int Dict = resOprnds.Count + resOprtrs.Count;
            richTextBox1.Text += "Словарь: " + Dict + "\n";
            int Length = operands.Count + operators.Count;
            richTextBox1.Text += "Длина: " + Length + "\n";
            richTextBox1.Text += "Объем: " + Convert.ToInt32(Length * Math.Log (Dict, 2)) + "\n";
        }

        
    }
}