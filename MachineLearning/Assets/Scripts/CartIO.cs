using Rosetta;
using System;
using UnityEngine;


[ExecuteInEditMode]
public class CartIO : MonoBehaviour
{
    protected Rigidbody rigidBody;
    public float force = 50f;
    public float reward = 0f;

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

    public void Debug() {
        Matrix A = new Matrix(
            6, 6,
            new double[] {
                1.1, 0.12, 0.13, 0.12, 0.14, -0.12,
                1.21, 0.63, 0.39, 0.25, 0.16, 0.1,
                1.03, 1.26, 1.58, 1.98, 2.49, 3.13,
                1.06, 1.88, 3.55, 6.7, 12.62, 23.8,
                1.12, 2.51, 6.32, 15.88, 39.9, 100.28,
                1.16, 3.14, 9.87, 31.01, 97.41, 306.02
            });
        Vector B = new Vector(new double[] { -0.01, 0.61, 0.91, 0.99, 0.60, 0.02 });
        A.ElimPartial(B);
        B.print();
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
