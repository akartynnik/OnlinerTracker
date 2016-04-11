﻿using RabbitMQ.Client;
using System;
using System.Text;
using System.Windows.Forms;

namespace RabbitMQ.Producer
{
    public partial class MainForm : Form
    {
        private IModel _chanel;

        public MainForm(IModel chanel)
        {
            _chanel = chanel;
            InitializeComponent();
        }

        private void sendToMQ1_Click(object sender, EventArgs e)
        {
            var message = messageBox.Text;
            var body = Encoding.UTF8.GetBytes(message);
            var properties = _chanel.CreateBasicProperties();
            properties.SetPersistent(true);
            _chanel.BasicPublish("", "chanel", properties, body);
            Console.WriteLine("Producer sent to MQ: \"{0}\"", message);
        }
    }
}

