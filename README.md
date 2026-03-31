RobertsRobotBar

RobertsRobotBar er en WPF-applikation, der styrer en Universal Robots robotarm til automatisk at mixe drinks.
Applikationen er udviklet med fokus på brug ved events og fungerer som en samlet løsning til både robotstyring og håndtering af events, baropsætninger og data.

Systemet gør det muligt at planlægge events, konfigurere bar setups, administrere drinks og samtidig styre selve robotten, som udfører drinkopgaverne.

Funktionalitet
Afvikling af robot scripts via en kø (sekventiel afvikling)
Event-drevet styring af robotten (ingen polling)
Oprettelse og håndtering af events
Gemte bar setups og konfigurationer
Administration af drinks og opskrifter
Mulighed for brugerdefinerede (“mix selv”) drinks
Registrering af data og statistik for events
Integration med Universal Robots via TCP
Teknologi

Projektet er udviklet i:

C# og .NET (WPF)
Clean Architecture og MVVM
Entity Framework Core
SQL database
TCP kommunikation mod robotten
NUnit og Moq til tests
Dependency Injection
Arkitektur

Projektet er struktureret efter Clean Architecture:

Presentation (WPF + MVVM)
UI og ViewModels
Application Layer
Forretningslogik og styring af flow
Domain
Entiteter som events, drinks og bar setups
Infrastructure
Database og robotkommunikation

Formålet med denne opdeling er at holde ansvar adskilt og gøre systemet lettere at vedligeholde og udvide.
