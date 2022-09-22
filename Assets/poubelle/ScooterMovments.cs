using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScooterMovments : MonoBehaviour
{
    public void GetInput()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");
    }
    private void Steer()
    {
        m_steeringAngle = maxSteerAngle * m_horizontalInput;
        Frontwheel.steerAngle = m_steeringAngle;
    }
    private void Accelerate()
    {
        Frontwheel.motorTorque = m_verticalInput * motorForce;
    }
    private void UpdateWheelPoses()
    {
        UpdateWheelPose(Frontwheel, FrontwheelT);

    }
    private void UpdateWheelPose(WheelCollider m_collider,Transform m_transform)
    {
        Vector3 m_pos = m_transform.position;
        Quaternion m_quat = m_transform.rotation;
        m_collider.GetWorldPose(out m_pos, out m_quat);
        m_transform.position = m_pos;
        m_transform.rotation = m_quat;
    }

    private void FixedUpdate()
    {
        GetInput();
        Steer();
        Accelerate();
        UpdateWheelPoses();
    }
    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steeringAngle;

    public WheelCollider Frontwheel;
    public Collider Handlebar;
    public Transform FrontwheelT;
    public Transform HandlebarT;
    public float maxSteerAngle = 30;
    public float motorForce = 50;
    // Start is called before the first frame update
 
}
