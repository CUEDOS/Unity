using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AllDroneTransmitter : MonoBehaviour
{
    // Persisting variables
    Vector3 Transmitter_Position;
    Vector3 Drone_Position;
    Vector3 Forward_Direction;
    Vector3 Reverse_Direction;
    float Distance_to_Drone;
    AllDroneReciever[] receivers;
    RaycastHit[] Signal_Ray_Hits;
    RaycastHit[] Signal_Ray_Hits_Rev;
    public List<float> Relative_Thickness = new List<float>();

    [Tooltip("Units: mW")]
    public float Transmitter_Signal_Power = 25; //mW
    [Tooltip("Units: dB/Km")]
    public float Building_Attenuation = 250f;   // dB/Km assuming 1 wall every 10m of 2.5 dB
    [Tooltip("Units: dB")]
    public float Transmitter_TX_Antenna = 5f;   //dB
    [Tooltip("Units: MHz")]
    public float Transmission_Frequency = 2400; //Mhz

    // Changed to floats to save cpu effort of converting between float and double, you won't lose
    // precision because the values you do your maths on were floats in the first place
    float dB_Loss_Air;
    float dB_Loss_Buildings;
    float dB_Loss_Total;
    public float[] Received_Transmitter_Signal_Powers;

    LineDrawer lineDrawer;

    // Start is called before the first frame update
    void Start()

    {
        receivers = FindObjectsOfType<AllDroneReciever>();
        Received_Transmitter_Signal_Powers = new float[receivers.Length];

        lineDrawer = new LineDrawer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Transmitter_Position = transform.position;

        for (int i = 0; i < receivers.Length; i++)
        {
            Signal_Monitor_Attenuated(i);
        }
        
    }

    void Signal_Monitor_Attenuated(int index)
    {
        // Get all the position and direction data
        Drone_Position = receivers[index].transform.position;
        Forward_Direction = Drone_Position - Transmitter_Position;
        Distance_to_Drone = Vector3.Distance(Transmitter_Position, Drone_Position);
        Reverse_Direction = (Forward_Direction) * -1;

        // Cast rays between the drone and transmitter
        Signal_Ray_Hits = Physics.RaycastAll(Transmitter_Position, Forward_Direction, Distance_to_Drone).OrderBy(h => h.distance).ToArray();
        Signal_Ray_Hits_Rev = Physics.RaycastAll(Drone_Position, Reverse_Direction, Distance_to_Drone).OrderBy(h => h.distance).ToArray();

        Relative_Thickness.Clear();
        // Moving from end to beginning
        for (int i = Signal_Ray_Hits.Length - 1; i > 0; i--)
        {
            // Using Vector3.Distance to get the distance between points
            Relative_Thickness.Add(Vector3.Distance(Signal_Ray_Hits_Rev[i - 1].point, Signal_Ray_Hits[Signal_Ray_Hits.Length - (i + 1)].point));
        }

        dB_Loss_Buildings = 0;
        // Don't need to check for the drone, the array only goes up to the drone
        for (int i = 0; i < Signal_Ray_Hits.Length - 1; i++)
        {
            dB_Loss_Buildings += ((Relative_Thickness[i] / 1000f) * Building_Attenuation) + 20; //20 db loss added for window/wall in out/ insulation etc
        }

        //Free Space path Loss equation in decibels constant for metres and megahertz
        dB_Loss_Air =   20f * Mathf.Log10(Distance_to_Drone)
                      + 20f * Mathf.Log10(Transmission_Frequency)
                      - 27.55f; 

        dB_Loss_Total = dB_Loss_Buildings + dB_Loss_Air;

        // first bit is converting from mW to dBm (Tx Pwr) https://www.electronics-notes.com/articles/antennas-propagation/propagation-overview/radio-link-budget-formula-calculator.php
        Received_Transmitter_Signal_Powers[index] = (10f * Mathf.Log10(Transmitter_Signal_Power)) + (Transmitter_TX_Antenna + receivers[index].Drone_RX_Antenna_Gain - dB_Loss_Total);

        // Tell the drone what signal it is receiving
        receivers[index].Received_Transmitter_Signal_Power = Received_Transmitter_Signal_Powers[index];

        lineDrawer.DrawLineInGameView(transform.position, receivers[0].transform.position, Color.blue);
    }

}
