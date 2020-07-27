using System;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

public class MercuryPowerMeter
{
    private byte _addr;
    private bool _verbose;
    private RequestPrepare _request;
    private SerialPort _serial;
    private int _timeout;

    public MercuryPowerMeter(SerialPort serial, bool verbose = false, int timeout = 50)
    {
        _serial = serial;
        _request = new RequestPrepare();
        _verbose = verbose;
        _timeout = timeout;
    }

    public void setAddress(int address = 1)
    {
        _addr = Convert.ToByte(address.ToString("X"), 16);
        _request.SetAddr(_addr);
    }

    public bool Check()
    {
        byte[] response = this.Execute(_request.Check(), 4);
        byte[] pattern = { _addr, 0x00 };

        if (!pattern.SequenceEqual(response) || response.Length <= 0) { return false; }

        return true;
    }

    public bool Open()
    {
        byte[] response = this.Execute(_request.Open(), 4);
        byte[] pattern = { _addr, 0x00 };

        if (!pattern.SequenceEqual(response) || response.Length <= 0) { return false; }

        return true;
    }

    public bool Close()
    {
        byte[] response = this.Execute(_request.Close(), 4);
        byte[] pattern = { _addr, 0x00 };

        if (!pattern.SequenceEqual(response) || response.Length <= 0) { return false; }

        return true;
    }

    public float[] getCurrentPower()
    {
        byte[] response = this.Execute(_request.getSerialNumber(), 15);
        float[] buffer = new float[4];

        if (response.Length <= 0) return new float[0];

        byte[] P = response.Skip(1).Take(3).ToArray();
        byte[] P1 = response.Skip(4).Take(3).ToArray();
        byte[] P2 = response.Skip(7).Take(3).ToArray();
        byte[] P3 = response.Skip(8).Take(3).ToArray();

        buffer[0] = this.B3F(P, 100);
        buffer[1] = this.B3F(P1, 100);
        buffer[2] = this.B3F(P2, 100);
        buffer[3] = this.B3F(P3, 100);

        return buffer;

        //Console.WriteLine("Мощность по всем фазам: {0} ВА", p.ToString("n2"));
        //Console.WriteLine("Мощность по фазе A: {0} ВА", p1.ToString("n2"));
        //Console.WriteLine("Мощность по фазе B: {0} ВА", p2.ToString("n2"));
        //Console.WriteLine("Мощность по фазе C: {0} ВА", p3.ToString("n2"));
    }

    public float[] getW()
    {
        //Сетевой адрес (1 байт)
        //
        //
        //
        //CRC(2 байта)

        byte[] response = this.Execute(_request.getW(), 19);
        float[] buffer = new float[4];

        if (response.Length <= 0) return new float[0];

        byte[] _Aplus = response.Skip(1).Take(4).ToArray();
        byte[] _Aminus = response.Skip(5).Take(4).ToArray();
        byte[] _Rplus = response.Skip(9).Take(4).ToArray();
        byte[] _Rminus = response.Skip(13).Take(4).ToArray();

        buffer[0] = this.B4F(_Aplus, 1000);
        buffer[1] = this.B4F(_Aminus, 1000);
        buffer[2] = this.B4F(_Rplus, 1000);
        buffer[3] = this.B4F(_Rminus, 1000);

        return buffer;

        //Console.WriteLine("A+ общая: {0} кВт*ч", Aplus.ToString("n2"));
        //Console.WriteLine("A+ по фазе А: {0} Вт*ч", Aminus.ToString("n2"));
        //Console.WriteLine("A+ по фазе В: {0} вар*ч", Rplus.ToString("n2"));
        //Console.WriteLine("A+ по фазе С: {0} вар*ч", Rminus.ToString("n2"));
    }

    public string getSerialNumber()
    {
        byte[] response = this.Execute(_request.getSerialNumber(), 10);
        int[] buffer = new int[10];

        byte[] _serial = response.Skip(1).Take(4).ToArray();
        byte[] _date = response.Skip(5).Take(3).ToArray();

        return _serial[0].ToString("00") + "" + _serial[1].ToString("00") + "" + _serial[2].ToString("00") + "" + _serial[3].ToString("00") + "-" + _date[2].ToString("00");

    }


    //Отправка запроса счетчику
    //Получение результата
    //byte[] Request - сформированый запрос
    //int len - длина ответа в байтах (включая CRC)
    private byte[] Execute(byte[] Request, int len)
    {
        byte[] response = new byte[len];
        byte[] nullCRC = { 0x00, 0x00 };

        if (_verbose) Console.WriteLine("Request: {0}", BitConverter.ToString(Request));

        _serial.Write(Request, 0, Request.Length);
        Thread.Sleep(_timeout);

        try
        {
            _serial.Read(response, 0, len);
        }
        catch
        {
            if (_verbose) Console.WriteLine("Response: TIMEOUT");
            return new byte[0];
        }


        if (_verbose) Console.WriteLine("Response: {0}", BitConverter.ToString(response));

        UInt16 crc = this.ModRTU_CRC(response, response.Length - 2);    //считаем CRC ответа
        byte[] crcByte = BitConverter.GetBytes(crc);

        if (crcByte.SequenceEqual(response.Skip(response.Length - 2)) && !nullCRC.SequenceEqual(crcByte))  //проверяем расчетный CRC и CRC в ответе
        {
            return response.Take(len - 2).ToArray(); // Возвращаем массив без CRC
        }
        else
        {
            if (_verbose) Console.WriteLine("CRC is not valid!");
            _serial.DiscardInBuffer();              //Очищаем входной буфер серийного порта
            return new byte[0];
        }
    }

    // Decode float from 3 bytes
    // Source: https://github.com/Shden/mercury236/blob/master/mercury236.c
    private float B3F(byte[] b, float factor)
    {
        int val = ((b[0] & 0x3F) << 16) | (b[2] << 8) | b[1];
        return val / factor;
    }

    // Decode float from 4 bytes
    // Source: https://github.com/Shden/mercury236/blob/master/mercury236.c
    private float B4F(byte[] b, float factor)
    {
        int val = ((b[1] & 0x3F) << 24) | (b[0] << 16) | (b[3] << 8) | b[2];
        return val / factor;
    }

    // Source: https://github.com/Shden/mercury236/blob/master/mercury236.c
    private UInt16 ModRTU_CRC(byte[] buf, int len)
    {
        UInt16 crc = 0xFFFF;

        for (int pos = 0; pos < len; pos++)
        {
            crc ^= (UInt16)buf[pos];

            for (int i = 8; i != 0; i--)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc >>= 1;
                    crc ^= 0xA001;
                }
                else
                    crc >>= 1;
            }
        }
        return crc;
    }
}
