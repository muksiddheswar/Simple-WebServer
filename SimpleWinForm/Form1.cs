namespace SimpleWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSick_Click(object sender, EventArgs e)
        {
            lblGoodJob.Text = "Good Job!";
        }
    }
}