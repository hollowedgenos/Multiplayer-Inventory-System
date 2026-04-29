## What is this?
---

This is a server authoritative inventory system which utilizes Photon Fusion 2. 
The system consists of a local inventory instantiated for each player which then connects with the network. 
Items are added to the inventory upon interaction. The quantity of items are tracked and stat effects assigned to each item are applied to the player.

---

<b>System Name:</b> Inventory System

<b>Summary:</b> An inventory system I created for the game Soulsync, made in Senior Production I and II during 2025 through 2026. The system is a local inventory for each player which connects with the network. The general idea is to allow players to interact with items synced on the network. The item’s stats then applies to the local inventory of the player who interacted with it. The inventory keeps track of different and multiple of the same items inside it and applies the item stats appropriately. 

<b>Technical Constraints/Goals:</b> A major challenge was attempting to serialize item data into a dictionary.  At first, I wanted the key to be the item name and the value to be the item effects. This was not a reliable way to keep track of more than one item in the inventory. I needed to have two dictionaries. The first stored the raw item data as the key, and the value was an integer indicating how many duplicate items there were. The second stored initialized values corresponding to the item effects. By this method, I was able to keep track of more than one of the same items in the inventory and correctly apply the status effects to the player.

<b>Design Pattern Rationale:</b> My rationale centered on the Scriptable Object asset which Unity provides. Through Unity’s scriptable object workflow, less memory would be required when accessing and storing item effects. Scale was not a major concern as the number of items planned to be in the game was not many. Ultimately, it made sense for the inventory system to function using an accumulator pattern to aggregate the player stats from picked up items and apply them to the player.

<b>Software & Language:</b> Unity/C#

<b>Contribution Notes:</b> I  was responsible for the creation of LocalInventory.cs script; [Jalen Carney](https://github.com/JalenCarney) contributed with the tracking of money in the inventory. ItemData.cs was also created by Jalen Carney, however I made major structural rewrites to the script. I created the WorldItem.cs script, but 
it was completely rewritten by [Ryan Pederson](https://github.com/RJP5546). My initial code from that WorldItem.cs was then transferred to the Item.cs script. I also 
minorly contributed to FaceCamera.cs script, which Ryan Pederson created. Ryan Pederson was also responsible for implementing the basic interaction system; 
this included the interactable detector. Lastly, I edited and added to the InteractableDetector.cs script so it could work with player 
inputs via Photon. Jalen Carney was responsible for implementing interaction with chests and shops in the detector script.

<b>External Assets & Libraries:</b> Photon Fusion 2

<b>Optimization Note:</b> During the system’s development period, the item script was initially made to map scriptable object values directly to it. However, this proved to be cumbersome and unrefined in implementation. Ryan Peterson suggested that I reconstruct the item script using a struct to declare item effects. This resulted in the ItemData script which is more elegant in implementation. 
