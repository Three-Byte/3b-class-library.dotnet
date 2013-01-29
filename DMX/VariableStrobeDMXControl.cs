using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.ComponentModel;
using log4net;
using ThreeByte.Serial;

namespace ThreeByte.DMX
{
    /// <summary>
    /// For output only DMX transmission using Enttec DMX USB PRO
    /// </summary>
    public class VariableStrobeDMXControl : VariableDMXControl
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(VariableStrobeDMXControl));
       

        public VariableStrobeDMXControl(string comPort) : this(comPort, 512, 0, 0, 512) { }

        public VariableStrobeDMXControl(string comPort, int channelCount, int startChannel, int startStrobeChannel, int strobeChannelCount) : base(comPort, channelCount, startChannel) {
            _dmxStrobeValues = new byte[_dmxValues.Length];
            _strobeEffect = new bool[_dmxValues.Length];
            int startStrobeIndex = startStrobeChannel - startChannel;
            for(int i = 0; i < channelCount; ++i) {
                _strobeEffect[i] = ((i >= startStrobeIndex) && (i < startStrobeIndex + strobeChannelCount));
            }
        }

        private byte[] _dmxStrobeValues;
        private bool[] _strobeEffect;

        private bool _strobeOut = false;  //Default
        public bool StrobeOut {
            get {
                return _strobeOut;
            }
            set {
                if(_strobeOut != value) {
                    _strobeOut = value;
                    NotifyPropertyChanged("StrobeOut");
                    SendDMXData(_dmxValues);
                }
            }
        }
     
        protected override void SendDMXData(byte[] data) {
            //Modify the data here
            if(StrobeOut) {
                data.CopyTo(_dmxStrobeValues, 0);
                for(int i = 0; i < _dmxStrobeValues.Length; i++) {
                    if(_strobeEffect[i]) {
                        _dmxStrobeValues[i] = 0;
                    }
                }
                base.SendDMXData(_dmxStrobeValues);
            } else {
                base.SendDMXData(data);
            }
        }

       
    }
}
