*  AI / RTS Project - Thomas Aubart  *

 - Inputs:

=> Double click on any colored square in the upper left corner to create Units.

* "0" : Unselect every Units
* "1" : Select "Light" units (Yellow)
* "2" : Select "Medium" units (Blue)
* "3" : Select "Heavy" units (Purple)
* "4" : Switch to "Arrow" formation
* "5" : Switch to "Square" formation 
* "6" : Switch to "Circle" formation

 - Implemented Feature:
 
* Unit Movements &Navigation

* Formation Movements (3 Different types)

* Basic AI ( Create units until you hit max unit count, they move randomly on the map)

 - Techniques used:

* Navigation :		A* Algorithm
* Mouvement :		Steering
* Group :		Abstract commanding units, only one Pathfind for the entire group.
* Formation : 		Dynamic pattern creation depending on unit number in the group -> Each one have its target created from the only Pathfind

 - Limitations:

1) No Collision between units
2) Collision with wall partialy functionnal, mostly when recreating formation or gathering units
3) Group of units will only move at the speed of the slowest unit in the group, to avoid dispersion

 - Improvements possible:

1) Movements prevision & Movement priority :
--> Avoid unit to overlap & enable to move smoother


2) Create A* that manage group size:
--> Path found would be further from the wall and would let every unit to move with ease

3) Formation Manager that know if every unit is actually with the other or if she is trying to catch up
--> Let the group slow down to let every unit catch up, possibly depending on their distance to the group