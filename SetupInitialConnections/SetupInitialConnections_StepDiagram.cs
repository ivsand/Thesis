﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupInitialConnections_StepDiagram : MonoBehaviour
{
    [SerializeField] public List<UIC_Node> fromNodes = new List<UIC_Node>();
    [SerializeField] public List<UIC_Node> toNodes = new List<UIC_Node>();
    public int size = 1;

    private GameObject TryAgainWindow;
    private GameObject CorrectWindow;
    private GameObject Canvas;

    void Start()
    {
        TryAgainWindow = GameObject.Find("TryAgainStepDiagram");
        CorrectWindow = GameObject.Find("CorrectStepDiagram");
        Canvas = GameObject.Find("Canvas - Manager_StepDiagram");
        Canvas.SetActive(true);
        TryAgainWindow.SetActive(false);
        CorrectWindow.SetActive(false);
    }
    public void CheckConnections()
    {
        bool correctConnections = CheckCorrectConnections();
        if (correctConnections)
        {
            // Debug.Log("correct!");
            CorrectWindow.SetActive(true);
            
        }
        else
        {
            // Debug.Log("try again");
            TryAgainWindow.SetActive(true);
        }
    }

    public bool CheckCorrectConnections()
    {
        int connCount = UIC_Manager.ConnectionsList.Count;

        // check if amount of correct connections is not the same as overall connections  
        if (size != connCount)
        {
            return false;
        }

        for (int i = 0; i < connCount; i++)
        {
            // use api to verify if nodes are connected 
            UIC_Connection connToCheck = UIC_Manager.NodesAreConnected(fromNodes[i], toNodes[i]);

            // check if right connections are not connected
            if (connToCheck == null)
            {
                return false;
            }
        }

        // if pass all, connections are correct
        return true;
    }
}