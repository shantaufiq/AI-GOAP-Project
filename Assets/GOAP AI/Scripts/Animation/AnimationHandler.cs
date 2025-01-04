using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AnimationHandler : MonoBehaviour
{
    public NavMeshAgent myAgent;
    public Animator myAnimator;

    private void OnValidate()
    {
        if (!myAgent) myAgent = GetComponent<NavMeshAgent>();
        if (!myAnimator) myAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (myAgent.hasPath)
        {
            var dir = (myAgent.steeringTarget - this.transform.position).normalized;
            var animDir = this.transform.InverseTransformDirection(dir);
            var isFacingMoveDirection = Vector3.Dot(dir, transform.forward) > .5f;

            myAnimator.SetFloat("HorizontalX", isFacingMoveDirection ? animDir.x : 0, 0.5f, Time.deltaTime);
            myAnimator.SetFloat("VerticalZ", isFacingMoveDirection ? animDir.z : 0, 0.5f, Time.deltaTime);

            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(dir), 180 * Time.deltaTime);

            if (Vector3.Distance(transform.position, myAgent.destination) < myAgent.radius)
            {
                myAgent.ResetPath();
            }
        }
        else
        {
            myAnimator.SetFloat("HorizontalX", 0, 0.25f, Time.deltaTime);
            myAnimator.SetFloat("VerticalZ", 0, 0.25f, Time.deltaTime);
        }

        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var isHit = Physics.Raycast(ray, out RaycastHit hit, 20);
            if (isHit)
            {
                // Debug.Log($"hit the plane {hit.point}");
                myAgent.destination = hit.point;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (myAgent.hasPath)
        {
            for (var i = 0; i < myAgent.path.corners.Length - 1; i++)
            {
                Debug.DrawLine(myAgent.path.corners[i], myAgent.path.corners[i + 1], Color.blue);
            }
        }
    }
#endif
}
