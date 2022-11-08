using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float Speed;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += Speed * Time.deltaTime * Vector3.forward;
        else if (Input.GetKey(KeyCode.S))
            transform.position += Speed * Time.deltaTime * Vector3.back;

        if (Input.GetKey(KeyCode.A))
            transform.position += Speed * Time.deltaTime * Vector3.left;
        else if (Input.GetKey(KeyCode.D))
            transform.position += Speed * Time.deltaTime * Vector3.right;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            SceneManager.LoadScene(0);
        }
    }
}
