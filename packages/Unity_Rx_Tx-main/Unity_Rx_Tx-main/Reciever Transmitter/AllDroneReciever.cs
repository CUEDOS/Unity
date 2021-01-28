using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllDroneReciever : MonoBehaviour
{
    //Drone RF Specification

    // public GameObject Transmitter;

    //[Label("Dbi")]
    public float Drone_RX_Antenna_Gain = 5f; //dB

    public float Minimum_Detectable_RF_Signal = -100f; //dBm
    public float Received_Transmitter_Signal_Power = 0f;
    public float Received_Jammer_Signal_Power = -200f;
    public int signalDropOuts = -1;

    // Start is called before the first frame update
    void Start()
    {
        // Including this so the drones with no transmitters will stay red
        Received_Transmitter_Signal_Power = Minimum_Detectable_RF_Signal - 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // This is now done by the transmitter itself
        //Transmitter transmitter = Transmitter.GetComponent<Transmitter>();
        //Recieved_Transmitter_Signal_Power = transmitter.Recieved_Transmitter_Signal_Power;


        //check if the drone has flown too far away from the GCS and lost signal
        if (Received_Transmitter_Signal_Power < Minimum_Detectable_RF_Signal)
        {
            signalDropOuts = signalDropOuts + 1;
            GetComponent<Renderer>().material.color = new Color(255, 0, 0); //red
            if (signalDropOuts >= 1)
            {
                Debug.Log("Signal Lost");
                gameObject.layer = 15;
            }
        }

        else if (Received_Jammer_Signal_Power > Received_Transmitter_Signal_Power)
        {
            Debug.Log("Signal has been jammed");
            gameObject.layer = 15;
        }

        else
        {
            //All is good continue with the flight
            GetComponent<Renderer>().material.color = new Color(0, 255, 0); //green
        }


    }

    public void Reset()
    {
        signalDropOuts = 0;
    }
}

