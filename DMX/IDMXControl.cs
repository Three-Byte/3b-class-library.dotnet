using System;
using System.Collections.Generic;
namespace ThreeByte.DMX {
    public interface IDMXControl {
        bool Enabled { get; set; }
        void Init();
        bool IsOpen { get; }
        void SetAll(byte val);
        void SetValues(Dictionary<int, byte> values);
        void SetValues(Dictionary<int, byte> values, int startChannel);
        string COMPort { get; set; }
    }
}
