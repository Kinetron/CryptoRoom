<h1 align="center">Программа шифрования данных «VanishBox»</h1>
<h2 align="center">Краткое описание.</h2>

<p align="left">
Программа предназначена для шифрования файлов с использованием гибридной системы шифрования.<br>
- RsaAesSha256, блочный шифр AES c длиной ключа 256 байт, ассиметричное шифрование RSA 4096, подпись файла HMACSHA256.<br>
- RsaGos, блочный шифр ГОСТ 34.12-2015, 34.13-2015, ассиметричный-RSA 4096, подпись файлов - ГОСТ 34.10-2012, 34.11-2012<br>
<p>
 <p align="left">
 Для хранения секретного ключа и закодированных файлов используется собственный формат.
<p>

<p align="left">
Внешний вид главного окна программы:
<img src="./mainWindow.jpg" width="100%">
<p>
<p align="left">
Для начала работы с программой необходимо сгенерировать закрытый(и открытый) ключ шифрования, который будет помещен в секретный ключевой контейнер.
<p>
<p align="left">
В меню «Настройки» выберите пункт «Сгенерировать ключ»:
<img src="./genKey1.jpg" width="100%">
<p>
<p align="left">
В появившемся окне выберите путь к папке в которую будет сохранен секретный ключ. Введите пароль для защиты контейнера секретного ключа.<br>
ВНИМАНИЕ! Утеря ключа или пароля приведет к невозможности расшифровки файлов.<br>
<img src="./genKey2.jpg" width="100%">
</p>
<p align="left">
Дождитесь завершения операции.<br>
<img src="./genKey3.jpg" width="100%">
</p>
<p align="left">
Шифруем файл:
<img src="./chipperProcess1.jpg" width="100%"><br>
<img src="./chipperProcess2.jpg" width="100%"><br>
<img src="./chipperProcess3.jpg" width="100%">
</p> 
<p align="left">
Зашифрованный файл:
<img src="./chipperProcess4.jpg" width="100%">
</p> 
<p align="left">
Расшифруем:
<img src="./chipperProcess5.jpg" width="100%"><br>
<img src="./chipperProcess6.jpg" width="100%"><br>
</p> 
<p align="left">
Файл расшифрован<br>
<img src="./chipperProcess7.jpg" width="100%"><br> 
</p>
<p align="left">
<strong>
Cкорости декодирования(при использовании  hdd):
</strong><br>
- RsaGos  2,3Мбайт/с (много поточность не реализована) <br>
- RsaAesSha256   90 Мбайт/с (20 поточный процессор с встроенным aes аппаратным блоком)<br>
</p>
<p align="left">
Алгоритм, который будет использовать программа можно настроить в файле 
appsettings.json<br>
<img src="./settings.jpg" width="100%"><br>
Возможные варианты:<br>
 RsaGos, RsaAesSha256 <br>
<br>
</p>

