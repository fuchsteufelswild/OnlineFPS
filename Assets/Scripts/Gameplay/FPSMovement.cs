using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OnlineFPS
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FPSMovement : MonoBehaviour
    {
        private static float maxRotationSpeed = 80.0f;
        private static float minRotationSpeed = 10.0f;

        [Header("Movement Parameters")]
        [SerializeField] private float characterSpeed = 0.0f;
        [SerializeField] private float runningCharacterSpeed;
        [SerializeField] private float maxSpeed = 2.0f;
        [SerializeField] private float rotationSpeed = 20.0f;
        [SerializeField] private float minX = -45;
        [SerializeField] private float maxX = 45;
        [SerializeField] private Transform shoulderTransform;

        [Header("Sounds")]
        [SerializeField] AudioClip walkingSound;
        [SerializeField] AudioClip runningSound;

        private float rotY = 0;
        private float rotX = 0;
        
        private Vector3 movementDirection;
        private float speed;

        private Vector3 previousPosition = Vector3.zero;
        private Vector3 lastPosition = Vector3.zero;

        private Vector3 previousEulerAngles = Vector3.zero;
        private Vector3 lastEulerAngles = Vector3.zero;

        private Vector3 cachedDeadReckoningMoveDirection;
        private (float x, float y) cachedDeadReckoningRotationAmount;

        private CharacterController characterController;
        private AudioSource audioSource;

        public float Speed => speed;
        public float XRotation => rotX;
        public float YRotation => rotY;

        public System.Func<NetworkCommunicationManager> GetCommunicationManager;

        FPSController fpsController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            rotY = transform.localEulerAngles.y;
            rotX = shoulderTransform.localEulerAngles.x;

            fpsController = GetComponent<FPSController>();
        }

        public void AddToRotationX(float value) =>
            rotX += value;

        public void AddToRotationY(float value) =>
            rotY += value;

        public void UpdateMovement()
        {
            (float, float) rotationAmount = ProcessRotation();
            Vector3 movementAmount = ProcessMovement();

            if (NetworkRoomManager.IsInit && !PhotonNetwork.IsMasterClient)
            {
                if (NetworkRoomManager.Instance.CommunicationManager != null)
                    NetworkCommunicationManager.ClientPacket.CreateAndAddMovePacket(movementAmount, rotationAmount.Item1, rotationAmount.Item2);
            }

            PlayFootstepSound();
        }

        /*
         * Update dead reckoning values using new state
         */ 
        public void OnNewStateArrived(Vector3 newPosition, float rotx, float roty)
        {
            previousPosition = lastPosition;

            cachedDeadReckoningMoveDirection = lastPosition - previousPosition;
            
            lastPosition = newPosition;
        }

        public void ApplyDeadReckoning()
        {
            ApplyMovement(cachedDeadReckoningMoveDirection);
        }

        /* 
         * Gets rotation inputs then updates x, y EulerAngles with clamped values
         */
        private (float xRot, float yRot) ProcessRotation()
        {
            float horizontalRotation = InputManager.Instance.HorizontalRotationInput;
            float verticalRotation = InputManager.Instance.VerticalRotationInput;

            float xRotationAmount = verticalRotation * rotationSpeed * Time.deltaTime;
            float yRotationAmount = horizontalRotation * rotationSpeed * Time.deltaTime;

            ApplyRotation(xRotationAmount, yRotationAmount);
            return (xRot: xRotationAmount, yRot: yRotationAmount);

            rotY += horizontalRotation * rotationSpeed * Time.deltaTime;
            rotX -= verticalRotation * rotationSpeed * Time.deltaTime;
            rotX = Mathf.Clamp(rotX, minX, maxX);

            transform.localEulerAngles = new Vector3(rotX, rotY, 0);
        }

        public void ApplyRotation(float xRotationAmount, float yRotationAmount)
        {
            rotY += yRotationAmount;
            rotX -= xRotationAmount;
            rotX = Mathf.Clamp(rotX, minX, maxX);

            transform.localEulerAngles = new Vector3(rotX, rotY, 0);
        }

        /* 
         * Gets movement input from InputManager then transforms it
         * from local space direction into world space. After that,
         * scales, clamps, adds gravity, then feeds this vector to the 
         * CharacterController.
         */
        private Vector3 ProcessMovement()
        {
            movementDirection = Vector3.zero;

            (movementDirection.x, movementDirection.z) = InputManager.Instance.MovementInput;

            movementDirection = transform.TransformDirection(movementDirection);

            movementDirection *= Input.GetKey(KeyCode.LeftShift) ? runningCharacterSpeed : characterSpeed;
            movementDirection = Vector3.ClampMagnitude(movementDirection, maxSpeed);
            movementDirection.y = -0.8f;

            movementDirection *= Time.deltaTime;

            ApplyMovement(movementDirection);

            return movementDirection;
        }

        public void ApplyMovement(Vector3 movementDirection)
        {
            characterController.Move(movementDirection);

            speed = movementDirection.x * movementDirection.x + movementDirection.z * movementDirection.z;
        }

        private void PlayFootstepSound()
        {
            if (characterController.isGrounded && speed > 0.01f)
            {
                audioSource.clip = Input.GetKey(KeyCode.LeftShift) ? runningSound : walkingSound;
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Pause();
            }
        }
    }
}