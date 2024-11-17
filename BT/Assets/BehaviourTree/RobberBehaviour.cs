using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobberBehaviour : MonoBehaviour
{
    BehaviourTree tree;

    [SerializeField] GameObject diamond;

    [SerializeField] GameObject van;

    [SerializeField] GameObject backDoor;

    NavMeshAgent agent;

    public enum ActionState
    {
        IDLE,
        WORKING
    };

    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();

        Sequence steal = new Sequence("Steal Something");
        Leaf goToBackDoor = new Leaf("Go To Back Door", GoToBackDoor);
        Leaf goToDiamond = new Leaf("Go To Diamond", GoToDiamond);
        Leaf goToVan = new Leaf("Go To Van", GoToVan);

        steal.AddChild(goToBackDoor);
        steal.AddChild(goToDiamond);
        steal.AddChild(goToBackDoor);
        steal.AddChild(goToVan);
        tree.AddChild(steal);

        tree.PrintTree();
    }

    // Update is called once per frame
    void Update()
    {
        if (treeStatus == Node.Status.RUNNING)
            treeStatus = tree.Process();
    }

    public Node.Status GoToBackDoor()
    {
        return GoToLocation(backDoor.transform.position);
    }

    public Node.Status GoToDiamond()
    {
        return GoToLocation(diamond.transform.position);
    }
    public Node.Status GoToVan()
    {
        return GoToLocation(van.transform.position);
    }

    Node.Status GoToLocation(Vector3 destination)
    {
        float distanceToTarget = Vector3.Distance(destination, this.transform.position);

        if (state == ActionState.IDLE)
        {
            agent.SetDestination(destination);

            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
        {
            state = ActionState.IDLE;

            return Node.Status.FAILURE;
        }
        else if (distanceToTarget < 2)
        {
            state = ActionState.IDLE;

            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }
}
