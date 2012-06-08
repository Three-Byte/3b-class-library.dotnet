using System.Collections.Generic;

namespace ThreeByte.DMX
{
    public interface IDMXControl {
        bool Enabled { get; set; }
        int DMXPacketSize { get; set; }
        string HardwareID { get; }
        bool IsOpen { get; }
        void SetAll(byte val);
        void SetValues(Dictionary<int, byte> values);
        void SetValues(Dictionary<int, byte> values, int startChannel);
    }
}
