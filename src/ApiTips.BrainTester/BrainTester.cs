using System.Collections.Concurrent;

namespace ApiTips.BrainTester
{
    public partial class BrainTester : Form
    {
        CancellationToken _token;
        BrainConnection _brain;
        private readonly string _serverHost = Environment.GetEnvironmentVariable("BRAIN_HOST") ?? string.Empty;
        private readonly int _serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("BRAIN_PORT") ?? "0");
        private System.Windows.Forms.Timer _timer;

        private readonly BlockingCollection<string> _history = new BlockingCollection<string>();

        public BrainTester()
        {
            InitializeComponent();
        }

        private void BrainTester_Load(object sender, EventArgs e)
        {
            _brain = new BrainConnection(_serverHost, _serverPort);
            _brain.MessageReceived += _brain_MessageReceived;
            _brain.MessageSended += _brain_MessageSended;

            _brain.Start();

            tbEnterMessage.ReadOnly = false;
            btnSend.Enabled = true;

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 200;
            _timer.Tick += UpdateHistory;
            _timer.Start();
        }

        private void _brain_MessageReceived(object? sender, string message)
        {
            _history.Add("Получили сообщение от мозга:" + Environment.NewLine + message);
        }

        private void _brain_MessageSended(object? sender, string message)
        {
            _history.Add("Отправили запрос:" + Environment.NewLine + message);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbEnterMessage.Text))
                return;

            _brain.PostString(tbEnterMessage.Text);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _brain.Start();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _brain.Stop();
        }

        private void UpdateHistory(object? sender, EventArgs e)
        {
            tbHistory.Lines = _history.ToArray();
            lbConnect.Text = _brain.IsConnected ? "Connected" : "Disconnected";
        }
    }
}
