using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace RemoteArduino.Commands
{
    public class DeserializableCommand
    {
        private readonly dynamic _command;

        public string CommandName
        {
            get { return _command.Name; }
        }

        public dynamic Command
        {
            get { return _command; }
        }

        public DeserializableCommand(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            byte[] messageBytes = message.GetBytes(); // this needs to be saved if needed later, because it can only be read once from the original Message
            string jsonObject = Encoding.ASCII.GetString(messageBytes);
            _command = JsonConvert.DeserializeObject<dynamic>(jsonObject);
        }
    }
}
