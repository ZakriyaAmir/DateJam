using UnityEngine;

namespace Watermelon
{
    public class CurvatureManager : MonoBehaviour
    {
        [SerializeField] float curveOffset = 5;
        [SerializeField] float curvePower = 1;

        private void Awake()
        {
            Shader.SetGlobalFloat("_CurveOffset", curveOffset);
            Shader.SetGlobalFloat("_CurvePower", curvePower);
        }

        private void OnValidate()
        {
            Shader.SetGlobalFloat("_CurveOffset", curveOffset);
            Shader.SetGlobalFloat("_CurvePower", curvePower);

        }
    }
}