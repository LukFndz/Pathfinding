using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private float _speed;

    private void Update()
    {
        Move();
    }

    public void Move()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += _speed * Time.deltaTime * new Vector3(0, 0, 1);
        else if (Input.GetKey(KeyCode.S))
            transform.position += _speed * Time.deltaTime * new Vector3(0, 0, -1);

        if (Input.GetKey(KeyCode.A))
            transform.position += _speed * Time.deltaTime * new Vector3(-1, 0, 0);
        else if (Input.GetKey(KeyCode.D))
            transform.position += _speed * Time.deltaTime * new Vector3(1, 0, 0);
    }
}
