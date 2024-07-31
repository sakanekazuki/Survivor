using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUpdateManager : MonoBehaviour
{
    public float UpdateDeltaTime = 0.02f;
    private float CurrentTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //CurrentTime += Time.deltaTime;
        //if(CurrentTime >= UpdateDeltaTime)
        {
            
            CurrentTime = 0f;
        }
    }
    private void FixedUpdate()
    {
        //Physics2D.Simulate(Time.fixedDeltaTime);
    }
}
