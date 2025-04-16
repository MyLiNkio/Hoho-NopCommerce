use hoho_prodsample_smart
UPDATE vendor SET Email = 'kolisnyak@gmail.com';
UPDATE messagetemplate SET BccEmailAddresses = 'kolisnyak@gmail.com';
/* Було б добре створити імейл для дебагінгу, щось типу debug@hoho.ge
 * Тоді всі імейли можна було б відправляти з нього і на нього
 * UPDATE messagetemplate SET BccEmailAddresses = 'debug@hoho.ge';
 * UPDATE vendor SET Email = 'debug@hoho.ge';
 * Потім додати його в [hoho_dev_13_12_2024].[dbo].[EmailAccount] (якщо його там ще нема), взяти айдішку з якою він додався,
 * І встановити цю айді в усі UPDATE messagetemplate SET EmailAccountId = <id>; 
 * Таким чином всі відправки також будуть з тестового імейла і не засмічуватимуть та не ставитимуть під загрозу оригінальні аккаунти.
 * 
 * Також створити admin@hoho.ge для адмінського тестового користувача. І бути John Smith з цього імейла,
 * щоб не засмічувати особисту поштову скриньку. 
 */

UPDATE Store
   SET 
		[Url] = 'https://localhost:52365/',
		[Hosts] = 'localhost:52365'
 WHERE Id = 1;

Truncate Table [Log];


/* Це записи для того щоб відображались атрибути в топ меню. Виконати цей код тільки якщо вони не відображаються
insert into [setting] (Name, Value, StoreId) values ('TopMenu.BaseCategoryIdToDisplaySpecificationAttr', '24', 0);
insert into [setting] (Name, Value, StoreId) values ('TopMenu.DisplaySpecificationAttr', '10,8,9', 0);
*/