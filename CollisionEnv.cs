using UnityEngine;

public class CollisionEnv : MonoBehaviour
{
    public GameObject area;
    [HideInInspector] public EnvController envController;
    public string redTouchBlue; //will be used to check if collided with red agent
    public string blueGoalTag; //will be used to check if collided with delivery zone
    public string wall; //will be used to check if collided with wall

    // Start is called before the first frame update
    void Start()
    {
        envController = area.GetComponent<EnvController>();
    }

    void OnCollisionEnter(Collision col)
    {
        
        if (col.gameObject.CompareTag(blueGoalTag)) //Blue agent touch green zone
        {
            envController.PointScored(0);
        }
        
        if (col.gameObject.CompareTag(redTouchBlue)) //Red agent caught blue agent
        {
            envController.PointScored(1);
        }

        if (col.gameObject.CompareTag(wall)) //Blue agent touch a wall
        {
            envController.PointScored(2);
        }
    }
}