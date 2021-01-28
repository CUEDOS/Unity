In the package there are 3 prefabs:
1. Drone
2. Enviroment
3. GCS

The Drone has the AllDroneReciever.cs script applied, and the Enviroment prefab has the AllDroneTransmitter.cs script applied.

The AllDroneTransmitter script locates any AllDroneRecivers scripts in the enviroment and casts a ray towards the ridgidbody they are attached to.
The entry and exit points of the ray passing through objects is recorded.
These points are then used to calculate the distance the signal has passed through a type of object.
The two types of object being air, and building.
Each object has a different attenuatuation factor which can be adjusted in the script.
The signal recieved by the drone is calculated as the signal transmitted * tx_antenna_gain * rx_antenna_gain - air attenuation - building attenuation.

The antenna gains, minimum recieved signal strength, and transmission power are all modified within the inspector. The units are dB, dBm and mW respectively.

To use the scripts in your own enviroment simply place to reciver script on the drone, and the transmitter scipt on your GCS. The scripts will find each other using their names so do not modify them unless you know what you are doing.

Note*

When applying the scripts to your own drone and GCS you must add a floor, roof, and wall to your enviroment. No objects can exist outside of these walls. The walls must be the final object the ray hits as "reflected" will be cast from the point to give the exit points of the signal through objects. (see tutorials on bullet penetration mechanics for a better explanation)



