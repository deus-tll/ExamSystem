# Проєкт ForbiddenWords
## Структура файлу із забороненими словами
```
Структура для файлу із забороненими словами може бути тільки щоб кожне окреме слово було на окремому рядку.
Також слово може бути розділене дефісом.
Приклад:

word1
word2 
word3
word4-word4
```
## Запуск через командний рядок

```
При запуску додатка через командний рядок обов'язково перший аргумент повинен бути шлях до папки де будуть
зберігатися результати, а другий аргумент це шлях до файлу в якому знаходяться заборонені слова.
Також в такому режимі інтерфейс відсутній, а значить не можна зупиняти, відновлювати, або повністю завершувати
операцію під час її виконання.
```

## Додаткові моменти

```
-Додаток в обох режимах може запуститися лише в одному екземплярі.
-Ви вказуєте шлях до папки з результатами, але програма сама розгортає систему збереження файлів всередині
тої папки яку ви вказали, вам не треба перейматися через це.
```
# Проєкт Moderator\Monitoring
## Особливі моменти
```
-Щоб додаток міг виконувати свою функцію, запускати його треба від імені адміністратора. Також комусь можливо
треба бути в cmd ввести таку команду(без лапок) - "wevtutil sl Microsoft-Windows-WMI-Activity/Trace /e:true /q".
-Якщо це ваш найперший запуск, то не вказавши налаштування спочатку, додаток не зможе запустити моніторинг.
-Все інше про режими описано в самому додатку.
```
