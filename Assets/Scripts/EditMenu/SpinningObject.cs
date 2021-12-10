using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningObject : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The current selected gameObject.")]
    GameObject m_SelectedObject;
    /// <summary>
    /// (Read Only) The current selected gameObject.
    /// </summary>
    public GameObject selectedObject
    {
        get => m_SelectedObject;
        set => m_SelectedObject = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(selectedObject, transform);
    }

    // Update is called once per frame
    void Update()
    {
        m_SelectedObject.transform.Rotate(0, 0, 50 * Time.deltaTime);
    }
}
