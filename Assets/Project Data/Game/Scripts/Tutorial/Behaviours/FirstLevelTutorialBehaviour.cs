using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FirstLevelTutorialBehaviour : MonoBehaviour
    {
        [SerializeField] Transform encirclePointTransform;
        public Transform EncirclePointTransform => encirclePointTransform;

        [SerializeField] Transform crystalsPointTransform;
        public Transform CrystalPointTransform => crystalsPointTransform;

        [SerializeField] GameObject crystalsBorderObject;
        public GameObject CrystalsBorderObject => crystalsBorderObject;

        [Header("Enemy")]
        [SerializeField] GameObject enemyBorderObject;
        public GameObject EnemyBorderObject => enemyBorderObject;
    }
}