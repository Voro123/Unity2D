using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touched : MonoBehaviour
{
    public float damage = 10; // 伤害
    public float force = 10; // 冲击力
    void Start()
    {
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Low") { return; }

        if (collision.tag == "Enemy") {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy.death) { return; }
            // 目标为怪物时，造成伤害和位移
            GameObject.Destroy(gameObject, 0.1f);
            Rigidbody2D rgb = collision.GetComponent<Rigidbody2D>();
            if (!rgb) { throw new System.Exception("对象上没有2D刚体组件"); }
            enemy.Hitted(transform.up, force * Random.Range(0.8f, 1.2f), damage);
        } else
        {
            // 目标为障碍物时
            GameObject.Destroy(gameObject, 0.1f);
        }
    }
}
