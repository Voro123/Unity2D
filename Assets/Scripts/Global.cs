using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class Global
    {
        private static CameraController camera;
        public static List<Transform> followingEnemys = new List<Transform> { }; // 存储正在追踪玩家的enemy对象数组
        public static void addFollowingEnemy(Transform enemy)
        {
            if (!followingEnemys.Contains(enemy))
            {
                followingEnemys.Add(enemy);
                transBgm(1);
            }
        }

        public static void removeFollowingEnemy(Transform enemy)
        {
            if (followingEnemys.Contains(enemy))
            {
                followingEnemys.Remove(enemy);
                transBgm(0);
            }
        }

        private static void transBgm(int id)
        {
            if (camera == null)
            {
                camera = GameObject.Find("Main Camera").GetComponent<CameraController>();
            }
            camera.TransBgm(id);
        }
    }
}
