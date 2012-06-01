using System;
using System.Collections.Generic;
using ThreeByte.Serial;
namespace ThreeByte.DMX {
    public interface IDMXControl {
        bool Enabled { get; set; }
        int DMXPacketSize { get; set; }
        //void Init();
        bool IsOpen { get; }
        void SetAll(byte val);
        void SetValues(Dictionary<int, byte> values);
        void SetValues(Dictionary<int, byte> values, int startChannel);
        string COMPort { get; set; }

    }
}
