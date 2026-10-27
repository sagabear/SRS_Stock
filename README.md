# Leltár

.NET 10 MAUI készletkezelő, lokális SQLite adatbázissal.

Leadáshoz:

1. milyen technikai döntéseket hoztál

- A legfontosabb döntés az volt, hogy az üzleti kód a MAUI-tól külön, a SRS_Stock.Core projektben legyen
- A szövegek erőforrásfájlban vannak, magyar az alap, az angol mellékelt fordítás. A feliratok egy saját kötésen keresztül jelennek meg, mert a nyelvváltásnak a már megnyitott oldalakat is frissítenie kell
- A hibák felderitéséhez logolást kelett beépiteni

2. volt-e olyan megoldás vagy részfeladat, amelyet több idő rendelkezésre állása esetén másképp
   valósítanál meg

- A lista minden találatot egyszerre betölt a memóriába, a készletértéket pedig ebből a már betöltött listából számolja. Néhány ezer tételnél ez elég. Több idővel a lekérdezés lapozott lenne, a darabszám és a készletérték pedig SQL-összegzéssel jönne, függetlenül attól, hogy hány sor látszik a képernyőn.
- A keresés az ASCII betűket nem különbözteti meg kis- és nagybetűként, az ékezeteseket viszont csak úgy találja meg, ahogy be vannak írva.
  Külön, ékezet nélkül tárolt keresőmezőt tennék a táblába, és arra keresnék.
- Nyelv váltásnál a már megnyitott űrlapon a számok formátuma sem követi az új nyelvet. Több időm lenne akkor a már használt CommunityToolkit üzenetküldőjére cserélném ezt
- Az adatbázis séma csak a tábla létrehozásáig van megoldva. Amíg egy tábla van és nem változik, ez elég. A következő oszlopnál már verziózott migráció kellene
- Az EAN-13, egyéb ellenőrző számát ellenőrizném mentéskor, a kamerás olvasást viszont külön feladatnak hagynám, mert ahhoz új engedély és platformfüggő kód kellene.
- Windows,Android, ios emulatorokban teszteltem, ott jól működött, de több idő kellene még a tesztekhez

3. milyen további fejlesztéseket tennél bele

- Vonalkód beolvasása a kamerán keresztűl, vagy vonalkód olvasóval
- A telefonon a vonalkód beolvasása nyitná meg a terméket, és egy plusz-mínusz gombbal lehetne a mennyiséget léptetni. A mező begépelése megmaradna, ha nincs kamera.
- Másolat az adatbázisról. Minden csak a telefon leltar.db fájljában van. Egy fájlba mentés és visszaolvasás, illetve egy egyszerű CSV export, hogy a készletérték táblázatban is megnyitható legyen.
- Kategóriák, termékképek, több eszköz közötti szinkron
- Eltérés lista, elindítod a számolást, termékenként beírod a talált mennyiséget, a végén pedig egy eltéréslista mutatja, hol volt több vagy kevesebb a nyilvántartottnál
- A loggolást ugy kellene megcsinálni, hogy az az eszközről kimenthető legyen egy fájlban pl megosztással vagy emailben küldéssel
- Felület design-ját frissiteném átbeszélném az illetékessel milyen legyen
- Esetleg login-t és uer kezelést és a muveleteik naplózását
