namespace TrackTrek
{
    public partial class Form1 : Form
    {

        private void Form1_Load(object sender, EventArgs e)
        {
            Console.WriteLine("(DEBUG): Solution loaded!");

            this.MouseClick += (sender, e) => this.ActiveControl = null;
            this.ActiveControl = null;
            //LinkInput.Location = new Point(this.ClientSize.Width / 2 - LinkInput.Size.Width / 2, this.ClientSize.Height / 10);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Window_Resized(object sender, EventArgs e)
        {
            //LinkInput.Size = new Size(this.ClientSize.Width / 2, this.ClientSize.Height / 10);
            //LinkInput.Location = new Point(this.ClientSize.Width / 2 - LinkInput.Size.Width / 2, this.ClientSize.Height / 10);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
