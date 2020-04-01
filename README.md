# Watchdog
## Poznamky
Aplikaci je nutne ve VS prelozit, ke spravnemu fungovani je treba mit nainstalovanou SQL Server databazi (napr. verzi LocalDB, pro vyvoj a test dostatecne). 
Aplikaci spustime pomoci souboru *WatchdogFramework.exe* bez parametru. Zobrazi se konzole s informacemi o prubehu, pokyny k ovladani, pripadne chybami. Veskera data jsou ulozena v databazi, kterou aplikace sama pri startu vytvori (za pomoci entity frameworku).
Konfigurovat lze v souboru *App.config* (*WatchdogFramework.exe.config*) dle zadani.  
## Implementacni detaily
Prichozi data (deal) se prvne ulozi do databaze, pak se pouzije jako vstup do ‘pipeline’, ktera prvne vypocte ratio a pak, v ramci nadefinovanych pravidel, vybere z jiz zpracovanych zaznamu ty, ktere maji s aktualne zpracovavanym spojitost. Tato informace se zase ulozi do databaze (deal group).
### Nedokonalosti/TODOs
* Metodou pokus/omyl jsem prisel na to, ze volani GetUserBalance() v ramci handleru nemohu pouzit stejnou instanci MT5Api, na kterou jsme navesili handler eventu prichozich novych obchodu. Vytvoril jsem tedy druhou instanci, ktera je pouzita prave k tomuto ucelu. Pri volani Dispose() nad jakoukoliv druhou vytvorenou instanci MT5Api se vsak program zasekne. Nevim, zda to je limitaci poskytnuteho API wrapperu, nebo nepochopeni jeho pouziti.
* Pravidlo pro porovnani obchodu pomoci volume-to-balance ratio neni pouzite, protoze jsem nebyl sto pochopit, jak toto ratio spravne vypocitat a pouzit. Aplikace byla napsana tak, aby to slo nakonec jednoduse doplnit. Ke spravnemu pochopeni tohoto pravidla bych pravdepodobne potreboval priklad s vypoctem, porovnanim, idelane i priklady toho, kdy se pravidlo splni a kdy ne.
