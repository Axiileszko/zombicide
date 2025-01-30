(Még kezdetleges)
Model:

- felhasználja a ZombieFactory statikus metódusát (CreateZombie) a SpawnZombies metódusában minden kör elején/ház kinyitásakor
- külön listákban tárolja a tuningfegyvereket, a feladatjelzőket és a tárgyakat (amik lehetnek használati tárgyak és sima fegyverek)

Character:

- a túlélőknek és a zombiknak is van akciópontja (ami a játékosok esetén változhat játék során), illetve életereje
- a támadás és mozgás túlélők esetén és zombik esetén külön lesz implementálva (erről még később)

Survivor:

- van egy trait adattagja, amit példányosításkor kap meg, ez alapján fogja implementálni a saját különleges képességét (minden túlélőnek egyedi)

Zombie

- impelentáljuk a stratégia tervezési mintát, hogy eltérő mozgást és támadást implementáljunk a különböző zombifajtákra
- az Abominációk mind megvalósítják a Singleton tervezési mintát (1 játék alatt csakis 1 létezik mindegyikből)

Weapon/PumpWeapon

- a tuningfegyver felülírhatja az ősének SpecialEffect metódusát (mindegyik tuningfegyvernek egyéni a képessége)
