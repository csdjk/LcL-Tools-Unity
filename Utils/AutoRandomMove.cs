using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LcLTools
{
    [AddComponentMenu("LcLTools/AutoRandomMove")]
    public class AutoRandomMove : MonoBehaviour
    {
        public float radius = 1f;
        [Range(0.1f, 100)]
        public float speed = 1;
        public int seed = 0;
        public bool isFixedMode = false;
        public Vector3 fixedStartPos;
        public Vector3 fixedEndPos;

        private Vector3 currentTarget;
        private Vector3 currentPos;
        private float moveTime;
        private float time;
        private float speedCache;
        private bool isMovingForward = true;

        void Start()
        {
            ResetPosition();
        }

        void ResetPosition()
        {
            if (!isFixedMode)
            {
                Random.InitState(seed++);
                var pos = Random.insideUnitCircle * radius;
                currentTarget = new Vector3(pos.x, transform.position.y, pos.y);
            }
            else
            {
                currentTarget = fixedStartPos;
            }
            currentPos = transform.position;
            moveTime = 10 / speed * Vector3.Distance(currentTarget, currentPos);
            time = 0;
        }

        void Update()
        {
            time += Time.deltaTime;
            if (time > moveTime)
            {
                if (!isFixedMode)
                {
                    ResetPosition();
                }
                else
                {
                    if (isMovingForward)
                    {
                        currentTarget = fixedEndPos;
                    }
                    else
                    {

                        currentTarget = fixedStartPos;
                    }
                    currentPos = transform.position;
                    isMovingForward = !isMovingForward;
                    moveTime = 10 / speed * Vector3.Distance(currentTarget, currentPos);
                    time = 0;
                }
            }
            float lerpValue = time / moveTime;
            Vector3 newPos = Vector3.Lerp(currentPos, currentTarget, lerpValue);
            transform.position = newPos;
        }
    }
}