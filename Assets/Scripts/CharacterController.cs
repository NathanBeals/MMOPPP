using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CharacterController : MonoBehaviour
{
    // Related Components
    Character mCharacter;

    // Inputs
    PlayerInputActions inputActions;
    Vector2 mMovementInput;
    float mRotationInput;

    // Scaling
    [SerializeField] float MaxVelocity = 10.0f;
    [SerializeField] float RotationRate = 10.0f;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.Move.performed += ctx => mMovementInput = ctx.ReadValue<UnityEngine.Vector2>();
        inputActions.PlayerControls.Rotate.performed += ctx => mRotationInput = ctx.ReadValue<float>();
        mCharacter = GetComponent<Character>();
    }

    void FixedUpdate()
    {
        if (!mCharacter.Local)
        {
            //TODO: test ai insertion point
            //TODO: remove
            gameObject.transform.position =
                gameObject.transform.position
                + gameObject.transform.forward * .1f * Time.deltaTime * MaxVelocity
                + gameObject.transform.right * .1f * Time.deltaTime * MaxVelocity;

            return;
        }

        // Standard movement
        var velocity = Time.deltaTime * MaxVelocity;
        gameObject.transform.position = 
            gameObject.transform.position 
            + gameObject.transform.forward * mMovementInput.normalized.y * velocity
            + gameObject.transform.right * mMovementInput.normalized.x * velocity;

        // Turning
        gameObject.transform.Rotate(new Vector3(0, mRotationInput * RotationRate * Time.deltaTime, 0));
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
