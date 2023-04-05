using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Djilb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "d://ВУЗ//metrology//Metrology//Djilb//Djilb//bin//Debug//";
                
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.Text = File.ReadAllText(openFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("Файл не выбран");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            int i = Metric.LogicalComplexity(richTextBox1.Text);
            label4.Text = i.ToString();
            label5.Text = Metric.RelativeComplexity(richTextBox1.Text, i).ToString();
            label6.Text = Metric.CalculateMaxTabDepth(richTextBox1.Text).ToString();
        }
    }
}