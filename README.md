# Vergaderruimtes reserveren

Een klein systeem waarin medewerkers een vergaderruimte kunnen reserveren, eenmalig of
elke week. Backend in .NET, frontend in Angular, database in SQLite.

## Draaien

Twee terminals, verder niets.

```
npm run web:install    # alleen de eerste keer
npm run api            # API op http://localhost:5161
npm run web            # app op http://localhost:4200
```

Ga daarna naar http://localhost:4200 en vul een naam in. Je hebt de .NET SDK 8.0.400 of
nieuwer nodig en Node 20.19+ of 22.12+.

De database is één bestand (`api/src/RoomBooking.Api/roombooking.db`) dat bij de eerste
start wordt aangemaakt en gevuld met vier ruimtes. Weggooien en opnieuw starten geeft je
een schone lei.

Wil je de API los bekijken zonder het scherm: http://localhost:5161/swagger.

## Keuzes en afwegingen

### Twee mensen die tegelijk dezelfde ruimte boeken

De oplossing zit in de volgorde. De database gaat op slot voordat er gekeken wordt, niet
pas bij het opslaan. Wie als tweede binnenkomt moet dus wachten tot de eerste helemaal
klaar is, en kijkt daarna naar de situatie inclusief die zojuist aangemaakte reservering.
Die ziet het conflict en krijgt een nette melding in plaats van een dubbele boeking.

SQLite laat maar één schrijver tegelijk toe en is een garantie die hier nodig is.
De belangrijkste reden dat ik voor SQLite heb gekozen is dat de zekerheid komt uit de
database zelf en niet uit code die iemand later per ongeluk kan overslaan.

Wat ik heb overwogen en niet heb gedaan:

Een slot in de applicatie zelf. Dat werkt prima zolang er één kopie van de API draait, en
stopt met werken op het moment dat er twee draaien.

### Een terugkerende reservering is één rij

Als je tien weken achter elkaar boekt, komt er één regel in de database. Daarin staat de
afspraak zelf: welke ruimte, hoe laat, vanaf wanneer en hoe vaak. De losse afspraken
worden pas uitgerekend op het moment dat je het rooster opvraagt.

Het handige daarvan is dat een gewone reservering gewoon een reeks van één keer is.
Daardoor hoeft nergens in de code de vraag "is dit een herhaling?" gesteld te worden.
Boeken, controleren op conflicten en tonen werken allemaal precies hetzelfde. Daarnaast zou het ook
moeilijker zijn om te zien welke afspraken allemaal onderdeel zijn van een reeks als we ze indiviueel zouden
opslaan.

Echter is er wel het nadeel dat de database niet afweet van alle individuele afspraken. Het leeft dus puur
in de applicatie. Dit nadeel vind ik persoonlijk kleiner dan het voordeel.

### Eén afspraak annuleren uit een reeks

Annuleer je één afspraak uit een reeks, dan blijft de reeks zelf nog steeds staan. Er komt een
regel bij in de database die als exception van de reeks geldt. Bij het tonen van het rooster
wordt die datum overgeslagen en alle andere weken blijven gewoon staan.

Het alternatief was de reeks opknippen in twee reeksen rond het gat. Dat is een stuk
ingewikkelder, en je raakt kwijt dat het oorspronkelijk één afspraak was.

In het scherm krijg je bij een reeks de keuze tussen "alleen deze keer" en "hele reeks".
Bij een losse reservering krijg je die keuze niet, want dan zijn het twee knoppen voor
hetzelfde.

### Een reeks die deels al bezet is

Stel je boekt tien weken en week vijf en zeven zijn al door iemand anders geclaimd.

Ik weiger dan eerst de hele reeks, en vertel erbij welke weken bezet zijn en door wie. In
het scherm zie je die weken staan met daaronder een knop "Boek de overige 8 weken". Als je die klikt
worden alleen de vrije weken geboekt en wordt van de overgeslagen weken
onthouden dat ze zijn overgeslagen.

Dit wordt laten zien in een melding, zodat de gebruiker weet waar hij/zij aan toe is. De overgebleven weken
worden niet stilletjes geboekt.

De controle wordt bij het bevestigen opnieuw gedaan, binnen
hetzelfde slot als hierboven. Als iemand tussen het tonen van de melding en jouw klik nog
snel week drie boekt, krijg je een nieuwe melding in plaats van stilletjes een andere
reeks dan je dacht.

### Tijden zijn kloktijden

"Elke maandag om 10:00" moet 10:00 blijven, ook nadat de klok eind oktober een uur
teruggaat. Sla je een tijdstip op als moment in de tijd, dan verschuift de hele reeks een
uur zodra je die grens passeert.

Daarom bewaar ik datum en tijd los van elkaar, zonder tijdzone, precies zoals iemand ze
intikt. Ook in de frontend blijven het teksten.

Het enige tijdstip dat wél een echt moment is, is het moment waarop een reservering
gemaakt werd. Dat is namelijk iets wat gebeurd is, geen afspraak die nog moet komen.

### Identiteit

Zoals aangegeven in de opdracht log in je in door je eigen naam in te typen. Met deze naam
boek je de ruimtes. Je kan geen ruimtes annuleren van iemand anders. Hierbij is natuurlijk wel het feit
dat er geen echte auth op zit, en je dus gewoon als iemand anders kan inloggen.

### Geen database-server

In verband met de scope van de opdracht heb ik besloten SQLite te gebruiken. Dit bracht een goeie oplossing
voor het concurrency probleem, maar bespaarde ook tijd met de opzet van een database server.

De database wordt bij het opstarten aangemaakt in plaats van via migraties. Dat kan omdat
er geen bestaande database is waarvan de geschiedenis gerespecteerd moet worden. In een echte
use case zou je natuurlijk migraties gebruiken.

### Frontend

Ik heb ng-bootstrap gebruikt omdat dat iets is wat ik standaard in mijn jaren heb gebruikt
voor de vormgeving. De Bootstrap classes besparen ook tijd in het stoppen van css.

Verder is de frontend opgedeeld in containers en componenten. De container haalt de gegevens op
en doet de verzoeken; de componenten daaronder krijgen de gegevens binnen en
geven alleen door wat de gebruiker aanklikt. Het weekrooster weet dus niet dat er een API
bestaat. Dat maakt het makkelijker te volgen, en je kunt het rooster in elke denkbare
toestand tonen zonder dat er iets opgehaald hoeft te worden.

Het rooster toont de ruimtes onder elkaar en de dagen van de week naast elkaar. Zo zie je
per ruimte in één oogopslag wat er die week gepland staat.

## Wat ik zou testen

Er zijn meerdere onderdelen die profijt zouden hebben van unit tests:

- De check dat afspraken niet kunnen overlappen.
- De race condition bij het opslaan van twee boekingen op hetzelfde moment.
- Het expanden van de occurrences bij een reeks.
- Dat de rest van een reeks blijft staan als je er één uit annuleert.
- Dat er bij het bevestigen van een gedeeltelijk conflict opnieuw gecontroleerd wordt.
- Dat je de reservering van iemand anders niet kunt annuleren.
- De validatie: eindtijd na starttijd, geen datum in het verleden en het aantal weken binnen de grenzen.

## Wat ik zou doen met meer tijd

Een afspraak kunnen wijzigen of verplaatsen.

Echte authenticatie en authorisatie.

Een database server met migraties in plaats van de database bij het opstarten aanmaken.

Nette foutmeldingen. De API antwoordt nu met Engelse, technische teksten en de frontend zet die ongefilterd op het scherm. Die zouden
Nederlands en begrijpelijk moeten zijn.
