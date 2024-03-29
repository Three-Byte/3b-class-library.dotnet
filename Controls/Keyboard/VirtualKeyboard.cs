﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using log4net;


namespace ThreeByte.Controls.Keyboard
{

    public enum VK : ushort
    {
        // Virtual Keys, Standard Set
        LBUTTON = 0x01,
        RBUTTON = 0x02,
        CANCEL = 0x03,
        MBUTTON = 0x04,    /* NOT contiguous with L & RBUTTON */

        XBUTTON1 = 0x05,    /* NOT contiguous with L & RBUTTON */
        XBUTTON2 = 0x06,    /* NOT contiguous with L & RBUTTON */

        // 0x07 : unassigned
        BACK = 0x08,
        TAB = 0x09,

        // 0x0A - 0x0B : reserved 
        CLEAR = 0x0C,
        RETURN = 0x0D,

        SHIFT = 0x10,
        CONTROL = 0x11,
        MENU = 0x12,    // ALT
        PAUSE = 0x13,
        CAPITAL = 0x14,

        KANA = 0x15,
        HANGEUL = 0x15,  /* old name - should be here for compatibility */
        HANGUL = 0x15,
        JUNJA = 0x17,
        FINAL = 0x18,
        HANJA = 0x19,
        KANJI = 0x19,

        ESCAPE = 0x1B,

        CONVERT = 0x1C,
        NONCONVERT = 0x1D,
        ACCEPT = 0x1E,
        MODECHANGE = 0x1F,

        SPACE = 0x20,
        PRIOR = 0x21,
        NEXT = 0x22,
        END = 0x23,
        HOME = 0x24,
        LEFT = 0x25,
        UP = 0x26,
        RIGHT = 0x27,
        DOWN = 0x28,
        SELECT = 0x29,
        PRINT = 0x2A,
        EXECUTE = 0x2B,
        SNAPSHOT = 0x2C,
        INSERT = 0x2D,
        DELETE = 0x2E,
        HELP = 0x2F,

        // 0 - 9 are the same as ASCII '0' - '9' (0x30 - 0x39) 
        _0 =  0x30,
        _1 = 0x31,
        _2 = 0x32,
        _3 = 0x33,
        _4 = 0x34,
        _5 = 0x35,
        _6 = 0x36,
        _7 = 0x37,
        _8 = 0x38,
        _9 = 0x39,

        // 0x40 : unassigned 
        // A - Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A) */

        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,


        LWIN = 0x5B,
        RWIN = 0x5C,
        APPS = 0x5D,

        // 0x5E : reserved
        SLEEP = 0x5F,

        NUMPAD0 = 0x60,
        NUMPAD1 = 0x61,
        NUMPAD2 = 0x62,
        NUMPAD3 = 0x63,
        NUMPAD4 = 0x64,
        NUMPAD5 = 0x65,
        NUMPAD6 = 0x66,
        NUMPAD7 = 0x67,
        NUMPAD8 = 0x68,
        NUMPAD9 = 0x69,
        MULTIPLY = 0x6A,
        ADD = 0x6B,
        SEPARATOR = 0x6C,
        SUBTRACT = 0x6D,
        DECIMAL = 0x6E,
        DIVIDE = 0x6F,
        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,

        /*
* 0x88 - 0x8F : unassigned */
        NUMLOCK = 0x90,
        SCROLL = 0x91,

        /*
* L* & R* - left and right Alt, Ctrl and Shift virtual keys. * Used only as parameters to GetAsyncKeyState() and GetKeyState(). * No other API or message will distinguish left and right keys in this way. */
        LSHIFT = 0xA0,
        RSHIFT = 0xA1,
        LCONTROL = 0xA2,
        RCONTROL = 0xA3,
        LMENU = 0xA4,
        RMENU = 0xA5,

        BROWSER_BACK = 0xA6,
        BROWSER_FORWARD = 0xA7,
        BROWSER_REFRESH = 0xA8,
        BROWSER_STOP = 0xA9,
        BROWSER_SEARCH = 0xAA,
        BROWSER_FAVORITES = 0xAB,
        BROWSER_HOME = 0xAC,

        VOLUME_MUTE = 0xAD,
        VOLUME_DOWN = 0xAE,
        VOLUME_UP = 0xAF,
        MEDIA_NEXT_TRACK = 0xB0,
        MEDIA_PREV_TRACK = 0xB1,
        MEDIA_STOP = 0xB2,
        MEDIA_PLAY_PAUSE = 0xB3,
        LAUNCH_MAIL = 0xB4,
        LAUNCH_MEDIA_SELECT = 0xB5,
        LAUNCH_APP1 = 0xB6,
        LAUNCH_APP2 = 0xB7,

        /*
* 0xB8 - 0xB9 : reserved */
        OEM_1 = 0xBA,   // ';:' for US
        OEM_PLUS = 0xBB,   // '+' any country
        OEM_COMMA = 0xBC,   // ',' any country
        OEM_MINUS = 0xBD,   // '-' any country
        OEM_PERIOD = 0xBE,   // '.' any country
        OEM_2 = 0xBF,   // '/?' for US
        OEM_3 = 0xC0,   // '`~' for US

        /*
* 0xC1 - 0xD7 : reserved */
        /*
* 0xD8 - 0xDA : unassigned */
        OEM_4 = 0xDB,  //  '[{' for US
        OEM_5 = 0xDC,  //  '\|' for US
        OEM_6 = 0xDD,  //  ']}' for US
        OEM_7 = 0xDE,  //  ''"' for US
        OEM_8 = 0xDF

        /*
* 0xE0 : reserved */

    }


    public class VirtualKeyboard : INotifyPropertyChanged

    {
        private readonly ILog log = LogManager.GetLogger(typeof(VirtualKeyboard));

        public Dictionary<VK, bool> pressedKeys = new Dictionary<VK, bool>();

        public void PressAndRelease(string key)
        {
            VK keyCode;
        //    if (key=="BACK") 
       //     {
            try
            {
                keyCode = (VK)Enum.Parse(typeof(VK), key);
                SendKey(keyCode);
                if(keyCode == VK.CAPITAL) {
                    System.Threading.ThreadPool.QueueUserWorkItem(RefreshCapsLockState);
                }
            }
            catch  { }

            ReleaseStickyKeys();

         //   } else if (key="LeftShift

//            Press(keyCode);
 //           Release(keyCode);
        }

        private void RefreshCapsLockState(object state) {
            //Must refresh after the key has been inserted into the stream and the keyboard has acknowledged the update.
            System.Threading.Thread.Sleep(10);
            NotifyPropertyChanged("CapsLock");
            NotifyPropertyChanged("Upper");
        }


        public void ReleaseStickyKeys()
        { 
            //TODO: any key can be configured to be "sticky". check the "pressedKeys" collection/dictionary
            Shift = false;
            Alt = false;
            Ctrl = false;
            RightShift = false;
            RightAlt = false;
            RightCtrl = false;
            Win = false;
        }

        //public void Press(string key)
        //{
        //    if (key.Length == 1)
        //    {
        //        Press((VK)Enum.Parse(typeof(VK), key));
        //    }
        //}






        //public void Press(VK key)
        //{

        //}


        public void PressAndHold(string key)
        {
            PressAndHold((VK)Enum.Parse(typeof(VK), key));
        }

        public void PressAndHold(VK keyCode)
        {
            switch (keyCode)
            { 
                case VK.LSHIFT:
                case VK.RSHIFT:
                case VK.SHIFT: this.Shift = !this.Shift;
                    //log.Debug("This shift = " + Shift);
                    //Console.WriteLine("This shift = " + Shift);
                    break;

                case VK.MENU: Alt= !Alt; break;
                    
                case VK.CONTROL:
                case VK.RCONTROL:
                case VK.LCONTROL: this.Ctrl = !this.Ctrl; break;

                case VK.LWIN: this.Win = !this.Win; break;
            }

            //TODO: consider implementing collection of pressed keys
            // instead of handling all cases manually

            //VK keyCode = (VK)Enum.Parse(typeof(VK), key);
            //if (!pressedKeys.Keys.Contains(keyCode)) pressedKeys.Add(keyCode, false);
            //if (null==pressedKeys[keyCode]) ReleaseSticky(keyCode); else PressSticky(keyCode);
            //pressedKeys[keyCode] = !pressedKeys[keyCode];
        }

        public void PressKey(VK keyCode)
        {
            uint intReturn = 0;
            INPUT structInput;
            structInput = new INPUT();
            structInput.type = (uint)1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;
            // Key down the actual key-code

            structInput.ki.wVk = (ushort)keyCode; //VK.SHIFT etc.
            intReturn = NativeWin32.SendInput(1, ref structInput, 28);//sizeof(INPUT));
        }


        public void ReleaseKey(VK keyCode)
        {
            uint intReturn = 0;
            INPUT structInput;
            structInput = new INPUT();
            structInput.type = (uint)1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;

            // Key up the actual key-code
            structInput.ki.dwFlags = NativeWin32.KEYEVENTF_KEYUP;
            structInput.ki.wVk = (ushort)keyCode;// (ushort)NativeWin32.VK.SNAPSHOT;//vk;
            intReturn = NativeWin32.SendInput((uint)1, ref structInput, Marshal.SizeOf(structInput));
        }

        public void SendKey(VK keyCode)
        {

            uint intReturn = 0;
            INPUT structInput;
            structInput = new INPUT();
            structInput.type = (uint)1;
            structInput.ki.wScan = 0;
            structInput.ki.time = 0;
            structInput.ki.dwFlags = 0;
            structInput.ki.dwExtraInfo = 0;

            
            // Press the key
            structInput.ki.wVk = (ushort)keyCode;
            intReturn = NativeWin32.SendInput((uint)1, ref structInput, Marshal.SizeOf(structInput));
            //log.Debug("Send press result: " + intReturn);

            // Release the key
            structInput.ki.dwFlags = NativeWin32.KEYEVENTF_KEYUP;
            structInput.ki.wVk = (ushort)keyCode;
            intReturn = NativeWin32.SendInput((uint)1, ref structInput, Marshal.SizeOf(structInput));
            //log.Debug("Send release result: " + intReturn);

        }

        private bool LeftShift { get; set; }
        private bool RightShift { get; set; }
        public bool Shift
        {
            get { return (LeftShift || RightShift); }
            set
            {
                //Console.WriteLine("LeftShift = " + LeftShift);
                //Console.WriteLine("value = " + value);

                if (LeftShift != value)
                {
                    LeftShift = value;
                    if (Shift) PressKey(VK.SHIFT); else ReleaseKey(VK.SHIFT);
                    NotifyPropertyChanged("Shift");
                    NotifyPropertyChanged("Upper");
                    //Console.WriteLine("Notified");
                }
            }
        }

        public bool CapsLock {
            get {
                return (((ushort)NativeWin32.GetKeyState((ushort)VK.CAPITAL) & 0xffff) != 0);
            }
        }


        /// <summary>
        /// Gets a value indicating whether or not the keyboard is shifted to upper case based on the state of CapsLock and Shift
        /// </summary>
        public bool Upper {
            get {
                return Shift ^ CapsLock;
            }
        }

        private bool LeftAlt { get; set; }
        private bool RightAlt { get; set; }
        public bool Alt
        {
            get { return (LeftAlt || RightAlt); }
            set
            {
                if (LeftAlt != value)
                {
                    LeftAlt = value;
                    if (LeftAlt) PressKey(VK.MENU); else ReleaseKey(VK.MENU);
                    NotifyPropertyChanged("Alt");
                }
            }
        }
        private bool LeftCtrl { get; set; }
        private bool RightCtrl { get; set; }
        public bool Ctrl
        {
            get { return (LeftCtrl || RightCtrl); }
            set
            {
                if (LeftCtrl != value)
                {
                    LeftCtrl = value;
                    if (LeftCtrl) PressKey(VK.CONTROL); else ReleaseKey(VK.CONTROL);
                    NotifyPropertyChanged("Ctrl");

                }
            }
        }
        private bool LeftWin { get; set; }
        public bool Win
        {
            get { return LeftWin; }
            set
            {
                if (LeftWin != value)
                {
                    LeftWin = value;
                    if (LeftWin) PressKey(VK.LWIN); else ReleaseKey(VK.LWIN);
                    NotifyPropertyChanged("Win");
                }
            }
        }

        //public bool Menu { get; set; }
        //public bool NumLock { get; set; }
        //public bool FLock { get; set; }
        //public bool Fn { get; set; }
        //public bool ScrollLock { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
