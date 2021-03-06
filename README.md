**NAME**

incotexdiscovery.exe



**SYNOPSIS**

incotexdiscovery.exe -serial=COM3 [-baudrate=9600] [-serialtimeout=100] [-timeout=50] [-verbose]



**DESCRIPTION**

Сканирование cети RS485 (CAN) с целью поиска подключенных счетчиков  электроэнергии от компании Incotex Electronics Group.  У обнаруженных счетчиков определяются сетевые адреса, серийные номера и год изготовления. Поддерживаемые модели: МЕРКУРИЙ (MERCURY) 203.2TD, 204, 208, 230, 231, 234, 236, 238. 

Возможные параметры  последовательного интерфейса:

- скорость, бод: 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200;
- биты данных – 8;
- четность – без контроля;
- стоповые биты – 1;

По умолчанию счетчики имеют параметры: 9600, 8, без контроля, 1;



**OPTIONS**

-verbose

​	Вывод подробностей в ходе работы программы (полученных и отправленных фреймов, ошибок проверки CRC)

-serial

​	Номер COM порта

-baudrate

​	Скорость COM порта. Значение по-умолчанию: 9600  

-serialtimeout

​	Максимальное время ответа устройства, мс. Значение по-умолчанию: 100

-timeout	

​	Timeout между запросами к счетчику, мс.  Значение по-умолчанию: 50



**EXAMPLES**

incotexdiscovery.exe -serial=COM3 

incotexdiscovery.exe -serial=COM3 -timeout=100

incotexdiscovery.exe -serial=COM3 -timeout=150 - verbose



**BUGS**

Получение серийного номера и года изготовления возможно при установленном пароле по умолчанию для первого уровня ("111111"). При любом другом пароле будет определен только сетевой адрес счетчика.



**AUTHORS**

Artem Pletnev <artirm.pletnev@gmail.com>

https://ensoelectric.ru
<support@ensoelectric.ru>
