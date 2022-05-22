namespace AES.Forms
{
    public partial class Form1 : Form
    {
        private string PathArquivoParaCriptografar;

        public Form1()
        {
            InitializeComponent();
            //Chave de teste.
            //textBox1.Text = "65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            var dialogResult = openFileDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                txtBoxArquivoParaCriptografar.Text = openFileDialog1.FileName;
                PathArquivoParaCriptografar = txtBoxArquivoParaCriptografar.Text;
            }
            else if(dialogResult == DialogResult.No)
            {
                MessageBox.Show("Falha ao abrir arquivo.");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var bytesArquivoParaCriptografar = File.ReadAllBytes(PathArquivoParaCriptografar);
                var aes = new AlgoritmoAES(new Chave(textBox1.Text));
                var pathArquivoCriptografado = aes.Criptografar(new ConteudoCifrar(bytesArquivoParaCriptografar), textBox2.Text);
                MessageBox.Show($"Path do arquivo criptografado: {pathArquivoCriptografado}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha ao efetuar criptografia. {ex.Message}");
            }
               
        }
    }
}