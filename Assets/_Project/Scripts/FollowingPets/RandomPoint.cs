using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPoint : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private PointFollower _pF;

    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _speed;

    private Vector3 _endPos, _distance;
    private float _current;
    void Start()
    {
        transform.position = _player.position + new Vector3(5, -5);
        _endPos = _player.position;
    }

    void Update()
    {

        // this prevent change children's position (pet) according to parent's position (player)
        _distance = _player.position - _endPos;
        _endPos = _player.position;
        transform.position -= _distance;

        if (Vector2.Distance(transform.position, _player.position) > 3)
        {
            _current = Mathf.MoveTowards(0, 1, _speed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, _player.position + new Vector3 (-1,1,0), 10 * _curve.Evaluate(_current));
        }

    }

    public Vector2 CreateNewPosition(float range)
    {
        Vector2 direction = transform.position + Random.insideUnitSphere * range;
        return direction;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _pF.Range);
    }
#endif
}
