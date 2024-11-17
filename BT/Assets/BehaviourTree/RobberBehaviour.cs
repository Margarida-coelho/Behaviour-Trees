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

    [SerializeField] GameObject frontDoor;

    NavMeshAgent agent;

    public enum ActionState
    {
        IDLE,
        WORKING
    };

    ActionState state = ActionState.IDLE;

    Node.Status treeStatus = Node.Status.RUNNING;

    [Range(0, 1000)]
    public int money = 800;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        tree = new BehaviourTree();

        Sequence steal = new Sequence("Steal Something");
        Selector openDoor = new Selector("Open Door");
        Leaf hasGotMoney = new Leaf("Has Got Money", HasEnoughMoney);
        Leaf goToBackDoor = new Leaf("Go To Back Door", GoToBackDoor);
        Leaf goToFrontDoor = new Leaf("Go To Front Door", GoToFrontDoor);
        Leaf goToDiamond = new Leaf("Go To Diamond", GoToDiamond);
        Leaf goToVan = new Leaf("Go To Van", GoToVan);

        openDoor.AddChild(goToBackDoor);
        openDoor.AddChild(goToFrontDoor);

        steal.AddChild(hasGotMoney);
        steal.AddChild(openDoor);
        steal.AddChild(goToDiamond);
        steal.AddChild(goToVan);
        tree.AddChild(steal);

        tree.PrintTree();
    }

    // Update is called once per frame
    void Update()
    {
        if (treeStatus != Node.Status.SUCCESS)
            treeStatus = tree.Process();
    }

    public Node.Status HasEnoughMoney()
    {
        if (money >= 500) return Node.Status.FAILURE;
        return Node.Status.SUCCESS;
    }

    public Node.Status GoToFrontDoor()
    {
        return CheckDoor(frontDoor);
    }

    public Node.Status GoToBackDoor()
    {
        return CheckDoor(backDoor);
    }

    public Node.Status CheckDoor(GameObject d)
    {
        Node.Status s = GoToLocation(d.transform.position);

        if (s == Node.Status.SUCCESS)
        {
            if (!d.GetComponent<Lock>().isLocked)
            {
                d.SetActive(false);
                s = Node.Status.SUCCESS;
            }
            else
                s = Node.Status.FAILURE;
        }

        return s;
    }

    public Node.Status GoToDiamond()
    {
        return PickUpObject(diamond);
    }

    public Node.Status PickUpObject(GameObject obj)
    {
        Node.Status s = GoToLocation(obj.transform.position);

        if (s == Node.Status.SUCCESS)
        {
            obj.transform.parent = this.transform;
        }

        return s;
    }

    public Node.Status GoToVan()
    {
        Node.Status s = GoToLocation(van.transform.position);

        if (s == Node.Status.SUCCESS)
        {
            money += 1500000;
            diamond.SetActive(false);
        }

        return s;
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
