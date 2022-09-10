using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public class AppCanvasSlideEffect : AbstractTransitionEffect
    {
        private enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right,
        }

        //public Utilities.EffectDirection _direction;
        [SerializeField]
        private bool useAutoDirection = false;
        [SerializeField]
        [Tooltip("Used if UseAutoDirection is set to false or when previous/next state is null.")]
        private MoveDirection moveDirection = MoveDirection.Up;
        [SerializeField]
        protected float _duration = .5f;
        [SerializeField]
        private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 target;
        private Vector3 origin;
        private RectTransform rt;

        private void Start()
        {
            rt = Owner.GetComponent<RectTransform>();
            origin = ((CanvasSubState)Owner).EditorPosition;

            if (!useAutoDirection)
            {
                SetStaticTarget();
            }
        }

        public override IEnumerator LaunchStart()
        {
            if (useAutoDirection)
            {
                UpdateDirection();
            }

            Vector3 from = Vector3.zero, to = Vector3.zero;

            if (IsEnterTransition)
            {
                from = target;
            }
            else
            {
                to = -target;
            }

            yield return StartCoroutine(Utilities.DelegateLerpDuration((float val) =>
            {
                var t = _moveCurve.Evaluate(val);
                rt.anchoredPosition = Vector3.Lerp(from, to, t);

            }, Utilities.EffectDirection.Forward, _duration, _delay, _useTimeScale));

            if (!IsEnterTransition)
            {
                rt.anchoredPosition = origin;
            }
        }

        private void SetStaticTarget()
        {
            target = Vector3.zero;

            switch (moveDirection)
            {
                case MoveDirection.Up:
                    {
                        target += Vector3.down * rt.rect.height;
                        break;
                    }
                case MoveDirection.Down:
                    {
                        target += Vector3.up * rt.rect.height;
                        break;
                    }
                case MoveDirection.Left:
                    {
                        target += Vector3.right * rt.rect.width;
                        break;
                    }
                case MoveDirection.Right:
                    {
                        target += Vector3.left * rt.rect.width;
                        break;
                    }
            }
        }

        private void UpdateDirection()
        {
            if ((IsEnterTransition && Owner.ParentStateMachine.PreviousSubState == null) || (!IsEnterTransition && Owner.ParentStateMachine.NextSubState == null))
            {
                SetStaticTarget();
            }
            else
            {
                Vector3 direction = (IsEnterTransition ? ((CanvasSubState)Owner.ParentStateMachine.PreviousSubState).EditorPosition - origin : ((CanvasSubState)Owner.ParentStateMachine.NextSubState).EditorPosition - origin).normalized;

                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    if (direction.x > 0)
                    {
                        direction = Vector3.right;
                    }
                    else
                    {
                        direction = Vector3.left;
                    }
                }
                else
                {
                    if (direction.y > 0)
                    {
                        direction = Vector3.up;
                    }
                    else
                    {
                        direction = Vector3.down;
                    }
                }

                target = (IsEnterTransition ? 1 : -1) * (Vector3.zero - direction * (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? rt.rect.width : rt.rect.height));
            }
        }
    }
}