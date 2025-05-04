using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src
{
    internal class MoveUtility
    {
        /*public void MoveUnitAlongGreatCircle(GameObject unit, Vector3 startPoint, Vector3 endPoint, float duration)
        {
            StartCoroutine(MoveCoroutine(unit, startPoint, endPoint, duration));
        }

        private IEnumerator MoveCoroutine(GameObject unit, Vector3 startPoint, Vector3 endPoint, float duration)
        {
            float startTime = Time.time;

            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;

                // Interpolation sphérique entre les points
                unit.transform.position = Vector3.Slerp(startPoint, endPoint, t);

                // Orienter l'unité dans la direction du mouvement
                if (t < 0.99f) // Éviter les calculs quand on est presque arrivé
                {
                    Vector3 nextPos = Vector3.Slerp(startPoint, endPoint, t + 0.01f);
                    unit.transform.forward = (nextPos - unit.transform.position).normalized;
                }

                yield return null;
            }

            // S'assurer que l'unité arrive exactement à la position finale
            unit.transform.position = endPoint;
        }*/
    }
}
