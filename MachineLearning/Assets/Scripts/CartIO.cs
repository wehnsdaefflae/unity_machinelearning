using System;
using UnityEngine;


public class CartIO : MonoBehaviour
{
    float timeLast;

    // objects
    protected Rigidbody cart;
    protected Rigidbody pole;
    protected Rigidbody tip;

    // agent
    private QLearning qLearning;

    // io
    public double[] sensor;
    public float reward;

    // Start is called before the first frame update
    private void Start() {
        
    }

    void Awake()
    {
        this.timeLast = Time.realtimeSinceStartup;

        this.cart = GetComponent<Rigidbody>();

        GameObject objectPole = this.transform.GetChild(0).gameObject;
        this.pole = objectPole.GetComponent<Rigidbody>();

        GameObject objectTip = this.transform.GetChild(1).gameObject;
        this.tip = objectTip.GetComponent<Rigidbody>();

        this.sensor = new double[4];
        this.qLearning = new QLearning(this.sensor.Length, .2f, .1f, .1f);

        this.updateIO();

    }

    // Update is called once per frame
    void Update() {
    }

    public void MoveLeft(float force) {
        this.cart.AddForce(new Vector3(-force, 0f, 0f));
    }

    public void MoveRight(float force) {
        this.cart.AddForce(new Vector3(force, 0f, 0f));
    }

   
    public void Reset() {
        Vector3 position = new Vector3(0f, 0f, -5f);
        this.pole.transform.position = position;

        Quaternion rotation = new Quaternion(0f, 0f, 0f, 0f);
        this.pole.transform.rotation = rotation;
    }


    private void borderTeleport() {
        Vector3 position = this.transform.position;
        if (position.x < -10f) {
            position.x = 10f;
            this.transform.position = position;

        } else if (position.x >= 10f) {
            position.x = -10f;
            this.transform.position = position;
        }
    }

    private void updateIO() {
        Vector3 positionTip = this.tip.position;
        this.reward = Math.Max(Math.Min(positionTip.y / 10f, 1f), -1f);

        double velocityCart = this.cart.velocity.x;
        double angularVelocityPole = this.cart.angularVelocity.z;
        double positionXTip = this.tip.transform.position.x - this.transform.position.x;
        double positionYTip = this.tip.transform.position.y - this.transform.position.y;

        this.sensor[0] = velocityCart;  // -20, 20
        this.sensor[1] = angularVelocityPole;
        this.sensor[2] = positionXTip;  // 
        this.sensor[3] = positionYTip;  // 
    }

    private void controlUpdate() {
        float action = (float) this.qLearning.React(this.sensor, this.reward);
            
        if (action < 0d) {
            this.MoveLeft(-action);
        } else {
            this.MoveRight(action);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            this.MoveLeft(50f);

        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            this.MoveRight(50f);
        }
    }

    void FixedUpdate() {
        this.updateIO();
        float time = Time.realtimeSinceStartup;
        if (.5f < time - this.timeLast) {
            this.controlUpdate();
            this.timeLast = time;
        }
        this.borderTeleport();
    }
}
