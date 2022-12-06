# BikeLetsGo
## How to launch the project ? 
### 1.Start the server
Go to [Serveur/ServerBike2/bin/Debug](Serveur/ServerBike2/bin/Debug) and launch as an Administrator ServerBike2.exe
<br> Then go to [Serveur/ServerProxy/bin/Debug](Serveur/ServerProxy/bin/Debug) and launch as an Administrator ServerProxy.exe
<br>You can also launch both files in their respective projects by using Visual Studio as an Administrator and launching each projects with the green arrow
### 2. Launch the JAVA Client
Open your favorite coding environnement and execute the main method in the file Client.java ! 
<br>(Client.java is located at [Client/BikeClient/src/main/java/com/soc/wsclient](Client/BikeClient/src/main/java/com/soc/wsclient) )
<br>You can also find some adress examples in the Exemples.txt file in that same folder.
### 3. Let's go biking ! 
The path is yours ! Choose a depature point and a arrival point, the application will find you an itinerary with the most bike trajectory possible !
<br>You can also choose to get a very detailled itinerary or just the adress of every main point of your travel.
<br>After every request, you'll also get a map to show you the way !
### 4. Little Warning
Please note that this application is optimized for a travel between the same municipality. If you try to travel from Nice to Paris, the bike itinerary will be optimize only in Nice since there are no bike contracts between the two cities ! 
<br>Also if the travel to go to the bike station is too big, you'll get a warning which will advice you to find another transportation.
