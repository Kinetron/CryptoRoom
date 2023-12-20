##Система криптографической защиты информации «CryptoRoom» .Net 6

*Разрабатывается с 2019 года*

<p align="left">
21 век ознаменовался появлением мошенников, использующих интернет и компьютерные технологии для кражи денег с банковских счетов.  Личными фотографиями, телефонами мошенники тоже не брезгуют. 
</p>
<p align="left">
К утечке таких данных приводят разные факторы. Один из них – хранение своих личных данных на облачных сервисах. 
</p>
<p align="left">
Как только злоумышленник получает пароль к облаку – все данные пользователя под угрозой.
</p>
<p align="left">
Для защиты файлов в облаке можно применить дополнительное шифрование внешними программами. 
</p>
<p align="left">
Но как правило,  если пользователь решил применить дополнительное шифрование - обычно это или пиратские версии, или ПО с непонятным происхождением.
</p>
<p align="left">
Зачастую бесплатные программы работают не прозрачно, а некоторые сохраняют ключи шифрования себе, и позволяют другим людям получить доступ к файлам.
</p>
<br>
<p align="left">
<strong>
Данный проект 
</strong>
– предназначен для шифрования файлов. Полностью прозрачен.
Гарантирует отсутствие Backdoor’s и похищение ключей шифрования. 
</p>
<p align="left">
Используется гибридное шифрование. Содержимое файлов шифруется блочным шифром, сеансовый ключ ассиметричным шифрованием. В дополнение к этому файл подписывается.
</p>
<p align="left">
Используется собственный формат файлов и контейнера ключа.
</p>
<p align="left">
<strong>
Текущая версия позволяет шифровать файлы двумя версиями алгоритмов:<br>
</strong>
- RsaAesSha256, блочный шифр AES c длиной ключа 256 байт, ассиметричное шифрование RSA 4096, подпись файла HMACSHA256.<br>
- RsaGos, блочный шифр ГОСТ 34.12-2015, 34.13-2015, ассиметричный-RSA 4096, подпись файлов - ГОСТ 34.10-2012, 34.11-2012<br>
</p>
<p align="left">
Проект не использует стороннюю библиотеку OpenSSL, GPG, и др.
</p>
<p align="left">
<strong>
Cкорости декодирования(при использовании  hdd):
</strong><br>
- RsaGos  2,3Мбайт/с (много поточность не реализована) <br>
- RsaAesSha256   90 Мбайт/с (20 поточный процессор с встроенным aes аппаратным блоком)<br>
</p>
<p align="left">
Ключи шифрования храниться в контейнере, защищенным паролем. Контейнер генерируется пользователем и сохраняется в любом указанном месте.
</p>
<br>
  <a href="./docs/VanishBox/Manual.md">Инструкция к программе шифрования данных "VanishBox"</a>
<br>
<br>
<strong>CryptoRoom</strong> содержит:<br><br>
  <p>
     <strong>CryptoRoomLib</strong>- .Net библиотека шифрования;
  </p>
  <p>
    <strong>CryptoRoomLib.Tests</strong>- тесты библиотеки;
  </p>
  <p>
  <strong>VanishBox</strong>- программа шифрования файлов, для UI используется Avalonia-11.0.0-preview8.
  </p>
  <p>
     <strong>CryptoRoomApp</strong>-консольный демо пример для работы с библиотекой(в разработке).
  </p> 
  <p>
    <strong>Ui\MessageBox.Avalonia </strong>-библиотека диалоговых окон  AvaloniaUI и MVVM (https://github.com/AvaloniaCommunity/MessageBox.Avalonia).
  </p>


</p>