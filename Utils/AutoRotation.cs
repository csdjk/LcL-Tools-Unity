using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LcLTools
{
    [AddComponentMenu("LcLTools/AutoRotation")]
    public class AutoRotation : MonoBehaviour
    {
        public GameObject target;
        public float speed = 10f;
        private Vector3 mouseReference;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseReference = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 displacement = mouseReference - Input.mousePosition;
                mouseReference = Input.mousePosition;
                float rotation = displacement.x * speed * Time.deltaTime;

                if (target)
                {
                    // 绕目标旋转
                    transform.RotateAround(target.transform.position, Vector3.up, rotation);
                }
                else
                {
                    // 绕自身旋转
                    transform.RotateAround(transform.position, Vector3.up, rotation);
                }
            }
            else
            {
                // 自动旋转
                if (target)
                {
                    transform.RotateAround(target.transform.position, Vector3.up, speed * Time.deltaTime);
                }
                else
                {
                    transform.RotateAround(transform.position, Vector3.up, speed * Time.deltaTime);
                }
            }
        }
    }
}
