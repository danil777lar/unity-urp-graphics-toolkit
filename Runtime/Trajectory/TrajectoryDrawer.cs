using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Larje.Graphics.Trajectory
{
public class TrajectoryDrawer : MonoBehaviour
{
    [Header("Simulation Options")] [SerializeField]
    private float _updateDelta;

    [SerializeField] private float _drag;
    [SerializeField] private float _gravityScale;
    [SerializeField] private string[] _ignoreLayers;
    [Header("Visual")] [SerializeField] private float _startCutLenght;
    [SerializeField] private Transform _drawRoot;
    [Header("Line")] [SerializeField] private LineRenderer _linePrefab;
    [Header("Trail")] [SerializeField] private float _trailSpeed;
    [SerializeField] private ParticleSystem _trailPrefab;
    [Header("Hit")] [SerializeField] private ParticleSystem _hitPrefab;

    public float maxLenght;
    public bool useTrail;

    private float _trailDistance;

    private LineRenderer _lineInstance;
    private ParticleSystem _trailInstance;
    private ParticleSystem _hitInstance;


    public void ShowTrajectory(Vector3 force)
    {
        TrySpawnLine();
        TrySpawnTrail();
        TrySpawnHit();

        bool showHit = false;
        float cuttedLenght = 0f;
        Vector3 velocity = force;
        Vector3 position = _drawRoot ? _drawRoot.position : transform.position;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < 100; i++)
        {
            points.Add(position);

            velocity = velocity * (1f - _updateDelta * _drag);
            velocity.y -= 9.8f * _gravityScale * _updateDelta;

            Vector3 prevPosition = position;
            position = position + velocity * _updateDelta;

            if (cuttedLenght < _startCutLenght)
            {
                cuttedLenght += (velocity * _updateDelta).magnitude;
                points.RemoveAt(0);
            }

            RaycastHit2D hit = Physics2D.Raycast(prevPosition, position - prevPosition,
                velocity.magnitude * _updateDelta, ~LayerMask.GetMask(_ignoreLayers));
            if (hit)
            {
                showHit = true;
                points.Add(new Vector3(hit.point.x, hit.point.y, prevPosition.z));
                break;
            }
        }

        float totalLenght = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (totalLenght + Vector3.Distance(points[i], points[i + 1]) >= maxLenght)
            {
                points[i + 1] = points[i] + (points[i + 1] - points[i]).normalized * (maxLenght - totalLenght);
                if (i < points.Count - 2)
                {
                    showHit = false;
                    points.RemoveRange(i + 2, points.Count - (i + 2));
                    break;
                }
            }
            else
            {
                totalLenght += Vector3.Distance(points[i], points[i + 1]);
            }
        }

        TryUpdateLine(points);
        TryUpdateTrail(points);
        TryUpdateHit(points, showHit);
    }

    public void HideTrajectory()
    {
        TryDestroyLine();
        TryDestroyTrail();
        TryDestroyHit();
    }


    #region Line

    private void TrySpawnLine()
    {
        if (_lineInstance || !_linePrefab) return;

        _lineInstance = Instantiate(_linePrefab);
        _lineInstance.transform.SetParent(transform);
        _lineInstance.transform.localPosition = Vector3.zero;
    }

    private void TryUpdateLine(List<Vector3> points)
    {
        if (!_lineInstance) return;

        _lineInstance.positionCount = points.Count;
        _lineInstance.SetPositions(points.ToArray());
    }

    private void TryDestroyLine()
    {
        if (_lineInstance)
        {
            Destroy(_lineInstance.gameObject);
        }
    }

    #endregion

    #region Trail

    private void TrySpawnTrail()
    {
        if (!_trailInstance && _trailPrefab)
        {
            _trailInstance = Instantiate(_trailPrefab);
            _trailInstance.transform.SetParent(transform);
            _trailDistance = 0f;
        }
    }

    private void TryUpdateTrail(List<Vector3> points)
    {
        if (!_trailInstance) return;

        if (!_trailInstance.isPlaying)
        {
            _trailDistance = 0f;
        }
        else
        {
            float distance = 0f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (distance + Vector3.Distance(points[i], points[i + 1]) > _trailDistance)
                {
                    _trailInstance.transform.position =
                        points[i] + (points[i + 1] - points[i]).normalized * (_trailDistance - distance);
                    break;
                }
                else if (i == points.Count - 2)
                {
                    TryDestroyTrail();
                    TrySpawnTrail();
                }
                else
                {
                    distance += Vector3.Distance(points[i], points[i + 1]);
                }
            }

            _trailDistance += _trailSpeed * (Time.deltaTime / Time.timeScale);
        }

        if (useTrail && !_trailInstance.isPlaying)
        {
            _trailInstance.Play();
        }

        if (!useTrail && _trailInstance.isPlaying)
        {
            _trailInstance.Stop();
        }
    }

    private void TryDestroyTrail()
    {
        if (_trailInstance)
        {
            _trailInstance.Stop();
            Destroy(_trailInstance.gameObject, _trailInstance.main.duration);
            _trailInstance = null;
        }
    }

    #endregion

    #region Hit

    private void TrySpawnHit()
    {
        if (!_hitInstance && _hitPrefab)
        {
            _hitInstance = Instantiate(_hitPrefab);
            _hitInstance.transform.SetParent(transform);
        }
    }

    private void TryUpdateHit(List<Vector3> points, bool show)
    {
        if (_hitInstance)
        {
            _hitInstance.transform.position = points[points.Count - 1];
            _hitInstance.gameObject.SetActive(show);
        }
    }

    private void TryDestroyHit()
    {
        if (_hitInstance)
        {
            _hitInstance.Stop();
            Destroy(_hitInstance.gameObject, _hitInstance.main.duration);
            _trailInstance = null;
        }
    }

    #endregion
}
}