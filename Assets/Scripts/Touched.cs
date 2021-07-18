using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touched : MonoBehaviour
{
    public float damage = 10; // �˺�
    public float force = 10; // �����
    void Start()
    {
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Low") { return; }

        if (collision.tag == "Enemy") {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy.death) { return; }
            // Ŀ��Ϊ����ʱ������˺���λ��
            GameObject.Destroy(gameObject, 0.1f);
            Rigidbody2D rgb = collision.GetComponent<Rigidbody2D>();
            if (!rgb) { throw new System.Exception("������û��2D�������"); }
            enemy.Hitted(transform.up, force * Random.Range(0.8f, 1.2f), damage);
        } else
        {
            // Ŀ��Ϊ�ϰ���ʱ
            GameObject.Destroy(gameObject, 0.1f);
        }
    }
}
