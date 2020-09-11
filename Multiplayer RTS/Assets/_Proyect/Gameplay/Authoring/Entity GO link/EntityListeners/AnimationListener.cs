using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(EntityFilter), typeof(Animator))]
public class AnimationListener : MonoBehaviour
{
    private static Vector2[] directionPoints = new Vector2[]
    {
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(0,-1),
        new Vector2(-1,0)
    };

    private Animator animator;
    private EntityFilter entityFilter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        entityFilter = GetComponent<EntityFilter>();
    }
    private void Start()
    {
        var entity = entityFilter.Entity;
        var entityManager = entityFilter.EntityManager;
        bool hasDirectionAverage = entityManager.HasComponent<DirectionAverage>(entity);
        Debug.Assert(hasDirectionAverage, "the Animation Listener component requires that the entity have a DirectionAverage comp");
    }

    private void Update()
    {
        if (! entityFilter.EntityManager.HasComponent<DirectionAverage>(entityFilter.Entity))
            return;

        var hexDirection = entityFilter.EntityManager.GetComponentData<DirectionAverage>(entityFilter.Entity).Value;
        var direction = FractionalHex.HexSpaceToCartesianSpace(hexDirection, Orientation.pointy);

        if(direction == Vector2.zero)
        {
            animator.SetBool("Moving", false);
            return;
        }    
        else
            animator.SetBool("Moving", true);


        if(directionPoints.Length > 0)
        {
            int bestDirIndex = 0;
            var vectorWeight = directionPoints[0] - direction;
            float bestDirWeight = Mathf.Abs(vectorWeight.x) + Mathf.Abs(vectorWeight.y);
            for (int i = 1; i < directionPoints.Length; i++)
            {
                var currVector = directionPoints[i] - direction;
                float currWeight = Mathf.Abs(currVector.x) + Mathf.Abs(currVector.y);
                if(currWeight < bestDirWeight)
                {
                    bestDirIndex = i;
                    bestDirWeight = currWeight;
                }
            }

            animator.SetFloat("Direction", bestDirIndex);
        }


    }
}
