using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public float RPM;
    [SerializeField] float m_gearCurveTime = 0f;//

    [Header("RPM")]
    public float RedZoneRPM;
    public float IdleRPM;
    [SerializeField] float m_rpmAcceleration;
    [SerializeField] float m_rpmDeceleration;
    [Space(10f)]

    [Header("기어비")]
    [SerializeField] AnimationCurve m_gearCurve;
    [SerializeField] AnimationCurve m_inverseGearCurve; //역함수 곡선이 멋대로 그려짐, 코드로 커브를 구현하기로 바꿈
    [Space(10f)]

    [SerializeField] AnimationCurve m_motorTorque;
    [SerializeField] float m_brakeTorque;

    void Awake()
    {
        RPM = IdleRPM;

        //create inverse speedcurve
        m_inverseGearCurve = new AnimationCurve();
        for (int i = 0; i < m_gearCurve.length; i++)
        {
            Keyframe inverseKey = new Keyframe(m_gearCurve.keys[i].value, m_gearCurve.keys[i].time);
            m_inverseGearCurve.AddKey(inverseKey);
        }
    }

    void Update()
    {
        if (InputManager.Instance.InputForward)
        {
            if (m_gearCurveTime < m_inverseGearCurve.Evaluate(RedZoneRPM + 10f))
            {
                m_gearCurveTime += m_rpmAcceleration * Time.deltaTime;
                RPM = m_gearCurve.Evaluate(m_gearCurveTime);
            }
            else
            {
                m_gearCurveTime = m_inverseGearCurve.Evaluate(RedZoneRPM + 10f) - 1f;
                RPM = m_gearCurve.Evaluate(m_gearCurveTime);
            }
        }
        else
        {
            if (m_gearCurveTime > m_inverseGearCurve.Evaluate(IdleRPM)) //TODO :: 수정
            {
                m_gearCurveTime -= m_rpmDeceleration * Time.deltaTime;
                RPM = m_gearCurve.Evaluate(m_gearCurveTime);
            }
            else
            {
                m_gearCurveTime = m_inverseGearCurve.Evaluate(IdleRPM);
                RPM = m_inverseGearCurve.Evaluate(m_gearCurveTime);
            }
        }
    }
}
