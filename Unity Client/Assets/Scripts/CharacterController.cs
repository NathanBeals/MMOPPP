using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CharacterController : MonoBehaviour
{
    // Related Components
    Character m_Character;

    // Inputs
    PlayerInputActions m_InputActions;
    Vector2 m_MovementInput;
    float m_RotationInput;

    // Scaling
    [SerializeField] float m_MaxVelocity = 10.0f;
    [SerializeField] float m_RotationRate = 10.0f;

    void Awake()
    {
        m_InputActions = new PlayerInputActions();
        m_InputActions.PlayerControls.Move.performed += ctx => m_MovementInput = ctx.ReadValue<UnityEngine.Vector2>();
        m_InputActions.PlayerControls.Rotate.performed += ctx => m_RotationInput = ctx.ReadValue<float>();
        m_Character = GetComponent<Character>();
    }

    void FixedUpdate()
    {
        if (!m_Character.m_Local)
        {
            //TODO: test ai insertion point
            //TODO: remove
            gameObject.transform.position =
                gameObject.transform.position
                + gameObject.transform.forward * .1f * Time.deltaTime * m_MaxVelocity
                + gameObject.transform.right * .1f * Time.deltaTime * m_MaxVelocity;

            return;
        }

        // Standard movement
        var velocity = Time.deltaTime * m_MaxVelocity;
        gameObject.transform.position = 
            gameObject.transform.position 
            + gameObject.transform.forward * m_MovementInput.normalized.y * velocity
            + gameObject.transform.right * m_MovementInput.normalized.x * velocity;

        // Turning
        gameObject.transform.Rotate(new Vector3(0, m_RotationInput * m_RotationRate * Time.deltaTime, 0));
    }

    private void OnEnable()
    {
        m_InputActions.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Disable();
    }
}
