## MULTIPLAYER INVENTORY SYSTEM
---
### What is this?
This is a server authoritative inventory system which utilizes Photon Fusion 2. 
The system consists of a local inventory instantiated for each player which then connects with the network. 
Items are added to the inventory upon interaction. The quantity of items are tracked and stat effects assigned to each item are applied to the player.


### Contribution Notes
I  was responsible for the creation of LocalInventory.cs script; [Jaylen Carney](https://github.com/JalenCarney) contributed with the tracking of money in the inventory. 
ItemData.cs was also created by Jaylen Carney, however I made major structural rewrites to the script. I created the WorldItem.cs script, but 
it was completely rewritten by [Ryan Peterson](https://github.com/RJP5546). My initial code from that WorldItem.cs was then transferred to the Item.cs script. I also 
minorly contributed to FaceCamera.cs script, which Ryan Peterson created. Ryan Peterson was also responsible for implementing the basic interaction system; 
this included the interactable detector. Lastly, I edited and added to the InteractableDetector.cs script so it could work with player 
inputs via Photon. Jaylen Carney was responsible for implementing interaction with chests and shops in the detector script.
