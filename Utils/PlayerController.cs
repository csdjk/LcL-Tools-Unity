using UnityEngine;
using System.Collections;

namespace LcLTools
{
    [AddComponentMenu("LcLTools/PlayerController", 0)]
    public class PlayerController : MonoBehaviour
    {
        public enum PlayerControlMode { FirstPerson, ThirdPerson }

        /*
        Writen by Windexglow 11-13-10. Use it, edit it, steal it I don't care.
        Converted to C# 27-02-13 - no credit wanted.
        Simple flycam I made, since I couldn't find any others made public.
        Made simple to use (drag and drop, done) for regular keyboard layout
        wasd : basic movement
        shift : Makes camera accelerate
        space : Moves camera on X and Z axis only. So camera doesn't gain any height*/
        public PlayerControlMode controlMode = PlayerControlMode.FirstPerson;
        public float mainSpeed = 10.0f; //regular speed
        public float shiftAdd = 10.0f; //multiplied by how long shift is held. Basically running
        float maxShift = 1000.0f; //Maximum speed when holdin gshift
        public float camSens = 0.25f; //How sensitive it with mouse
        private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
        private float totalRun = 1.0f;

        private bool isMouseRotation = true;

        void Update()
        {
            if (controlMode == PlayerControlMode.FirstPerson)
            {
                RotationCamera();
            }
            Translate();
        }

        private void RotationCamera()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 0) return;

                Touch touch = Input.GetTouch(0);
                bool isMatching = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Moved && touch.position.x >= Screen.width / 2)
                    {
                        touch = Input.GetTouch(i);
                        isMatching = true;
                        break;
                    }
                }
                if (!isMatching) return;

                //旋转Camera
                Vector2 touchDeltaPosition = touch.deltaPosition;
                lastMouse = new Vector3(-touchDeltaPosition.y * camSens, touchDeltaPosition.x * camSens, 0);
                lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
                transform.eulerAngles = lastMouse;
            }
            else
            {
                if (Input.GetKey(KeyCode.Escape))
                {
                    isMouseRotation = !isMouseRotation;
                }
                if (isMouseRotation)
                {
                    lastMouse = Input.mousePosition - lastMouse;
                    lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
                    lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
                    transform.eulerAngles = lastMouse;
                    lastMouse = Input.mousePosition;
                }
            }
        }

        private void Translate()
        {
            Vector3 velocity = GetBaseInput();
            if (velocity.sqrMagnitude > 0)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    totalRun += Time.deltaTime;
                    velocity = velocity * totalRun * shiftAdd;
                    velocity.x = Mathf.Clamp(velocity.x, -maxShift, maxShift);
                    velocity.y = Mathf.Clamp(velocity.y, -maxShift, maxShift);
                    velocity.z = Mathf.Clamp(velocity.z, -maxShift, maxShift);
                }
                else
                {
                    totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                    velocity = velocity * mainSpeed;
                }

                velocity = velocity * Time.deltaTime;
                Vector3 newPosition = transform.position;
                if (Input.GetKey(KeyCode.Space) && controlMode == PlayerControlMode.FirstPerson)
                {
                    // 只在X轴和Z轴上移动
                    transform.Translate(velocity);
                    newPosition.x = transform.position.x;
                    newPosition.z = transform.position.z;
                    transform.position = newPosition;
                }
                else
                {
                    transform.Translate(velocity);
                }
            }
        }
        Vector2 startPos;
        Vector2 dir;

        private Vector3 GetBaseInput()
        {
            Vector3 p_Velocity = new Vector3();
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 0) return p_Velocity;

                Touch touch = Input.GetTouch(0);
                bool isMatching = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    touch = Input.GetTouch(i);
                    var isLeft = touch.position.x < Screen.width / 2;
                    if (touch.phase == TouchPhase.Began && isLeft)
                    {
                        startPos = touch.position;
                        break;
                    }else if (touch.phase == TouchPhase.Moved && isLeft)
                    {
                        dir = (touch.position - startPos).normalized;
                        isMatching = true;
                        break;
                    }else if (touch.phase == TouchPhase.Stationary && isLeft)
                    {
                        dir = (touch.position - startPos).normalized;
                        isMatching = true;
                        break;
                    }else if (touch.phase == TouchPhase.Ended && isLeft)
                    {
                        isMatching = false;
                        break;
                    }
                }

                if (!isMatching) return p_Velocity;
                Vector2 touchDeltaPosition = touch.deltaPosition;
                // p_Velocity += new Vector3(touchDeltaPosition.x, 0, touchDeltaPosition.y).normalized;
                p_Velocity += new Vector3(dir.x, 0, dir.y);


            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                {
                    p_Velocity += new Vector3(0, 0, 1);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    p_Velocity += new Vector3(0, 0, -1);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    p_Velocity += new Vector3(-1, 0, 0);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    p_Velocity += new Vector3(1, 0, 0);
                }
            }
            return p_Velocity;
        }
    }
}