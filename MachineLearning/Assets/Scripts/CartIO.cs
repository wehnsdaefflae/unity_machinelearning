using System;
using UnityEngine;


[ExecuteInEditMode]
public class CartIO : MonoBehaviour
{
    protected Rigidbody rigidBody;
    public float force = 50f;
    public float reward = 0f;
    private RegressorPolynomial regressorPolynomial = new RegressorPolynomial(1, 4);

    // Start is called before the first frame update
    void Start()
    {
        this.rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
    }

    public void MoveLeft(float force) {
        this.rigidBody.AddForce(new Vector3(-force, 0f, 0f));
    }

    public void MoveRight(float force) {
        this.rigidBody.AddForce(new Vector3(force, 0f, 0f));
    }

   
    public void Reset() {
        GameObject objectPole = this.transform.GetChild(0).gameObject;
        Rigidbody pole = objectPole.GetComponent<Rigidbody>();

        Vector3 position = new Vector3(0f, 0f, -5f);
        pole.transform.position = position;

        Quaternion rotation = new Quaternion(0f, 0f, 0f, 0f);
        pole.transform.rotation = rotation;
    }

    private void keyInput() {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            this.MoveLeft(this.force);
        
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            this.MoveRight(this.force);
        }
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

    private void updateReward() {
        GameObject objectPole = this.transform.GetChild(0).gameObject;
        GameObject tip = objectPole.transform.GetChild(0).gameObject;

        Vector3 positionTip = tip.transform.position;
        this.reward = Math.Max(Math.Min(positionTip.y / 10f, 1f), -1f);
    }

    void FixedUpdate() {
        this.keyInput();
        this.borderTeleport();
        this.updateReward();

    }
}
