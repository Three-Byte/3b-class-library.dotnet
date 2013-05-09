using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using log4net;
using ThreeByte.Converters;

namespace ThreeByte.Network.Devices
{
    public class SoundwebPreset : SoundwebBlock
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SoundwebPreset));

        public SoundwebPreset(string ipAddress) : base(ipAddress) {            
 
        }

        public void Recall(int preset) {
            byte[] presetMessage = new byte[5];
            presetMessage[0] = 0x8C;  //Preset

            BitConverter.GetBytes(preset).Reverse().ToArray().CopyTo(presetMessage, 1);

            PackAndSendMessage(presetMessage);
        }
    }
}
