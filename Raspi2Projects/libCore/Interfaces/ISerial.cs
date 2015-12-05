using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCore.Interfaces
{
    public interface ISerial
    {
        event NotifyTextDelegate NotifyTextEvent;
        event NotifyexceptionDelegate NotifyexceptionEvent;
        event NotifyMessageReceivedDelegate NotifyMessageReceivedEvent;

        string Port { get; set; }
        System.UInt32 BaudRate { get; set; }
        SerialParity Parity { get; set; }
        SerialStopBitCount StopBits { get; set; }
        SerialDataBits DataBits { get; set; }

        bool IsConnected { get; }
    }

    public interface ISerialSync : ISerial
    {
        void SendData(byte[] data);
        void SendText(string text);
        void Start();
        void Stop();
        byte[] Read();
    }

    public interface ISerialAsync : ISerial
    {
        Task SendData(byte[] data);
        Task SendText(string text);
        Task Start();
        Task Stop();
        Task<byte[]> Read();
    }

    // Zusammenfassung:
    //     Definiert Werte für das Paritätsbit für die serielle Datenübermittlung. Die Werte
    //     werden von der Parity-Eigenschaft im SerialDevice-Objekt verwendet.
    public enum SerialParity
    {
        //
        // Zusammenfassung:
        //     Es wird keine Paritätsprüfung ausgeführt.
        None = 0,
        //
        // Zusammenfassung:
        //     Legt das Paritätsbit fest, sodass die Gesamtanzahl der festgelegten Datenbits
        //     eine ungerade Zahl ist.
        Odd = 1,
        //
        // Zusammenfassung:
        //     Legt das Paritätsbit fest, sodass die Gesamtanzahl der festgelegten Datenbits
        //     eine gerade Zahl ist.
        Even = 2,
        //
        // Zusammenfassung:
        //     Behält die Festlegung des Paritätsbits auf 1 bei.
        Mark = 3,
        //
        // Zusammenfassung:
        //     Behält die Festlegung des Paritätsbits auf 0 bei.
        Space = 4
    }

    //
    // Zusammenfassung:
    //     Definiert Werte, die die Anzahl von Stoppbits angeben, die in einer Übertragung
    //     verwendet werden.
    public enum SerialStopBitCount
    {
        //
        // Zusammenfassung:
        //     Ein Stoppbit wird verwendet.
        One = 0,
        //
        // Zusammenfassung:
        //     1,5 Stoppbits werden verwendet.
        OnePointFive = 1,
        //
        // Zusammenfassung:
        //     Zwei Stoppbits werden verwendet.
        Two = 2
    }

    public enum SerialDataBits
    {
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8
    }
}
