using System;

class RequestPrepare
{
    private byte _addr;
    //private byte _pass;


    public RequestPrepare()
    {

    }

    public void SetAddr(int address)
    {
        _addr = Convert.ToByte(address.ToString("X"), 16);
        //_pass = Convert.ToByte(password.ToString("X"), 16);
    }

    //Подготовка запроса тестирования канала связи
    public byte[] Check()
    {
        //Cетевой адрес (1 байт)
        //Код запроса(1 байт)
        //CRC(2 байта)

        byte[] request = { _addr, 0x00, 0x00, 0x00 };

        UInt16 crc = this.ModRTU_CRC(request, 2);

        byte[] crcByte = BitConverter.GetBytes(crc);

        request[2] = crcByte[0];
        request[3] = crcByte[1];

        return request;
    }

    //Подготовка запроса открытия канала связи
    public byte[] Open()
    {
        //Сетевой адрес (1 байт)
        //Код запроса = 1h (1 байт)
        //Уровень доступа (1 байт)
        //Пароль (6 байт)
        //CRC(2 байта)
        byte[] request = { _addr, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00 };

        UInt16 crc = this.ModRTU_CRC(request, 9);
        byte[] crcByte = BitConverter.GetBytes(crc);
        request[9] = crcByte[0];
        request[10] = crcByte[1];

        return request;
    }

    //Подготовка запроса получения мгновенной полной мощности по сумме фаз
    public byte[] getCurrentPower()
    {
        //Сетевой адрес (1 байт)
        //??
        //Код параметров
        //BWRI
        //CRC(2 байта)

        byte[] request = { _addr, 0x08, 0x16, 0x00, 0x00, 0x00 };

        UInt16 crc = this.ModRTU_CRC(request, 4);
        byte[] crcByte = BitConverter.GetBytes(crc);
        request[4] = crcByte[0];
        request[5] = crcByte[1];

        return request;
    }

    public byte[] getW()
    {
        //Сетевой адрес (1 байт)
        //
        //
        //
        //CRC(2 байта)

        byte[] request = { _addr, 0x05, 0x00, 0x00, 0x00, 0x00 };

        UInt16 crc = this.ModRTU_CRC(request, 4);
        byte[] crcByte = BitConverter.GetBytes(crc);
        request[4] = crcByte[0];
        request[5] = crcByte[1];

        return request;
    }

    public byte[] getSerialNumber()
    {
        //Сетевой адрес (1 байт)
        //
        //
        //
        //CRC(2 байта)

        byte[] request = { _addr, 0x08, 0x00, 0x00, 0x00 };

        UInt16 crc = this.ModRTU_CRC(request, 3);
        byte[] crcByte = BitConverter.GetBytes(crc);
        request[3] = crcByte[0];
        request[4] = crcByte[1];

        return request;
    }

    //Подготовка запроса закрытия канала связи
    public byte[] Close()
    {
        //Cетевой адрес (1 байт)
        //Код запроса(1 байт)
        //CRC(2 байта)

        byte[] request = { _addr, 0x02, 0x00, 0x00 };

        UInt16 crc = this.ModRTU_CRC(request, 2);

        byte[] crcByte = BitConverter.GetBytes(crc);

        request[2] = crcByte[0];
        request[3] = crcByte[1];

        return request;
    }

    //Расчитываем CRC
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